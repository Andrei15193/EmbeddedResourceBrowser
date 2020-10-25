using System;
using System.IO;
using CodeMap.DeclarationNodes;
using CodeMap.Handlebars;

namespace EmbeddedResourceBrowser.Documentation
{
    internal static class Program
    {
        internal static void Main()
        {
            var embeddedResourceBrowserAssembly = typeof(EmbeddedDirectory).Assembly;
            var memberReferenceResolver = new DefaultMemberReferenceResolver(embeddedResourceBrowserAssembly, "netstandard-1.6");
            var templateWriter = new EmbeddedResourceBrowserHandlebarsTemplateWriter(memberReferenceResolver);
            var nodeVisitor = new FileTemplateWriterDeclarationNodeVisitor(memberReferenceResolver, templateWriter);

            DeclarationNode.Create(embeddedResourceBrowserAssembly).Apply(new DocumentationAdditon()).Accept(nodeVisitor);

            var assets = new EmbeddedDirectory(typeof(Program).Assembly).Subdirectories["Assets"];
            assets.CopyTo(new DirectoryInfo(Environment.CurrentDirectory));
        }
    }
}