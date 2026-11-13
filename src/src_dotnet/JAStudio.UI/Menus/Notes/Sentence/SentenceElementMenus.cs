using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.UI.Menus.UIAgnosticMenuStructure;
using JAStudio.UI.Utils;
using JAStudio.UI.ViewModels.Workspace.Editors.Parsing;
using JAStudio.UI.ViewModels.Workspace.NotePages;
using JAStudio.UI.ViewModels.Workspace.NotePages.Sentences;
using JAStudio.UI.Views.Workspace.Editors.Matching;

namespace JAStudio.UI.Menus.Notes.Sentence;

/// <summary>
/// The right-click menus of the elements that show a part of a sentence: a match, as a breakdown entry on the sentence's page in a workspace tab<br/>
/// (<see cref="SentencePageViewModel"/>) or a row of its parsing view (<see cref="SentenceParsingViewModel"/>) shows it, and a compound part or a kanji<br/>
/// under a breakdown entry. Each starts with the element's own items, only those that apply to it as it is, such as marking a match incorrect, then the<br/>
/// menu of the element's note, the sentence's menu, and what can be done with the text selected, if any. See <c>dev_docs/jastudio-workspace.md</c>, "The<br/>
/// right-click menu by what's right-clicked".
/// </summary>
/// <remarks>
/// An item has the same key in whichever element's menu shows it, and the two halves of a toggle share one, such as marking a match incorrect and<br/>
/// unmarking it, so that the keys become muscle memory. What doesn't apply to the element is left out rather than shown disabled.
/// </remarks>
sealed class SentenceElementMenus
{
   readonly TemporaryServiceCollection _services;
   readonly SentenceNote _sentence;
   readonly INotePageHost _host;
   readonly NoteContextMenu _noteMenus;

   /// <param name="services">The application's services.</param>
   /// <param name="sentence">The sentence the elements show a part of.</param>
   /// <param name="host">The workspace tab showing the sentence, which shows the notes the menus open.</param>
   public SentenceElementMenus(TemporaryServiceCollection services, SentenceNote sentence, INotePageHost host)
   {
      _services = services;
      _sentence = sentence;
      _host = host;
      _noteMenus = new NoteContextMenu(services);
   }

   /// <summary>
   /// The menu of <paramref name="match"/>, as a breakdown entry or a row of the sentence's parsing view shows it, with <paramref name="selection"/>, the text<br/>
   /// selected on it, if any.
   /// </summary>
   public List<SpecMenuItem> OfAMatch(Match match, string selection) =>
      match switch
      {
         VocabMatch vocabMatch => [CheckingTheMatchConditionAgainst(vocabMatch),
                                   .. MarkingByHand(vocabMatch),
                                   .. LinkedNoteItems(TheLinkTo(vocabMatch.Vocab), TheVocabsMenu),
                                   .. TheSentencesAndTheSelectionsMenus(selection)],
         DictionaryMatch dictionaryMatch => [.. MarkingByHand(dictionaryMatch),
                                             CreatingTheVocab($"Create the vocab {dictionaryMatch.ParsedForm} from the dictionary", dictionaryMatch),
                                             SearchingTheWebFor(dictionaryMatch),
                                             .. TheSentencesAndTheSelectionsMenus(selection)],
         MissingMatch missingMatch => [.. MarkingByHand(missingMatch),
                                       CreatingTheVocab($"Create the vocab {missingMatch.ParsedForm}", missingMatch),
                                       SearchingTheWebFor(missingMatch),
                                       .. ShowingTheTokens(),
                                       .. TheSentencesAndTheSelectionsMenus(selection)],
         _ => throw new ArgumentException($"A match of no known kind: {match}.", nameof(match))
      };

   /// <summary>The menu of <paramref name="part"/>, a compound part under a breakdown entry, about the part's vocab, with <paramref name="selection"/>, the text selected on it, if any.</summary>
   public List<SpecMenuItem> OfACompoundPart(CompoundPartEntryViewModel part, string selection) =>
      [.. LinkedNoteItems(part.Question, TheVocabsMenu), .. TheSentencesAndTheSelectionsMenus(selection)];

   /// <summary>The menu of <paramref name="kanji"/>, a kanji under a breakdown entry, about the kanji's note, with <paramref name="selection"/>, the text selected on it, if any.</summary>
   public List<SpecMenuItem> OfAKanji(BreakdownKanjiViewModel kanji, string selection) =>
      [.. LinkedNoteItems(kanji.Kanji, TheKanjisMenu), .. TheSentencesAndTheSelectionsMenus(selection)];

   /// <summary>
   /// Opens the match-condition window of the vocab of <paramref name="match"/>, checking its tree against the word matched, valid or not, so that each<br/>
   /// line shows whether the word meets its condition.
   /// </summary>
   SpecMenuItem CheckingTheMatchConditionAgainst(VocabMatch match) =>
      SpecMenuItem.Command(ShortcutFinger.Home1($"Match-condition of {match.Vocab.GetQuestion()}, checked against this word"),
                           () => MatchConditionWindow.OpenCheckingTheTreeAgainst(match, _services));

   /// <summary>The items that mark <paramref name="match"/> by hand in the sentence, or unmark it, those that apply to it as it is: incorrect, hidden and highlighted.</summary>
   List<SpecMenuItem> MarkingByHand(Match match) => [.. new[] { MarkingIncorrectOrUnmarking(match), HidingOrUnhiding(match), HighlightingOrRemovingTheHighlight(match) }.OfType<SpecMenuItem>()];

   /// <summary>Unmarks <paramref name="match"/> when the sentence marks it incorrect, or marks it incorrect at its position when it's valid. Neither when it's invalid otherwise.</summary>
   SpecMenuItem? MarkingIncorrectOrUnmarking(Match match)
   {
      var incorrectMatches = _sentence.Configuration.IncorrectMatches;
      if(incorrectMatches.Contains(match))
         return SpecMenuItem.UICommand(ShortcutFinger.Home2(TheLabelOfUnmarking("Unmark incorrect", incorrectMatches, match)), () => incorrectMatches.Remove(match));

      return match.IsValid ? SpecMenuItem.UICommand(ShortcutFinger.Home2("Mark incorrect here"), () => incorrectMatches.Add(match)) : null;
   }

   /// <summary>Unhides <paramref name="match"/> when the sentence hides it, or hides it at its position when it's shown. Neither when it's hidden otherwise.</summary>
   SpecMenuItem? HidingOrUnhiding(Match match)
   {
      var hiddenMatches = _sentence.Configuration.HiddenMatches;
      if(hiddenMatches.Contains(match))
         return SpecMenuItem.UICommand(ShortcutFinger.Home3(TheLabelOfUnmarking("Unhide", hiddenMatches, match)), () => hiddenMatches.Remove(match));

      return match.IsValidForDisplay ? SpecMenuItem.UICommand(ShortcutFinger.Home3("Hide here"), () => hiddenMatches.Add(match)) : null;
   }

   /// <summary>
   /// The label of the item that unmarks <paramref name="match"/>: <paramref name="unmarking"/>, saying so when <paramref name="marks"/> contain its form<br/>
   /// everywhere in the sentence, since unmarking it then unmarks the form at every position.
   /// </summary>
   static string TheLabelOfUnmarking(string unmarking, WordExclusionSet marks, Match match) =>
      marks.ContainsItsFormEverywhereInTheSentence(match) ? $"{unmarking}, everywhere in the sentence" : unmarking;

   /// <summary>
   /// Removes the highlight of <paramref name="match"/>, or highlights it. A highlighted match is valid whatever else it fails, so highlighting also makes<br/>
   /// it match here; a token nothing has (<see cref="MissingMatch"/>) is never valid, so it isn't offered there.
   /// </summary>
   SpecMenuItem? HighlightingOrRemovingTheHighlight(Match match)
   {
      if(_sentence.Configuration.Highlights(match))
         return SpecMenuItem.UICommand(ShortcutFinger.Home4("Remove the highlight"), () => _sentence.Configuration.RemoveTheHighlightOf(match));

      return match is MissingMatch ? null : SpecMenuItem.UICommand(ShortcutFinger.Home4("Highlight"), () => _sentence.Configuration.Highlight(match));
   }

   /// <summary>
   /// Creates the vocab of the word <paramref name="match"/> is of, with the dictionary's entry for it if it has one, and opens it: <paramref name="label"/><br/>
   /// says which. Opens the vocab when it exists already, saying so.
   /// </summary>
   SpecMenuItem CreatingTheVocab(string label, Match match) =>
      SpecMenuItem.Command(ShortcutFinger.Up2(label), () => _noteMenus.OpenTheVocabNamedOrCreatedFromTheDictionary(match.ParsedForm), enabled: _services.AnkiAvailability.IsAvailable);

   /// <summary>The web searches for the word <paramref name="match"/> is of, such as its dictionaries' and its sentences'.</summary>
   static SpecMenuItem SearchingTheWebFor(Match match) => WebSearchMenuBuilder.BuildWebSearchMenu(ShortcutFinger.Up3($"Web: {match.ParsedForm}"), () => match.ParsedForm);

   /// <summary>
   /// Switches the sentence breakdown to edit mode, which shows the sentence's tokens: how the tokenizer split the sentence is usually why a token<br/>
   /// matched nothing. Only offered outside edit mode.
   /// </summary>
   List<SpecMenuItem> ShowingTheTokens()
   {
      var editMode = _services.CoreApp.Config.ShowSentenceBreakdownInEditMode;
      return editMode.Value ? [] : [SpecMenuItem.UICommand(ShortcutFinger.Up4("Show the tokens (edit mode)"), () => editMode.Value = true)];
   }

   /// <summary>
   /// The items of the note <paramref name="link"/> links to: opening it in the tab, opening it in a background tab, and its own menu, made by<br/>
   /// <paramref name="itsMenu"/>.
   /// </summary>
   List<SpecMenuItem> LinkedNoteItems(NoteLinkViewModel link, Func<NoteLinkViewModel, SpecMenuItem> itsMenu) =>
   [
      SpecMenuItem.Command(ShortcutFinger.Home5($"Open {link.Text}"), () => _host.ShowNote(link.LinkedNote)),
      SpecMenuItem.Command(ShortcutFinger.Up1($"Open {link.Text} in a background tab"), () => _host.OpenNoteInBackgroundTab(link.LinkedNote)),
      itsMenu(link)
   ];

   /// <summary>The menu of the vocab <paramref name="link"/> links to, as its own page has it with nothing selected.</summary>
   SpecMenuItem TheVocabsMenu(NoteLinkViewModel link) =>
      SpecMenuItem.Submenu(ShortcutFinger.Down1($"Vocab: {link.Text}"), _noteMenus.BuildVocabContextMenuSpec(link.LinkedNote, "", ""));

   /// <summary>The menu of the kanji <paramref name="link"/> links to, as its own page has it with nothing selected.</summary>
   SpecMenuItem TheKanjisMenu(NoteLinkViewModel link) =>
      SpecMenuItem.Submenu(ShortcutFinger.Down1($"Kanji: {link.Text}"), _noteMenus.BuildKanjiContextMenuSpec(link.LinkedNote, "", ""));

   /// <summary>
   /// The menus every element's menu ends with: <see cref="TheSentencesMenu"/>, and <see cref="TheSelectionsMenu"/> when <paramref name="selection"/>, the<br/>
   /// text selected on the element, isn't empty.
   /// </summary>
   List<SpecMenuItem> TheSentencesAndTheSelectionsMenus(string selection) =>
      selection.Length > 0 ? [TheSentencesMenu(), TheSelectionsMenu(selection)] : [TheSentencesMenu()];

   /// <summary>The sentence's menu, as a right-click on no element gives it with nothing selected.</summary>
   SpecMenuItem TheSentencesMenu() => SpecMenuItem.Submenu(ShortcutFinger.Down2("Sentence"), _noteMenus.BuildSentenceContextMenuSpec(_sentence.GetId(), "", ""));

   /// <summary>What can be done with <paramref name="selection"/>: the submenu the sentence's menu has for the text selected.</summary>
   SpecMenuItem TheSelectionsMenu(string selection) => _noteMenus.BuildSelectionMenuSpec(ShortcutFinger.Down3("Selection:"), selection, _sentence);

   /// <summary>A link to <paramref name="vocab"/>, by its question, which the items opening it and its menu are labelled with.</summary>
   static NoteLinkViewModel TheLinkTo(VocabNote vocab) => new(vocab.GetQuestion(), vocab.GetId());
}
