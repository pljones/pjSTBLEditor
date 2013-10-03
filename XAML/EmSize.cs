using System;
using System.Windows.Markup;
using System.Windows;
using System.Windows.Controls;

namespace StringTableEditorView
{
    // http://stackoverflow.com/questions/653918/wpf-analogy-for-em-unit/6355722#6355722
    // Thanks to http://stackoverflow.com/users/685535/ashish for the code below, minor
    // adjustments to rename to "EmSize" (as it's not really the FontSize I'm talking about),
    // to search for any Control and to use a default of 11 not 12 and split out reusable code.

    /// <summary>
    /// 
    /// </summary>
    /// <example><code><![CDATA[
    /// xmlns:my="clr-namespace:StringTableEditorView"
    /// <TextBlock Text="Sample Font" Width="{my:EmSize 10}" Height="{my:EmSize 10}"/>
    /// <ComboBox Width="{my:EmSize 20}" />
    /// ]]></code>
    /// Note that in the <c>ComboBox</c>, the "20" ems includes space for the button.</example>
    [MarkupExtensionReturnType(typeof(double))]
    public class EmSize : MarkupExtension
    {
        public EmSize() { }

        public EmSize(double size) { Size = size; }

        [ConstructorArgument("size")]
        public double Size { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                return null;

            // get the target of the extension from the IServiceProvider interface
            IProvideValueTarget ipvt = (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget));
            if (ipvt.TargetObject.GetType().FullName == "System.Windows.SharedDp")
                return this;

            DependencyObject targetObject = ipvt.TargetObject as DependencyObject;

            var ctrl = targetObject.TryFindParent<Control>();
            return (ctrl != null ? ctrl.FontSize : 11) * Size;
        }
    }
}
