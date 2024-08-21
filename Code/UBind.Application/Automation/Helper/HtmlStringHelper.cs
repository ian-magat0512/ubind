// <copyright file="HtmlStringHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Helper
{
    using HtmlAgilityPack;

    public class HtmlStringHelper
    {
        private static readonly string[] CharacterFormattingInlineTags = {
            "b", "bold", "strong", "sup", "sub", "em", "i", "u", "s", "strike", "del",
        };

        private static readonly string[] SupportedHtmlTags = {
            "html", "head", "title", "meta", "link", "style", "script", "body", "div", "br", "hr", "p", "h1", "h2", "h3",
            "h4", "h5", "h6", "a", "img", "ul", "ol", "li", "table", "thead", "tbody", "tr", "td", "th", "td", "form",
            "input", "textarea", "select", "button", "span",
        };

        // This method gets all the parent tags (attributes included) of the first occurrence of the text within the html string
        // Example: if htmlString = "<body><div>(shouldn't be included)</div><div class='test'>(Included)
        // <div>(shouldn't be included)</div><p style='color: red;'>(Included)<b>Test</b> Text</p> Test</div></body>",
        // and the text = "Test", Then
        // it should return: [ "<body>", "<div class='test'>", "<p style='color: red;'>", "<b>" ]
        public static List<string> GetParentTagsOfText(string htmlString, string text)
        {
            List<string> tags = new List<string>();

            if (!IsValidHtml(htmlString))
            {
                return tags;
            }

            var doc = new HtmlDocument();
            doc.LoadHtml(htmlString);

            var directParent = GetDirectParent(doc.DocumentNode, text);
            if (directParent != null)
            {
                foreach (var parent in directParent.AncestorsAndSelf().Where(a => a.NodeType == HtmlNodeType.Element))
                {
                    var tag = parent.OuterHtml.Substring(0, parent.OuterHtml.IndexOf('>') + 1);
                    tags.Add(tag);
                }
            }

            tags.Reverse();
            return tags;
        }

        public static List<string> FindAllTags(string htmlString, string tagName = null)
        {
            List<string> tags = new List<string>();

            if (!IsValidHtml(htmlString))
            {
                return tags;
            }

            var doc = new HtmlDocument();
            doc.LoadHtml(htmlString);

            var childNodes = doc.DocumentNode.Descendants().Where(a => a.NodeType == HtmlNodeType.Element);
            if (!string.IsNullOrEmpty(tagName))
            {
                childNodes = childNodes.Where(a => a.OriginalName == tagName);
            }

            foreach (var element in childNodes)
            {
                var tag = element.OuterHtml.Substring(0, element.OuterHtml.IndexOf('>') + 1);
                tags.Add(tag);
            }

            return tags;
        }

        public static List<string> ExtractStyleAttributes(string htmlTag)
        {
            List<string> styleAttributes = new List<string>();

            var tag = ExtractOpeningTag(htmlTag);
            if (tag == null)
            {
                return styleAttributes;
            }

            var styleAttributeRawValue = tag.GetAttributeValue("style", string.Empty);
            var styles = styleAttributeRawValue.Split(";");
            if (styles != null)
            {
                var styleNames = styles.Where(a => a.Contains(':')).Select(a => a.Split(":")[0].Trim());
                styleAttributes.AddRange(styleNames);
            }

            return styleAttributes;
        }

        public static string GetAttribute(string htmlTag, string attributeName)
        {
            var tag = ExtractOpeningTag(htmlTag);
            if (tag == null)
            {
                return null;
            }

            var attr = tag.GetAttributeValue(attributeName, string.Empty);
            if (string.IsNullOrEmpty(attr))
            {
                return null;
            }

            return attr;
        }

        public static bool IsCharacterFormattingTag(string tag)
        {
            return CharacterFormattingInlineTags.Contains(ExtractOpeningTag(tag)?.Name);
        }

        public static string RemoveTagAttributes(string tagString)
        {
            var tag = ExtractOpeningTag(tagString);
            if (tag == null)
            {
                return string.Empty;
            }

            return $"<{tag.OriginalName}>";
        }

        public static bool IsValidHtml(string htmlString)
        {
            try
            {
                HtmlDocument htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(htmlString);
                return !htmlDocument.DocumentNode
                    .DescendantsAndSelf()
                    .Any(a => a.NodeType == HtmlNodeType.Element
                        && !SupportedHtmlTags.Contains(a.Name)
                        && !CharacterFormattingInlineTags.Contains(a.Name));
            }
            catch
            {
                return false;
            }
        }

        private static HtmlNode? ExtractOpeningTag(string htmlTag)
        {
            if (!IsValidHtml(htmlTag))
            {
                return null;
            }

            var dom = new HtmlDocument();
            dom.LoadHtml(htmlTag);

            return dom.DocumentNode.ChildNodes.Where(a => a.NodeType == HtmlNodeType.Element).FirstOrDefault();
        }

        private static HtmlNode? GetDirectParent(HtmlNode node, string text)
        {
            if (node.NodeType == HtmlNodeType.Text && string.Equals(node.InnerText.Trim(), text.Trim()))
            {
                return node.ParentNode;
            }
            else if (node.HasChildNodes)
            {
                foreach (HtmlNode n in node.ChildNodes)
                {
                    var p = GetDirectParent(n, text);
                    if (p != null)
                    {
                        return p;
                    }
                }
            }

            return null;
        }
    }
}
