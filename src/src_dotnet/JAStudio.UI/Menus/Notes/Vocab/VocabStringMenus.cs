using System.Collections.Generic;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Note.Vocabulary.Commands;
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

   static SpecMenuItem BuildAddMenuSpec(string text, VocabNote vocab)
   {
      var synonyms = vocab.RelatedNotes.Synonyms.Strings();
      var antonyms = vocab.RelatedNotes.Antonyms.Strings();
      var seeAlso = vocab.RelatedNotes.SeeAlso.Strings();
      var confusedWith = vocab.RelatedNotes.ConfusedWith.Get();
      var perfectSynonyms = vocab.RelatedNotes.PerfectSynonyms.Get();
      var matchedForms = vocab.Forms.MatchedFormsSet();

      var items = new List<SpecMenuItem>
                  {
                     SpecMenuItem.UICommand(ShortcutFinger.Home1("Synonym"), () => vocab.RelatedNotes.Synonyms.Add(text), enabled: !synonyms.Contains(text)),
                     NoteCommandMenuItem.Running(ShortcutFinger.Home2, VocabCommands.AddSynonymsTransitivelyOneLevel, vocab, text),
                     SpecMenuItem.UICommand(ShortcutFinger.Home3("Confused with"), () => vocab.RelatedNotes.ConfusedWith.Add(text), enabled: !confusedWith.Contains(text)),
                     SpecMenuItem.UICommand(ShortcutFinger.Home4("Antonym"), () => vocab.RelatedNotes.Antonyms.Add(text), enabled: !antonyms.Contains(text)),
                     SpecMenuItem.UICommand(ShortcutFinger.Home5("Form"), () => vocab.Forms.Add(text), enabled: !matchedForms.Contains(text)),
                     SpecMenuItem.UICommand(ShortcutFinger.Up1("See also"), () => vocab.RelatedNotes.SeeAlso.Add(text), enabled: !seeAlso.Contains(text)),
                     SpecMenuItem.UICommand(ShortcutFinger.Down1("Perfect synonym, automatically synchronize answers"), () => vocab.RelatedNotes.PerfectSynonyms.AddOverwritingTheAnswerOfTheAddedSynonym(text), enabled: !perfectSynonyms.Contains(text))
                  };

      return SpecMenuItem.Submenu(ShortcutFinger.Home1("Add"), items);
   }

   static SpecMenuItem BuildSetMenuSpec(string text, VocabNote vocab)
   {
      var items = new List<SpecMenuItem>
                  {
                     SpecMenuItem.UICommand(ShortcutFinger.Home1("Ergative twin"), () => vocab.RelatedNotes.ErgativeTwin.Set(text)),
                     SpecMenuItem.UICommand(ShortcutFinger.Home2("Derived from"), () => vocab.RelatedNotes.DerivedFrom.Set(text))
                  };

      return SpecMenuItem.Submenu(ShortcutFinger.Home2("Set"), items);
   }

   static SpecMenuItem BuildRemoveMenuSpec(string text, VocabNote vocab)
   {
      var synonyms = vocab.RelatedNotes.Synonyms.Strings();
      var antonyms = vocab.RelatedNotes.Antonyms.Strings();
      var seeAlso = vocab.RelatedNotes.SeeAlso.Strings();
      var confusedWith = vocab.RelatedNotes.ConfusedWith.Get();
      var perfectSynonyms = vocab.RelatedNotes.PerfectSynonyms.Get();
      var enteredForms = vocab.Forms.EnteredFormsSet();
      var ergativeTwin = vocab.RelatedNotes.ErgativeTwin.Get();
      var derivedFrom = vocab.RelatedNotes.DerivedFrom.Get();

      var items = new List<SpecMenuItem>
                  {
                     SpecMenuItem.UICommand(ShortcutFinger.Home1("Synonym"), () => vocab.RelatedNotes.Synonyms.Remove(text), enabled: synonyms.Contains(text)),
                     SpecMenuItem.UICommand(ShortcutFinger.Home2("Confused with"), () => vocab.RelatedNotes.ConfusedWith.Remove(text), enabled: confusedWith.Contains(text)),
                     SpecMenuItem.UICommand(ShortcutFinger.Home3("Antonym"), () => vocab.RelatedNotes.Antonyms.Remove(text), enabled: antonyms.Contains(text)),
                     SpecMenuItem.UICommand(ShortcutFinger.Home4("Ergative twin"), () => vocab.RelatedNotes.ErgativeTwin.Remove(), enabled: text == ergativeTwin),
                     SpecMenuItem.UICommand(ShortcutFinger.Home5("Form"), () => vocab.Forms.Remove(text), enabled: enteredForms.Contains(text)),
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

   static SpecMenuItem BuildCreateCombinedMenuSpec(string text, VocabNote vocab)
   {
      return SpecMenuItem.Submenu(
         ShortcutFinger.Up1("Create combined"),
         new List<SpecMenuItem>
         {
            SpecMenuItem.Submenu(ShortcutFinger.Home1("Prefix-onto"), BuildPrefixMenuItems()),
            SpecMenuItem.Submenu(ShortcutFinger.Home2("Suffix-onto"), BuildSuffixMenuItems())
         }
      );

      List<SpecMenuItem> BuildPrefixMenuItems() =>
      [
         Creating(ShortcutFinger.Home1($"{VocabCommands.PrefixOntoDictionaryForm.Label}: {text}{vocab.GetQuestion()}"), VocabCommands.PrefixOntoDictionaryForm),
         Creating(ShortcutFinger.Home2($"{VocabCommands.PrefixOntoChopping1.Label}  {vocab.Cloner.PrefixToChoppedPreview(text, 1)}"), VocabCommands.PrefixOntoChopping1),
         Creating(ShortcutFinger.Home3($"{VocabCommands.PrefixOntoChopping2.Label}  {vocab.Cloner.PrefixToChoppedPreview(text, 2)}"), VocabCommands.PrefixOntoChopping2),
         Creating(ShortcutFinger.Home4($"{VocabCommands.PrefixOntoChopping3.Label}  {vocab.Cloner.PrefixToChoppedPreview(text, 3)}"), VocabCommands.PrefixOntoChopping3)
      ];

      List<SpecMenuItem> BuildSuffixMenuItems() =>
      [
         Creating(ShortcutFinger.Home1(VocabCommands.SuffixOntoDictionaryForm.Label), VocabCommands.SuffixOntoDictionaryForm),
         Creating(ShortcutFinger.Home2($"{VocabCommands.SuffixOntoIStem.Label} {vocab.Cloner.SuffixToIStemPreview(text)}"), VocabCommands.SuffixOntoIStem),
         Creating(ShortcutFinger.Home3($"{VocabCommands.SuffixOntoTeStem.Label}  {vocab.Cloner.SuffixToTeStemPreview(text)}"), VocabCommands.SuffixOntoTeStem),
         Creating(ShortcutFinger.Home4($"{VocabCommands.SuffixOntoEStem.Label}  {vocab.Cloner.SuffixToEStemPreview(text)}"), VocabCommands.SuffixOntoEStem),
         Creating(ShortcutFinger.Home5($"{VocabCommands.SuffixOntoAStem.Label}  {vocab.Cloner.SuffixToAStemPreview(text)}"), VocabCommands.SuffixOntoAStem),
         Creating(ShortcutFinger.Up1($"{VocabCommands.SuffixOntoChopping1.Label}  {vocab.Cloner.SuffixToChoppedPreview(text, 1)}"), VocabCommands.SuffixOntoChopping1),
         Creating(ShortcutFinger.Up2($"{VocabCommands.SuffixOntoChopping2.Label}  {vocab.Cloner.SuffixToChoppedPreview(text, 2)}"), VocabCommands.SuffixOntoChopping2),
         Creating(ShortcutFinger.Up3($"{VocabCommands.SuffixOntoChopping3.Label}  {vocab.Cloner.SuffixToChoppedPreview(text, 3)}"), VocabCommands.SuffixOntoChopping3),
         Creating(ShortcutFinger.Up4($"{VocabCommands.SuffixOntoChopping4.Label}  {vocab.Cloner.SuffixToChoppedPreview(text, 4)}"), VocabCommands.SuffixOntoChopping4)
      ];

      SpecMenuItem Creating(string label, VocabCreatingCommandTakingAText command) => NoteCommandMenuItem.Creating(label, command, vocab, text);
   }
}
