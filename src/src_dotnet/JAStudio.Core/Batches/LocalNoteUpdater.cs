using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JAStudio.Core.Configuration;
using JAStudio.Core.LanguageServices.JamdictEx;
using JAStudio.Core.LanguageServices.JanomeEx.Tokenizing.PreProcessingStage;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;
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
      scope.RunBatch(_sentences.All().ToList(), it => { it.UpdateGeneratedData(); }, "Updating sentences");
   }

   public void UpdateKanji()
   {
      using var scope = _taskRunner.Current("Updating kanji");
      scope.RunBatch(_kanji.All().ToList(), it => { it.UpdateGeneratedData(); }, "Updating kanji");
   }

   public void UpdateVocab()
   {
      using var scope = _taskRunner.Current("Updating vocab");
      scope.RunBatch(_vocab.All().ToList(), it => { it.UpdateGeneratedData(); }, "Updating vocab");
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
      scope.RunBatch(_sentences.All().ToList(), TagSentence, "Tagging sentence notes");
   }

   public void TagVocabMetadata()
   {
      void TagNote(VocabNote vocab)
      {
         vocab.Tags.Toggle(Tags.Vocab.Matching.Uses.RequiredPrefix, vocab.MatchingConfiguration.ConfigurableRules.RequiredPrefix.Any());
         vocab.Tags.Toggle(Tags.Vocab.Matching.Uses.PrefixIsNot, vocab.MatchingConfiguration.ConfigurableRules.PrefixIsNot.Any());
         vocab.Tags.Toggle(Tags.Vocab.Matching.Uses.SuffixIsNot, vocab.MatchingConfiguration.ConfigurableRules.SuffixIsNot.Any());
         vocab.Tags.Toggle(Tags.Vocab.Matching.Uses.SurfaceIsNot, vocab.MatchingConfiguration.ConfigurableRules.SurfaceIsNot.Any());
         vocab.Tags.Toggle(Tags.Vocab.IsIchidanHidingGodanPotential, IchidanGodanPotentialOrImperativeHybridSplitter.IsIchidanHidingGodan(vocab));
      }

      using var scope = _taskRunner.Current("Tagging vocab");
      scope.RunBatch(_vocab.All().ToList(), TagNote, "Tag vocab notes");
   }

   public void TagKanjiMetadata()
   {
      var primaryReadingPattern = new Regex("<primary>(.*?)</primary>", RegexOptions.Compiled);
      var knownKanji = _kanji.All()
                             .Where(kanji => kanji.IsStudying())
                             .Select(kanji => kanji.GetQuestion())
                             .ToHashSet();

      void TagKanji(KanjiNote kanji)
      {
         var vocabWithKanjiInMainForm = _vocab.WithKanjiInMainForm(kanji);
         var vocabWithKanjiInAnyForm = _vocab.WithKanjiInAnyForm(kanji);

         var isRadical = _kanji.WithRadical(kanji.GetQuestion()).Any();
         kanji.Tags.Toggle(Tags.Kanji.InVocabMainForm, vocabWithKanjiInMainForm.Any());
         kanji.Tags.Toggle(Tags.Kanji.InAnyVocabForm, vocabWithKanjiInAnyForm.Any());

         var studyingReadingVocab = vocabWithKanjiInMainForm
                                   .Where(voc => voc.IsStudying(CardTypes.Reading))
                                   .ToList();
         kanji.Tags.Toggle(Tags.Kanji.WithStudyingVocab, studyingReadingVocab.Any());

         var readingsHtml = $"{kanji.ReadingOnHtml.Value} {kanji.ReadingKunHtml.Value} {kanji.ReadingNanHtml.Value}";
         var primaryReadings = primaryReadingPattern.Matches(readingsHtml)
                                                    .Select(m => m.Groups[1].Value)
                                                    .ToList();
         kanji.Tags.Toggle(Tags.Kanji.WithNoPrimaryReadings, !primaryReadings.Any());

         var primaryOnReadings = primaryReadingPattern.Matches(kanji.ReadingOnHtml.Value)
                                                      .Select(m => m.Groups[1].Value)
                                                      .ToList();
         var nonPrimaryOnReadings = kanji.ReadingsOn
                                         .Where(reading => !primaryReadings.Contains(reading))
                                         .ToList();

         kanji.Tags.Toggle(Tags.Kanji.WithNoPrimaryOnReadings, !primaryOnReadings.Any());

         bool ReadingIsInVocabReadings(string kanjiReading, VocabNote voc)
         {
            return voc.GetReadings().Any(vocabReading =>
                                            ReadingInVocabReading(kanji, kanjiReading, vocabReading, voc.GetQuestion()));
         }

         bool HasVocabWithReading(string kanjiReading)
         {
            return vocabWithKanjiInMainForm.Any(voc =>
                                                   voc.GetReadings().Any(vocabReading =>
                                                                            ReadingInVocabReading(kanji, kanjiReading, vocabReading, voc.GetQuestion())));
         }

         bool VocabHasOnlyKnownKanji(VocabNote voc) => voc.Kanji.ExtractAllKanji().All(knownKanji.Contains);

         bool HasVocabWithReadingAndNoUnknownKanji(string kanjiReading)
         {
            return vocabWithKanjiInMainForm.Any(voc =>
                                                   ReadingIsInVocabReadings(kanjiReading, voc) && VocabHasOnlyKnownKanji(voc));
         }

         kanji.Tags.Toggle(Tags.Kanji.WithVocabWithPrimaryOnReading,
                           primaryOnReadings.Any() && HasVocabWithReading(primaryOnReadings[0]));

         bool HasStudyingVocabWithReading(string kanjiReading)
         {
            return studyingReadingVocab.Any(voc =>
                                               voc.GetReadings().Any(vocabReading =>
                                                                        ReadingInVocabReading(kanji, kanjiReading, vocabReading, voc.GetQuestion())));
         }

         kanji.Tags.Toggle(Tags.Kanji.WithStudyingVocabWithPrimaryOnReading,
                           primaryOnReadings.Any() && HasStudyingVocabWithReading(primaryOnReadings[0]));
         kanji.Tags.Toggle(Tags.Kanji.HasStudyingVocabForEachPrimaryReading,
                           primaryReadings.Any() && primaryReadings.All(HasStudyingVocabWithReading));
         kanji.Tags.Toggle(Tags.Kanji.HasPrimaryReadingWithNoStudyingVocab,
                           primaryReadings.Any() && studyingReadingVocab.Any() &&
                           primaryReadings.Any(reading => !HasStudyingVocabWithReading(reading)));
         kanji.Tags.Toggle(Tags.Kanji.HasNonPrimaryOnReadingVocab,
                           nonPrimaryOnReadings.Any(HasVocabWithReading));
         kanji.Tags.Toggle(Tags.Kanji.HasNonPrimaryOnReadingVocabWithOnlyKnownKanji,
                           nonPrimaryOnReadings.Any(HasVocabWithReadingAndNoUnknownKanji));

         var allReadings = kanji.ReadingsClean;

         bool VocabMatchesPrimaryReading(VocabNote vocab)
         {
            return primaryReadings.Any(pr =>
                                          vocab.GetReadings().Any(vr => vr.Contains(pr)));
         }

         bool VocabMatchesReading(VocabNote vocab)
         {
            return allReadings.Any(r =>
                                      vocab.GetReadings().Any(vr => vr.Contains(r)));
         }

         kanji.Tags.Toggle(Tags.Kanji.HasStudyingVocabWithNoMatchingPrimaryReading,
                           studyingReadingVocab.Any(vocab =>
                                                       !VocabMatchesPrimaryReading(vocab) && VocabMatchesReading(vocab)));

         kanji.Tags.Toggle(Tags.Kanji.IsRadical, isRadical);
         kanji.Tags.Toggle(Tags.Kanji.IsRadicalPurely, isRadical && !vocabWithKanjiInAnyForm.Any());
         kanji.Tags.Toggle(Tags.Kanji.IsRadicalSilent, isRadical && !primaryReadings.Any());
      }

      void TagHasSingleKanjiVocabWithReadingDifferentFromKanjiPrimaryReading(KanjiNote kanji)
      {
         var vocabs = _vocab.WithKanjiInMainForm(kanji);
         var singleKanjiVocab = vocabs.Where(v => v.GetQuestion() == kanji.GetQuestion()).ToList();
         kanji.Tags.Toggle(Tags.Kanji.WithSingleKanjiVocab, singleKanjiVocab.Any());

         if(singleKanjiVocab.Any())
         {
            var readingsHtml = $"{kanji.ReadingOnHtml.Value} {kanji.ReadingKunHtml.Value} {kanji.ReadingNanHtml.Value}";
            var primaryReadings = primaryReadingPattern.Matches(readingsHtml)
                                                       .Select(m => m.Groups[1].Value)
                                                       .ToList();

            kanji.Tags.Unset(Tags.Kanji.WithSingleKanjiVocabWithDifferentReading);
            kanji.Tags.Unset(Tags.Kanji.WithStudyingSingleKanjiVocabWithDifferentReading);

            foreach(var vocab in singleKanjiVocab)
            {
               foreach(var reading in vocab.GetReadings())
               {
                  if(!primaryReadings.Contains(reading))
                  {
                     kanji.Tags.Set(Tags.Kanji.WithSingleKanjiVocabWithDifferentReading);
                  }
               }
            }
         }
      }

      using var scope = _taskRunner.Current("Tagging kanji");
      var allKanji = _kanji.All().ToList();
      scope.RunBatch(allKanji, TagKanji, "Tagging kanji with studying metadata");
      scope.RunBatch(allKanji, TagHasSingleKanjiVocabWithReadingDifferentFromKanjiPrimaryReading, "Tagging kanji with single kanji vocab");
   }

   public void ReparseAllSentences()
   {
      using(_taskRunner.Current("Reparse all sentences"))
      {
         ReparseSentences(_sentences.All().ToList());
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
      sentences = sentences.OrderBy(_ => random.Next()).ToList();

      using var runner = _taskRunner.Current("Reparse Sentences");
      runner.RunBatch(sentences,
                      ReparseSentence,
                      "Reanalysing sentences.",
                      threads: ThreadCount.WithThreads((int)_config.ReanalysisThreads.Value));
   }

   public void ReparseSentencesForVocab(VocabNote vocab)
   {
      vocab.UpdateGeneratedData();
      using var scope = _taskRunner.Current("Reanalysing sentences for vocab");

      var sentences = scope.RunIndeterminate(
         "Fetching sentences to reparse",
         () => _sentences.PotentiallyMatchingVocab(vocab).ToHashSet());

      sentences.UnionWith(vocab.Sentences.All());
      ReparseSentences(sentences.ToList());
   }

   public void ReparseMatchingSentences(string questionSubstring)
   {
      var sentencesToUpdate = _sentences.SentencesWithSubstring(questionSubstring);
      ReparseSentences(sentencesToUpdate.ToList());
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

      scope.RunBatch(_kanji.All().ToList(), it => { it.Flush(); }, "Flushing kanji notes");
      scope.RunBatch(_vocab.All().ToList(), it => { it.Flush(); }, "Flushing vocab notes");
      scope.RunBatch(_sentences.All().ToList(), it => { it.Flush(); }, "Flushing sentence notes");
   }

   public void WriteFileSystemRepository()
   {
      using var scope = _taskRunner.Current("Writing file system repository");

      Task.WaitAll(scope.RunBatchAsync(_kanji.All(), _fileSystemNoteRepository.Save, "Writing kanji"),
                   scope.RunBatchAsync(_vocab.All(), _fileSystemNoteRepository.Save, "Writing vocab"),
                   scope.RunBatchAsync(_sentences.All(), _fileSystemNoteRepository.Save, "Writing sentences"));
   }
}
