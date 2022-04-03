using System;
using System.Collections;
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
        public static T Read<T>(XmlElement xmlElement) => (T)DeseralizeObject(xmlElement, typeof(T));

        public static T Read<T>(XmlElement xmlElement, T defaultValue) => (T)(DeseralizeObject(xmlElement, typeof(T)) ?? defaultValue);

        public static void Write<T>(XmlElement xmlElement, T data) => SerializeObject(xmlElement, data, typeof(T));

        private static string GetXmlTypeId(Type type) => Type.GetType(type.FullName) == type ? type.FullName : type.AssemblyQualifiedName;

        private static object DeseralizeObject(XmlElement xmlElement, Type type)
        {
            var dataTypeName = xmlElement.GetAttribute("Type");
            var dataType = string.IsNullOrEmpty(dataTypeName) ? type : Type.GetType(dataTypeName);
            XmlSerializer serializer = new XmlSerializer(dataType, new XmlRootAttribute { ElementName = xmlElement.LocalName, Namespace = "" });
            using (XmlReader reader = new XmlNodeReader(xmlElement))
            {
                return serializer.Deserialize(reader);
            }
        }

        private static void SerializeObject(XmlElement xmlElement, object data, Type type)
        {
            var dataType = data.GetType();
            var document = new XmlDocument();
            var navigator = document.CreateNavigator();

            using (XmlWriter writer = navigator.AppendChild())
            {
                var serializer = new XmlSerializer(dataType, new XmlRootAttribute { ElementName = xmlElement.LocalName, Namespace = "" });
                var namespaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
                serializer.Serialize(writer, data);
            }

            UpdateChildrenFromOtherDocument(xmlElement, document.FirstChild);

            // TBD: should we always store type information or only in case of polymorphism?
            if (dataType != type)
            {
                xmlElement.SetAttribute("Type", GetXmlTypeId(dataType));
            }
            else
            {
                xmlElement.RemoveAttribute("Type");
            }
        }

        // Make oldNode have children equal to newNode, but modify only what has changed to avoid flickering,
        // assuming order of nodes stay the same and that nodes can only be deleted and inserted
        // (moving a node is both: delete and insert).
        // Document of newNode differs from the document of oldNode, so we need to copy them by XmlDocument.ImportNode().
        // When a node is different and needs be copied, all off its children are copied too.
        private static void UpdateChildrenFromOtherDocument(XmlNode oldNode, XmlNode newNode)
        {
#if false // quick and simple
            Debug.Assert(oldNode.NodeType == XmlNodeType.Element);
            oldNode.Attributes.RemoveAll();
            for (int i = 0; i < newNode.Attributes.Count; i++)
                oldNode.Attributes.Append((XmlAttribute)oldNode.OwnerDocument.ImportNode(newNode.Attributes[i], true));

            oldNode.RemoveAll();
            for (int i = 0; i < newNode.ChildNodes.Count; i++)
                oldNode.AppendChild(oldNode.OwnerDocument.ImportNode(newNode.ChildNodes[i], true));
#else
            if (oldNode.NodeType == XmlNodeType.Element)
                UpdateChildrenFromOtherDocument(new XmlNodeChildren(oldNode, true), new XmlNodeChildren(newNode, true));
            UpdateChildrenFromOtherDocument(new XmlNodeChildren(oldNode), new XmlNodeChildren(newNode));
#endif
        }

        private static void UpdateChildrenFromOtherDocument(XmlNodeChildren oldChildren, XmlNodeChildren newChildren)
        {
            // Finding the minimal sequence of insert and delete to make oldNode have the same children as newNode
            // is expensive, but in the most relevant cases only one child was inserted or removed or changed.
            // So we implement a simpler strategy which has "only" worst case complexity O(n^2) and typical case complexity O(n).
            // If only the value of a node changes, it can be fixed immediately.
            if (oldChildren.Count == 0 && newChildren.Count == 0)
                return;

            var oldDocument = oldChildren.ParentNode.OwnerDocument;

            for (int index = 0; index < newChildren.Count; index++)
            {
                if (index >= oldChildren.Count)
                {
                    var importNode = oldDocument.ImportNode(newChildren[index], true);
                    oldChildren.AppendChild(importNode);
                    continue;
                }

                var equal = Compare(oldChildren[index], newChildren[index]);
                if (equal == Similarity.CanBeMadeEqual)
                {
                    equal = MakeEqual(oldChildren[index], newChildren[index]);
                }

                if (equal == Similarity.Equal)
                {
                    UpdateChildrenFromOtherDocument(oldChildren[index], newChildren[index]);
                    continue;
                }

                int oldIndex = oldChildren.IndexOf(newChildren[index], index + 1);
                int newIndex = newChildren.IndexOf(oldChildren[index], index + 1);
                if (newIndex >= 0 && (oldIndex < 0 || oldIndex > newIndex))
                {
                    // insert from newIndex
                    var importNode = oldDocument.ImportNode(newChildren[newIndex], true);
                    oldChildren.InsertBefore(importNode, oldChildren[index]);
                }
                else
                {
                    // delete
                    oldChildren.RemoveChild(oldChildren[index]);
                }
            }

            while (oldChildren.Count > newChildren.Count)
            {
                oldChildren.RemoveChild(oldChildren[oldChildren.Count - 1]);
            }
        }

        // Wrapper around XmlNode that allows to treat Attributes as children instead of ChildNodes.
        [DebuggerStepThrough]
        private class XmlNodeChildren : IEnumerable
        {
            private readonly XmlNode _xmlNode;
            private readonly bool _attributes;

            public XmlNodeChildren(XmlNode xmlNode, bool childrenAreAttributes = false)
            {
                _xmlNode = xmlNode;
                _attributes = childrenAreAttributes;
            }

            public XmlNode ParentNode => _xmlNode;
            public int Count => _attributes ? _xmlNode.Attributes.Count : _xmlNode.ChildNodes.Count;
            public XmlNode this[int i] => _attributes ? _xmlNode.Attributes[i] : _xmlNode.ChildNodes[i];
            public IEnumerator GetEnumerator() => _attributes ? _xmlNode.Attributes.GetEnumerator() : _xmlNode.GetEnumerator();

            public XmlNode AppendChild(XmlNode newChild) => _attributes
                ? _xmlNode.Attributes.Append((XmlAttribute)newChild)
                : _xmlNode.AppendChild(newChild);

            public XmlNode InsertBefore(XmlNode newChild, XmlNode refChild) => _attributes
                ? _xmlNode.Attributes.InsertBefore((XmlAttribute)newChild, (XmlAttribute)refChild)
                : _xmlNode.InsertBefore(newChild, refChild);

            public XmlNode RemoveChild(XmlNode oldChild) => _attributes
                ? _xmlNode.Attributes.Remove((XmlAttribute)oldChild)
                : _xmlNode.RemoveChild(oldChild);

            public int IndexOf(XmlNode item, int startIndex = 0)
            {
                for (int i = startIndex; i < Count; i++)
                {
                    if (Compare(this[i], item) == Similarity.Equal)
                        return i;
                }

                return -1;
            }
        }

        private enum Similarity { Different, CanBeMadeEqual, Equal }
        private static Similarity Compare(XmlNode oldNode, XmlNode newNode)
        {
            if (oldNode.NodeType != newNode.NodeType)
                return Similarity.Different;

            if (oldNode.NodeType == XmlNodeType.Element)
            {
                if (oldNode.Attributes.Count != newNode.Attributes.Count)
                    return oldNode.IsReadOnly ? Similarity.Different : Similarity.CanBeMadeEqual;

                for (int i = 0; i < oldNode.Attributes.Count; i++)
                {
                    if (Compare(oldNode.Attributes[i], newNode.Attributes[i]) != Similarity.Equal)
                        return oldNode.IsReadOnly ? Similarity.Different : Similarity.CanBeMadeEqual;
                }
            }

            if (string.Compare(oldNode.Name, newNode.Name, StringComparison.InvariantCulture) != 0 ||
                string.Compare(oldNode.LocalName, newNode.LocalName, StringComparison.InvariantCulture) != 0)
                return Similarity.Different;

            return
                string.Compare(oldNode.Prefix, newNode.Prefix, StringComparison.InvariantCulture) == 0 &&
                string.Compare(oldNode.Value, newNode.Value, StringComparison.InvariantCulture) == 0
                ? Similarity.Equal : (oldNode.IsReadOnly ? Similarity.Different : Similarity.CanBeMadeEqual);
        }

        private static Similarity MakeEqual(XmlNode oldNode, XmlNode newNode)
        {
            if (oldNode.NodeType == XmlNodeType.Element)
                UpdateChildrenFromOtherDocument(new XmlNodeChildren(oldNode, true), new XmlNodeChildren(newNode, true));

            if (string.Compare(oldNode.Prefix, newNode.Prefix, StringComparison.InvariantCulture) != 0)
                oldNode.Prefix = newNode.Prefix;

            if (string.Compare(oldNode.Value, newNode.Value, StringComparison.InvariantCulture) != 0)
                oldNode.Value = newNode.Value;

            return Compare(oldNode, newNode);
        }
    }
}
