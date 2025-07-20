using Dotsum.Generator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Dotsum.Generator;

[Generator]
public class Generator : IIncrementalGenerator
{
    record CaseData(int Index, string Name, string? Type);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var dotsumClasses = 
            context.SyntaxProvider
            .ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: "Dotsum.CaseAttribute",
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                transform: static (ctx, _) => ctx.TargetSymbol as INamedTypeSymbol)
            .Where(static m => m is not null)
            .Collect();

        context.RegisterSourceOutput(
            context.CompilationProvider.Combine(dotsumClasses),
            static (spc, source) => Execute(source.Left, source.Right!, spc));

        static bool IsSyntaxTargetForGeneration(SyntaxNode node)
        {
            return node switch
            {
                ClassDeclarationSyntax c => c.Modifiers.Any(SyntaxKind.PartialKeyword),
                StructDeclarationSyntax s => s.Modifiers.Any(SyntaxKind.PartialKeyword),
                _ => false,
            };
        }

        /*static INamedTypeSymbol? GetSemanticTargetForGeneration(GeneratorAttributeSyntaxContext context)
        {
            var symbol = context.TargetSymbol;

            if (symbol is not INamedTypeSymbol namedTypeSymbol)
            {
                return null;
            }

            var attributeData = namedTypeSymbol.GetAttributes().FirstOrDefault(ad =>
                string.Equals(ad.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), "global::Dotsum.CaseAttribute"));

            return attributeData is null ? null : namedTypeSymbol;
        }*/
    }

    private static void Execute(Compilation compilation, ImmutableArray<INamedTypeSymbol> targets, SourceProductionContext context)
    {
        try
        {

            var caseAttrSymbol = compilation.GetTypeByMetadataName("Dotsum.CaseAttribute")!;

            var enableJsonSymbol = compilation.GetTypeByMetadataName("Dotsum.EnableJsonSerializationAttribute")!;

            var builder = new StringBuilder();

            foreach (var symbol in targets.Distinct(SymbolEqualityComparer.Default))
            {
                builder.Clear();

                var symbolHandler = new SymbolHandler(builder, (INamedTypeSymbol)symbol!, caseAttrSymbol, enableJsonSymbol);

                symbolHandler.Emit();

                context.AddSource($"{symbolHandler.NameWithoutTypeArguments}.g.cs", SourceText.From(builder.ToString(), Encoding.UTF8));
            }
        }
        catch (Exception ex)
        {
            throw new Exception(ex.StackTrace);
        }
    }
}