using System;
using System.Collections.Generic;
using JAStudio.Anki;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.UI.Menus.UIAgnosticMenuStructure;
using JAStudio.UI.Utils;

namespace JAStudio.UI.Menus.Notes.Vocab;

/// <summary>
/// Vocab string menu builders (selection/clipboard context menus).
/// Corresponds to notes/vocab/string_menu.py in Python.
/// </summary>
class VocabStringMenus
{
   readonly Core.TemporaryServiceCollection _services;

   public VocabStringMenus(Core.TemporaryServiceCollection services) => _services = services;

   public SpecMenuItem BuildStringMenuSpec(string title, string text, VocabNote vocab) =>
      SpecMenuItem.Submenu(
         title,
         new List<SpecMenuItem>
         {
            BuildAddMenuSpec(text, vocab),
            BuildSetMenuSpec(text, vocab),
            BuildRemoveMenuSpec(text, vocab),
            BuildSentenceMenuSpec(text, vocab),
            BuildCreateCombinedMenuSpec(text, vocab)
         }
      );

   SpecMenuItem BuildAddMenuSpec(string text, VocabNote vocab)
   {
      var synonyms = vocab.RelatedNotes.Synonyms.Strings();
      var antonyms = vocab.RelatedNotes.Antonyms.Strings();
      var seeAlso = vocab.RelatedNotes.SeeAlso.Strings();
      var confusedWith = vocab.RelatedNotes.ConfusedWith.Get();
      var perfectSynonyms = vocab.RelatedNotes.PerfectSynonyms.Get();
      var forms = vocab.Forms.AllSet();

      var items = new List<SpecMenuItem>
                  {
                     SpecMenuItem.UICommand(ShortcutFinger.Home1("Synonym"), () => vocab.RelatedNotes.Synonyms.Add(text), enabled: !synonyms.Contains(text)),
                     SpecMenuItem.UICommand(ShortcutFinger.Home2("Synonyms transitively one level"), () => vocab.RelatedNotes.Synonyms.AddTransitivelyOneLevel(text)),
                     SpecMenuItem.UICommand(ShortcutFinger.Home3("Confused with"), () => vocab.RelatedNotes.ConfusedWith.Add(text), enabled: !confusedWith.Contains(text)),
                     SpecMenuItem.UICommand(ShortcutFinger.Home4("Antonym"), () => vocab.RelatedNotes.Antonyms.Add(text), enabled: !antonyms.Contains(text)),
                     SpecMenuItem.UICommand(ShortcutFinger.Home5("Form"), () => vocab.Forms.Add(text), enabled: !forms.Contains(text)),
                     SpecMenuItem.UICommand(ShortcutFinger.Up1("See also"), () => vocab.RelatedNotes.SeeAlso.Add(text), enabled: !seeAlso.Contains(text)),
                     SpecMenuItem.UICommand(ShortcutFinger.Down1("Perfect synonym, automatically synchronize answers"), () => vocab.RelatedNotes.PerfectSynonyms.AddOverwritingTheAnswerOfTheAddedSynonym(text), enabled: !perfectSynonyms.Contains(text))
                  };

      return SpecMenuItem.Submenu(ShortcutFinger.Home1("Add"), items);
   }

   SpecMenuItem BuildSetMenuSpec(string text, VocabNote vocab)
   {
      var items = new List<SpecMenuItem>
                  {
                     SpecMenuItem.UICommand(ShortcutFinger.Home1("Ergative twin"), () => vocab.RelatedNotes.ErgativeTwin.Set(text)),
                     SpecMenuItem.UICommand(ShortcutFinger.Home2("Derived from"), () => vocab.RelatedNotes.DerivedFrom.Set(text))
                  };

      return SpecMenuItem.Submenu(ShortcutFinger.Home2("Set"), items);
   }

   SpecMenuItem BuildRemoveMenuSpec(string text, VocabNote vocab)
   {
      var synonyms = vocab.RelatedNotes.Synonyms.Strings();
      var antonyms = vocab.RelatedNotes.Antonyms.Strings();
      var seeAlso = vocab.RelatedNotes.SeeAlso.Strings();
      var confusedWith = vocab.RelatedNotes.ConfusedWith.Get();
      var perfectSynonyms = vocab.RelatedNotes.PerfectSynonyms.Get();
      var forms = vocab.Forms.AllSet();
      var ergativeTwin = vocab.RelatedNotes.ErgativeTwin.Get();
      var derivedFrom = vocab.RelatedNotes.DerivedFrom.Get();

      var items = new List<SpecMenuItem>
                  {
                     SpecMenuItem.UICommand(ShortcutFinger.Home1("Synonym"), () => vocab.RelatedNotes.Synonyms.Remove(text), enabled: synonyms.Contains(text)),
                     SpecMenuItem.UICommand(ShortcutFinger.Home2("Confused with"), () => vocab.RelatedNotes.ConfusedWith.Remove(text), enabled: confusedWith.Contains(text)),
                     SpecMenuItem.UICommand(ShortcutFinger.Home3("Antonym"), () => vocab.RelatedNotes.Antonyms.Remove(text), enabled: antonyms.Contains(text)),
                     SpecMenuItem.UICommand(ShortcutFinger.Home4("Ergative twin"), () => vocab.RelatedNotes.ErgativeTwin.Remove(), enabled: text == ergativeTwin),
                     SpecMenuItem.UICommand(ShortcutFinger.Home5("Form"), () => vocab.Forms.Remove(text), enabled: forms.Contains(text)),
                     SpecMenuItem.UICommand(ShortcutFinger.Up1("See also"), () => vocab.RelatedNotes.SeeAlso.Remove(text), enabled: seeAlso.Contains(text)),
                     SpecMenuItem.UICommand(ShortcutFinger.Down1("Perfect synonym"), () => vocab.RelatedNotes.PerfectSynonyms.Remove(text), enabled: perfectSynonyms.Contains(text)),
                     SpecMenuItem.UICommand(ShortcutFinger.Down2("Derived from"), () => vocab.RelatedNotes.DerivedFrom.Set(""), enabled: text == derivedFrom)
                  };

      return SpecMenuItem.Submenu(ShortcutFinger.Home3("Remove"), items);
   }

   SpecMenuItem BuildSentenceMenuSpec(string text, VocabNote vocab)
   {
      var sentences = _services.CoreApp.Collection.Sentences.WithQuestion(text);
      var hasSentences = sentences.Count > 0;
      var disambiguationName = vocab.Question.DisambiguationName;

      var isHighlighted = hasSentences && sentences[0].Configuration.HighlightedWords.Contains(disambiguationName);

      var items = new List<SpecMenuItem>
                  {
                     SpecMenuItem.UICommand(ShortcutFinger.Home1("Add Highlight"),
                                            () =>
                                            {
                                               if(sentences.Count > 0)
                                                  sentences[0].Configuration.AddHighlightedWord(disambiguationName);
                                            },
                                            enabled: hasSentences && !isHighlighted),
                     SpecMenuItem.UICommand(ShortcutFinger.Home2("Remove highlight"),
                                            () =>
                                            {
                                               foreach(var sent in sentences)
                                                  sent.Configuration.RemoveHighlightedWord(disambiguationName);
                                            },
                                            enabled: hasSentences && isHighlighted),
                     SpecMenuItem.UICommand(ShortcutFinger.Home3("Remove-sentence: Mark as incorrect match in sentence"),
                                            () =>
                                            {
                                               foreach(var sent in sentences)
                                                  sent.Configuration.IncorrectMatches.AddGlobal(disambiguationName);
                                            },
                                            enabled: hasSentences)
                  };

      return SpecMenuItem.Submenu(ShortcutFinger.Home4("Sentence"), items);
   }

   SpecMenuItem BuildCreateCombinedMenuSpec(string text, VocabNote vocab)
   {
      // Nested local function for suffix operations (mirroring Python structure)
      List<SpecMenuItem> BuildSuffixMenuItems()
      {
         return
         [
            CreateNoteCommand(ShortcutFinger.Home1("dictionary-form"), () => vocab.Cloner.CreateSuffixVersion(text)),
            CreateNoteCommand(ShortcutFinger.Home2($"い-stem {vocab.Cloner.SuffixToIStemPreview(text)}"), () => vocab.Cloner.SuffixToIStem(text)),
            CreateNoteCommand(ShortcutFinger.Home3($"て-stem  {vocab.Cloner.SuffixToTeStemPreview(text)}"), () => vocab.Cloner.SuffixToTeStem(text)),
            CreateNoteCommand(ShortcutFinger.Home4($"え-stem  {vocab.Cloner.SuffixToEStemPreview(text)}"), () => vocab.Cloner.SuffixToEStem(text)),
            CreateNoteCommand(ShortcutFinger.Home5($"あ-stem  {vocab.Cloner.SuffixToAStemPreview(text)}"), () => vocab.Cloner.SuffixToAStem(text)),
            CreateNoteCommand(ShortcutFinger.Up1($"chop-1  {vocab.Cloner.SuffixToChoppedPreview(text, 1)}"), () => vocab.Cloner.SuffixToChopped(text, 1)),
            CreateNoteCommand(ShortcutFinger.Up2($"chop-2  {vocab.Cloner.SuffixToChoppedPreview(text, 2)}"), () => vocab.Cloner.SuffixToChopped(text, 2)),
            CreateNoteCommand(ShortcutFinger.Up3($"chop-3  {vocab.Cloner.SuffixToChoppedPreview(text, 3)}"), () => vocab.Cloner.SuffixToChopped(text, 3)),
            CreateNoteCommand(ShortcutFinger.Up4($"chop-4  {vocab.Cloner.SuffixToChoppedPreview(text, 4)}"), () => vocab.Cloner.SuffixToChopped(text, 4))
         ];
      }

      // Nested local function for prefix operations (mirroring Python structure)
      List<SpecMenuItem> BuildPrefixMenuItems()
      {
         return
         [
            CreateNoteCommand(ShortcutFinger.Home1($"Dictionary form: {text}{vocab.GetQuestion()}"), () => vocab.Cloner.PrefixToDictionaryForm(text)),
            CreateNoteCommand(ShortcutFinger.Home2($"chop-1  {vocab.Cloner.PrefixToChoppedPreview(text, 1)}"), () => vocab.Cloner.PrefixToChopped(text, 1)),
            CreateNoteCommand(ShortcutFinger.Home3($"chop-2  {vocab.Cloner.PrefixToChoppedPreview(text, 2)}"), () => vocab.Cloner.PrefixToChopped(text, 2)),
            CreateNoteCommand(ShortcutFinger.Home4($"chop-3  {vocab.Cloner.PrefixToChoppedPreview(text, 3)}"), () => vocab.Cloner.PrefixToChopped(text, 3))
         ];
      }

      return SpecMenuItem.Submenu(
         ShortcutFinger.Up1("Create combined"),
         new List<SpecMenuItem>
         {
            SpecMenuItem.Submenu(ShortcutFinger.Home1("Prefix-onto"), BuildPrefixMenuItems()),
            SpecMenuItem.Submenu(ShortcutFinger.Home2("Suffix-onto"), BuildSuffixMenuItems())
         }
      );
   }

   public SpecMenuItem CreateNoteCommand(string name, Func<JPNote> createNote, char? acceleratorKey = null, string? shortcut = null, bool enabled = true)
      => SpecMenuItem.Command(name,
                              () => AnkiFacade.Browser.ExecuteLookupAndShowPreviewer(_services.QueryBuilder().NotesLookup([createNote()])),
                              acceleratorKey,
                              shortcut,
                              enabled: enabled);
}
