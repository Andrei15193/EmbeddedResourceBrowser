using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CodeMap.DeclarationNodes;
using CodeMap.Handlebars;
using EmbeddedResourceBrowser.Documentation;

if (args.Length == 0)
    throw new ArgumentException("Expected output directory path as first argument.");

var outputDirectory = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, args.First()));
outputDirectory.Create();
foreach (var file in outputDirectory.GetFiles())
    file.Delete();

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