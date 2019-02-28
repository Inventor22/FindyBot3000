
namespace FindyBot3000.AzureFunction
{
    public class FindTagsResponse
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public int Row { get; set; }
        public int Col { get; set; }
        public int TagsMatched { get; set; }
        public float Confidence { get; set; }
    }
}
