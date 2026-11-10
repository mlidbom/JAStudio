using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices;
using JAStudio.Core.Storage.Dto;
using JAStudio.Core.Note.Tagging;
using JAStudio.Core.Storage.Converters;

namespace JAStudio.Core.Note.Vocabulary;

public class VocabCloner
{
   public VocabCloner(VocabNote note) => Note = note;

   VocabNote Note { get; }

   public VocabNote PrefixToDictionaryForm(string prefix) => CreatePostfixPrefixVersion(prefix, [PartOfSpeech.Expression], isPrefix: true);

   public VocabNote PrefixToChopped(string prefix, int chopCharacters) => CreatePostfixPrefixVersion(prefix, [PartOfSpeech.Expression], isPrefix: true, chopOffCharacters: chopCharacters);

   public string PrefixToChoppedPreview(string formPrefix, int chopCharacters) => Note.GetQuestion().Length > chopCharacters
                                                                                     ? formPrefix + Note.GetQuestion().Substring(chopCharacters)
                                                                                     : "Vocab to short";

   public VocabNote CreateSuffixVersion(string suffix) => CreatePostfixPrefixVersion(suffix, [PartOfSpeech.Expression]);

   VocabNote CreatePostfixPrefixVersion(string addendum, IEnumerable<PartOfSpeech> partsOfSpeech, bool isPrefix = false, bool setCompounds = true, int chopOffCharacters = 0)
   {
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

      var vocabNote = Note;
      var newQuestion = AppendPrependAddendum(Note.GetQuestion());
      var newReadings = vocabNote.GetReadings().Select(AppendPrependAddendum).ToList();

      var newVocab = CreateNewVocabCopyingTagsRegisterAndMatchingConfiguration(newQuestion, Note.GetAnswer(), newReadings);

      if(setCompounds)
      {
         var compounds = isPrefix
                            ? new List<WordReference> { new(addendum), Note.Question.Reference }
                            : new List<WordReference> { Note.Question.Reference, new(addendum) };
         newVocab.CompoundParts.SetPrimary(compounds);
      }

      newVocab.PartsOfSpeech.Set(partsOfSpeech);
      newVocab.Forms.SetEnteredForms(Note.Forms.EnteredFormsSet().Select(AppendPrependAddendum).Select(EnteredForm.NotOwned));

      return newVocab;
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
      foreach(var tag in Note.Tags.Where(t => t.Name.StartsWith(Tags.Vocab.Root)))
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
      foreach(var tag in Note.Tags.Where(it => it.Name.StartsWith(Tags.Source.Folder)))
         clone.Tags.Set(tag);
   }

   public VocabNote Clone()
   {
      var source = VocabNoteConverter.ToDto(Note);
      var cloneData = new VocabDto
                      {
                         Id = Guid.NewGuid(),
                         Tags = [],
                         Question = source.Question,
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

   public VocabNote CloneToForm(string form)
   {
      var clone = Clone();
      clone.Question.SetText(form);
      return clone;
   }

   public VocabNote CreateNaAdjective() => CreatePostfixPrefixVersion("な", [PartOfSpeech.NaAdjective]);

   public VocabNote CreateNoAdjective() => CreatePostfixPrefixVersion("の", [PartOfSpeech.Expression, PartOfSpeech.NoAdjective]);

   public VocabNote CreateNiAdverb() => CreatePostfixPrefixVersion("に", [PartOfSpeech.Adverb]);

   public VocabNote CreateToAdverb() => CreatePostfixPrefixVersion("と", [PartOfSpeech.ToAdverb]);

   public VocabNote CreateTePrefixedWord() => CreatePostfixPrefixVersion("て", [PartOfSpeech.Auxiliary], isPrefix: true);

   public VocabNote CreateOPrefixedWord() => CreatePostfixPrefixVersion("お", Note.PartsOfSpeech.All(), isPrefix: true);

   public VocabNote CreateNSuffixedWord() => CreatePostfixPrefixVersion("ん", [PartOfSpeech.Expression]);

   public VocabNote CreateKaSuffixedWord() => CreatePostfixPrefixVersion("か", [PartOfSpeech.Expression]);

   public VocabNote CreateSuruVerb(bool shimasu = false)
   {
      var suruVerb = CreatePostfixPrefixVersion(shimasu ? "します" : "する", [PartOfSpeech.SuruVerb]);

      var forms = suruVerb.Forms.QuestionAndEnteredFormsList()
                          .Concat(suruVerb.Forms.QuestionAndEnteredFormsList().Select(form => form.Replace("する", "をする")))
                          .ToList();
      suruVerb.Forms.SetEnteredForms(forms.Select(EnteredForm.NotOwned));

      if(Note.PartsOfSpeech.IsTransitive()) suruVerb.PartsOfSpeech.Add(PartOfSpeech.Transitive);
      if(Note.PartsOfSpeech.IsIntransitive()) suruVerb.PartsOfSpeech.Add(PartOfSpeech.Intransitive);

      return suruVerb;
   }

   public VocabNote CreateShimasuVerb() => CreateSuruVerb(shimasu: true);

   public VocabNote CreateKuForm() => CreatePostfixPrefixVersion("く", [PartOfSpeech.Adverb], setCompounds: true, chopOffCharacters: 1);

   public VocabNote CreateSaForm() => CreatePostfixPrefixVersion("さ", [PartOfSpeech.Noun], setCompounds: true, chopOffCharacters: 1);

   public VocabNote CloneToDerivedForm(string formSuffix, Func<VocabNote, string, string> createFormRoot)
   {
      string CreateFullForm(string form) => createFormRoot(Note, form) + formSuffix;

      var clone = CreateNewVocabCopyingTagsRegisterAndMatchingConfiguration(CreateFullForm(Note.GetQuestion()), Note.GetAnswer(), []);
      clone.Forms.SetEnteredForms(Note.Forms.EnteredFormsList().Select(CreateFullForm).Select(EnteredForm.NotOwned));
      var readings = Note.GetReadings().Select(CreateFullForm).ToList();
      clone.SetReadings(readings);
      clone.PartsOfSpeech.Set([PartOfSpeech.Expression]);
      var compounds = new List<WordReference> { Note.Question.Reference, new(formSuffix) };
      clone.CompoundParts.SetPrimary(compounds);
      return clone;
   }

   string CreatePreviewForm(string formSuffix, Func<VocabNote, string, string> createFormRoot) => createFormRoot(Note, Note.GetQuestion()) + formSuffix;

   public VocabNote SuffixToAStem(string formSuffix) => CloneToDerivedForm(formSuffix, Conjugator.GetAStemVocab);
   public string SuffixToAStemPreview(string formSuffix) => CreatePreviewForm(formSuffix, Conjugator.GetAStemVocab);

   public VocabNote SuffixToChopped(string formSuffix, int chopCharacters)
   {
      return CloneToDerivedForm(formSuffix, (_, form) => form.Substring(0, form.Length - chopCharacters));
   }

   public string SuffixToChoppedPreview(string formSuffix, int chopCharacters) => Note.GetQuestion().Substring(0, Math.Max(0, Note.GetQuestion().Length - chopCharacters)) + formSuffix;

   public VocabNote SuffixToIStem(string formSuffix) => CloneToDerivedForm(formSuffix, Conjugator.GetIStemVocab);
   public string SuffixToIStemPreview(string formSuffix) => CreatePreviewForm(formSuffix, Conjugator.GetIStemVocab);

   public VocabNote SuffixToEStem(string formSuffix) => CloneToDerivedForm(formSuffix, Conjugator.GetEStemVocab);
   public string SuffixToEStemPreview(string formSuffix) => CreatePreviewForm(formSuffix, Conjugator.GetEStemVocab);

   public VocabNote SuffixToTeStem(string formSuffix) => CloneToDerivedForm(formSuffix, Conjugator.GetTeStemVocab);
   public string SuffixToTeStemPreview(string formSuffix) => CreatePreviewForm(formSuffix, Conjugator.GetTeStemVocab);

   public VocabNote CreateMasuForm() => SuffixToIStem("ます");
   public VocabNote CreateTeForm() => SuffixToTeStem("て");
   public VocabNote CreateTaForm() => SuffixToTeStem("た");
   public VocabNote CreateBaForm() => SuffixToEStem("ば");

   public VocabNote CreateReceptiveForm()
   {
      var result = SuffixToAStem("れる");
      var compoundParts = result.CompoundParts.Primary();
      compoundParts[^1] = new WordReference("あれる");
      result.CompoundParts.SetPrimary(compoundParts);
      return result;
   }

   public VocabNote CreateCausativeForm()
   {
      var result = SuffixToAStem("せる");
      var compoundParts = result.CompoundParts.Primary();
      compoundParts[^1] = new WordReference("あせる");
      result.CompoundParts.SetPrimary(compoundParts);
      return result;
   }

   public VocabNote CreateNaiForm() => SuffixToAStem("ない");

   public VocabNote CreateImperative()
   {
      string CreateImperativeForm(string form) =>
         Conjugator.GetImperative(form, Note.PartsOfSpeech.IsIchidan(), Note.PartsOfSpeech.IsGodan());

      var clone = CreateNewVocabCopyingTagsRegisterAndMatchingConfiguration(CreateImperativeForm(Note.GetQuestion()), Note.GetAnswer(), []);
      clone.Forms.SetEnteredForms(Note.Forms.EnteredFormsList().Select(CreateImperativeForm).Select(EnteredForm.NotOwned));
      var readings = Note.GetReadings().Select(CreateImperativeForm).ToList();
      clone.SetReadings(readings);
      clone.PartsOfSpeech.Set([PartOfSpeech.Expression]);
      return clone;
   }

   public VocabNote CreatePotentialGodan()
   {
      var clone = SuffixToEStem("る");
      clone.CompoundParts.SetPrimary([Note.Question.Reference, new WordReference("える")]);
      return clone;
   }
}
