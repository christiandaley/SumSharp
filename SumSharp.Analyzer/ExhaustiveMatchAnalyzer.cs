using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace SumSharp.Analyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ExhaustiveMatchAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "SumSharp0001";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        title: "Non-exhaustive match",
        messageFormat: "Match fails to handle cases: {0}. Either handle all cases or provide a default case (_) handler",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {

        var invocation = (InvocationExpressionSyntax)context.Node;

        var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation);

        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
        {
            return;
        }

        if (methodSymbol.Name != "Match" && methodSymbol.Name != "Switch")
        {
            return;
        }

        var receiverType = methodSymbol.ContainingType;

        if (!IsSumSharpUnion(receiverType))
        {
            return;
        }

        var caseNames = GetCaseNames(receiverType);

        var passedArgs = 
            invocation.ArgumentList.Arguments
            .Select((arg, i) => arg.NameColon?.Name.Identifier.Text ?? caseNames[i])
            .ToImmutableHashSet();

        bool hasDefault = passedArgs.Contains("_");

        var missingCases = caseNames.Where(name => !passedArgs.Contains(name)).ToArray();

        if (missingCases.Length > 0 && !hasDefault)
        {
            var diagnostic = Diagnostic.Create(
                Rule,
                invocation.GetLocation(),
                $"[{string.Join(", ", missingCases)}]");

            context.ReportDiagnostic(diagnostic);
        }
    }

    private bool IsSumSharpUnion(INamedTypeSymbol type)
    {
        return type.GetAttributes().Any(attr =>
            attr.AttributeClass.Name == "UnionCaseAttribute");
    }

    private string[] GetCaseNames(INamedTypeSymbol unionType)
    {
        return 
            unionType.GetAttributes()
            .Where(attr => attr.AttributeClass.Name == "UnionCaseAttribute")
            .Select(attr => (string)attr.ConstructorArguments[0].Value!)
            .ToArray();
    }
}
