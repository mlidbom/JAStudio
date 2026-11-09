using System;
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

   /// <summary>The Add menu: the text as a word of one of the vocab's relationships, such as a synonym, or as one of its forms.</summary>
   static SpecMenuItem BuildAddMenuSpec(string text, VocabNote vocab)
   {
      var related = vocab.RelatedNotes;
      var word = WordReference.FromDisambiguationNameOrNone(text);
      var matchedForms = vocab.Forms.MatchedFormsSet();

      var items = new List<SpecMenuItem>
                  {
                     Adding(ShortcutFinger.Home1("Synonym"), related.Synonyms.Contains, related.Synonyms.Add),
                     NoteCommandMenuItem.Running(ShortcutFinger.Home2, VocabCommands.AddSynonymsTransitivelyOneLevel, vocab, text, enabled: word is not null),
                     Adding(ShortcutFinger.Home3("Confused with"), related.ConfusedWith.Contains, related.ConfusedWith.Add),
                     Adding(ShortcutFinger.Home4("Antonym"), related.Antonyms.Contains, related.Antonyms.Add),
                     SpecMenuItem.UICommand(ShortcutFinger.Home5("Form"), () => vocab.Forms.Add(text), enabled: !matchedForms.Contains(text)),
                     Adding(ShortcutFinger.Up1("See also"), related.SeeAlso.Contains, related.SeeAlso.Add),
                     Adding(ShortcutFinger.Up2("Ergative twin"), related.ErgativeTwins.Contains, related.ErgativeTwins.Add),
                     Adding(ShortcutFinger.Down1("Perfect synonym, automatically synchronize answers"), related.PerfectSynonyms.Contains, related.PerfectSynonyms.AddOverwritingTheAnswerOfTheAddedSynonym)
                  };

      return SpecMenuItem.Submenu(ShortcutFinger.Home1("Add"), items);

      SpecMenuItem Adding(string name, Func<WordReference, bool> hasIt, Action<WordReference> add) => ForTheWord(word, name, it => !hasIt(it), add);
   }

   /// <summary>The Set menu: the text as the word the vocab is derived from (<see cref="Core.Note.Vocabulary.RelatedVocab.RelatedVocab.DerivedFrom"/>).</summary>
   static SpecMenuItem BuildSetMenuSpec(string text, VocabNote vocab)
   {
      var word = WordReference.FromDisambiguationNameOrNone(text);
      var items = new List<SpecMenuItem>
                  {
                     ForTheWord(word, ShortcutFinger.Home2("Derived from"), it => it != vocab.RelatedNotes.DerivedFrom.Word, it => vocab.RelatedNotes.DerivedFrom.Set(it))
                  };

      return SpecMenuItem.Submenu(ShortcutFinger.Home2("Set"), items);
   }

   /// <summary>The Remove menu: the text as a word of one of the vocab's relationships, or as one of its forms.</summary>
   static SpecMenuItem BuildRemoveMenuSpec(string text, VocabNote vocab)
   {
      var related = vocab.RelatedNotes;
      var word = WordReference.FromDisambiguationNameOrNone(text);
      var enteredForms = vocab.Forms.EnteredFormsSet();

      var items = new List<SpecMenuItem>
                  {
                     Removing(ShortcutFinger.Home1("Synonym"), related.Synonyms.Contains, related.Synonyms.Remove),
                     Removing(ShortcutFinger.Home2("Confused with"), related.ConfusedWith.Contains, related.ConfusedWith.Remove),
                     Removing(ShortcutFinger.Home3("Antonym"), related.Antonyms.Contains, related.Antonyms.Remove),
                     Removing(ShortcutFinger.Home4("Ergative twin"), related.ErgativeTwins.Contains, related.ErgativeTwins.Remove),
                     SpecMenuItem.UICommand(ShortcutFinger.Home5("Form"), () => vocab.Forms.Remove(text), enabled: enteredForms.Contains(text)),
                     Removing(ShortcutFinger.Up1("See also"), related.SeeAlso.Contains, related.SeeAlso.Remove),
                     Removing(ShortcutFinger.Down1("Perfect synonym"), related.PerfectSynonyms.Contains, related.PerfectSynonyms.Remove),
                     Removing(ShortcutFinger.Down2("Derived from"), it => it == related.DerivedFrom.Word, _ => related.DerivedFrom.Set(null))
                  };

      return SpecMenuItem.Submenu(ShortcutFinger.Home3("Remove"), items);

      SpecMenuItem Removing(string name, Func<WordReference, bool> hasIt, Action<WordReference> remove) => ForTheWord(word, name, hasIt, remove);
   }

   /// <summary>
   /// An item doing <paramref name="action"/> to <paramref name="word"/>, the word the selected text names, enabled when <paramref name="applies"/> to it.<br/>
   /// Disabled when the selected text names no word (<see cref="WordReference.FromDisambiguationNameOrNone"/>).
   /// </summary>
   static SpecMenuItem ForTheWord(WordReference? word, string name, Func<WordReference, bool> applies, Action<WordReference> action) =>
      word is {} it
         ? SpecMenuItem.UICommand(name, () => action(it), enabled: applies(it))
         : SpecMenuItem.UICommand(name, () => {}, enabled: false);

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
