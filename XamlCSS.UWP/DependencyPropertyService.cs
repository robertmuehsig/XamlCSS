﻿using System;
using System.Linq;
using System.Reflection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using XamlCSS.ComponentModel;
using XamlCSS.Dom;
using XamlCSS.Utils;

namespace XamlCSS.UWP
{
    public class DependencyPropertyService : IDependencyPropertyService<DependencyObject, Style, DependencyProperty>
    {
        private UWPTypeConverterProvider typeConverter = new UWPTypeConverterProvider();

        public DependencyProperty GetDependencyProperty(DependencyObject frameworkElement, string propertyName)
        {
            return GetDependencyProperty(frameworkElement.GetType(), propertyName);
        }
        public DependencyProperty GetDependencyProperty(Type dependencyObjectType, string propertyName)
        {
            DependencyProperty result;

            result = TypeHelpers.GetDependencyPropertyInfo<DependencyProperty>(dependencyObjectType, propertyName)?.Property;

            return result;
        }

        public object GetDependencyPropertyValue(Type frameworkElementType, string propertyName, DependencyProperty property, object propertyValue)
        {
            Type propertyType = null;

            var prop = TypeHelpers.DeclaredProperty(frameworkElementType,propertyName);

            if (prop == null)
            {
                var metadata = property.GetMetadata(frameworkElementType);
                if (metadata.DefaultValue != null)
                {
                    propertyType = metadata.DefaultValue.GetType();
                }
                else
                {
                    return propertyValue;
                }
            }
            else
            {
                propertyType = prop.PropertyType;
            }

            if (!propertyType.GetTypeInfo().IsAssignableFrom(propertyValue?.GetType().GetTypeInfo()))
            {
                var converter = typeConverter.GetConverter(propertyType);

                if (converter != null)
                {
                    if ((propertyType == typeof(float) ||
                        propertyType == typeof(double)) &&
                        (propertyValue as string)?.StartsWith(".", StringComparison.Ordinal) == true)
                    {
                        var stringValue = propertyValue as string;
                        propertyValue = "0" + (stringValue.Length > 1 ? stringValue : "");
                    }

                    propertyValue = converter.ConvertFromInvariantString(propertyValue as string);
                }
                else if (propertyType == typeof(bool))
                {
                    propertyValue = propertyValue.Equals("true");
                }
                else if (propertyType.GetTypeInfo().IsEnum)
                {
                    propertyValue = Enum.Parse(propertyType, propertyValue as string);
                }
            }

            return propertyValue;
        }

        public string GetClass(DependencyObject obj)
        {
            return Css.GetClass(obj) as string;
        }

        public Style GetInitialStyle(DependencyObject obj)
        {
            return Css.GetInitialStyle(obj) as Style;
        }

        public string GetName(DependencyObject obj)
        {
            return (obj as FrameworkElement)?.Name;
        }

        public StyleDeclarationBlock GetStyle(DependencyObject obj)
        {
            return Css.GetStyle(obj) as StyleDeclarationBlock;
        }

        public StyleSheet GetStyleSheet(DependencyObject obj)
        {
            return Css.GetStyleSheet(obj) as StyleSheet;
        }

        public void SetClass(DependencyObject obj, string value)
        {
            Css.SetClass(obj, value);
        }
        
        public void SetInitialStyle(DependencyObject obj, Style value)
        {
            Css.SetInitialStyle(obj, value);
        }
        
        public void SetName(DependencyObject obj, string value)
        {
            (obj as FrameworkElement).Name = value;
        }

        public void SetStyle(DependencyObject obj, StyleDeclarationBlock value)
        {
            Css.SetStyle(obj, value);
        }

        public void SetStyleSheet(DependencyObject obj, StyleSheet value)
        {
            Css.SetStyleSheet(obj, value);
        }

        public void RegisterLoadedOnce(DependencyObject obj, Action<object> func)
        {
            var frameworkElement = obj as FrameworkElement;

            RoutedEventHandler handler = null;
            handler = (s, e) =>
            {
                frameworkElement.Loaded -= handler;
                func(s);
            };

            frameworkElement.Loaded += handler;
        }
        
        public IDomElement<DependencyObject> GetDomElement(DependencyObject obj, SelectorType selectorType)
        {
            if (selectorType == SelectorType.LogicalTree)
            {
                return Css.GetDomElement(obj);
            }
            else
            {
                return Css.GetVisualDomElement(obj);
            }
        }

        public void SetDomElement(DependencyObject obj, IDomElement<DependencyObject> value, SelectorType selectorType)
        {
            if (selectorType == SelectorType.LogicalTree)
            {
                Css.SetDomElement(obj, value);
            }
            else
            {
                Css.SetVisualDomElement(obj, value);
            }
        }

        public bool IsLoaded(DependencyObject obj)
        {
            var frameworkElement = obj as FrameworkElement;

            return frameworkElement.Parent != null ||
                frameworkElement is Frame;
        }

        public object GetValue(DependencyObject obj, string propertyName)
        {
            if (obj == null)
            {
                return null;
            }

            var dp = TypeHelpers.GetDependencyPropertyInfo<DependencyProperty>(obj.GetType(), propertyName);
            return obj.GetValue(dp.Property);
        }

        public void SetValue(DependencyObject obj, string propertyName, object value)
        {
            if (obj == null)
            {
                return;
            }

            var dp = TypeHelpers.GetDependencyPropertyInfo<DependencyProperty>(obj.GetType(), propertyName);
            obj.SetValue(dp.Property, value);
        }
    }
}
