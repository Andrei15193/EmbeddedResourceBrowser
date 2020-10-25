using System;
using System.IO;
using CodeMap.DeclarationNodes;
using CodeMap.Handlebars;

namespace EmbeddedResourceBrowser.Documentation
{
    public class FileTemplateWriterDeclarationNodeVisitor : TemplateWriterDeclarationNodeVisitor
    {
        private readonly IMemberReferenceResolver _memberFileNameResolver;

        public FileTemplateWriterDeclarationNodeVisitor(IMemberReferenceResolver memberFileNameResolver, TemplateWriter templateWriter)
            : base(templateWriter)
            => _memberFileNameResolver = memberFileNameResolver;

        protected override TextWriter GetTextWriter(DeclarationNode declarationNode)
            => new StreamWriter(new FileStream(Path.Combine(Environment.CurrentDirectory, _memberFileNameResolver.GetFileName(declarationNode)), FileMode.Create, FileAccess.Write, FileShare.Read));
    }
}