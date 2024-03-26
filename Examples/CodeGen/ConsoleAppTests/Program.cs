// See https://aka.ms/new-console-template for more information

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

string code = """
using GrainAuger.Abstractions.AugerJobs;
using GrainAuger.Examples.FraudDetection.WebApi.Detectors;

namespace GrainAuger.Examples.FraudDetection.WebApi.JobConfiguration;

public class FraudDetectionJob : ISomeOtherInterface, IAugerJobConfiguration
{
    public void RandomNoiseMethod()
    {
        Console.WriteLine("Random method");
    }

    public void Configure(AugerJobBuilder builder)
    {
        var inputStream = builder.FromStream("AugerStreamProvider", "input");
        
        var overLimitStream = inputStream.Process<OverLimitDetector>();
        var expiredCardStream = inputStream.Process<ExpiredCardDetector>();
        var normalDistributionStream = inputStream.Process<NormalDistributionDetector>();
        var smallThenLargeStream = inputStream.Process<SmallThenLargeDetector>();
    }
}
""";

var syntaxTree = CSharpSyntaxTree.ParseText(code);

var root = syntaxTree.GetRoot();

var classDeclaration = root.DescendantNodes().OfType<ClassDeclarationSyntax>()?
    .Where(c => c.BaseList?.Types.Any(t => t.Type.ToString() == "IAugerJobConfiguration") == true).FirstOrDefault();
var methodDeclaration = classDeclaration?.DescendantNodes().OfType<MethodDeclarationSyntax>()?
    .Where(m => m.Identifier.Text == "Configure").FirstOrDefault();

Dictionary<string, List<string>> dag = new();

if (methodDeclaration is not null)
{
    var statements = methodDeclaration.Body?.Statements;

    if (statements is not null)
    {
        foreach (var statement in statements)
        {
            Console.WriteLine(statement);
            Console.WriteLine(statement.GetType());
            
            // var overLimitStream = inputStream.Process<OverLimitDetector>();
            // Microsoft.CodeAnalysis.CSharp.Syntax.LocalDeclarationStatementSyntax
            // overLimitStream -> var
            // 
            // I wanted to extract from the above statement:
            // overLimitStream, OverLimitDetector, inputStream
            // and write
            // inputStream -[OverLimitDetector]-> overLimitStream
            
            // Instead I got:
            // var inputStream = builder.FromStream("AugerStreamProvider", "input");
            // Microsoft.CodeAnalysis.CSharp.Syntax.LocalDeclarationStatementSyntax
            // var -[]-> inputStream
            
            if (statement is LocalDeclarationStatementSyntax declarationStatement)
            {
                if (declarationStatement.Declaration.Type is not null)
                {
                    var outputStream = declarationStatement.Declaration.Variables.FirstOrDefault()?.Identifier.Text;
                    var inputStream = declarationStatement.Declaration.Type.ToString();
                    var genericType = declarationStatement.Declaration.Type.DescendantNodes().OfType<GenericNameSyntax>().FirstOrDefault();
                    
                    Console.WriteLine($"{inputStream} -[{genericType}]-> {outputStream}");
                    
                    if (dag.ContainsKey(inputStream))
                    {
                        dag[inputStream].Add(outputStream.ToString());
                    }
                    else
                    {
                        dag[inputStream] = new List<string> { outputStream.ToString() };
                    }
                }
            }
            
            Console.WriteLine();
            // if (statement is DeclarationExpressionSyntax expressionSyntax)
            // {
            //     if (expressionSyntax.Designation is SingleVariableDesignationSyntax variableDesignation)
            //     {
            //         var streamName = variableDesignation.Identifier.Text;
            //         var detectorName = expressionSyntax.Type.ToString();
            //
            //         Console.WriteLine($"{streamName} -> {detectorName}");
            //         
            //         if (dag.ContainsKey(streamName))
            //         {
            //             dag[streamName].Add(detectorName);
            //         }
            //         else
            //         {
            //             dag[streamName] = new List<string> { detectorName };
            //         }
            //     }
            // }
        }
    }
}

foreach (var pair in dag)
{
    Console.WriteLine($"{pair.Key} -> {string.Join(", ", pair.Value)}");
}
