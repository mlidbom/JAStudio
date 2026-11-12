using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Compze.Contracts;
using JAStudio.Core.Configuration;
using JAStudio.Core.LanguageServices.JamdictEx;
using JAStudio.Core.LanguageServices.JanomeEx.Tokenizing.PreProcessingStage;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Tagging;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Rules;
using JAStudio.Core.Storage;
using JAStudio.Core.SysUtils;
using JAStudio.Core.TaskRunners;

namespace JAStudio.Core.Batches;

public class LocalNoteUpdater
{
   readonly TaskRunner _taskRunner;
   readonly VocabCollection _vocab;
   readonly KanjiCollection _kanji;
   readonly SentenceCollection _sentences;
   readonly JapaneseConfig _config;
   readonly DictLookup _dictLookup;
   readonly VocabNoteFactory _vocabNoteFactory;
   readonly FileSystemNoteRepository _fileSystemNoteRepository;

   internal LocalNoteUpdater(TaskRunner taskRunner, VocabCollection vocab, KanjiCollection kanji, SentenceCollection sentences, JapaneseConfig config, DictLookup dictLookup, VocabNoteFactory vocabNoteFactory, FileSystemNoteRepository fileSystemNoteRepository)
   {
      _taskRunner = taskRunner;
      _vocab = vocab;
      _kanji = kanji;
      _sentences = sentences;
      _config = config;
      _dictLookup = dictLookup;
      _vocabNoteFactory = vocabNoteFactory;
      _fileSystemNoteRepository = fileSystemNoteRepository;
   }

   /// <summary>
   /// The threads each batch of notes runs on, as many as <see cref="JapaneseConfig.ReanalysisThreads"/> says. A batch can run on many, since what it<br/>
   /// does to a note changes that note only, as <see cref="JPNote.UpdateGeneratedData"/> does. The batches themselves run one after another.
   /// </summary>
   ThreadCount TheReanalysisThreads => ThreadCount.WithThreads((int)_config.ReanalysisThreads.Value);

   public void UpdateAll()
   {
      using(_taskRunner.Current("Updating everything but sentence reanalysis"))
      {
         UpdateSentences();
         UpdateKanji();
         UpdateVocab();
         TagNoteMetadata();
      }
   }

   /// <summary>
   /// Rebuilds everything the corpus derives from its notes: reparses every sentence, then updates every note's generated data.
   /// </summary>
   /// <remarks>
   /// One run reaches the result a second run would leave unchanged, because nothing in a note's generated data is read by parsing. If something<br/>
   /// is, the reparse reads it as the previous run left it, and the specification <c>When_running_a_full_rebuild</c> fails: then update the vocab's<br/>
   /// generated data before the reparse too, as <see cref="ReparseSentencesForVocab"/> does for one vocab.
   /// </remarks>
   public void FullRebuild()
   {
      using(_taskRunner.Current("Full rebuild"))
      {
         ReparseAllSentences();
         UpdateAll();
      }
   }

   public void UpdateSentences()
   {
      using var scope = _taskRunner.Current("Updating sentences");
      scope.RunBatch([.. _sentences.All()], it => { it.UpdateGeneratedData(); }, "Updating sentences", TheReanalysisThreads);
   }

   public void UpdateKanji()
   {
      using var scope = _taskRunner.Current("Updating kanji");
      scope.RunBatch([.. _kanji.All()], it => { it.UpdateGeneratedData(); }, "Updating kanji", TheReanalysisThreads);
   }

   public void UpdateVocab()
   {
      using var scope = _taskRunner.Current("Updating vocab");
      scope.RunBatch([.. _vocab.All()], it => { it.UpdateGeneratedData(); }, "Updating vocab", TheReanalysisThreads);
   }

   public void TagNoteMetadata()
   {
      using(_taskRunner.Current("Tagging notes"))
      {
         TagKanjiMetadata();
         TagVocabMetadata();
         TagSentenceMetadata();
      }
   }

   public void TagSentenceMetadata()
   {
      void TagSentence(SentenceNote sentence)
      {
         sentence.Tags.Toggle(Tags.Sentence.Uses.IncorrectMatches, sentence.Configuration.IncorrectMatches.Get().Any());
         sentence.Tags.Toggle(Tags.Sentence.Uses.HiddenMatches, sentence.Configuration.HiddenMatches.Get().Any());
      }

      using var scope = _taskRunner.Current("Tagging sentence notes");
      scope.RunBatch([.. _sentences.All()], TagSentence, "Tagging sentence notes", TheReanalysisThreads);
   }

   public void TagVocabMetadata()
   {
      void TagNote(VocabNote vocab)
      {
         vocab.Tags.Toggle(Tags.Vocab.Matching.Uses.RequiredPrefix, vocab.MatchingConfiguration.Rules.TextsOf(MatchingRules.RequiredPrefix).Any());
         vocab.Tags.Toggle(Tags.Vocab.Matching.Uses.PrefixIsNot, vocab.MatchingConfiguration.Rules.TextsOf(MatchingRules.PrefixIsNot).Any());
         vocab.Tags.Toggle(Tags.Vocab.Matching.Uses.SuffixIsNot, vocab.MatchingConfiguration.Rules.TextsOf(MatchingRules.SuffixIsNot).Any());
         vocab.Tags.Toggle(Tags.Vocab.Matching.Uses.SurfaceIsNot, vocab.MatchingConfiguration.Rules.TextsOf(MatchingRules.SurfaceIsNot).Any());
         vocab.Tags.Toggle(Tags.Vocab.IsIchidanHidingGodanPotential, IchidanGodanPotentialOrImperativeHybridSplitter.IsIchidanHidingGodan(vocab));
      }

      using var scope = _taskRunner.Current("Tagging vocab");
      scope.RunBatch([.. _vocab.All()], TagNote, "Tag vocab notes", TheReanalysisThreads);
   }

   /// <summary>
   /// Tags each kanji with what the corpus says about it: whether vocab use it, whether it's a radical of other kanji, and how its<br/>
   /// primary readings relate to its vocab's readings. Derived data, kept as tags for now so they can be searched by (see <c>corpus-data-model.md</c>).
   /// </summary>
   public void TagKanjiMetadata()
   {
      void TagKanji(KanjiNote kanji)
      {
         var vocabWithKanjiInMainForm = _vocab.WithKanjiInMainForm(kanji);
         var vocabWithKanjiInAnyForm = _vocab.WithKanjiInAnyForm(kanji);

         var isRadical = _kanji.WithRadical(kanji.GetQuestion()).Any();
         kanji.Tags.Toggle(Tags.Kanji.InVocabMainForm, vocabWithKanjiInMainForm.Any());
         kanji.Tags.Toggle(Tags.Kanji.InAnyVocabForm, vocabWithKanjiInAnyForm.Any());

         var primaryReadings = kanji.Readings.PrimaryKana();
         kanji.Tags.Toggle(Tags.Kanji.WithNoPrimaryReadings, !primaryReadings.Any());

         var primaryOnReadings = kanji.Readings.On.PrimaryKana();
         var nonPrimaryOnReadings = kanji.Readings.On.AllKana()
                                         .Where(reading => !primaryReadings.Contains(reading))
                                         .ToList();

         kanji.Tags.Toggle(Tags.Kanji.WithNoPrimaryOnReadings, !primaryOnReadings.Any());

         bool HasVocabWithReading(string kanjiReading)
         {
            return vocabWithKanjiInMainForm.Any(voc =>
                                                   voc.GetReadings().Any(vocabReading =>
                                                                            ReadingInVocabReading(kanji, kanjiReading, vocabReading, voc.GetQuestion())));
         }

         kanji.Tags.Toggle(Tags.Kanji.WithVocabWithPrimaryOnReading,
                           primaryOnReadings.Any() && HasVocabWithReading(primaryOnReadings[0]));
         kanji.Tags.Toggle(Tags.Kanji.HasNonPrimaryOnReadingVocab,
                           nonPrimaryOnReadings.Any(HasVocabWithReading));

         kanji.Tags.Toggle(Tags.Kanji.IsRadical, isRadical);
         kanji.Tags.Toggle(Tags.Kanji.IsRadicalPurely, isRadical && !vocabWithKanjiInAnyForm.Any());
         kanji.Tags.Toggle(Tags.Kanji.IsRadicalSilent, isRadical && !primaryReadings.Any());
      }

      void TagHasSingleKanjiVocabWithReadingDifferentFromKanjiPrimaryReading(KanjiNote kanji)
      {
         var singleKanjiVocab = _vocab.WithKanjiInMainForm(kanji).Where(v => v.GetQuestion() == kanji.GetQuestion()).ToList();
         var primaryReadings = kanji.Readings.PrimaryKana();

         kanji.Tags.Toggle(Tags.Kanji.WithSingleKanjiVocab, singleKanjiVocab.Any());
         kanji.Tags.Toggle(Tags.Kanji.WithSingleKanjiVocabWithDifferentReading,
                           singleKanjiVocab.Any(vocab => vocab.GetReadings().Any(reading => !primaryReadings.Contains(reading))));
      }

      using var scope = _taskRunner.Current("Tagging kanji");
      var allKanji = _kanji.All().ToList();
      scope.RunBatch(allKanji, TagKanji, "Tagging kanji", TheReanalysisThreads);
      scope.RunBatch(allKanji, TagHasSingleKanjiVocabWithReadingDifferentFromKanjiPrimaryReading, "Tagging kanji with single kanji vocab", TheReanalysisThreads);
   }

   public void ReparseAllSentences()
   {
      using(_taskRunner.Current("Reparse all sentences"))
      {
         ReparseSentences([.. _sentences.All()]);
      }
   }

   public bool ReadingInVocabReading(KanjiNote kanji, string kanjiReading, string vocabReading, string vocabForm)
   {
      vocabForm = vocabForm.StripHtmlAndBracketMarkupAndNoiseCharacters();
      if(vocabForm.StartsWith(kanji.GetQuestion()))
      {
         return vocabReading.StartsWith(kanjiReading);
      }

      if(vocabForm.EndsWith(kanji.GetQuestion()))
      {
         return vocabReading.EndsWith(kanjiReading);
      }

      return vocabReading.Length >= 2
                ? vocabReading.Substring(1, vocabReading.Length - 2).Contains(kanjiReading)
                : kanjiReading == "";
   }

   public void ReparseSentences(List<SentenceNote> sentences)
   {
      void ReparseSentence(SentenceNote sentence)
      {
         sentence.UpdateParsedWords(force: true);
      }

      // Shuffle to get accurate time estimations
      var random = new Random();
      sentences = [.. sentences.OrderBy(_ => random.Next())];

      using var runner = _taskRunner.Current("Reparse Sentences");
      runner.RunBatch(sentences,
                      ReparseSentence,
                      "Reanalysing sentences.",
                      threads: TheReanalysisThreads);
   }

   /// <summary>
   /// Reparses the sentences that may contain <paramref name="vocab"/>.<br/>
   /// The vocab's generated data is updated before the reparse, which uses its forms,<br/>
   /// and again after it, because its stored sentence count depends on which sentences the reparse matched.
   /// </summary>
   public void ReparseSentencesForVocab(VocabNote vocab)
   {
      vocab.UpdateGeneratedData();
      using var scope = _taskRunner.Current("Reanalysing sentences for vocab");

      var sentences = scope.RunIndeterminate(
         "Fetching sentences to reparse",
         () => _sentences.PotentiallyMatchingVocab(vocab).ToHashSet());

      sentences.UnionWith(vocab.Sentences.All());
      ReparseSentences([.. sentences]);
      vocab.UpdateGeneratedData();
   }

   /// <summary>
   /// <see cref="ReparseSentencesForVocab"/> without the progress dialog, for a caller that shows its progress itself,<br/>
   /// such as an editor that reparses after every change, which would otherwise pop the dialog up every time.
   /// </summary>
   public void ReparseSentencesForVocabWithoutProgressDialog(VocabNote vocab)
   {
      using var hiddenScope = _taskRunner.Current("Reanalysing sentences for vocab without a progress dialog", forceHide: true);
      ReparseSentencesForVocab(vocab);
   }

   public void ReparseMatchingSentences(string questionSubstring)
   {
      var sentencesToUpdate = _sentences.SentencesWithSubstring(questionSubstring);
      ReparseSentences([.. sentencesToUpdate]);
   }

   /// <summary>
   /// The vocab <paramref name="question"/> names, or else one created for it from the dictionary (<see cref="VocabNoteFactory.CreateWithDictionary"/>), with<br/>
   /// the sentences that may contain it reparsed. <paramref name="question"/> has no disambiguation: a vocab that needs one is made by cloning another.
   /// </summary>
   public CreatedOrExistingVocab TheVocabNamedOrCreatedFromTheDictionary(WordReference question)
   {
      Contract.Argument.Assert(!question.IsDisambiguated, () => $"{question.DisambiguationName} has a disambiguation, and a vocab made from the dictionary has none.");
      return _vocab.NamedExactlyByOrCreated(question, CreatedFromTheDictionary);

      VocabNote CreatedFromTheDictionary()
      {
         var created = _vocabNoteFactory.CreateWithDictionary(question.Text);
         ReparseSentencesForVocab(created);
         return created;
      }
   }

   public void CreateMissingVocabWithDictionaryEntries()
   {
      using var scope = _taskRunner.Current("Creating vocab notes for parsed words with no vocab notes");

      var dictionaryWordsWithNoVocab = scope.RunIndeterminate(
         "Fetching parsed words with no vocab notes from parsing results",
         () => _sentences.All()
                         .SelectMany(sentence => sentence.GetParsingResult().ParsedWords
                                                         .Where(word => word.VocabId == null)
                                                         .Select(word => word.ParsedForm))
                         .Distinct()
                         .Select(form => _dictLookup.LookupWord(form))
                         .Where(result => result.FoundWords())
                         .OrderBy(result => result.PrioritySpec().Priority)
                         .ToList());

      void CreateVocabIfNotAlreadyCreated(DictLookupResult result)
      {
         // We may well have created a vocab that provides this form already...
         if(!_vocab.WithForm(result.Word).Any())
         {
            _vocabNoteFactory.CreateWithDictionary(result.Word);
         }
      }

      scope.RunBatch(dictionaryWordsWithNoVocab, CreateVocabIfNotAlreadyCreated, "Creating vocab notes");
   }

   public void RegenerateJamdictVocabAnswers()
   {
      using var scope = _taskRunner.Current("Regenerating vocab source answers from dictionary");
      var vocabNotes = _vocab.All().ToList();
      scope.RunBatch(vocabNotes, it => { it.GenerateAndSetAnswer(); }, "Regenerating vocab source answers from dictionary");
   }

   public void ForceFlushAllNotes()
   {
      using var scope = _taskRunner.Current("Flushing all notes");

      scope.RunBatch([.. _kanji.All()], it => { it.Flush(); }, "Flushing kanji notes");
      scope.RunBatch([.. _vocab.All()], it => { it.Flush(); }, "Flushing vocab notes");
      scope.RunBatch([.. _sentences.All()], it => { it.Flush(); }, "Flushing sentence notes");
   }

   public void WriteFileSystemRepository()
   {
      using var scope = _taskRunner.Current("Writing file system repository");

      Task.WaitAll(scope.RunBatchAsync(_kanji.All(), _fileSystemNoteRepository.Save, "Writing kanji"),
                   scope.RunBatchAsync(_vocab.All(), _fileSystemNoteRepository.Save, "Writing vocab"),
                   scope.RunBatchAsync(_sentences.All(), _fileSystemNoteRepository.Save, "Writing sentences"));
   }
}
