using Pluralize.NET.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SingularizeTesting
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            Pluralizer p = new Pluralizer();

            Console.WriteLine(p.Singularize("Yellow LEDs"));
            Console.WriteLine(p.Singularize("breadboard headers"));
            Console.WriteLine(p.Singularize("volt regulators"));

            HashSet<string> hs = 
                "these are some tags"
                .ToLowerInvariant()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(tag => p.Singularize(tag.Trim()))
                .ToHashSet<string>();

            Console.WriteLine("HS1: " + string.Join(",", hs));

            HashSet<string> hs2 =
                ""
                .ToLowerInvariant()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(tag => p.Singularize(tag.Trim()))
                .ToHashSet<string>();

            Console.WriteLine("HS2: " + string.Join(",", hs2));
        }
    }
}
