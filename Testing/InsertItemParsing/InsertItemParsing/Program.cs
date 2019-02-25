using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InsertItemParsing
{
    class Program
    {
        // b. "<item name> into a <small box|big box> with tags <tag0 tag1 tag2 ...>"
        // c. "<item name> with tags <tag0 tag1 tag2 ...> into a <small box|big box>"
        static void Main(string[] args)
        {
            string[] jsonTests = {
                "{\"Info\":\"Yellow LED into a small box with tags yellow led diode light\",\"Quantity\":2}",
                "{\"Info\":\"Yellow LED with tags yellow led diode light into a small box\",\"Quantity\":2}",
                "{\"Info\":\"Yellow LED with tags yellow led diode light\",\"Quantity\":2}",
                "{\"Info\":\"Yellow LED into a small box\",\"Quantity\":2}",
                "{\"Info\":\"Yellow LED into a large container with tags yellow led emitting diode light\",\"Quantity\":2}",
                "{\"Info\":\"Yellow LED with tags yellow led diode emitting light into a big box\",\"Quantity\":2}",
            };

            foreach (string json in jsonTests)
            {

                dynamic jsonRequestData = JsonConvert.DeserializeObject(json);

                string info = jsonRequestData["Info"];
                int quantity = jsonRequestData["Quantity"];

                string infoLower = info.ToLowerInvariant();

                bool hasBox = TryGetBoxInfo(infoLower, out int boxIndex, out string boxSearch, out bool useSmallBox);
                bool hasTags = TryGetTagsInfo(infoLower, boxIndex, out int tagsIndex, out HashSet<string> tags);
                string item = GetItemInfo(info, hasBox, hasTags, boxIndex, tagsIndex);

                string itemLower = item.ToLowerInvariant();

                tags.UnionWith(itemLower.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(a => a.Trim()));

                Console.WriteLine(json);
                Console.WriteLine(JsonConvert.SerializeObject(jsonRequestData, Formatting.Indented));
                Console.WriteLine($"item: {item}\nQuantity: {quantity}\nTags:[{string.Join(",", tags)}]\nIsSmallBox: {useSmallBox}\nhasBox: {hasBox}\nhasTags: {hasTags}\nboxIndex: {boxIndex}\ntagsIndex {tagsIndex}");
                Console.WriteLine();
            }

            Console.ReadKey();
        }

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

        public static bool TryGetBoxInfo(string info, out int startIndex, out string searchTerm, out bool useSmallBox)
        {
            string[] boxPrefix = { "into a", "in a" };
            BoxType[] boxTypes = 
            {
                new BoxType("big", false), new BoxType("large", false),
                new BoxType("small", true), new BoxType("little", true)
            };
            string[] boxNames = { "box", "container" };
            foreach(string prefix in boxPrefix)
            {
                foreach(BoxType boxType in boxTypes)
                {
                    foreach(string name in boxNames)
                    {
                        string searchString = $" {prefix} {boxType.Type} {name}";
                        int index = info.IndexOf(searchString);
                        if (index != -1)
                        {
                            useSmallBox = boxType.IsSmallBox;
                            searchTerm = searchString;
                            startIndex = index;
                            return true;
                        }
                    }
                }
            }

            searchTerm = string.Empty;
            useSmallBox = true;
            startIndex = -1;
            return false;
        }

        public static bool TryGetTagsInfo(string info, int boxIndex, out int startIndex, out HashSet<string> tags)
        {
            string tagsTag = " with tags ";
            startIndex = info.IndexOf(tagsTag);

            if (startIndex == -1)
            {
                tags = new HashSet<string>();
                return false;
            }

            string tagsString = string.Empty;
            int tagsStartIndex = startIndex + tagsTag.Length;

            if (startIndex < boxIndex)
            {
                tagsString = info.Substring(tagsStartIndex, boxIndex - tagsStartIndex);
            }
            else
            {
                tagsString = info.Substring(tagsStartIndex);
            }

            if (!string.IsNullOrEmpty(tagsString))
            {
                tagsString = tagsString.ToLowerInvariant();

                tags = new HashSet<string>(
                    tagsString
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Select(a => a.Trim()));

                return true;
            }
            else
            {
                tags = new HashSet<string>();
                return false;
            }
        }

        public static string GetItemInfo(string info, bool hasBox, bool hasTags, int boxIndex, int tagsIndex)
        {
            string item = string.Empty;
            if (hasBox && hasTags)
            {
                item = info.Substring(0, tagsIndex < boxIndex ? tagsIndex : boxIndex);
                return item;
            }
            else if (hasBox)
            {
                item = info.Substring(0, boxIndex);
                return item;
            }
            else if (hasTags)
            {
                item = info.Substring(0, tagsIndex);
                return item;
            }
            return info;
        }
    }
}
