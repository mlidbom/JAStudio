using System;
using System.Collections.Generic;
using JAStudio.Anki;
using JAStudio.UI.Menus.UIAgnosticMenuStructure;
using JAStudio.UI.Utils;

namespace JAStudio.UI.Menus;

/// <summary>
/// Builds "Open in Anki" menus for searching notes in the Anki browser.
/// Ported from jastudio/ui/menus/open_in_anki.py
/// Now uses UI-agnostic MenuItem specifications and AnkiFacade for Anki calls.
/// </summary>
class OpenInAnkiMenus
{
   readonly Core.TemporaryServiceCollection _services;

   public OpenInAnkiMenus(Core.TemporaryServiceCollection services) => _services = services;

   /// <summary>
   /// Build the complete "Open in Anki" menu structure as a UI-agnostic specification.
   /// Can be passed to AvaloniaMenuAdapter, PyQt adapter, or any other UI framework.
   /// </summary>
   /// <param name="title"></param>
   /// <param name="getSearchText">Function that returns the text to search for</param>
   public SpecMenuItem BuildOpenInAnkiMenuSpec(string title, Func<string> getSearchText) =>
      SpecMenuItem.Submenu(
         title,
         new List<SpecMenuItem>
         {
            SpecMenuItem.Submenu(ShortcutFinger.Home1("Exact matches"),
                                 new List<SpecMenuItem>
                                 {
                                    CreateLookupSpec(ShortcutFinger.Home1("Open Exact matches | no sentences | reading cards"), () => _services.QueryBuilder().ExactMatchesNoSentencesReadingCards(getSearchText())),
                                    CreateLookupSpec(ShortcutFinger.Home2("Open Exact matches with sentences"), () => _services.QueryBuilder().ExactMatches(getSearchText()))
                                 }
            ),
            SpecMenuItem.Submenu(ShortcutFinger.Home2("Kanji"),
                                 new List<SpecMenuItem>
                                 {
                                    CreateLookupSpec(ShortcutFinger.Home1("All kanji in string"), () => _services.QueryBuilder().KanjiInString(getSearchText())),
                                    CreateLookupSpec(ShortcutFinger.Home2("By reading part"), () => _services.QueryBuilder().KanjiWithReadingPart(getSearchText())),
                                    CreateLookupSpec(ShortcutFinger.Home3("By reading exact"), () => _services.QueryBuilder().NotesLookup(_services.CoreApp.Collection.Kanji.WithReading(getSearchText()))),
                                    CreateLookupSpec(ShortcutFinger.Home4("With radicals"), () => _services.QueryBuilder().KanjiWithRadicalsInString(getSearchText())),
                                    CreateLookupSpec(ShortcutFinger.Up1("With meaning"), () => _services.QueryBuilder().KanjiWithMeaning(getSearchText()))
                                 }
            ),
            SpecMenuItem.Submenu(ShortcutFinger.Home3("Vocab"),
                                 new List<SpecMenuItem>
                                 {
                                    CreateLookupSpec(ShortcutFinger.Home1("form -"), () => _services.QueryBuilder().SingleVocabByFormExact(getSearchText())),
                                    CreateLookupSpec(ShortcutFinger.Home2("form - read card only"), () => _services.QueryBuilder().SingleVocabByFormExactReadCardOnly(getSearchText())),
                                    CreateLookupSpec(ShortcutFinger.Home3("form, reading or answer"), () => _services.QueryBuilder().SingleVocabByQuestionReadingOrAnswerExact(getSearchText())),
                                    CreateLookupSpec(ShortcutFinger.Home4("Wildcard"), () => _services.QueryBuilder().SingleVocabWildcard(getSearchText())),
                                    CreateLookupSpec(ShortcutFinger.Up1("Text words"), () => _services.QueryBuilder().TextVocabLookup(getSearchText()))
                                 }
            ),
            SpecMenuItem.Submenu(ShortcutFinger.Home4("Sentence"),
                                 new List<SpecMenuItem>
                                 {
                                    CreateLookupSpec(ShortcutFinger.Home1("Parse Vocabulary"), () => _services.QueryBuilder().SentenceSearch(getSearchText(), exact: false)),
                                    CreateLookupSpec(ShortcutFinger.Home2("Exact String"), () => _services.QueryBuilder().SentenceSearch(getSearchText(), exact: true))
                                 }
            )
         }
      );

   static SpecMenuItem CreateLookupSpec(string header, Func<string> getQuery)
   {
      return SpecMenuItem.UICommand(
         header,
         () =>
         {
            var query = getQuery();
            AnkiFacade.Browser.ExecuteLookup(query);
         }
      );
   }
}
