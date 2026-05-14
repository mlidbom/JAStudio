using System.Collections.Generic;
using System.Linq;
using Avalonia.Input.Platform;
using Avalonia.Threading;
using JAStudio.Anki;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.SysUtils;
using JAStudio.UI.Menus.UIAgnosticMenuStructure;
using JAStudio.UI.Utils;
using JAStudio.UI.Views;

// ReSharper disable once CheckNamespace
namespace JAStudio.UI.Menus;

/// <summary>
/// Vocab note-specific menu builders.
/// Corresponds to notes/vocab/main.py in Python.
/// </summary>
class VocabNoteMenus(Core.TemporaryServiceCollection services)
{
   readonly Core.TemporaryServiceCollection _services = services;

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
                                                            SpecMenuItem.Command(ShortcutFinger.Home1("Forms"), () => AnkiFacade.Browser.ExecuteLookup(_services.QueryBuilder().NotesLookup(vocab.Forms.AllListNotes()))),
                                                            SpecMenuItem.Command(ShortcutFinger.Home2("Compound parts"), () => AnkiFacade.Browser.ExecuteLookup(_services.QueryBuilder().VocabsLookupStrings(vocab.CompoundParts.All()))),
                                                            SpecMenuItem.Command(ShortcutFinger.Home3("In compounds"), () => AnkiFacade.Browser.ExecuteLookup(_services.QueryBuilder().NotesLookup(vocab.RelatedNotes.InCompounds()))),
                                                            SpecMenuItem.Command(ShortcutFinger.Home4("Synonyms"), () => AnkiFacade.Browser.ExecuteLookup(_services.QueryBuilder().NotesLookup(vocab.RelatedNotes.Synonyms.Notes()))),
                                                            SpecMenuItem.Command(ShortcutFinger.Home5("See also"), () => AnkiFacade.Browser.ExecuteLookup(_services.QueryBuilder().NotesLookup(vocab.RelatedNotes.SeeAlso.Notes()))),
                                                            BuildOpenHomonymsMenuSpec(vocab, ShortcutFinger.Up1("Homonyms")),
                                                            SpecMenuItem.Command(ShortcutFinger.Up2("Dependencies"), () => AnkiFacade.Browser.ExecuteLookup(_services.QueryBuilder().VocabDependenciesLookupQuery(vocab)))
                                                         }),
                                    SpecMenuItem.Submenu(ShortcutFinger.Home2("Sentences"),
                                                         new List<SpecMenuItem>
                                                         {
                                                            SpecMenuItem.Command(ShortcutFinger.Home1("Sentences I'm Studying"), () => AnkiFacade.Browser.ExecuteLookup(_services.QueryBuilder().NotesLookup(vocab.Sentences.Studying()))),
                                                            SpecMenuItem.Command(ShortcutFinger.Home2("Sentences"), () => AnkiFacade.Browser.ExecuteLookup(_services.QueryBuilder().NotesLookup(vocab.Sentences.All()))),
                                                            SpecMenuItem.Command(ShortcutFinger.Home3("Sentences with primary form"), () => AnkiFacade.Browser.ExecuteLookup(_services.QueryBuilder().NotesLookup(vocab.Sentences.WithPrimaryForm()))),
                                                            SpecMenuItem.Command(ShortcutFinger.Home4("Sentences with this word highlighted"), () => AnkiFacade.Browser.ExecuteLookup(_services.QueryBuilder().NotesLookup(vocab.Sentences.UserHighlighted()))),
                                                            SpecMenuItem.Command(ShortcutFinger.Home5("Potentially matching sentences"), () => AnkiFacade.Browser.ExecuteLookup(_services.QueryBuilder().PotentiallyMatchingSentencesForVocab(vocab))),
                                                            SpecMenuItem.Command(ShortcutFinger.Up1("Marked invalid in sentences"), () => AnkiFacade.Browser.ExecuteLookup(_services.QueryBuilder().NotesLookup(vocab.Sentences.InvalidIn())))
                                                         }),
                                    SpecMenuItem.Command(ShortcutFinger.Home3("Kanji"), () => AnkiFacade.Browser.ExecuteLookup(_services.QueryBuilder().KanjiInString(vocab.GetQuestion()))),
                                    BuildOpenErgativeTwinMenuSpec(ShortcutFinger.Home4("Ergative twin"), vocab)
                                 }.Where(it => it != null).ToList()!
            ),
            SpecMenuItem.Command(ShortcutFinger.Home2("Edit"), () => Dispatcher.UIThread.Invoke(() => new VocabEditorDialog(vocab).ShowNearCursor())),
            SpecMenuItem.Command(ShortcutFinger.Home3("Edit Matching Config"), () => OnEditVocabFlags(vocab)),
            SpecMenuItem.Submenu(ShortcutFinger.Home4("Create"),
                                 new List<SpecMenuItem>
                                 {
                                    BuildCreateCloneToFormMenuSpec(ShortcutFinger.Home1("Clone to form"), vocab),
                                    SpecMenuItem.Submenu(ShortcutFinger.Home2("Noun variations"),
                                                         new List<SpecMenuItem>
                                                         {
                                                            SpecMenuItem.Command(ShortcutFinger.Home1("する-verb"), () => vocab.Cloner.CreateSuruVerb()),
                                                            SpecMenuItem.Command(ShortcutFinger.Home2("します-verb"), () => vocab.Cloner.CreateShimasuVerb()),
                                                            SpecMenuItem.Command(ShortcutFinger.Home3("な-adjective"), () => vocab.Cloner.CreateNaAdjective()),
                                                            SpecMenuItem.Command(ShortcutFinger.Home4("の-adjective"), () => vocab.Cloner.CreateNoAdjective()),
                                                            SpecMenuItem.Command(ShortcutFinger.Up1("に-adverb"), () => vocab.Cloner.CreateNiAdverb()),
                                                            SpecMenuItem.Command(ShortcutFinger.Up2("と-adverb"), () => vocab.Cloner.CreateToAdverb())
                                                         }),
                                    SpecMenuItem.Submenu(ShortcutFinger.Home3("Verb variations"),
                                                         new List<SpecMenuItem>
                                                         {
                                                            SpecMenuItem.Command(ShortcutFinger.Home1("ます-form"), () => vocab.Cloner.CreateMasuForm()),
                                                            SpecMenuItem.Command(ShortcutFinger.Home2("て-form"), () => vocab.Cloner.CreateTeForm()),
                                                            SpecMenuItem.Command(ShortcutFinger.Home3("た-form"), () => vocab.Cloner.CreateTaForm()),
                                                            SpecMenuItem.Command(ShortcutFinger.Home4("ない-form"), () => vocab.Cloner.CreateNaiForm()),
                                                            SpecMenuItem.Command(ShortcutFinger.Home5($"え-stem/godan-imperative {vocab.Cloner.SuffixToEStemPreview("")}"), () => vocab.Cloner.SuffixToEStem("")),
                                                            SpecMenuItem.Command(ShortcutFinger.Up1("ば-form"), () => vocab.Cloner.CreateBaForm()),
                                                            SpecMenuItem.Command(ShortcutFinger.Up2("{receptive/passive}-form"), () => vocab.Cloner.CreateReceptiveForm()),
                                                            SpecMenuItem.Command(ShortcutFinger.Up3("causative"), () => vocab.Cloner.CreateCausativeForm()),
                                                            SpecMenuItem.Command(ShortcutFinger.Up4("imperative"), () => vocab.Cloner.CreateImperative()),
                                                            SpecMenuItem.Command(ShortcutFinger.Down1("Potential-godan"), () => vocab.Cloner.CreatePotentialGodan())
                                                         }),
                                    SpecMenuItem.Submenu(ShortcutFinger.Home4("Misc"),
                                                         new List<SpecMenuItem>
                                                         {
                                                            SpecMenuItem.Command(ShortcutFinger.Home1("く-form-of-い-adjective"), () => vocab.Cloner.CreateKuForm()),
                                                            SpecMenuItem.Command(ShortcutFinger.Home2("さ-form-of-い-adjective"), () => vocab.Cloner.CreateSaForm()),
                                                            SpecMenuItem.Command(ShortcutFinger.Home3("て-prefixed"), () => vocab.Cloner.CreateTePrefixedWord()),
                                                            SpecMenuItem.Command(ShortcutFinger.Home4("お-prefixed"), () => vocab.Cloner.CreateOPrefixedWord()),
                                                            SpecMenuItem.Command(ShortcutFinger.Home5("ん-suffixed"), () => vocab.Cloner.CreateNSuffixedWord()),
                                                            SpecMenuItem.Command(ShortcutFinger.Up1("か-suffixed"), () => vocab.Cloner.CreateKaSuffixedWord())
                                                         })
                                 }),
            SpecMenuItem.Submenu(ShortcutFinger.Home5("Copy"),
                                 new List<SpecMenuItem>
                                 {
                                    SpecMenuItem.Command(ShortcutFinger.Home1("Question"), () => CopyToClipboard(vocab.GetQuestion())),
                                    SpecMenuItem.Command(ShortcutFinger.Home2("Answer"), () => CopyToClipboard(vocab.GetAnswer())),
                                    SpecMenuItem.Command(ShortcutFinger.Home3("Definition (question:answer)"), () => CopyToClipboard($"{vocab.GetQuestion()}:{vocab.GetAnswer()}")),
                                    SpecMenuItem.Command(ShortcutFinger.Home4("Sentences: max 30"), () => CopyToClipboard(string.Join("\n", vocab.Sentences.All().Take(30).Select(s => s.Question.WithoutInvisibleSpace()))))
                                 }),
            SpecMenuItem.Submenu(ShortcutFinger.Up1("Misc"),
                                 new List<SpecMenuItem>
                                 {
                                    SpecMenuItem.Command(ShortcutFinger.Home1("Accept meaning"), () => vocab.User.Answer.Set(FormatVocabMeaning(vocab.GetAnswer())), enabled: !vocab.User.Answer.HasValue()),
                                    SpecMenuItem.Command(ShortcutFinger.Home2("Generate answer"), vocab.GenerateAndSetAnswer),
                                    SpecMenuItem.Command(ShortcutFinger.Home3("Reparse potentially matching sentences: (Only reparse all sentences is sure to catch everything)"), () => _services.BackgroundTaskManager.Run(() => _services.LocalNoteUpdater.ReparseSentencesForVocab(vocab))),
                                    SpecMenuItem.Command(ShortcutFinger.Home4("Repopulate TOS"), () => vocab.PartsOfSpeech.SetAutomaticallyFromDictionary()),
                                    SpecMenuItem.Command(ShortcutFinger.Home5("Autogenerate compounds"), () => vocab.CompoundParts.AutoGenerate())
                                 }),
            SpecMenuItem.Submenu(ShortcutFinger.Up2("Remove"),
                                 new List<SpecMenuItem>
                                 {
                                    SpecMenuItem.Command(ShortcutFinger.Home1("User explanation"), () => vocab.User.Explanation.Empty(), enabled: vocab.User.Explanation.HasValue()),
                                    SpecMenuItem.Command(ShortcutFinger.Home2("User explanation long"), () => vocab.User.ExplanationLong.Empty(), enabled: vocab.User.ExplanationLong.HasValue()),
                                    SpecMenuItem.Command(ShortcutFinger.Home3("User mnemonic"), () => vocab.User.Mnemonic.Empty(), enabled: vocab.User.Mnemonic.HasValue()),
                                    SpecMenuItem.Command(ShortcutFinger.Home4("User answer"), () => vocab.User.Answer.Empty(), enabled: vocab.User.Answer.HasValue())
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
         items.Add(SpecMenuItem.Command(
                      ShortcutFinger.FingerByPriorityOrder(i, $"Homonyms: {reading}"),
                      () => AnkiFacade.Browser.ExecuteLookup(_services.QueryBuilder().NotesLookup(_services.CoreApp.Collection.Vocab.WithReading(reading)))));
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

      return SpecMenuItem.Command(title, () => AnkiFacade.Browser.ExecuteLookup(_services.QueryBuilder().NotesLookup(ergativeTwinNotes)));
   }

   SpecMenuItem BuildCreateCloneToFormMenuSpec(string title, VocabNote vocab)
   {
      var col = _services.CoreApp.Collection;
      var formsWithNoVocab = vocab.Forms.AllSet()
                                  .Where(form => !col.Vocab.WithQuestion(form).Any())
                                  .ToList();

      var items = new List<SpecMenuItem>();
      for(var i = 0; i < formsWithNoVocab.Count; i++)
      {
         var form = formsWithNoVocab[i];
         items.Add(SpecMenuItem.Command(
                      ShortcutFinger.FingerByPriorityOrder(i, form),
                      () => vocab.Cloner.CloneToForm(form)));
      }

      return SpecMenuItem.Submenu(title, items);
   }

   // Action handlers

   void OnEditVocabFlags(VocabNote vocab)
   {
      Dispatcher.UIThread.Invoke(() => new VocabFlagsDialog(vocab, _services).ShowNearCursor());
   }

   static string FormatVocabMeaning(string meaning) => meaning.Replace(" SOURCE", "").Replace(", ", "/").Replace(" ", "-").ToLower().StripHtmlAndBracketMarkupAndNoiseCharacters();

   static void CopyToClipboard(string text)
   {
      Dispatcher.UIThread.Invoke(() =>
      {
         var topLevel = Avalonia.Application.Current?.ApplicationLifetime
                           is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                           ? desktop.MainWindow
                           : null;

         if(topLevel?.Clipboard != null)
         {
            topLevel.Clipboard.SetTextAsync(text).Wait();
         }
      });
   }
}
