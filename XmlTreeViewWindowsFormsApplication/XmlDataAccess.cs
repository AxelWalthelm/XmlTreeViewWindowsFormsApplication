using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace XmlTreeViewWindowsFormsApplication
{
    class XmlDataAccess
    {
        public static object DeseralizeObject(XmlNode xmlNode, Type type)
        {
            var overrides = new XmlAttributeOverrides();
            overrides.Add(type, new XmlAttributes { XmlType = new XmlTypeAttribute(xmlNode.LocalName) });
            XmlSerializer serializer = new XmlSerializer(type, overrides);
            using (XmlReader reader = new XmlNodeReader(xmlNode))
            {
                return serializer.Deserialize(reader);
            }
        }

        public static void SerializeObject(XmlNode xmlNode, object o)
        {
            var document = new XmlDocument();
            var navigator = document.CreateNavigator();

            using (XmlWriter writer = navigator.AppendChild())
            {
                var serializer = new XmlSerializer(o.GetType());
                var namespaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
                serializer.Serialize(writer, o, namespaces);
            }

            UpdateChildrenFromOtherDocument(xmlNode, document.FirstChild);
        }

        // Make oldNode have children equal to newNode, but modify only what has changed to avoid flickering,
        // assuming order of nodes stay the same and that nodes can only be deleted and inserted
        // (moving a node is both: delete and insert).
        // Document of newNode differs from the document of oldNode, so we need to copy them by XmlDocument.ImportNode().
        // When a node is different and needs be copied, all off its children are copied too.
        // TODO/TBD: could we change nodes instead of replacing them?
        private static void UpdateChildrenFromOtherDocument(XmlNode oldNode, XmlNode newNode)
        {
#if true // quick and simple
            while (oldNode.ChildNodes.Count > 0)
                oldNode.RemoveChild(oldNode.ChildNodes[0]);
            for (int i = 0; i < newNode.ChildNodes.Count; i++)
                oldNode.AppendChild(oldNode.OwnerDocument.ImportNode(newNode.ChildNodes[i], true));
#else
            var oldDocument = oldNode.OwnerDocument;

            for (int index = 0; index < newNode.ChildNodes.Count; index++)
            {
                if (index >= oldNode.ChildNodes.Count)
                {
                    var importNode = oldDocument.ImportNode(newNode.ChildNodes[index], true);
                    oldNode.AppendChild(importNode);
                    continue;
                }

                // TODO/TBD: could we change nodes to make them equal?

                if (IsEqual(oldNode.ChildNodes[index], newNode.ChildNodes[index]))
                {
                    UpdateChildrenFromOtherDocument(newNode.ChildNodes[index], oldNode.ChildNodes[index]);
                    continue;
                }

                int oldIndex = IndexOf(oldNode.ChildNodes, newNode.ChildNodes[index], index + 1);
                int newIndex = IndexOf(newNode.ChildNodes, oldNode.ChildNodes[index], index + 1);
                if (newIndex >= 0 && (oldIndex < 0 || oldIndex > newIndex))
                {
                    // insert from newIndex
                    var importNode = oldDocument.ImportNode(newNode.ChildNodes[newIndex], true);
                    oldNode.InsertBefore(importNode, oldNode.ChildNodes[index]);
                }
                else
                {
                    // delete
                    oldNode.RemoveChild(oldNode.ChildNodes[index]);
                }
            }

            while (oldNode.ChildNodes.Count > newNode.ChildNodes.Count)
            {
                oldNode.RemoveChild(oldNode.ChildNodes[oldNode.ChildNodes.Count - 1]);
            }
#endif
        }

        private static int IndexOf(XmlNodeList list, XmlNode item, int startIndex = 0)
        {
            for (int i = startIndex; i < list.Count; i++)
            {
                if (IsEqual(list[i], item))
                    return i;
            }

            return -1;
        }

        private static bool IsEqual(XmlNode xmlNode1, XmlNode xmlNode2)
        {
            if (xmlNode1 == null || xmlNode2 == null)
                return xmlNode1 == null && xmlNode2 == null;

            if (xmlNode1.NodeType != xmlNode2.NodeType)
                return false;

            if (xmlNode1.NodeType == XmlNodeType.Element)
            {
                if (xmlNode1.Attributes.Count != xmlNode2.Attributes.Count)
                    return false;

                for (int i = 0; i < xmlNode1.Attributes.Count; i++)
                {
                    if (!IsEqual(xmlNode1.Attributes[i], xmlNode2.Attributes[i]))
                        return false;
                }
            }

            return
                string.Compare(xmlNode1.Name, xmlNode2.Name, StringComparison.InvariantCulture) == 0 &&
                string.Compare(xmlNode1.LocalName, xmlNode2.LocalName, StringComparison.InvariantCulture) == 0 &&
                string.Compare(xmlNode1.Prefix, xmlNode2.Prefix, StringComparison.InvariantCulture) == 0 &&
                string.Compare(xmlNode1.Value, xmlNode2.Value, StringComparison.InvariantCulture) == 0;
        }
    }
}
