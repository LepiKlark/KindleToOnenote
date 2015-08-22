using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpKindleHighlightsExtractorLib;

namespace CSharpHighlightExamples
{
    class Program
    {
        static void Main(string[] args)
        {
            HighlightsExtractor extractor = new HighlightsExtractor();
            extractor.LogIn("AleksandarTomic88@gmail.com", "enter your password here.");

            foreach (BookWithHighlights bookWithHighlights in extractor.Crawl())
            {
                Console.WriteLine("===============================");
                Console.WriteLine("BookName {0}", bookWithHighlights.BookName);
                foreach (string highlight in bookWithHighlights.Highlights)
                {
                    Console.WriteLine(highlight);
                }
            }
        }
    }
}
