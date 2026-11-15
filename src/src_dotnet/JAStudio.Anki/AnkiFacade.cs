using System.Collections.Generic;
using JAStudio.PythonInterop;
using JAStudio.PythonInterop.Utilities;

namespace JAStudio.Anki;

/// <summary>
/// What is left of the static facade over the Anki add-on's Python functions (<c>jastudio.dotnet.anki_facade_backend</c>): the calls that<br/>
/// <c>JAStudio.UI</c> still makes itself. Everything else JAStudio calls in Anki goes through the interfaces of <see cref="AnkisFunctions"/>.
/// </summary>
/// <remarks>
/// wip: each of the three goes with work that is planned in <c>dev_docs/moving-jastudio-out-of-anki.md</c>: the menu of Anki's browser with the Qt<br/>
/// menus (increment 3), and the refresh after a menu command with the answer to "how the menu commands refresh the spaced repetition system's<br/>
/// windows".
/// </remarks>
public static class AnkiFacade
{
   static readonly PythonObjectWrapper Backend = PythonEnvironment.Import("jastudio.dotnet.anki_facade_backend");

   public static class Browser
   {
      /// <summary>The commands of the menu JAStudio adds to Anki's browser, on the cards selected there.</summary>
      public static class MenuActions
      {
         /// <summary>Prioritize selected cards (sets card.due based on note type priority).</summary>
         public static void PrioritizeCards(IReadOnlyList<long> cardIds) => Backend.Use(it => it.browser_prioritize_cards(PythonDotNetShim.LongList.ToPython(cardIds)));

         /// <summary>Spread selected cards over days (distributes due dates across time range).</summary>
         public static void SpreadCardsOverDays(IReadOnlyList<long> cardIds, int startDay, int daysApart) => Backend.Use(it => it.browser_spread_cards_over_days(PythonDotNetShim.LongList.ToPython(cardIds), startDay, daysApart));
      }
   }

   public static class UIUtils
   {
      /// <summary>Has Anki's reviewer, previewer and browser draw what they show again (<see cref="AnkisFunctions.IAnkisWindows.Refresh"/>), for <c>SpecMenuItem.UICommand</c>, which has no object to ask.</summary>
      public static void Refresh() => Backend.Use(it => it.ui_refresh());
   }

   /// <summary>The id of the Anki note that the card <paramref name="cardId"/> belongs to, for the menu of Anki's browser.</summary>
   public static long GetNoteIdFromCardId(long cardId) => Backend.Use(it => (long)it.get_note_id_from_card_id(cardId));
}
