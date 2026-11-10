using System.Linq;
using Avalonia.Controls;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.UI.Utils;

// ReSharper disable InconsistentNaming

namespace JAStudio.UI.Views.Specifications.AICreatedSpecs.Dialogs;

/// <summary>
/// The dialogs that tell or ask a person something (<see cref="DialogWithAMessage{TResult}"/>), shown over a window and rendered: a message, as when a vocab<br/>
/// a person asked to create existed already, and a confirmation, as before a tag is deleted.
/// </summary>
public class When_a_dialog_with_a_message_is_shown_over_a_window : SpecificationOpeningWindows, IAIGeneratedSpec
{
   protected readonly Window Owner;

   public When_a_dialog_with_a_message_is_shown_over_a_window() => Owner = Open(() =>
   {
      var window = new Window { Width = 800, Height = 600 };
      window.Show();
      return window;
   });

   protected Window TheDialog => OnTheUIThread(() => Owner.OwnedWindows.Single());

   public class a_message : When_a_dialog_with_a_message_is_shown_over_a_window
   {
      public a_message()
      {
         OnTheUIThread(() => MessageDialog.ShowOver(Owner, "The vocab exists already", "\"食べます\" exists already, so it's opened rather than created: no two vocab may share a question."));
         Render(TheDialog, "MessageDialog");
      }

      [XF] public void the_dialog_is_shown() => OnTheUIThread(() => TheDialog.IsVisible).Must().BeTrue();
   }

   public class a_confirmation : When_a_dialog_with_a_message_is_shown_over_a_window
   {
      public a_confirmation()
      {
         OnTheUIThread(() => _ = ConfirmDialog.ShowOverAsync(Owner, "Delete a tag", "Delete the tag tmp::x? It's removed from 3 notes."));
         Render(TheDialog, "ConfirmDialog");
      }

      [XF] public void the_dialog_is_shown() => OnTheUIThread(() => TheDialog.IsVisible).Must().BeTrue();
   }
}
