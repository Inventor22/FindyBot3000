

namespace FindyBot3000.AzureFunction
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class TagSet : HashSet<string>
    {
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
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Select(tag => QueryHelper.Instance.SingularizeAndLower(tag.Trim())));

                this.Remove("and");
            }
        }

        public void FormatAndAddTag(string tag)
        {
            this.Add(QueryHelper.Instance.SingularizeAndLower(tag));
            this.Remove("and");
        }

        public void ParseAndUnionWith(string sentence)
        {
            this.UnionWith(
                sentence
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(tag => QueryHelper.Instance.SingularizeAndLower(tag.Trim())));
            this.Remove("and");
        }

        public void FormatAndUnionWith(IEnumerable<string> tags)
        {
            this.UnionWith(tags.Select(tag => QueryHelper.Instance.SingularizeAndLower(tag.Trim())));
            this.Remove("and");
        }
    }
}
