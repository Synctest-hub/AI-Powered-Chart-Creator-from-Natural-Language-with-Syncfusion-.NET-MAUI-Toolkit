using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syncfusion.Maui.Core.Internals;
using Syncfusion.Maui.Toolkit.Charts;

#if ANDROID
using PlatformView = Android.Widget.EditText;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.TextBox;
#endif
using PointerEventArgs = Syncfusion.Maui.Core.Internals.PointerEventArgs;
namespace ChartGenerator
{
    public class CustomEditor : Editor, IKeyboardListener
    {
        public CustomEditor()
        {
            this.AddKeyboardListener(this);
        }
#if WINDOWS || ANDROID
        protected override void OnHandlerChanged()
        {
#if WINDOWS
            // Hide editor border and underline.
            var platformView = this.Handler?.PlatformView as PlatformView;
            if (platformView != null)
            {
                this.ApplyTextBoxStyle(platformView);
            }
#else
            var platformView = this.Handler?.PlatformView as PlatformView;
            if (platformView != null)
            {
                this.ApplyTextBoxStyle(platformView);
            }
#endif
            base.OnHandlerChanged();
        }
#endif
#if WINDOWS || ANDROID
        private void ApplyTextBoxStyle(PlatformView? platformView)
        {
            if (platformView != null)
            {
#if WINDOWS
                var textBoxStyle = new Microsoft.UI.Xaml.Style(typeof(Microsoft.UI.Xaml.Controls.TextBox));
                textBoxStyle.Setters.Add(new Microsoft.UI.Xaml.Setter() { Property = Microsoft.UI.Xaml.Controls.Control.BorderBrushProperty, Value = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0)) });
                textBoxStyle.Setters.Add(new Microsoft.UI.Xaml.Setter() { Property = Microsoft.UI.Xaml.Controls.Control.BorderThicknessProperty, Value = new Thickness(0) });

                platformView.Resources.Add(typeof(Microsoft.UI.Xaml.Controls.TextBox), textBoxStyle);
#else
                platformView.Background = null;
                platformView.SetPadding(0, 0, 0, 0);
#endif
            }
        }
#endif
        public void OnKeyDown(KeyEventArgs args)
        {
        }

        public void OnKeyUp(KeyEventArgs args)
        {
        }

        public void OnPreviewKeyDown(KeyEventArgs args)
        {
            if (args.Key == KeyboardKey.Enter && !args.IsShiftKeyPressed)
            {
                var bindingContext = this.BindingContext as ChartViewModel;
                args.Handled = true;
                if (!string.IsNullOrWhiteSpace(bindingContext.InputText))
                {
                    bindingContext.SendButtonCommand.Execute(null);
                }
            }
        }
    }
}
