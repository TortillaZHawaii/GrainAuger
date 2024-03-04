using System.IO;
using Microsoft.CodeAnalysis;

namespace SourceGenerators;

/// <summary>
/// A sample source generator that creates C# classes based on the text file (in this case, Domain Driven Design ubiquitous language registry).
/// When using a simple text file as a baseline, we can create a non-incremental source generator.
/// </summary>
[Generator]
public class SampleSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        // No initialization required for this generator.
    }

    public void Execute(GeneratorExecutionContext context)
    {
        // If you would like to put some data to non-compilable file (e.g. a .txt file), mark it as an Additional File.

        // Go through all files marked as an Additional File in file properties.
        foreach (var additionalFile in context.AdditionalFiles)
        {
            if (additionalFile == null)
                continue;

            // Check if the file name is the specific file that we expect.
            if (Path.GetFileName(additionalFile.Path) != "DDD.UbiquitousLanguageRegistry.txt")
                continue;

            var text = additionalFile.GetText();
            if (text == null)
                continue;

            foreach (var line in text.Lines)
            {
                var className = line.ToString().Trim();

                // Build up the source code.
                string source = $@"// <auto-generated/>

namespace Entities
{{
    public partial class {className}
    {{
    }}
}}
";

                // Add the source code to the compilation.
                context.AddSource($"{className}.g.cs", source);
            }
        }
    }
}