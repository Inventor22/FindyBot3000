

namespace FindyBot3000.AzureFunction
{
    using Pluralize.NET.Core;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class TagSet : HashSet<string>
    {
        // Only create the pluralizer when we need it
        private Lazy<Pluralizer> lazyPluralizer = new Lazy<Pluralizer>(() => new Pluralizer());
        private Pluralizer pluralizer => lazyPluralizer.Value;
        
        public TagSet()
        {
        }

        public TagSet(string sentence)
        {
            this.Clear();

            if (sentence != null)
            {
                this.UnionWith(
                    sentence
                    .ToLowerInvariant()
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Select(tag => pluralizer.Singularize(tag.Trim())));
            }
        }

        public void FormatAndAddTag(string tag)
        {
            this.Add(pluralizer.Singularize(tag.ToLowerInvariant()));
        }

        public void ParseAndUnionWith(string sentence)
        {
            this.UnionWith(
                sentence
                .ToLowerInvariant()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(tag => pluralizer.Singularize(tag.Trim())));
        }

        public void FormatAndUnionWith(IEnumerable<string> tags)
        {
            this.UnionWith(tags.Select(tag => pluralizer.Singularize(tag.Trim())));
        }
    }
}
