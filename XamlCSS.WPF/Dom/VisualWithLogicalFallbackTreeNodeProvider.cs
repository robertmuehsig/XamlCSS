﻿using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using XamlCSS.Dom;

namespace XamlCSS.WPF.Dom
{
    public class VisualWithLogicalFallbackTreeNodeProvider : TreeNodeProviderBase<DependencyObject, Style, DependencyProperty>
    {
        public ITreeNodeProvider<DependencyObject> VisualTreeNodeProvider { get; }
        public ITreeNodeProvider<DependencyObject> LogicalTreeNodeProvider { get; }

        public VisualWithLogicalFallbackTreeNodeProvider(IDependencyPropertyService<DependencyObject, Style, DependencyProperty> dependencyPropertyService,
            ITreeNodeProvider<DependencyObject> visualTreeNodeProvider,
            ITreeNodeProvider<DependencyObject> logicalTreeNodeProvider
            )
            : base(dependencyPropertyService, SelectorType.VisualTree)
        {
            this.VisualTreeNodeProvider = visualTreeNodeProvider;
            this.LogicalTreeNodeProvider = logicalTreeNodeProvider;
        }

        public override IDomElement<DependencyObject> CreateTreeNode(DependencyObject dependencyObject)
        {
            return new VisualDomElement(dependencyObject, GetDomElement(GetParent(dependencyObject)), this, namespaceProvider);
        }

        public override bool IsCorrectTreeNode(IDomElement<DependencyObject> node)
        {
            return node is VisualDomElement;
        }

        public override IEnumerable<DependencyObject> GetChildren(DependencyObject element)
        {
            var list = new List<DependencyObject>();

            if (element == null)
            {
                return list;
            }

            try
            {
                var children = new List<DependencyObject>();

                if (element is Visual ||
                    element is Visual3D)
                {
                    children = VisualTreeNodeProvider.GetChildren(element).ToList();
                    for (int i = 0; i < children.Count; i++)
                    {
                        var child = children[i];

                        if (child != null)
                        {
                            list.Add(child);
                        }
                    }
                }

                if (element is IContentHost)
                {
                    children = LogicalTreeNodeProvider.GetChildren(element).ToList();
                    for (int i = 0; i < children.Count; i++)
                    {
                        var child = children[i];

                        if (child != null)
                        {
                            list.Add(child);
                        }
                    }
                }
                /*else
                {
                    children = LogicalTreeNodeProvider.GetChildren(element).Where(x => !(element is Visual || element is Visual3D)).ToList();
                    for (int i = 0; i < children.Count; i++)
                    {
                        var child = children[i];

                        if (child != null)
                        {
                            list.Add(child);
                        }
                    }
                }*/

            }
            catch { }
            return list;
        }

        public override DependencyObject GetParent(DependencyObject element)
        {
            if (element == null)
            {
                return null;
            }

            if (element is Visual ||
                element is Visual3D)
            {

                return VisualTreeNodeProvider.GetParent(element);
            }
            else if (element is FrameworkContentElement)
            {
                return LogicalTreeHelper.GetParent(element);
            }

            return null;
        }

        public override bool IsInTree(DependencyObject element)
        {
            return VisualTreeNodeProvider.IsInTree(element);
        }
    }
}
