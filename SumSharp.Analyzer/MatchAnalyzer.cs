using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace SumSharp.Analyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MatchAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor NonExhaustiveMatchRule = new DiagnosticDescriptor(
        "SumSharp0001",
        title: "Non-exhaustive match",
        messageFormat: "Failure to handle case(s): {0}. Handle all cases or provide a default case (_) handler",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor RedundantDefaultCaseRule = new DiagnosticDescriptor(
        "SumSharp0002",
        title: "Redundant default case",
        messageFormat: "All cases are handled. Default case handler will never be used",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor UnamedCaseHandlerRule = new DiagnosticDescriptor(
        "SumSharp0003",
        title: "Unnamed case handler",
        messageFormat: "Handler for case(s) {0} specified by position rather than name. Consider specifying by name to make code clearer and prevent bugs/compilation errors if case ordering changes",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(NonExhaustiveMatchRule, RedundantDefaultCaseRule, UnamedCaseHandlerRule);

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

        var caseNames = 
            methodSymbol.ContainingType.GetAttributes()
            .Where(attr => attr.AttributeClass.Name == "UnionCaseAttribute")
            .Select(attr => (string)attr.ConstructorArguments[0].Value!)
            .ToArray();

        if (caseNames.Length == 0)
        {
            return;
        }

        var unnamedArgs = new List<string>();

        var passedArgs = 
            invocation.ArgumentList.Arguments
            .Select((arg, i) =>
            {
                if (arg.NameColon is not null)
                {
                    return arg.NameColon.Name.Identifier.Text;
                }

                unnamedArgs.Add(caseNames[i]);

                return caseNames[i];
            })
            .ToImmutableHashSet();

        bool hasDefaultHandler = passedArgs.Contains("_");

        var missingCases = caseNames.Where(name => !passedArgs.Contains(name)).ToArray();

        if (missingCases.Length > 0 && !hasDefaultHandler)
        {
            var diagnostic = Diagnostic.Create(
                NonExhaustiveMatchRule,
                invocation.GetLocation(),
                $"[{string.Join(", ", missingCases)}]");

            context.ReportDiagnostic(diagnostic);
        }
        else if (missingCases.Length == 0 && hasDefaultHandler)
        {
            var diagnostic = Diagnostic.Create(
                RedundantDefaultCaseRule,
                invocation.GetLocation());

            context.ReportDiagnostic(diagnostic);
        }

        if (unnamedArgs.Count > 0)
        {
            var diagnostic = Diagnostic.Create(
                UnamedCaseHandlerRule,
                invocation.GetLocation(),
                $"[{string.Join(", ", unnamedArgs)}]");

            context.ReportDiagnostic(diagnostic);
        }
    }
}
