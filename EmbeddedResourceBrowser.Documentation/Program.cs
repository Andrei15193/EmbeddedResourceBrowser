using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

            var embeddedResourceBrowserAssembly = Assembly.LoadFrom(Path.Combine(new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName, "EmbeddedResourceBrowser.dll"));
            var templateWriter = new HandlebarsTemplateWriter(
                new MemberReferenceResolver(
                    new Dictionary<Assembly, IMemberReferenceResolver>
                    {
                        { embeddedResourceBrowserAssembly, new CodeMapMemberReferenceResolver() }
                    },
                    new MicrosoftDocsMemberReferenceResolver("netstandard-1.6")
                )
            );

            DeclarationNode
                .Create(embeddedResourceBrowserAssembly)
                .Apply(new DocumentationAdditon())
                .Accept(new HandlebarsWriterDeclarationNodeVisitor(outputDirectory, templateWriter));
        }

        private static void _ClearFolder(DirectoryInfo directory)
        {
            foreach (var file in directory.GetFiles())
                file.Delete();
        }
    }
}