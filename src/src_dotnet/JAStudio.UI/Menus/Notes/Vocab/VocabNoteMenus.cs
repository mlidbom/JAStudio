using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Threading;
using JAStudio.Core.LanguageServices.JanomeEx;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Note.Vocabulary.Commands;
using JAStudio.UI.Views.TextToSpeech;
using JAStudio.UI.Menus.UIAgnosticMenuStructure;
using JAStudio.UI.Utils;
using JAStudio.UI.Views.Workspace;

// ReSharper disable once CheckNamespace
namespace JAStudio.UI.Menus.Notes.Vocab;

/// <summary>
/// Vocab note-specific menu builders.
/// Corresponds to notes/vocab/main.py in Python.
/// </summary>
class VocabNoteMenus(Core.TemporaryServiceCollection services)
{
   readonly Core.TemporaryServiceCollection _services = services;

   JPCollection Collection => _services.CoreApp.Collection;

   /// <summary>The vocab and kanji <paramref name="vocab"/> is built from: the vocab for each word its question parses into, and the kanji in its question.</summary>
   List<JPNote> Dependencies(VocabNote vocab)
   {
      var words = TextAnalysis.FromText(_services.Resolve<AnalysisServices>(), vocab.GetQuestion()).AllWordsStrings();
      return [..Collection.Vocab.WithAnyFormIn(words), ..KanjiIn(vocab.GetQuestion())];
   }

   /// <summary>The kanji notes for the characters of <paramref name="text"/> that are kanji with a note.</summary>
   List<KanjiNote> KanjiIn(string text) => Collection.Kanji.WithAnyKanjiIn([..text.Select(character => character.ToString())]);

   public SpecMenuItem BuildNoteActionsMenuSpec(string title, VocabNote vocab)
   {
      return SpecMenuItem.Submenu(
         title,
         new List<SpecMenuItem>
         {
            SpecMenuItem.Submenu(ShortcutFinger.Home1("Open"),
                                 new List<SpecMenuItem?>
                                 {
                                    SpecMenuItem.Submenu(ShortcutFinger.Home1("Vocab"),
                                                         new List<SpecMenuItem>
                                                         {
                                                            NoteListMenuItem.Showing(ShortcutFinger.Home1("Forms"), $"Vocab for the forms of {vocab.GetQuestion()}", vocab.Forms.VocabForQuestionAndEnteredForms),
                                                            NoteListMenuItem.Showing(ShortcutFinger.Home2("Compound parts"), $"Compound parts of {vocab.GetQuestion()}", () => Collection.Vocab.ReferredToByAny(vocab.CompoundParts.All())),
                                                            NoteListMenuItem.Showing(ShortcutFinger.Home3("In compounds"), $"Compounds with {vocab.GetQuestion()}", vocab.RelatedNotes.InCompounds),
                                                            NoteListMenuItem.Showing(ShortcutFinger.Home4("Synonyms"), $"Synonyms of {vocab.GetQuestion()}", vocab.RelatedNotes.Synonyms.Notes),
                                                            NoteListMenuItem.Showing(ShortcutFinger.Home5("See also"), $"See also for {vocab.GetQuestion()}", vocab.RelatedNotes.SeeAlso.Notes),
                                                            BuildOpenHomonymsMenuSpec(vocab, ShortcutFinger.Up1("Homonyms")),
                                                            NoteListMenuItem.Showing(ShortcutFinger.Up2("Dependencies"), $"Dependencies of {vocab.GetQuestion()}", () => Dependencies(vocab))
                                                         }),
                                    SpecMenuItem.Submenu(ShortcutFinger.Home2("Sentences"),
                                                         new List<SpecMenuItem>
                                                         {
                                                            NoteListMenuItem.Showing(ShortcutFinger.Home1("Sentences I'm Studying"), $"Sentences with {vocab.GetQuestion()} that I'm studying", vocab.Sentences.Studying),
                                                            NoteListMenuItem.Showing(ShortcutFinger.Home2("Sentences"), $"Sentences with {vocab.GetQuestion()}", vocab.Sentences.All),
                                                            NoteListMenuItem.Showing(ShortcutFinger.Home3("Sentences with primary form"), $"Sentences with the primary form of {vocab.GetQuestion()}", vocab.Sentences.WithPrimaryForm),
                                                            NoteListMenuItem.Showing(ShortcutFinger.Home4("Sentences with this word highlighted"), $"Sentences with {vocab.GetQuestion()} highlighted", vocab.Sentences.Highlighted),
                                                            NoteListMenuItem.Showing(ShortcutFinger.Home5("Potentially matching sentences"), $"Sentences that may match {vocab.GetQuestion()}", () => Collection.Sentences.PotentiallyMatchingVocab(vocab)),
                                                            NoteListMenuItem.Showing(ShortcutFinger.Up1("Marked incorrect in sentences"), $"Sentences where {vocab.GetQuestion()} is marked incorrect", vocab.Sentences.MarkedIncorrectIn)
                                                         }),
                                    NoteListMenuItem.Showing(ShortcutFinger.Home3("Kanji"), $"Kanji in {vocab.GetQuestion()}", () => KanjiIn(vocab.GetQuestion())),
                                    BuildOpenErgativeTwinsMenuSpec(ShortcutFinger.Home4("Ergative twins"), vocab)
                                 }.Where(it => it != null).ToList()!
            ),
            SpecMenuItem.Command(ShortcutFinger.Home2("Edit"), () => WorkspaceWindow.EditNote(vocab)),
            SpecMenuItem.Command(ShortcutFinger.Home3("Text-to-speech audio..."), () => Dispatcher.UIThread.Invoke(() => TextToSpeechDialog.OpenFor(vocab, _services))),
            SpecMenuItem.Submenu(ShortcutFinger.Home4("Create"),
                                 new List<SpecMenuItem>
                                 {
                                    BuildCreateCloneToFormMenuSpec(ShortcutFinger.Home1(VocabCommands.CloneToForm.Label), vocab),
                                    SpecMenuItem.Submenu(ShortcutFinger.Home2("Noun variations"),
                                                         new List<SpecMenuItem>
                                                         {
                                                            NoteCommandMenuItem.Creating(ShortcutFinger.Home1, VocabCommands.CreateSuruVerb, vocab),
                                                            NoteCommandMenuItem.Creating(ShortcutFinger.Home2, VocabCommands.CreateShimasuVerb, vocab),
                                                            NoteCommandMenuItem.Creating(ShortcutFinger.Home3, VocabCommands.CreateNaAdjective, vocab),
                                                            NoteCommandMenuItem.Creating(ShortcutFinger.Home4, VocabCommands.CreateNoAdjective, vocab),
                                                            NoteCommandMenuItem.Creating(ShortcutFinger.Up1, VocabCommands.CreateNiAdverb, vocab),
                                                            NoteCommandMenuItem.Creating(ShortcutFinger.Up2, VocabCommands.CreateToAdverb, vocab)
                                                         }),
                                    SpecMenuItem.Submenu(ShortcutFinger.Home3("Verb variations"),
                                                         new List<SpecMenuItem>
                                                         {
                                                            NoteCommandMenuItem.Creating(ShortcutFinger.Home1, VocabCommands.CreateMasuForm, vocab),
                                                            NoteCommandMenuItem.Creating(ShortcutFinger.Home2, VocabCommands.CreateTeForm, vocab),
                                                            NoteCommandMenuItem.Creating(ShortcutFinger.Home3, VocabCommands.CreateTaForm, vocab),
                                                            NoteCommandMenuItem.Creating(ShortcutFinger.Home4, VocabCommands.CreateNaiForm, vocab),
                                                            NoteCommandMenuItem.Creating(ShortcutFinger.Home5($"{VocabCommands.CreateEStem.Label} {vocab.Cloner.SuffixToEStemPreview("")}"), VocabCommands.CreateEStem, vocab),
                                                            NoteCommandMenuItem.Creating(ShortcutFinger.Up1, VocabCommands.CreateBaForm, vocab),
                                                            NoteCommandMenuItem.Creating(ShortcutFinger.Up2, VocabCommands.CreateReceptiveForm, vocab),
                                                            NoteCommandMenuItem.Creating(ShortcutFinger.Up3, VocabCommands.CreateCausativeForm, vocab),
                                                            NoteCommandMenuItem.Creating(ShortcutFinger.Up4, VocabCommands.CreateImperative, vocab),
                                                            NoteCommandMenuItem.Creating(ShortcutFinger.Down1, VocabCommands.CreatePotentialGodan, vocab)
                                                         }),
                                    SpecMenuItem.Submenu(ShortcutFinger.Home4("Misc"),
                                                         new List<SpecMenuItem>
                                                         {
                                                            NoteCommandMenuItem.Creating(ShortcutFinger.Home1, VocabCommands.CreateKuForm, vocab),
                                                            NoteCommandMenuItem.Creating(ShortcutFinger.Home2, VocabCommands.CreateSaForm, vocab),
                                                            NoteCommandMenuItem.Creating(ShortcutFinger.Home3, VocabCommands.CreateTePrefixed, vocab),
                                                            NoteCommandMenuItem.Creating(ShortcutFinger.Home4, VocabCommands.CreateOPrefixed, vocab),
                                                            NoteCommandMenuItem.Creating(ShortcutFinger.Home5, VocabCommands.CreateNSuffixed, vocab),
                                                            NoteCommandMenuItem.Creating(ShortcutFinger.Up1, VocabCommands.CreateKaSuffixed, vocab)
                                                         })
                                 }),
            SpecMenuItem.Submenu(ShortcutFinger.Home5("Copy"),
                                 new List<SpecMenuItem>
                                 {
                                    SpecMenuItem.UICommand(ShortcutFinger.Home1("Question"), () => CopyToClipboard(vocab.GetQuestion())),
                                    SpecMenuItem.UICommand(ShortcutFinger.Home2("Answer"), () => CopyToClipboard(vocab.GetAnswer())),
                                    SpecMenuItem.UICommand(ShortcutFinger.Home3("Definition (question:answer)"), () => CopyToClipboard($"{vocab.GetQuestion()}:{vocab.GetAnswer()}")),
                                    SpecMenuItem.UICommand(ShortcutFinger.Home4("Sentences: max 30"), () => CopyToClipboard(string.Join("\n", vocab.Sentences.All().Take(30).Select(s => s.Question.WithoutInvisibleSpace()))))
                                 }),
            SpecMenuItem.Submenu(ShortcutFinger.Up1("Misc"),
                                 new List<SpecMenuItem>
                                 {
                                    NoteCommandMenuItem.Running(ShortcutFinger.Home1, VocabCommands.AcceptMeaning, vocab, enabled: !vocab.CorpusAnswer.HasValue()),
                                    NoteCommandMenuItem.Running(ShortcutFinger.Home2, VocabCommands.GenerateAnswer, vocab),
                                    SpecMenuItem.UICommand(ShortcutFinger.Home3("Reparse potentially matching sentences: (Only reparse all sentences is sure to catch everything)"), () => _services.BackgroundTaskManager.Run(() => _services.LocalNoteUpdater.ReparseSentencesForVocab(vocab))),
                                    NoteCommandMenuItem.Running(ShortcutFinger.Home4, VocabCommands.RepopulatePartsOfSpeech, vocab),
                                    NoteCommandMenuItem.Running(ShortcutFinger.Home5, VocabCommands.AutogenerateCompoundParts, vocab)
                                 }),
            SpecMenuItem.Submenu(ShortcutFinger.Up2("Remove"),
                                 new List<SpecMenuItem>
                                 {
                                    SpecMenuItem.UICommand(ShortcutFinger.Home1("Explanation"), () => vocab.Explanation.Empty(), enabled: vocab.Explanation.HasValue()),
                                    SpecMenuItem.UICommand(ShortcutFinger.Home2("Long explanation"), () => vocab.ExplanationLong.Empty(), enabled: vocab.ExplanationLong.HasValue()),
                                    SpecMenuItem.UICommand(ShortcutFinger.Home3("Mnemonic"), () => vocab.Mnemonic.Empty(), enabled: vocab.Mnemonic.HasValue()),
                                    SpecMenuItem.UICommand(ShortcutFinger.Home4("Corpus answer"), () => vocab.CorpusAnswer.Empty(), enabled: vocab.CorpusAnswer.HasValue())
                                 })
         }
      );
   }

   SpecMenuItem BuildOpenHomonymsMenuSpec(VocabNote vocab, string title)
   {
      var readings = vocab.GetReadings();
      var items = new List<SpecMenuItem>();

      for(var i = 0; i < readings.Count; i++)
      {
         var reading = readings[i];
         items.Add(NoteListMenuItem.Showing(ShortcutFinger.FingerByPriorityOrder(i, $"Homonyms: {reading}"),
                                            $"Vocab read {reading}",
                                            () => Collection.Vocab.WithReading(reading)));
      }

      return SpecMenuItem.Submenu(title, items);
   }

   static SpecMenuItem? BuildOpenErgativeTwinsMenuSpec(string title, VocabNote vocab)
   {
      var ergativeTwinNotes = vocab.RelatedNotes.ErgativeTwins.Notes();
      if(ergativeTwinNotes.Count == 0)
         return null;

      return NoteListMenuItem.Showing(title, $"Ergative twins of {vocab.GetQuestion()}", () => ergativeTwinNotes);
   }

   SpecMenuItem BuildCreateCloneToFormMenuSpec(string title, VocabNote vocab)
   {
      var col = _services.CoreApp.Collection;
      //review: unlike every other command that creates a vocab (NoteCommandMenuItem.Creating), these items don't open the clone in the workspace.
      //   And they offer only the forms no vocab has as its text, though CloneToForm keeps the vocab's disambiguation, so 其れなり:x can't be
      //   created while 其れなり exists. Should they open the clone, and offer every form whose question, disambiguation included, no vocab has?
      var formsWithNoVocab = vocab.Forms.EnteredFormsSet()
                                  .Where(form => !col.Vocab.WithQuestion(form).Any())
                                  .ToList();

      var items = new List<SpecMenuItem>();
      for(var i = 0; i < formsWithNoVocab.Count; i++)
      {
         var form = formsWithNoVocab[i];
         items.Add(SpecMenuItem.UICommand(
                      ShortcutFinger.FingerByPriorityOrder(i, form),
                      () => VocabCommands.CloneToForm.CreateFrom(vocab, form),
                      enabled: _services.AnkiAvailability.IsAvailable));
      }

      return SpecMenuItem.Submenu(title, items);
   }

   static void CopyToClipboard(string text) => Dispatcher.UIThread.Invoke(() => new Window().Clipboard?.SetTextAsync(text).Wait());
}
