using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CodeMap.DeclarationNodes;
using CodeMap.DocumentationElements;
using CodeMap.ReferenceData;
using static CodeMap.DocumentationElements.DocumentationElement;

namespace EmbeddedResourceBrowser.Documentation
{
    public class DocumentationAdditon : AssemblyDocumentationAddition
    {
        public override bool CanApply(AssemblyDeclaration assembly)
            => true;

        public override SummaryDocumentationElement GetSummary(AssemblyDeclaration assembly)
            => Summary(
                Paragraph(
                    Text(
                        (string)assembly.Attributes.Single(attribute => attribute.Type == typeof(AssemblyDescriptionAttribute)).PositionalParameters.Single().Value
                    )
                )
            );

        public override RemarksDocumentationElement GetRemarks(AssemblyDeclaration assembly)
        {
            var memberReferenceFactory = new MemberReferenceFactory();
            return Remarks(
                Paragraph(
                    Text("The library is rather small and provides a directory-like structure for browsing embedded resources. You can load resources in multiple ways depending on how you wish to use them in your application. Before detailing each approach it is important to note that embedded resources in .NET assemblies use the dot ("),
                    InlineCode("."),
                    Text(") character as a separator for directories. This can be a bit confusing because we can add the dot ("),
                    InlineCode("."),
                    Text(") in the name of a directory or a file and there will be absolutely no difference of having a directory structure or using the dot ("),
                    InlineCode("."),
                    Text(") in the name. For instance, the following two resources are identical and the compiler will issue an error indicating this.")
                ),
                CodeBlock(@"
Assembly/
    Directory/
        embedded file.txt
    Directory.embedded file.txt

Both resources are embedded (by default) with the following logical name:
Assembly.Directory.embedded file.txt
                ".Trim()),
                Paragraph(
                    Text("If you wish, you can provide a custom logical name for your embedded resource, keep in mind that you need to specify this manually. For instance, you can use the forwards slash ("),
                    InlineCode("/"),
                    Text(") as a separator by specifying the logical name of an embedded resource. For more information refer to: "),
                    Hyperlink("https://docs.microsoft.com/visualstudio/msbuild/common-msbuild-project-items#embeddedresource", "Common MSBuild project items - EmbeddedResource"),
                    Text(".")
                ),
                CodeBlock("<EmbeddedResource Include=\"Directory.embedded file.txt\" LogicalName=\"AssemblyName/Directory/embedded file.txt\"/>", new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "language", "xml" } }),
                Paragraph(
                    Text("The library detects the path separator from the embedded file name. It expects that embedded resource names start with the assembly name followed by the path separator. By default, this would mean the dot character ("),
                    InlineCode("."),
                    Text("), however resources can be embedded under a different name using the "),
                    InlineCode("LogicalName"),
                    Text(" attribute. When using this option ensure that the assembly name is added at the beginning of the name and add the path separator after it.")
                ),
                Paragraph(
                    Text("See "),
                    MemberReference(memberReferenceFactory.Create(typeof(EmbeddedDirectory))),
                    Text(" for more information.")
                ),

                Paragraph(
                    new InlineDocumentationElement[]
                    {
                        Text("Generally, you may be working with just one assembly from where you load embedded resources. In this case initialize a new "),
                        MemberReference(memberReferenceFactory.Create(typeof(EmbeddedDirectory))),
                        Text(" by passing the assembly object containing the embedded resources to the constructor. This will load all resources in a directory tree where the root is the directory having the same name as the assembly name and all resources can be browsed, and loaded, from there. The following method will concatenate the contents of each embedded file at the top level of the assembly.")
                    },
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "section", "Working With One Assembly" } }
                ),
                CodeBlock(@"
string ConcatResources()
{
    var embeddedDirectory = new EmbeddedDirectory(typeof(ACustomType).Assembly);

    var resultBuilder = new StringBuilder();
    foreach (var file in embeddedDirectory.Files)
        using (var streamReader = new StreamReader(file.OpenRead()))
            resultBuilder.AppendLine(streamReader.ReadToEnd());
    return resultBuilder.ToString();
}
                    ".Trim(),
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "language", "c#" } }
                ),

                Paragraph(
                    new InlineDocumentationElement[]
                    {
                        Text("Sometimes the project can be rather complex and embedded resources are placed in multiple assemblies. In this case an "),
                        MemberReference(memberReferenceFactory.Create(typeof(EmbeddedDirectory))),
                        Text(" can be created by providing all assemblies as constructor arguments which in turn will create a root directory containing a subdirectory for each provided assembly that have the same structure as in the working with one assembly scenario. The following method will concatenate the contents of each embedded file at the top level of each assembly, but also append the assembly name from where files are being read.")
                    },
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "section", "Working With Multiple Assemblies" } }
                ),
                CodeBlock(@"
string ConcatResources()
{
    var embeddedDirectory = new EmbeddedDirectory(typeof(ACustomTypeFromAssembly1).Assembly, typeof(ACustomTypeFromAssembly2).Assembly);

    var resultBuilder = new StringBuilder();
    foreach (var assemblyDirectory in embeddedDirectory.Subdirectories)
    {
        resultBuilder.AppendLine(assemblyDirectory.Name);
        resultBuilder.AppendLine(new string('-', 80));
        foreach (var file in embeddedDirectory.Files)
            using (var streamReader = new StreamReader(file.OpenRead()))
                resultBuilder.AppendLine(streamReader.ReadToEnd());
    }
    return resultBuilder.ToString();
}
                    ".Trim(),
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "language", "c#" } }
                ),
                Paragraph(
                    Text("This works well with pattern matching. To find different file or directory structures you can use the "),
                    MemberReference(memberReferenceFactory.Create(typeof(EmbeddedDirectory).GetMethod(nameof(EmbeddedDirectory.GetAllFiles)))),
                    Text(" and "),
                    MemberReference(memberReferenceFactory.Create(typeof(EmbeddedDirectory).GetMethod(nameof(EmbeddedDirectory.GetAllSubdirectories)))),
                    Text(" methods to get all files/directories and use "),
                    Hyperlink("https://docs.microsoft.com/dotnet/csharp/programming-guide/concepts/linq", "LINQ"),
                    Text(" to match different structures.")
                ),

                Paragraph(
                    new InlineDocumentationElement[]
                    {
                        Text("A different scenario when working with multiple assemblies is when we want to have resources that are placed in multiple assemblies, but instead of loading each of them we want to have a precedence between them, so even though we have multiple assemblies with embedded resources, we want to provide an implicit set that can be overriden in other assemblies. For instance, I would like to provide a default set of embedded resources that can be overridden by users of my library.")
                    },
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "section", "Working With Multiple Assemblies (Merge Method)" } }
                ),
                Paragraph(
                    Text("Given the following structure of embedded directories, I can override any of these resources (or add) from a different assembly using the "),
                    MemberReference(memberReferenceFactory.Create(typeof(EmbeddedDirectory).GetMethod(nameof(EmbeddedDirectory.Merge), 0, new[] { typeof(IEnumerable<Assembly>) }))),
                    Text(" method.")
                ),
                CodeBlock(@"
MyAssembly/
    Directory1/
        File 1.txt
        File 2.txt
    Directory2/
        File 1.txt
                    ".Trim()
                ),
                Paragraph(
                    Text("This would be the default set of resources, now, when loading mapping resources from multiple assemblies using the "),
                    MemberReference(memberReferenceFactory.Create(typeof(EmbeddedDirectory).GetMethod(nameof(EmbeddedDirectory.Merge), 0, new[] { typeof(IEnumerable<Assembly>) }))),
                    Text(" method some of these resources can be overridden and a few more resources can be added to the set. Having a second assembly with the following structure will create an "),
                    MemberReference(memberReferenceFactory.Create(typeof(EmbeddedDirectory))),
                    Text(" with resources from both assemblies.")
                ),
                CodeBlock(@"
OtherAssembly/
    Directory1/
        File 2.txt
        File 3.txt
    Directory3/
        File 1.txt
                    ".Trim()
                ),
                Paragraph(
                    Text("The resulting structure will be the following, note that the root directory, or any directory, does not have the assembly name anymore since directories can contain resources from mixed assemblies.")
                ),
                CodeBlock(@"
/
    Directory1/
        File 1.txt (from MyAssembly)
        File 2.txt (from OtherAssembly, matching resource names)
        File 3.txt (from OtherAssembly)
    Directory2/
        File 1.txt (from MyAssembly)
    Directory3/
        File 1.txt (from OtherAssembly)
                    ".Trim()
                ),
                Paragraph(
                    Text("To create an "),
                    MemberReference(memberReferenceFactory.Create(typeof(EmbeddedDirectory))),
                    Text(" using the "),
                    MemberReference(memberReferenceFactory.Create(typeof(EmbeddedDirectory).GetMethod(nameof(EmbeddedDirectory.Merge), 0, new[] { typeof(IEnumerable<Assembly>) }))),
                    Text(" method simply call the static method instead of calling the constructor.")
                ),
                CodeBlock(@"
string ConcatResources()
{
    var embeddedDirectory = EmbeddedDirectory.Merge(typeof(ACustomTypeFromAssembly1).Assembly, typeof(ACustomTypeFromAssembly2).Assembly);

    var resultBuilder = new StringBuilder();
    foreach (var file in embeddedDirectory.Files)
        using (var streamReader = new StreamReader(file.OpenRead()))
            resultBuilder.AppendLine(streamReader.ReadToEnd());
    return resultBuilder.ToString();
}
                    ".Trim(),
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "language", "c#" } }
                )
            );
        }
    }
}