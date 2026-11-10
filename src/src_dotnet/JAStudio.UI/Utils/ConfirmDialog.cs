using System.Threading.Tasks;
using Avalonia.Controls;

namespace JAStudio.UI.Utils;

/// <summary>
/// A Yes/No confirmation dialog (<see cref="DialogWithAMessage{TResult}"/>): centred on the screen (<see cref="ShowAsync"/>), or modal over a window<br/>
/// (<see cref="ShowOverAsync"/>). Enter picks Yes, and Esc, No or closing the dialog picks No.
/// </summary>
static class ConfirmDialog
{
   const double Width = 400;

   static readonly (string Label, bool Result)[] YesAndNo = [("Yes", true), ("No", false)];

   /// <summary>Shows the dialog centred on the screen, as from a menu outside JAStudio's windows, and returns whether Yes was picked.</summary>
   public static Task<bool> ShowAsync(string title, string message) => DialogWithAMessage<bool>.ShowCentredOnTheScreenAsync(title, message, Width, YesAndNo);

   /// <summary>Shows the dialog modally, centered over <paramref name="owner"/>, and returns whether Yes was picked.</summary>
   public static Task<bool> ShowOverAsync(Window owner, string title, string message) => DialogWithAMessage<bool>.ShowModallyOverAsync(owner, title, message, Width, YesAndNo);
}
