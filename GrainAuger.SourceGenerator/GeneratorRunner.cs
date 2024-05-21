using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace GrainAuger.SourceGenerator;

/// <summary>
/// Required to run Orleans source generators AFTER GrainAuger build.
/// This is a workaround for the current limitation of the Roslyn source generators, as they do not have the ability to run in a specific order.
/// See this issue for more information:
/// https://github.com/dotnet/roslyn/issues/57239#issuecomment-1661052045 
/// </summary>
public class GeneratorRunner
{
    public static void Run(
        SourceProductionContext context,
        string hintNamePrefix,
        ISourceGenerator[] generators,
        params SyntaxTree[] syntaxTrees)
    {
        var compilation = CSharpCompilation.Create(
            null,
            syntaxTrees);

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generators);

        driver = driver.RunGenerators(compilation, context.CancellationToken);
        var runResult = driver.GetRunResult();

        foreach (var diagnostic in runResult.Diagnostics)
        {
            ReportDiagnostic(diagnostic);
        }

        foreach (var generatedSource in runResult.Results.SelectMany(result => result.GeneratedSources))
        {
            context.AddSource(GetHintName(generatedSource.HintName), generatedSource.SourceText);
        }

        void ReportDiagnostic(Diagnostic diagnostic)
        {
            // There will be an error if we report a diagnostic
            // from a different compilation, so we create a new one.
            var newDiagnostic = Diagnostic.Create(
                diagnostic.Descriptor,
                Location.None,
                diagnostic.Severity,
                diagnostic.AdditionalLocations,
                diagnostic.Properties,
                // SimpleDiagnostic class and _messageArgs field are internal.
                // We use Krafs.Publicizer to access them.
                []
                // TODO: Uncomment this line when the issue is resolved:
                //((Diagnostic.SimpleDiagnostic)diagnostic)._messageArgs
            );

            context.ReportDiagnostic(newDiagnostic);
        }

        string GetHintName(string nestedHintName)
        {
            return hintNamePrefix switch
            {
                _ when hintNamePrefix.EndsWith(".g.cs") => hintNamePrefix.Substring(0, hintNamePrefix.Length - ".g.cs".Length),
                _ when hintNamePrefix.EndsWith(".cs") => hintNamePrefix.Substring(0, hintNamePrefix.Length - ".cs".Length),
                _ => hintNamePrefix,
            } + "__" + nestedHintName;
        }
    }
}