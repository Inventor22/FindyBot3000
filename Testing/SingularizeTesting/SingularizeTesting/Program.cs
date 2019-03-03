using Pluralize.NET.Core;
using System;

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
        }
    }
}
