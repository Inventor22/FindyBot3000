

namespace FindyBot3000.AzureFunction
{
    using Pluralize.NET.Core;

    public class QueryHelper
    {
        private Pluralizer pluralizer = new Pluralizer();

        static QueryHelper()
        {
        }

        public static QueryHelper Instance { get; } = new QueryHelper();

        public string SingularizeAndLower(string text)
        {
            return this.pluralizer.Singularize(text.ToLowerInvariant());
        }
    }
}
