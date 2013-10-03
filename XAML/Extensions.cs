using System;
using System.Collections.Generic;
using System.Windows.Markup;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

namespace StringTableEditorView
{
    public static class Extensions
    {
        // http://stackoverflow.com/questions/653918/wpf-analogy-for-em-unit/6355722#6355722
        // Thanks to http://stackoverflow.com/users/685535/ashish for the code below, minor
        // adjustments to split out reusable code.

        public static T TryFindParent<T>(this DependencyObject child) where T : DependencyObject
        {
            //get parent item
            DependencyObject parentObject = child.GetParentObject();

            //we've reached the end of the tree
            if (parentObject == null) return null;

            //check if the parent matches the type we're looking for
            T parent = parentObject as T;
            if (parent != null)
            {
                return parent;
            }
            else
            {
                //use recursion to proceed with next level
                return parentObject.TryFindParent<T>();
            }
        }

        public static DependencyObject GetParentObject(this DependencyObject child)
        {
            if (child == null) return null;

            //handle content elements separately
            ContentElement contentElement = child as ContentElement;
            if (contentElement != null)
            {
                DependencyObject parent = ContentOperations.GetParent(contentElement);
                if (parent != null) return parent;

                FrameworkContentElement fce = contentElement as FrameworkContentElement;
                return fce != null ? fce.Parent : null;
            }

            //also try searching for parent in framework elements (such as DockPanel, etc)
            FrameworkElement frameworkElement = child as FrameworkElement;
            if (frameworkElement != null)
            {
                DependencyObject parent = frameworkElement.Parent;
                if (parent != null) return parent;
            }

            //if it's not a ContentElement/FrameworkElement, rely on VisualTreeHelper
            return VisualTreeHelper.GetParent(child);
        }
    }
}
