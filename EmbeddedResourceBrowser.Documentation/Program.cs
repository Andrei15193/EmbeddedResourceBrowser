using System;
using System.IO;
using System.Linq;
using CodeMap.DeclarationNodes;
using CodeMap.Handlebars;

namespace EmbeddedResourceBrowser.Documentation
{
    internal static class Program
    {
        internal static void Main(params string[] args)
        {
            if (args.Length == 0)
                throw new ArgumentException("Expected output directory path as first argument.");

            var outputDirectory = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, args.First()));
            outputDirectory.Create();
            _ClearFolder(outputDirectory);

            var embeddedResourceBrowserAssembly = typeof(EmbeddedDirectory).Assembly;
            var memberReferenceResolver = new DefaultMemberReferenceResolver(embeddedResourceBrowserAssembly, "netstandard-1.6");
            var templateWriter = new EmbeddedResourceBrowserHandlebarsTemplateWriter(memberReferenceResolver);
            var nodeVisitor = new FileTemplateWriterDeclarationNodeVisitor(outputDirectory, memberReferenceResolver, templateWriter);

            DeclarationNode.Create(embeddedResourceBrowserAssembly).Apply(new DocumentationAdditon()).Accept(nodeVisitor);

            var assets = new EmbeddedDirectory(typeof(Program).Assembly).Subdirectories["Assets"];
            assets.CopyTo(outputDirectory);
        }

        private static void _ClearFolder(DirectoryInfo directory)
        {
            foreach (var file in directory.GetFiles())
                file.Delete();
        }
    }
}