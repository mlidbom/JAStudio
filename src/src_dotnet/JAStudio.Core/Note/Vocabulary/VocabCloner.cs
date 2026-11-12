using Compze.Internals.SystemCE;
using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices;
using JAStudio.Core.Storage.Dto;
using JAStudio.Core.Note.Tagging;
using JAStudio.Core.Storage.Converters;

namespace JAStudio.Core.Note.Vocabulary;

/// <summary>
/// Creates vocab from a vocab: copies of it, such as one with another of its forms as its question, and versions derived from it, such as its て-form.<br/>
/// No two vocab share a question, so when the vocab to create is there already, that vocab is given back, unchanged, rather than a duplicate created,<br/>
/// and the <see cref="CreatedOrExistingVocab"/> given back says which of the two it is.
/// </summary>
public class VocabCloner
{
   public VocabCloner(VocabNote note) => Note = note;

   VocabNote Note { get; }

   /// <summary>
   /// The vocab <paramref name="question"/> names exactly, if there is one, or else the vocab <paramref name="create"/> creates<br/>
   /// (<see cref="JAStudio.Core.Note.Collection.VocabCollection.NamedExactlyByOrCreated"/>).
   /// </summary>
   CreatedOrExistingVocab TheVocabNamedOrCreated(WordReference question, Func<VocabNote> create) => Note.Services.Collection.Vocab.NamedExactlyByOrCreated(question, create);

   public CreatedOrExistingVocab PrefixToDictionaryForm(string prefix) => CreatePostfixPrefixVersion(prefix, [PartOfSpeech.Expression], isPrefix: true);

   public CreatedOrExistingVocab PrefixToChopped(string prefix, int chopCharacters) => CreatePostfixPrefixVersion(prefix, [PartOfSpeech.Expression], isPrefix: true, chopOffCharacters: chopCharacters);

   public string PrefixToChoppedPreview(string formPrefix, int chopCharacters) => Note.GetQuestion().Length > chopCharacters
                                                                                     ? formPrefix + Note.GetQuestion().Substring(chopCharacters)
                                                                                     : "Vocab to short";

   public CreatedOrExistingVocab CreateSuffixVersion(string suffix) => CreatePostfixPrefixVersion(suffix, [PartOfSpeech.Expression]);

   /// <summary>
   /// The version of the vocab with <paramref name="addendum"/> added to its question, as a suffix or, with <paramref name="isPrefix"/>, a prefix, after chopping<br/>
   /// <paramref name="chopOffCharacters"/> off that end: created with <paramref name="partsOfSpeech"/>, then finished by <paramref name="finish"/>, unless it exists already.
   /// </summary>
   CreatedOrExistingVocab CreatePostfixPrefixVersion(string addendum, IEnumerable<PartOfSpeech> partsOfSpeech, bool isPrefix = false, bool setCompounds = true, int chopOffCharacters = 0,
                                        Action<VocabNote>? finish = null)
   {
      return TheVocabNamedOrCreated(new WordReference(AppendPrependAddendum(Note.GetQuestion())), Create);

      VocabNote Create()
      {
         var newReadings = Note.GetReadings().Select(AppendPrependAddendum).ToList();
         var newVocab = CreateNewVocabCopyingTagsRegisterAndMatchingConfiguration(AppendPrependAddendum(Note.GetQuestion()), Note.GetAnswer(), newReadings);

         if(setCompounds)
         {
            var compounds = isPrefix
                               ? new List<WordReference> { new(addendum), Note.Question.Reference }
                               : new List<WordReference> { Note.Question.Reference, new(addendum) };
            newVocab.CompoundParts.SetPrimary(compounds);
         }

         newVocab.PartsOfSpeech.Set(partsOfSpeech);
         newVocab.Forms.SetEnteredForms(Note.Forms.EnteredFormsSet().Select(AppendPrependAddendum).Select(EnteredForm.NotOwned));
         finish?.Invoke(newVocab);
         return newVocab;
      }

      string AppendPrependAddendum(string baseStr)
      {
         if(!isPrefix)
         {
            return chopOffCharacters == 0
                      ? baseStr + addendum
                      : baseStr.Substring(0, baseStr.Length - chopOffCharacters) + addendum;
         }

         return addendum + baseStr.Substring(chopOffCharacters);
      }
   }

   /// <summary>Creates a vocab with the question, answer and readings given, and the vocab's tags, register and matching configuration.</summary>
   VocabNote CreateNewVocabCopyingTagsRegisterAndMatchingConfiguration(string question, string answer, List<string> readings)
   {
      var clone = Note.Services.VocabNoteFactory.CreateWithCorpusAnswer(question, answer, readings);
      CopyVocabTagsTo(clone);
      clone.Register.OverwriteWith(Note.Register);
      clone.MatchingConfiguration.OverwriteWith(Note.MatchingConfiguration);
      return clone;
   }

   void CopyVocabTagsTo(VocabNote target)
   {
      foreach(var tag in Note.Tags.Where(t => t.Name.StartsWithOrdinal(Tags.Vocab.Root)))
      {
         target.Tags.Set(tag);
      }
   }

   /// <summary>
   /// Gives <paramref name="clone"/> the vocab's source tags: a clone is a copy of the vocab, its source data included, so its content<br/>
   /// comes from where the vocab's came from. A version derived from the vocab, such as its suffix version, is our own instead (<see cref="VocabNoteFactory.CreateWithCorpusAnswer"/>).
   /// </summary>
   void CopySourceTagsTo(VocabNote clone)
   {
      foreach(var tag in Note.Tags.Where(it => it.Name.StartsWithOrdinal(Tags.Source.Folder)))
         clone.Tags.Set(tag);
   }

   /// <summary>
   /// A copy of the vocab with <paramref name="form"/>, one of its forms, as its question, its disambiguation kept, such as 其れなり from それなり,<br/>
   /// unless a vocab has that question already.
   /// </summary>
   public CreatedOrExistingVocab CloneToForm(string form) => CloneWithTheQuestionUnlessAVocabHasIt(new WordReference(form, Note.Question.Disambiguation));

   /// <summary>
   /// A copy of the vocab whose question differs from its own only in <paramref name="disambiguation"/>, such as <c>入る:masu</c> from <c>入る</c>, so that two<br/>
   /// meanings or uses of the word can be told apart, unless a vocab has that question already.
   /// </summary>
   public CreatedOrExistingVocab CloneWithDisambiguation(string disambiguation) => CloneWithTheQuestionUnlessAVocabHasIt(new WordReference(Note.Question.Text, disambiguation));

   /// <summary>The vocab whose question is <paramref name="question"/>, if there is one, or else a copy of this vocab with that question (<see cref="CloneWithTheQuestion"/>).</summary>
   CreatedOrExistingVocab CloneWithTheQuestionUnlessAVocabHasIt(WordReference question) => TheVocabNamedOrCreated(question, () => CloneWithTheQuestion(question));

   /// <summary>
   /// A copy of the vocab, its source data, media, tags and settings included, with <paramref name="question"/> as its question, added to the collection.<br/>
   /// The copy is given its question before it's added, so that it's never a duplicate of the vocab, even for a moment.
   /// </summary>
   VocabNote CloneWithTheQuestion(WordReference question)
   {
      var source = VocabNoteConverter.ToDto(Note);
      var cloneData = new VocabDto
                      {
                         Id = Guid.NewGuid(),
                         Tags = [],
                         Question = new VocabQuestionDto { Text = question.Text, Disambiguation = question.Disambiguation },
                         CorpusAnswer = source.CorpusAnswer,
                         Explanation = source.Explanation,
                         ExplanationLong = source.ExplanationLong,
                         Mnemonic = source.Mnemonic,
                         CompoundParts = source.CompoundParts,
                         Readings = source.Readings,
                         PartsOfSpeech = source.PartsOfSpeech,
                         Register = source.Register,
                         UsuallyWrittenInKana = source.UsuallyWrittenInKana,
                         Audio = source.Audio,
                         Forms = source.Forms,
                         SentenceCount = source.SentenceCount,
                         TechnicalNotes = source.TechnicalNotes,
                         References = source.References,
                         Images = source.Images,
                         Matching = source.Matching,
                         RelatedVocab = source.RelatedVocab,
                         SourceData = source.SourceData,
                      };
      var clone = new VocabNote(Note.Services, cloneData);

      CopyVocabTagsTo(clone);
      CopySourceTagsTo(clone);

      foreach(var related in clone.RelatedNotes.Synonyms.Words.ToList())
      {
         clone.RelatedNotes.Synonyms.Add(related);
      }

      Note.Services.Collection.Vocab.Add(clone);
      return clone;
   }

   public CreatedOrExistingVocab CreateNaAdjective() => CreatePostfixPrefixVersion("な", [PartOfSpeech.NaAdjective]);

   public CreatedOrExistingVocab CreateNoAdjective() => CreatePostfixPrefixVersion("の", [PartOfSpeech.Expression, PartOfSpeech.NoAdjective]);

   public CreatedOrExistingVocab CreateNiAdverb() => CreatePostfixPrefixVersion("に", [PartOfSpeech.Adverb]);

   public CreatedOrExistingVocab CreateToAdverb() => CreatePostfixPrefixVersion("と", [PartOfSpeech.ToAdverb]);

   public CreatedOrExistingVocab CreateTePrefixedWord() => CreatePostfixPrefixVersion("て", [PartOfSpeech.Auxiliary], isPrefix: true);

   public CreatedOrExistingVocab CreateOPrefixedWord() => CreatePostfixPrefixVersion("お", Note.PartsOfSpeech.All(), isPrefix: true);

   public CreatedOrExistingVocab CreateNSuffixedWord() => CreatePostfixPrefixVersion("ん", [PartOfSpeech.Expression]);

   public CreatedOrExistingVocab CreateKaSuffixedWord() => CreatePostfixPrefixVersion("か", [PartOfSpeech.Expression]);

   public CreatedOrExistingVocab CreateSuruVerb(bool shimasu = false)
   {
      return CreatePostfixPrefixVersion(shimasu ? "します" : "する", [PartOfSpeech.SuruVerb], finish: FinishTheSuruVerb);

      void FinishTheSuruVerb(VocabNote suruVerb)
      {
         var forms = suruVerb.Forms.QuestionAndEnteredFormsList()
                             .Concat(suruVerb.Forms.QuestionAndEnteredFormsList().Select(form => form.Replace("する", "をする")))
                             .ToList();
         suruVerb.Forms.SetEnteredForms(forms.Select(EnteredForm.NotOwned));

         if(Note.PartsOfSpeech.IsTransitive()) suruVerb.PartsOfSpeech.Add(PartOfSpeech.Transitive);
         if(Note.PartsOfSpeech.IsIntransitive()) suruVerb.PartsOfSpeech.Add(PartOfSpeech.Intransitive);
      }
   }

   public CreatedOrExistingVocab CreateShimasuVerb() => CreateSuruVerb(shimasu: true);

   public CreatedOrExistingVocab CreateKuForm() => CreatePostfixPrefixVersion("く", [PartOfSpeech.Adverb], setCompounds: true, chopOffCharacters: 1);

   public CreatedOrExistingVocab CreateSaForm() => CreatePostfixPrefixVersion("さ", [PartOfSpeech.Noun], setCompounds: true, chopOffCharacters: 1);

   /// <summary>
   /// The version of the vocab with <paramref name="formSuffix"/> suffixed onto the root <paramref name="createFormRoot"/> gives each of its forms, such as its<br/>
   /// い-stem: created as an expression, then finished by <paramref name="finish"/>, unless it exists already.
   /// </summary>
   CreatedOrExistingVocab CloneToDerivedForm(string formSuffix, Func<VocabNote, string, string> createFormRoot, Action<VocabNote>? finish = null)
   {
      return TheVocabNamedOrCreated(new WordReference(CreateFullForm(Note.GetQuestion())), Create);

      VocabNote Create()
      {
         var clone = CreateNewVocabCopyingTagsRegisterAndMatchingConfiguration(CreateFullForm(Note.GetQuestion()), Note.GetAnswer(), []);
         clone.Forms.SetEnteredForms(Note.Forms.EnteredFormsList().Select(CreateFullForm).Select(EnteredForm.NotOwned));
         var readings = Note.GetReadings().Select(CreateFullForm).ToList();
         clone.SetReadings(readings);
         clone.PartsOfSpeech.Set([PartOfSpeech.Expression]);
         var compounds = new List<WordReference> { Note.Question.Reference, new(formSuffix) };
         clone.CompoundParts.SetPrimary(compounds);
         finish?.Invoke(clone);
         return clone;
      }

      string CreateFullForm(string form) => createFormRoot(Note, form) + formSuffix;
   }

   string CreatePreviewForm(string formSuffix, Func<VocabNote, string, string> createFormRoot) => createFormRoot(Note, Note.GetQuestion()) + formSuffix;

   public CreatedOrExistingVocab SuffixToAStem(string formSuffix) => CloneToDerivedForm(formSuffix, Conjugator.GetAStemVocab);
   public string SuffixToAStemPreview(string formSuffix) => CreatePreviewForm(formSuffix, Conjugator.GetAStemVocab);

   public CreatedOrExistingVocab SuffixToChopped(string formSuffix, int chopCharacters)
   {
      return CloneToDerivedForm(formSuffix, (_, form) => form.Substring(0, form.Length - chopCharacters));
   }

   public string SuffixToChoppedPreview(string formSuffix, int chopCharacters) => Note.GetQuestion().Substring(0, Math.Max(0, Note.GetQuestion().Length - chopCharacters)) + formSuffix;

   public CreatedOrExistingVocab SuffixToIStem(string formSuffix) => CloneToDerivedForm(formSuffix, Conjugator.GetIStemVocab);
   public string SuffixToIStemPreview(string formSuffix) => CreatePreviewForm(formSuffix, Conjugator.GetIStemVocab);

   public CreatedOrExistingVocab SuffixToEStem(string formSuffix) => CloneToDerivedForm(formSuffix, Conjugator.GetEStemVocab);
   public string SuffixToEStemPreview(string formSuffix) => CreatePreviewForm(formSuffix, Conjugator.GetEStemVocab);

   public CreatedOrExistingVocab SuffixToTeStem(string formSuffix) => CloneToDerivedForm(formSuffix, Conjugator.GetTeStemVocab);
   public string SuffixToTeStemPreview(string formSuffix) => CreatePreviewForm(formSuffix, Conjugator.GetTeStemVocab);

   public CreatedOrExistingVocab CreateMasuForm() => SuffixToIStem("ます");
   public CreatedOrExistingVocab CreateTeForm() => SuffixToTeStem("て");
   public CreatedOrExistingVocab CreateTaForm() => SuffixToTeStem("た");
   public CreatedOrExistingVocab CreateBaForm() => SuffixToEStem("ば");

   public CreatedOrExistingVocab CreateReceptiveForm() => CloneToDerivedForm("れる", Conjugator.GetAStemVocab, finish: it => ReplaceTheLastCompoundPart(it, "あれる"));

   public CreatedOrExistingVocab CreateCausativeForm() => CloneToDerivedForm("せる", Conjugator.GetAStemVocab, finish: it => ReplaceTheLastCompoundPart(it, "あせる"));

   /// <summary>Makes <paramref name="part"/> the last of <paramref name="vocab"/>'s primary compound parts, in place of the one there.</summary>
   static void ReplaceTheLastCompoundPart(VocabNote vocab, string part)
   {
      var compoundParts = vocab.CompoundParts.Primary();
      compoundParts[^1] = new WordReference(part);
      vocab.CompoundParts.SetPrimary(compoundParts);
   }

   public CreatedOrExistingVocab CreateNaiForm() => SuffixToAStem("ない");

   public CreatedOrExistingVocab CreateImperative()
   {
      return TheVocabNamedOrCreated(new WordReference(CreateImperativeForm(Note.GetQuestion())), Create);

      VocabNote Create()
      {
         var clone = CreateNewVocabCopyingTagsRegisterAndMatchingConfiguration(CreateImperativeForm(Note.GetQuestion()), Note.GetAnswer(), []);
         clone.Forms.SetEnteredForms(Note.Forms.EnteredFormsList().Select(CreateImperativeForm).Select(EnteredForm.NotOwned));
         var readings = Note.GetReadings().Select(CreateImperativeForm).ToList();
         clone.SetReadings(readings);
         clone.PartsOfSpeech.Set([PartOfSpeech.Expression]);
         return clone;
      }

      string CreateImperativeForm(string form) => Conjugator.GetImperative(form, Note.PartsOfSpeech.IsIchidan(), Note.PartsOfSpeech.IsGodan());
   }

   public CreatedOrExistingVocab CreatePotentialGodan() =>
      CloneToDerivedForm("る", Conjugator.GetEStemVocab, finish: it => it.CompoundParts.SetPrimary([Note.Question.Reference, new WordReference("える")]));
}
