using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JAStudio.UI.Utils;

/// <summary>
/// A dialog that asks a person to choose one of a few <typeparamref name="TChoice"/>s, a button each, or to cancel (<see cref="DialogWithAMessage{TResult}"/>):<br/>
/// centred on the screen (<see cref="ShowAsync"/>). Enter picks the first choice, and Esc, Cancel or closing the dialog cancels.
/// </summary>
static class ChoiceDialog<TChoice> where TChoice : struct
{
   /// <summary>Shows the dialog centred on the screen, as from a menu outside JAStudio's windows, and returns the choice picked, or null when cancelled.</summary>
   public static Task<TChoice?> ShowAsync(string title, string message, IReadOnlyList<(string Label, TChoice Choice)> choices) =>
      DialogWithAMessage<TChoice?>.ShowCentredOnTheScreenAsync(title, message, width: 480, [..choices.Select(it => (it.Label, (TChoice?)it.Choice)), ("Cancel", null)]);
}
