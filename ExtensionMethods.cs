using System.Linq;
using System.Xml.Linq;


namespace XMerge
{
    public static class ExtensionMethods
    {
        public static XElement FindOrAddElement(this XContainer xml, string nodeName)
        {
            var node = xml.Descendants().FirstOrDefault(x => x.Name == nodeName);
            if (node == null)
                xml.Add(new XElement(nodeName));
            return xml.Descendants().FirstOrDefault(x => x.Name == nodeName);
        }

        //public static string GetPath(this XElement node)
        //{
        //    string path = node.Name.ToString();
        //    XElement currentNode = node;
        //    while (currentNode.Parent != null)
        //    {
        //        currentNode = currentNode.Parent;
        //        path = currentNode.Name.ToString() + @"\" + path;
        //    }
        //    return path;
        //}

        public static string GetPath(this XElement element)
        {
            return string.Join("/", element.AncestorsAndSelf().Reverse()
                .Select(e =>
                {
                    var index = GetIndex(e);

                    if (index == 1)
                    {
                        return e.Name.LocalName;
                    }

                    return string.Format("{0}[{1}]", e.Name.LocalName, GetIndex(e));
                }));

        }

        private static int GetIndex(XElement element)
        {
            var i = 1;

            if (element.Parent == null)
            {
                return 1;
            }

            foreach (var e in element.Parent.Elements(element.Name.LocalName))
            {
                if (e == element)
                {
                    break;
                }

                i++;
            }

            return i;
        }
    }
}
