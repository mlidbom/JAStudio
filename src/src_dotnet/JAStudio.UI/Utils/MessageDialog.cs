using Avalonia.Controls;

namespace JAStudio.UI.Utils;

/// <summary>
/// A dialog that tells a person something, with an OK button that closes it, as Enter and Esc do (<see cref="DialogWithAMessage{TResult}"/>):<br/>
/// modal over a window (<see cref="ShowOver"/>).
/// </summary>
static class MessageDialog
{
   /// <summary>Shows the dialog modally, centred over <paramref name="owner"/>, and returns at once, rather than when the dialog closes.</summary>
   public static void ShowOver(Window owner, string title, string message) =>
      DialogWithAMessage<bool>.ShowModallyOver(owner, title, message, width: 400, [("OK", Result: true)]);
}
