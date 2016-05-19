using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace ThwargControls
{
    class MaskedTextBox : TextBox
    {
        private static string[] ReservedChars = { "|", "\\", "?", "*", "<", "\"", ":", ">" };

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }
        protected override void OnPreviewTextInput(System.Windows.Input.TextCompositionEventArgs e)
        {
            base.OnPreviewTextInput(e);
            foreach (var str in ReservedChars)
            {
                if (e.Text == str)
                {
                    e.Handled = true;
                    return;
                }
            }
        }
    }
}
