﻿using XamlCSS.CssParsing;
using XamlCSS.Dom;

namespace XamlCSS
{
    public class TypeMatcher : SelectorMatcher
    {
        private StyleSheet initializedWith;

        public TypeMatcher(CssNodeType type, string text) : base(type, text)
        {
            
        }

        private void Initialize(StyleSheet styleSheet)
        {
            var namespaceSeparatorIndex = Text.IndexOf('|');
            string @namespace = null;
            //string prefix = null;
            string alias = "";
            string localName = null;
            if (namespaceSeparatorIndex > -1)
            {
                alias = Text.Substring(0, namespaceSeparatorIndex);
                localName = Text.Substring(namespaceSeparatorIndex + 1);
                if (alias != "*")
                {
                    @namespace = styleSheet.GetNamespaceUri(alias, localName);
                }
                else
                {

                }
            }
            else
            {
                localName = Text;
                @namespace = styleSheet.GetNamespaceUri("", localName);
            }

            this.Alias = alias;
            this.LocalName = localName;
            this.NamespaceUri = @namespace;
            this.initializedWith = styleSheet;
        }

        public override MatchResult Match<TDependencyObject>(StyleSheet styleSheet, ref IDomElement<TDependencyObject> domElement, SelectorMatcher[] fragments, ref int currentIndex)
        {
            if (initializedWith != styleSheet)
            {
                Initialize(styleSheet);
            }

            var isMatch = domElement.LocalName == LocalName && (Alias == "*" || domElement.AssemblyQualifiedNamespaceName == NamespaceUri);
            return isMatch ? MatchResult.Success : MatchResult.ItemFailed;
        }

        public string Alias { get; private set; }
        public string LocalName { get; private set; }
        public string NamespaceUri { get; private set; }
    }
}