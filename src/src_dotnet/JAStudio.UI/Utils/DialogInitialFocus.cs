using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace JAStudio.UI.Utils;

/// <summary>
/// Puts the keyboard in a small dialog's main input when the dialog opens: the first visible input marked with the <see cref="ClassName"/> class,<br/>
/// at the end of any text already in it, so a dialog can mark one input for each way it shows. The search's condition dialogs and the tag picker dialog use it.
/// </summary>
/// <remarks>
/// The dialog first takes the Windows keyboard focus for its own native window. Focusing the input only moves Avalonia's focus,<br/>
/// and if the window behind had the Windows keyboard focus, the keys would keep going there.
/// </remarks>
static class DialogInitialFocus
{
   /// <summary>The class that marks a dialog's main input, such as <c>&lt;TextBox Classes="initialFocus" /&gt;</c>.</summary>
   public const string ClassName = "initialFocus";

   /// <summary>Call after <paramref name="dialog"/> has opened and its content is laid out.</summary>
   public static void PutTheKeyboardInTheMainInputOf(Window dialog)
   {
      NativeKeyboardFocus.MoveToWindowItself(dialog);
      var input = dialog.GetVisualDescendants().OfType<InputElement>().FirstOrDefault(it => it.Classes.Contains(ClassName) && it.IsEffectivelyVisible)
               ?? throw new InvalidOperationException($"The dialog {dialog.GetType().Name} marks no input with the {ClassName} class.");
      input.Focus();
      if(input is TextBox textBox)
         textBox.CaretIndex = textBox.Text?.Length ?? 0;
   }
}
