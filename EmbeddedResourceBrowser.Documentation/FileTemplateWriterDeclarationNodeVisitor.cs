using System.IO;
using CodeMap.DeclarationNodes;
using CodeMap.Handlebars;

namespace EmbeddedResourceBrowser.Documentation
{
    public class FileTemplateWriterDeclarationNodeVisitor : TemplateWriterDeclarationNodeVisitor
    {
        private readonly IMemberReferenceResolver _memberFileNameResolver;
        private readonly DirectoryInfo _outputDirectory;

        public FileTemplateWriterDeclarationNodeVisitor(DirectoryInfo outputDirectory, IMemberReferenceResolver memberFileNameResolver, TemplateWriter templateWriter)
            : base(templateWriter)
        {
            _memberFileNameResolver = memberFileNameResolver;
            _outputDirectory = outputDirectory;
        }

        protected override TextWriter GetTextWriter(DeclarationNode declarationNode)
            => new StreamWriter(new FileStream(Path.Combine(_outputDirectory.FullName, _memberFileNameResolver.GetFileName(declarationNode)), FileMode.Create, FileAccess.Write, FileShare.Read));
    }
}