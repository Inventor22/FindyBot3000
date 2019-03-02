

namespace FindyBot3000.AzureFunction
{
    public class BoxType
    {
        public string Type { get; set; }

        public bool IsSmallBox { get; set; }

        public BoxType(string type, bool isSmallBox)
        {
            this.Type = type;
            this.IsSmallBox = isSmallBox;
        }
    }
}
