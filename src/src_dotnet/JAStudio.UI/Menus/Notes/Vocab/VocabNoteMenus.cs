using System;
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
using JAStudio.Core.SysUtils;
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
                                                            NoteListMenuItem.Showing(ShortcutFinger.Home2("Compound parts"), $"Compound parts of {vocab.GetQuestion()}", () => Collection.Vocab.WithAnyFormIn(vocab.CompoundParts.All())),
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
                                                            NoteListMenuItem.Showing(ShortcutFinger.Home4("Sentences with this word highlighted"), $"Sentences with {vocab.GetQuestion()} highlighted", vocab.Sentences.UserHighlighted),
                                                            NoteListMenuItem.Showing(ShortcutFinger.Home5("Potentially matching sentences"), $"Sentences that may match {vocab.GetQuestion()}", () => Collection.Sentences.PotentiallyMatchingVocab(vocab)),
                                                            NoteListMenuItem.Showing(ShortcutFinger.Up1("Marked invalid in sentences"), $"Sentences where {vocab.GetQuestion()} is marked invalid", vocab.Sentences.InvalidIn)
                                                         }),
                                    NoteListMenuItem.Showing(ShortcutFinger.Home3("Kanji"), $"Kanji in {vocab.GetQuestion()}", () => KanjiIn(vocab.GetQuestion())),
                                    BuildOpenErgativeTwinMenuSpec(ShortcutFinger.Home4("Ergative twin"), vocab)
                                 }.Where(it => it != null).ToList()!
            ),
            SpecMenuItem.Command(ShortcutFinger.Home2("Edit"), () => WorkspaceWindow.EditNote(vocab)),
            SpecMenuItem.Command(ShortcutFinger.Home3("Text-to-speech audio..."), () => Dispatcher.UIThread.Invoke(() => TextToSpeechDialog.OpenFor(vocab, _services))),
            SpecMenuItem.Submenu(ShortcutFinger.Home4("Create"),
                                 new List<SpecMenuItem>
                                 {
                                    BuildCreateCloneToFormMenuSpec(ShortcutFinger.Home1("Clone to form"), vocab),
                                    SpecMenuItem.Submenu(ShortcutFinger.Home2("Noun variations"),
                                                         new List<SpecMenuItem>
                                                         {
                                                            CreateNoteCommand(ShortcutFinger.Home1("する-verb"), () => vocab.Cloner.CreateSuruVerb()),
                                                            CreateNoteCommand(ShortcutFinger.Home2("します-verb"), () => vocab.Cloner.CreateShimasuVerb()),
                                                            CreateNoteCommand(ShortcutFinger.Home3("な-adjective"), () => vocab.Cloner.CreateNaAdjective()),
                                                            CreateNoteCommand(ShortcutFinger.Home4("の-adjective"), () => vocab.Cloner.CreateNoAdjective()),
                                                            CreateNoteCommand(ShortcutFinger.Up1("に-adverb"), () => vocab.Cloner.CreateNiAdverb()),
                                                            CreateNoteCommand(ShortcutFinger.Up2("と-adverb"), () => vocab.Cloner.CreateToAdverb())
                                                         }),
                                    SpecMenuItem.Submenu(ShortcutFinger.Home3("Verb variations"),
                                                         new List<SpecMenuItem>
                                                         {
                                                            CreateNoteCommand(ShortcutFinger.Home1("ます-form"), () => vocab.Cloner.CreateMasuForm()),
                                                            CreateNoteCommand(ShortcutFinger.Home2("て-form"), () => vocab.Cloner.CreateTeForm()),
                                                            CreateNoteCommand(ShortcutFinger.Home3("た-form"), () => vocab.Cloner.CreateTaForm()),
                                                            CreateNoteCommand(ShortcutFinger.Home4("ない-form"), () => vocab.Cloner.CreateNaiForm()),
                                                            CreateNoteCommand(ShortcutFinger.Home5($"え-stem/godan-imperative {vocab.Cloner.SuffixToEStemPreview("")}"), () => vocab.Cloner.SuffixToEStem("")),
                                                            CreateNoteCommand(ShortcutFinger.Up1("ば-form"), () => vocab.Cloner.CreateBaForm()),
                                                            CreateNoteCommand(ShortcutFinger.Up2("{receptive/passive}-form"), () => vocab.Cloner.CreateReceptiveForm()),
                                                            CreateNoteCommand(ShortcutFinger.Up3("causative"), () => vocab.Cloner.CreateCausativeForm()),
                                                            CreateNoteCommand(ShortcutFinger.Up4("imperative"), () => vocab.Cloner.CreateImperative()),
                                                            CreateNoteCommand(ShortcutFinger.Down1("Potential-godan"), () => vocab.Cloner.CreatePotentialGodan())
                                                         }),
                                    SpecMenuItem.Submenu(ShortcutFinger.Home4("Misc"),
                                                         new List<SpecMenuItem>
                                                         {
                                                            CreateNoteCommand(ShortcutFinger.Home1("く-form-of-い-adjective"), () => vocab.Cloner.CreateKuForm()),
                                                            CreateNoteCommand(ShortcutFinger.Home2("さ-form-of-い-adjective"), () => vocab.Cloner.CreateSaForm()),
                                                            CreateNoteCommand(ShortcutFinger.Home3("て-prefixed"), () => vocab.Cloner.CreateTePrefixedWord()),
                                                            CreateNoteCommand(ShortcutFinger.Home4("お-prefixed"), () => vocab.Cloner.CreateOPrefixedWord()),
                                                            CreateNoteCommand(ShortcutFinger.Home5("ん-suffixed"), () => vocab.Cloner.CreateNSuffixedWord()),
                                                            CreateNoteCommand(ShortcutFinger.Up1("か-suffixed"), () => vocab.Cloner.CreateKaSuffixedWord())
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
                                    SpecMenuItem.UICommand(ShortcutFinger.Home1("Accept meaning"), () => vocab.User.Answer.Set(FormatVocabMeaning(vocab.GetAnswer())), enabled: !vocab.User.Answer.HasValue()),
                                    SpecMenuItem.UICommand(ShortcutFinger.Home2("Generate answer"), vocab.GenerateAndSetAnswer),
                                    SpecMenuItem.UICommand(ShortcutFinger.Home3("Reparse potentially matching sentences: (Only reparse all sentences is sure to catch everything)"), () => _services.BackgroundTaskManager.Run(() => _services.LocalNoteUpdater.ReparseSentencesForVocab(vocab))),
                                    SpecMenuItem.UICommand(ShortcutFinger.Home4("Repopulate TOS"), () => vocab.PartsOfSpeech.SetAutomaticallyFromDictionary()),
                                    SpecMenuItem.UICommand(ShortcutFinger.Home5("Autogenerate compounds"), () => vocab.CompoundParts.AutoGenerate())
                                 }),
            SpecMenuItem.Submenu(ShortcutFinger.Up2("Remove"),
                                 new List<SpecMenuItem>
                                 {
                                    SpecMenuItem.UICommand(ShortcutFinger.Home1("User explanation"), () => vocab.User.Explanation.Empty(), enabled: vocab.User.Explanation.HasValue()),
                                    SpecMenuItem.UICommand(ShortcutFinger.Home2("User explanation long"), () => vocab.User.ExplanationLong.Empty(), enabled: vocab.User.ExplanationLong.HasValue()),
                                    SpecMenuItem.UICommand(ShortcutFinger.Home3("User mnemonic"), () => vocab.User.Mnemonic.Empty(), enabled: vocab.User.Mnemonic.HasValue()),
                                    SpecMenuItem.UICommand(ShortcutFinger.Home4("User answer"), () => vocab.User.Answer.Empty(), enabled: vocab.User.Answer.HasValue())
                                 })
         }
      );
   }

   public SpecMenuItem CreateNoteCommand(string name, Func<JPNote> createNote, char? acceleratorKey = null, string? shortcut = null, bool enabled = true)
      => SpecMenuItem.Command(name,
                              () => WorkspaceWindow.OpenNote(createNote()),
                              acceleratorKey,
                              shortcut,
                              enabled: enabled);

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

   SpecMenuItem? BuildOpenErgativeTwinMenuSpec(string title, VocabNote vocab)
   {
      var ergativeTwinQuestion = vocab.RelatedNotes.ErgativeTwin.Get();
      if(string.IsNullOrEmpty(ergativeTwinQuestion))
         return null;

      var ergativeTwinNotes = _services.CoreApp.Collection.Vocab.WithQuestion(ergativeTwinQuestion);
      if(!ergativeTwinNotes.Any())
         return null;

      return NoteListMenuItem.Showing(title, $"Ergative twin of {vocab.GetQuestion()}", () => ergativeTwinNotes);
   }

   SpecMenuItem BuildCreateCloneToFormMenuSpec(string title, VocabNote vocab)
   {
      var col = _services.CoreApp.Collection;
      var formsWithNoVocab = vocab.Forms.EnteredFormsSet()
                                  .Where(form => !col.Vocab.WithQuestion(form).Any())
                                  .ToList();

      var items = new List<SpecMenuItem>();
      for(var i = 0; i < formsWithNoVocab.Count; i++)
      {
         var form = formsWithNoVocab[i];
         items.Add(SpecMenuItem.UICommand(
                      ShortcutFinger.FingerByPriorityOrder(i, form),
                      () => vocab.Cloner.CloneToForm(form)));
      }

      return SpecMenuItem.Submenu(title, items);
   }

   // Action handlers

   static string FormatVocabMeaning(string meaning) => meaning.Replace(" SOURCE", "").Replace(", ", "/").Replace(" ", "-").ToLower().StripHtmlAndBracketMarkupAndNoiseCharacters();

   static void CopyToClipboard(string text) => Dispatcher.UIThread.Invoke(() => new Window().Clipboard?.SetTextAsync(text).Wait());
}
