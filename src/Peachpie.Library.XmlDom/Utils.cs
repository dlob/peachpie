﻿#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml;
using System.Linq;
using System.Xml.Resolvers;
using HtmlAgilityPack;
using Pchp.Core;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace Peachpie.Library.XmlDom
{
    internal static class Utils
    {
        internal static void ParseQualifiedName(string qualifiedName, out string prefix, out string localName)
        {
            if (qualifiedName == null)
            {
                prefix = null;
                localName = null;
            }
            else
            {
                int index = qualifiedName.IndexOf(':');
                if (index >= 0)
                {
                    prefix = qualifiedName.Substring(0, index);
                    localName = qualifiedName.Substring(index + 1);
                }
                else
                {
                    prefix = String.Empty;
                    localName = qualifiedName;
                }
            }
        }

        internal static Encoding/*!*/ GetNodeEncoding(Context ctx, XmlNode xmlNode)
        {
            var xml_document = xmlNode as XmlDocument ?? xmlNode.OwnerDocument;

            Encoding encoding;

            if (xml_document != null && xml_document.FirstChild is XmlDeclaration decl && !string.IsNullOrEmpty(decl.Encoding))
            {
                encoding = Encoding.GetEncoding(decl.Encoding);
            }
            else
            {
                encoding = ctx.StringEncoding;
            }

            return (encoding is UTF8Encoding)
                ? new UTF8Encoding(false)   // no BOM for UTF-8 please!
                : encoding;
        }

        public static XmlDocument/*!*/GetXmlDocument(this XmlNode node) => node as XmlDocument ?? node.OwnerDocument ?? throw new InvalidOperationException();

        //#region XmlHtmlResolver

        //sealed internal class HtmlXmlDtdResolver : XmlPreloadedResolver
        //{
        //    public static readonly Lazy<XmlResolver> Instance = new Lazy<XmlResolver>(() => new HtmlXmlDtdResolver());

        //    /// <summary>
        //    /// Represents DTD information stored in an embedded resource file.
        //    /// </summary>
        //    internal struct XmlKnownDtdData
        //    {
        //        public string PublicId { get; }

        //        public string SystemId { get; }

        //        string ResourceName { get; }

        //        internal XmlKnownDtdData(string publicId, string systemId, string resourceName)
        //        {
        //            Debug.Assert(resourceName != null);

        //            PublicId = publicId;
        //            SystemId = systemId;
        //            ResourceName = resourceName ?? throw new ArgumentNullException(nameof(resourceName));
        //        }

        //        public Stream AsStream()
        //        {
        //            var executingAssembly = Assembly.GetExecutingAssembly();
        //            return executingAssembly.GetManifestResourceStream(ResourceName);
        //        }

        //        public TextReader AsTextReader() => new StreamReader(AsStream());
        //    }

        //    /// <summary>
        //    /// Default HTML DTD.
        //    /// </summary>
        //    internal static XmlKnownDtdData DefaultDtd => Html4Loose;

        //    internal static XmlKnownDtdData Html4Loose => new XmlKnownDtdData("-//W3C//DTD HTML 4.01 Transitional//EN", "http://www.w3.org/TR/html4/loose.dtd", "html4-loose.dtd");

        //    internal static XmlKnownDtdData Html4Strict => new XmlKnownDtdData("-//W3C//DTD HTML 4.01//EN", "http://www.w3.org/TR/html4/strict.dtd", "html4-strict.dtd");

        //    internal static XmlKnownDtdData Html4Frameset => new XmlKnownDtdData("-//W3C//DTD HTML 4.01 Frameset//EN", "http://www.w3.org/TR/html4/frameset.dtd", "html4-frameset.dtd");

        //    readonly Dictionary<Uri, XmlKnownDtdData> _knownHtmlDtdMapping = new Dictionary<Uri, XmlKnownDtdData>(16);

        //    /// <summary>
        //    /// HTML DTDs.
        //    /// </summary>
        //    readonly static XmlKnownDtdData[] s_knownHtmlDtd = new XmlKnownDtdData[]
        //    {
        //        Html4Loose,
        //        Html4Strict,
        //        Html4Frameset,

        //        //new XmlKnownDtdData("-//W3C//ENTITIES Latin1//EN//HTML", "", ""),
        //    };

        //    private HtmlXmlDtdResolver() // XmlKnownDtds.All
        //    {
        //        // init dictionary:
        //        Add(s_knownHtmlDtd);
        //    }

        //    void Add(XmlKnownDtdData[] knownDtds)
        //    {
        //        foreach (var data in knownDtds)
        //        {
        //            Add(data.PublicId, data);
        //            Add(data.SystemId, data);
        //        }
        //    }

        //    void Add(string uri, XmlKnownDtdData data)
        //    {
        //        if (string.IsNullOrEmpty(uri))
        //        {
        //            return;
        //        }

        //        _knownHtmlDtdMapping[new Uri(uri, UriKind.RelativeOrAbsolute)] = data;
        //    }

        //    public override Uri ResolveUri(Uri baseUri, string relativeUri)
        //    {
        //        if (relativeUri != null && relativeUri.StartsWith("-//W3C//", StringComparison.OrdinalIgnoreCase))
        //        {
        //            return new Uri(relativeUri, UriKind.Relative);
        //        }

        //        return base.ResolveUri(baseUri, relativeUri);
        //    }

        //    public override bool SupportsType(Uri absoluteUri, Type type)
        //    {
        //        return type == null || type == typeof(Stream) || base.SupportsType(absoluteUri, type);
        //    }

        //    public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
        //    {
        //        if (_knownHtmlDtdMapping.TryGetValue(absoluteUri, out var data))
        //        {
        //            if (ofObjectToReturn == null || ofObjectToReturn == typeof(Stream) || ofObjectToReturn == typeof(object))
        //            {
        //                return data.AsStream();
        //            }
        //            else if (ofObjectToReturn == typeof(TextReader))
        //            {
        //                return data.AsTextReader();
        //            }
        //            else
        //            {
        //                throw new ArgumentException(nameof(ofObjectToReturn));
        //            }
        //        }

        //        // XML DTDs:
        //        return base.GetEntity(absoluteUri, role, ofObjectToReturn);
        //    }

        //    public override Task<object> GetEntityAsync(Uri absoluteUri, string role, Type ofObjectToReturn)
        //    {
        //        if (_knownHtmlDtdMapping.TryGetValue(absoluteUri, out var data))
        //        {
        //            if (ofObjectToReturn == null || ofObjectToReturn == typeof(Stream) || ofObjectToReturn == typeof(object))
        //            {
        //                return Task.FromResult<object>(data.AsStream());
        //            }
        //            else if (ofObjectToReturn == typeof(TextReader))
        //            {
        //                return Task.FromResult<object>(data.AsTextReader());
        //            }
        //            else
        //            {
        //                throw new ArgumentException(nameof(ofObjectToReturn));
        //            }
        //        }

        //        // XML DTDs:
        //        return base.GetEntityAsync(absoluteUri, role, ofObjectToReturn);
        //    }
        //}

        //#endregion

        /// <summary>
        /// Loads HTML document into XML document.
        /// </summary>
        /// <param name="xmldoc"></param>
        /// <param name="htmldoc"></param>
        internal static void LoadHtml(this XmlDocument xmldoc, HtmlDocument htmldoc)
        {
            xmldoc.RemoveAll();
            //xmldoc.XmlResolver = HtmlXmlDtdResolver.Instance.Value;   // causes DTD validation, needed for DTD's and GetAttributeById() to work
            LoadHtml(xmldoc, htmldoc.DocumentNode);
        }

        /// <summary>
        /// Finds first node of given type.
        /// </summary>
        static bool TryGetNodeOfType(this XmlNodeList nodes, XmlNodeType nt, out XmlNode? node)
        {
            if (nodes != null && nodes.Count != 0)
            {
                foreach (XmlNode child in nodes)
                {
                    if (child.NodeType == nt)
                    {
                        node = child;
                        return true;
                    }
                }
            }

            node = null;
            return false;
        }

        static void LoadHtml(this XmlNode containing, HtmlNode node)
        {
            //var dtd = HtmlXmlDtdResolver.DefaultDtd;
            var xmldoc = containing.GetXmlDocument();
            var phpdoc = xmldoc as PhpXmlDocument;

            switch (node.NodeType)
            {
                case HtmlNodeType.Comment:
                    var commentnode = (HtmlCommentNode)node;
                    if (DocumentTypeUtils.TryParseDocumentType(commentnode.Comment, out var doctypeName, out var publicId, out var systemId, out var subset))
                    {
                        var doctype = xmldoc.CreateDocumentType(doctypeName, publicId, systemId, subset); // NOTE: causes DTD validation
                        containing.AppendChild(doctype);
                    }
                    else
                    {
                        containing.AppendChild(xmldoc.CreateComment(commentnode.Comment));
                    }
                    break;

                case HtmlNodeType.Document:
                    foreach (var child in node.ChildNodes)
                    {
                        LoadHtml(containing, child);
                    }
                    break;

                case HtmlNodeType.Element:

                    var name = node.Name;   // NOTE: Name is lowercased, needed for XPath queries

                    if (name.Length == 0 || name[0] == '?' || name[0] == '!')
                    {
                        // <?xml >

                        // ??

                        break;
                    }

                    name = HtmlDocument.GetXmlName(name, isAttribute: false, preserveXmlNamespaces: true);

                    // <element
                    var element = xmldoc.CreateElement(name);

                    if (containing.ParentNode == null && TryGetNodeOfType(containing.ChildNodes, XmlNodeType.Element, out var existingroot)) // root can only have one child element
                    {
                        // !!!
                        // HACK: nest the element into the existing root
                        existingroot.AppendChild(element);
                    }
                    else
                    {
                        containing.AppendChild(element);
                    }

                    // attributes
                    foreach (var attr in node.Attributes)
                    {
                        var element_attr = xmldoc.CreateAttribute(attr.OriginalName);
                        element_attr.Value = attr.Value;

                        element.Attributes.Append(element_attr);

                        // add the attribute to the attribute ID map:
                        phpdoc?.HandleNewAttribute(element_attr);
                    }

                    // nested children
                    foreach (var child in node.ChildNodes)
                    {
                        LoadHtml(element, child);
                    }

                    // </element>

                    break;

                case HtmlNodeType.Text:
                    var text = ((HtmlTextNode)node).Text;
                    if (containing.ParentNode == null) // root
                    {
                        if (string.IsNullOrWhiteSpace(text)) break;
                        throw new NotSupportedException();  // TODO: we can wrap the Text element into something and append
                    }
                    containing.AppendChild(xmldoc.CreateTextNode(text));
                    break;
            }
        }
    }

    static class DocumentTypeUtils
    {
        /// <summary>
        /// Simple parser getting words subsequently.
        /// </summary>
        static bool ConsumeToken(string text, ref int pos, out ReadOnlySpan<char> token)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            Debug.Assert(pos >= 0);

            // skip whitespaces
            while (pos < text.Length && char.IsWhiteSpace(text[pos]))
            {
                pos++;
            }

            if (pos >= text.Length)
            {
                token = ReadOnlySpan<char>.Empty;
                return false;
            }

            // word:
            int start = pos;
            if (text[pos] == '\"')
            {
                pos++;

                // word enclosed in quotes:
                while (pos < text.Length)
                {
                    var ch = text[pos++];
                    if (ch == '\"')
                    {
                        break;
                    }
                }

                token = text.AsSpan(start + 1, pos - start - 2);
            }
            else
            {
                for (; pos < text.Length; pos++)
                {
                    var ch = text[pos];
                    if (char.IsWhiteSpace(ch) || ch == '>' || ch == '\"')
                    {
                        break;
                    }
                }

                token = text.AsSpan(start, pos - start);
            }

            //
            return token.Length != 0; // token found and valid (not '>')
        }

        /// <summary>
        /// Parses the DOCTYPE comment if possible.
        /// </summary>
        public static bool TryParseDocumentType(string text, out string? name, out string? publicId, out string? systemId, out string? subset)
        {
            name = publicId = systemId = subset = null;

            const string doctypeprefix = "<!doctype";
            if (text != null && text.StartsWith(doctypeprefix, StringComparison.OrdinalIgnoreCase))
            {
                name = "html"; // default

                int pos = doctypeprefix.Length;
                int index = 0; // token index

                // <!DOCTYPE {NAME} PUBLIC|SYSTEM {ID1} {ID2}>
                string pub = string.Empty;

                while (ConsumeToken(text, ref pos, out var token))
                {
                    Debug.Assert(token.Length != 0);

                    switch (index++)
                    {
                        case 0:
                            name = token.ToString();
                            break;

                        case 1:
                            pub = token.ToString();
                            break;

                        case 2:
                            if (string.Equals(pub, "PUBLIC", StringComparison.OrdinalIgnoreCase))
                            {
                                publicId = token.ToString();
                            }
                            else if (string.Equals(pub, "SYSTEM", StringComparison.OrdinalIgnoreCase))
                            {
                                systemId = token.ToString();
                            }
                            else
                            {
                                goto default;
                            }
                            break;

                        case 3:
                            if (publicId == null)
                            {
                                goto default;
                            }

                            systemId = token.ToString();
                            break;

                        default:
                            Debug.WriteLine("Unexpected DOCTYPE value: " + text);
                            return true; // done
                    }

                }

                return true;
            }

            //
            return false;
        }
    }
}
