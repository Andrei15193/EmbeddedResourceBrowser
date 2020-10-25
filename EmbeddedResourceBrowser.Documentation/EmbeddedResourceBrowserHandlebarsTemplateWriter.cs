using System;
using System.Collections.Generic;
using CodeMap.Handlebars;

namespace EmbeddedResourceBrowser.Documentation
{
    public class EmbeddedResourceBrowserHandlebarsTemplateWriter : HandlebarsTemplateWriter
    {
        public EmbeddedResourceBrowserHandlebarsTemplateWriter(IMemberReferenceResolver memberReferenceResolver)
            : base(memberReferenceResolver)
        {
        }

        protected override IReadOnlyDictionary<string, string> GetPartials()
            => new Dictionary<string, string>(base.GetPartials(), StringComparer.OrdinalIgnoreCase)
            {
                ["Breadcrumbs"] = ReadFromEmbeddedResource(typeof(EmbeddedResourceBrowserHandlebarsTemplateWriter).Assembly, "EmbeddedResourceBrowser.Documentation.Partials.Breadcrumbs.hbs"),
                ["Layout"] = ReadFromEmbeddedResource(typeof(EmbeddedResourceBrowserHandlebarsTemplateWriter).Assembly, "EmbeddedResourceBrowser.Documentation.Partials.Layout.hbs")
            };

        protected override IReadOnlyDictionary<string, string> GetTemplates()
            => new Dictionary<string, string>(base.GetTemplates(), StringComparer.OrdinalIgnoreCase)
            {
                ["Assembly"] = ReadFromEmbeddedResource(typeof(EmbeddedResourceBrowserHandlebarsTemplateWriter).Assembly, "EmbeddedResourceBrowser.Documentation.Templates.Assembly.hbs")
            };
    }
}