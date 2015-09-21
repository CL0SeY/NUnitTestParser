using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace NUnitTestParser
{
    class Program
    {
        static readonly List<KeyValuePair<string, double>> TestFixtures = new List<KeyValuePair<string, double>>();
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: NUnitTestParser.exe <nunit-2-results-xml-file>");
                return;
            }
            var doc = XDocument.Load(new FileStream(args[0], FileMode.Open),LoadOptions.None);
            var results = doc.Root;
            
            ListTests(results, "");

            foreach (var result in TestFixtures.OrderByDescending(f => f.Value))
            {
                Console.WriteLine("{0} - {1}", result.Key, result.Value);
            }
        }

        private static void ListTests(XElement results, string prefix)
        {
            foreach (var xNode in results.Nodes().Where(n => n is XElement))
            {
                var thing = (XElement) xNode;
                var name = prefix;
                if (thing.Name == "test-suite")
                {
                    name = (prefix + "." + GetAttribute(thing, "name")).TrimStart('.');
                    if (name.EndsWith(".dll"))
                    {
                        name = "";
                    }
                    else
                    {
                        var time = GetAttribute(thing, "time");
                        if (time != null)
                        {
                            TestFixtures.Add(new KeyValuePair<string, double>(name, Double.Parse(time)));
                        }
                        else
                        {
                            double computedTime = 0;
                            foreach (var xNode1 in thing.Nodes().Where(n => n is XElement))
                            {
                                var tests = (XElement) xNode1;
                                if (tests.Name == "results")
                                {
                                    foreach (var xNode2 in tests.Nodes().Where(n => n is XElement))
                                    {
                                        var testCase = (XElement) xNode2;
                                        if (testCase.Name == "test-case")
                                        {
                                            var testTime = GetAttribute(testCase, "time");
                                            if (testTime != null)
                                            {
                                                computedTime += Double.Parse(testTime);
                                            }
                                        }
                                    }
                                }
                            }
                            if (computedTime > 0)
                            {
                                TestFixtures.Add(new KeyValuePair<string, double>(name, computedTime));
                            }
                        }
                    }
                }
                ListTests(thing, name);
            }
        }

        private static string GetAttribute(XElement results, string p)
        {
            var attr = results.Attributes().FirstOrDefault(a => a.Name == p);
            if (attr == null) return null;
            return attr.Value;
        }
    }
}
