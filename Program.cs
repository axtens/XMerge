using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace XMerge
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Needs path to xml files");
                return 1;
            }

            if (!Directory.Exists(args[0]))
            {
                Console.WriteLine("Needs path to xml files that's really there!");
                return 2;
            }

            var xmls = Directory.GetFiles(args[0]);
            var firstXml = xmls[0];
            Console.WriteLine(firstXml);
            var master = XDocument.Parse(File.ReadAllText(firstXml));
            master = Prefill(master);
            for (var i = 1; i < xmls.Length; i++)
            {
                var nextXml = xmls[i];
                Console.WriteLine($"\t{nextXml}");
                var next = XDocument.Parse(File.ReadAllText(nextXml));
                master = Merge(master, next);
                master = Prefill(master);
            }

            File.WriteAllText($"{(args.Length > 1 ? args[1] : "master")}.xml", master.Document.ToString());

            return 0;
        }

        private static XDocument Prefill(XDocument master)
        {
            foreach (var node in from node in (from node in master.Descendants()
                                               where node.DescendantNodes().Count() == 1
                                               select node)
                                 where !node.HasAttributes
                                 select node)
            {
                node.SetAttributeValue("maxlength", node.Value.Length);
                if (int.TryParse(node.Value, out int len))
                {
                    node.SetAttributeValue("type", len.GetType().Name);
                }
                else
                {
                    node.SetAttributeValue("type", node.Value.GetType().Name);
                }
            }

            return master;
        }

        private static XDocument Merge(XDocument master, XDocument next)
        {
            var nodes = from node in next.Descendants() select node;
            foreach (var node in nodes)
            {
                var path = node.GetPath();
                var there = master.XPathSelectElement(path);
                if (there == null)
                {
                    // add it somehow
                    var pathParts = path.Split('/').ToList();
                    pathParts.RemoveAt(pathParts.Count - 1);
                    path = string.Join("/", pathParts.ToArray());
                    there = master.XPathSelectElement(path);
                    there.Add(node);
                }
                else
                {
                    var attribs = from attr in there.Attributes() where attr.Name == "maxlength" select attr;
                    if (there.HasAttributes && attribs.Count() > 0)
                    {
                        var nlength = node.Value.Length; // int.Parse(there.Attribute("maxlength").Value);
                        var mlength = int.Parse(there.Attribute("maxlength").Value);
                        if (nlength > mlength)
                        {
                            there.SetAttributeValue("maxlength", nlength);
                        }
                    }
                }
            }

            return master;
        }

    }
}
