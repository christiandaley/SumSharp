using SumSharp.Generator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace SumSharp.Generator;

[Generator]
public class Generator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var SumSharpClasses = 
            context.SyntaxProvider
            .ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: "SumSharp.UnionCaseAttribute",
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                transform: static (ctx, _) => ctx.TargetSymbol as INamedTypeSymbol)
            .Where(static m => m is not null)
            .Collect();

        context.RegisterSourceOutput(
            context.CompilationProvider.Combine(SumSharpClasses),
            static (spc, source) => Execute(source.Left, source.Right!, spc));

        static bool IsSyntaxTargetForGeneration(SyntaxNode node)
        {
            return node switch
            {
                ClassDeclarationSyntax c => c.Modifiers.Any(SyntaxKind.PartialKeyword),
                StructDeclarationSyntax s => s.Modifiers.Any(SyntaxKind.PartialKeyword),
                RecordDeclarationSyntax r => r.Modifiers.Any(SyntaxKind.PartialKeyword),
                _ => false,
            };
        }
    }

    private static void Execute(Compilation compilation, ImmutableArray<INamedTypeSymbol> targets, SourceProductionContext context)
    {
        var caseAttrSymbol = compilation.GetTypeByMetadataName("SumSharp.UnionCaseAttribute")!;

        var enableJsonSymbol = compilation.GetTypeByMetadataName("SumSharp.EnableJsonSerializationAttribute")!;

        var storageSymbol = compilation.GetTypeByMetadataName("SumSharp.StorageAttribute")!;

        var disableValueEqualitySymbol = compilation.GetTypeByMetadataName("SumSharp.DisableValueEqualityAttribute")!;

        var enableOneOfConversionsSymbol = compilation.GetTypeByMetadataName("SumSharp.EnableOneOfConversionsAttribute")!;

        var disableNullableSymbol = compilation.GetTypeByMetadataName("SumSharp.DisableNullableAttribute")!;

        var builder = new StringBuilder();

        foreach (var symbol in targets.Distinct(SymbolEqualityComparer.Default))
        {
            if (symbol == null)
            {
                continue;
            }

            builder.Clear();

            var symbolHandler = new SymbolHandler(builder,
                                                compilation,
                                                (INamedTypeSymbol)symbol,
                                                caseAttrSymbol,
                                                enableJsonSymbol,
                                                storageSymbol,
                                                disableValueEqualitySymbol,
                                                enableOneOfConversionsSymbol,
                                                disableNullableSymbol);

            symbolHandler.Emit();

            var fileName = $"{symbolHandler.FileFriendlyName}.g.cs";

            context.AddSource(fileName, SourceText.From(builder.ToString(), Encoding.UTF8));
        }
    }
}