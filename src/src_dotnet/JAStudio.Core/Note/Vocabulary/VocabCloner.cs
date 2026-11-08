using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices;
using JAStudio.Core.Note.CorpusData;
using JAStudio.Core.Note.Tagging;
using JAStudio.Core.Storage.Converters;

namespace JAStudio.Core.Note.Vocabulary;

public class VocabCloner
{
   public VocabCloner(VocabNote note) => Note = note;

   VocabNote Note { get; }

   public VocabNote PrefixToDictionaryForm(string prefix, string speechType = POS.Expression) => CreatePostfixPrefixVersion(prefix, speechType, isPrefix: true);

   public VocabNote PrefixToChopped(string prefix, int chopCharacters) => CreatePostfixPrefixVersion(prefix, POS.Expression, isPrefix: true, chopOffCharacters: chopCharacters);

   public string PrefixToChoppedPreview(string formPrefix, int chopCharacters) => Note.GetQuestion().Length > chopCharacters
                                                                                     ? formPrefix + Note.GetQuestion().Substring(chopCharacters)
                                                                                     : "Vocab to short";

   public VocabNote CreateSuffixVersion(string suffix, string speechType = POS.Expression, bool setCompounds = true, int truncateCharacters = 0) => CreatePostfixPrefixVersion(suffix, speechType, setCompounds: setCompounds, chopOffCharacters: truncateCharacters);

   VocabNote CreatePostfixPrefixVersion(string addendum, string speechType, bool isPrefix = false, bool setCompounds = true, int chopOffCharacters = 0)
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

      var newVocab = CreateNewVocabWithSomeDataCopied(newQuestion, Note.GetAnswer(), newReadings);

      if(setCompounds)
      {
         var compounds = isPrefix
                            ? new List<string> { addendum, Note.Question.DisambiguationName }
                            : new List<string> { Note.Question.DisambiguationName, addendum };
         newVocab.CompoundParts.SetPrimary(compounds);
      }

      newVocab.PartsOfSpeech.SetRawStringValue(speechType);
      newVocab.Forms.SetEnteredForms(Note.Forms.EnteredFormsSet().Select(AppendPrependAddendum).Select(EnteredForm.NotOwned));

      return newVocab;
   }

   VocabNote CreateNewVocabWithSomeDataCopied(string question, string answer, List<string> readings, bool copyVocabTags = true, bool copyMatchingRules = true)
   {
      var clone = Note.Services.VocabNoteFactory.CreateFromUserData(question, answer, readings);
      if(copyVocabTags)
      {
         CopyVocabTagsTo(clone);
      }

      if(copyMatchingRules)
      {
         clone.MatchingConfiguration.ConfigurableRules.OverwriteWith(Note.MatchingConfiguration.ConfigurableRules);
      }

      return clone;
   }

   void CopyVocabTagsTo(VocabNote target)
   {
      foreach(var tag in Note.Tags.Where(t => t.Name.StartsWith(Tags.Vocab.Root)))
      {
         target.Tags.Set(tag);
      }
   }

   public VocabNote Clone()
   {
      var source = VocabNoteConverter.ToCorpusData(Note);
      var cloneData = new VocabData
                      {
                         Id = Guid.NewGuid(),
                         Tags = [],
                         Question = source.Question,
                         SourceAnswer = source.SourceAnswer,
                         UserAnswer = source.UserAnswer,
                         ActiveAnswer = source.ActiveAnswer,
                         UserExplanation = source.UserExplanation,
                         UserExplanationLong = source.UserExplanationLong,
                         UserMnemonic = source.UserMnemonic,
                         CompoundParts = source.CompoundParts,
                         Readings = source.Readings,
                         PartsOfSpeech = source.PartsOfSpeech,
                         SourceMnemonic = source.SourceMnemonic,
                         SourceReadingMnemonic = source.SourceReadingMnemonic,
                         AudioB = source.AudioB,
                         AudioG = source.AudioG,
                         AudioTTS = source.AudioTTS,
                         Forms = source.Forms,
                         SentenceCount = source.SentenceCount,
                         TechnicalNotes = source.TechnicalNotes,
                         References = source.References,
                         Image = source.Image,
                         UserImage = source.UserImage,
                         MatchingRules = source.MatchingRules,
                         RelatedVocab = source.RelatedVocab,
                      };
      var clone = new VocabNote(Note.Services, cloneData);

      CopyVocabTagsTo(clone);

      foreach(var related in clone.RelatedNotes.Synonyms.Strings())
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

   public VocabNote CreateNaAdjective() => CreatePostfixPrefixVersion("な", "na-adjective");

   public VocabNote CreateNoAdjective() => CreatePostfixPrefixVersion("の", "expression, no-adjective");

   public VocabNote CreateNiAdverb() => CreatePostfixPrefixVersion("に", "adverb");

   public VocabNote CreateToAdverb() => CreatePostfixPrefixVersion("と", "to-adverb");

   public VocabNote CreateTePrefixedWord() => CreatePostfixPrefixVersion("て", "auxiliary", isPrefix: true);

   public VocabNote CreateOPrefixedWord() => CreatePostfixPrefixVersion("お", Note.PartsOfSpeech.RawStringValue(), isPrefix: true);

   public VocabNote CreateNSuffixedWord() => CreatePostfixPrefixVersion("ん", POS.Expression);

   public VocabNote CreateKaSuffixedWord() => CreatePostfixPrefixVersion("か", POS.Expression);

   public VocabNote CreateSuruVerb(bool shimasu = false)
   {
      var suruVerb = CreatePostfixPrefixVersion(shimasu ? "します" : "する", POS.SuruVerb);

      var forms = suruVerb.Forms.QuestionAndEnteredFormsList()
                          .Concat(suruVerb.Forms.QuestionAndEnteredFormsList().Select(form => form.Replace("する", "をする")))
                          .ToList();
      suruVerb.Forms.SetEnteredForms(forms.Select(EnteredForm.NotOwned));

      if(Note.PartsOfSpeech.IsTransitive())
      {
         var value = suruVerb.PartsOfSpeech.RawStringValue() + ", " + POS.Transitive;
         suruVerb.PartsOfSpeech.SetRawStringValue(value);
      }

      if(Note.PartsOfSpeech.IsIntransitive())
      {
         var value = suruVerb.PartsOfSpeech.RawStringValue() + ", " + POS.Intransitive;
         suruVerb.PartsOfSpeech.SetRawStringValue(value);
      }

      return suruVerb;
   }

   public VocabNote CreateShimasuVerb() => CreateSuruVerb(shimasu: true);

   public VocabNote CreateKuForm() => CreatePostfixPrefixVersion("く", "adverb", setCompounds: true, chopOffCharacters: 1);

   public VocabNote CreateSaForm() => CreatePostfixPrefixVersion("さ", POS.Noun, setCompounds: true, chopOffCharacters: 1);

   public VocabNote CloneToDerivedForm(string formSuffix, Func<VocabNote, string, string> createFormRoot)
   {
      string CreateFullForm(string form) => createFormRoot(Note, form) + formSuffix;

      var clone = CreateNewVocabWithSomeDataCopied(CreateFullForm(Note.GetQuestion()), Note.GetAnswer(), []);
      clone.Forms.SetEnteredForms(Note.Forms.EnteredFormsList().Select(CreateFullForm).Select(EnteredForm.NotOwned));
      var readings = Note.GetReadings().Select(CreateFullForm).ToList();
      clone.SetReadings(readings);
      clone.PartsOfSpeech.SetRawStringValue(POS.Expression);
      var compounds = new List<string> { Note.Question.DisambiguationName, formSuffix };
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
      compoundParts[^1] = "あれる";
      result.CompoundParts.SetPrimary(compoundParts);
      return result;
   }

   public VocabNote CreateCausativeForm()
   {
      var result = SuffixToAStem("せる");
      var compoundParts = result.CompoundParts.Primary();
      compoundParts[^1] = "あせる";
      result.CompoundParts.SetPrimary(compoundParts);
      return result;
   }

   public VocabNote CreateNaiForm() => SuffixToAStem("ない");

   public VocabNote CreateImperative()
   {
      string CreateImperativeForm(string form) =>
         Conjugator.GetImperative(form, Note.PartsOfSpeech.IsIchidan(), Note.PartsOfSpeech.IsGodan());

      var clone = CreateNewVocabWithSomeDataCopied(CreateImperativeForm(Note.GetQuestion()), Note.GetAnswer(), []);
      clone.Forms.SetEnteredForms(Note.Forms.EnteredFormsList().Select(CreateImperativeForm).Select(EnteredForm.NotOwned));
      var readings = Note.GetReadings().Select(CreateImperativeForm).ToList();
      clone.SetReadings(readings);
      clone.PartsOfSpeech.SetRawStringValue(POS.Expression);
      return clone;
   }

   public VocabNote CreatePotentialGodan()
   {
      var clone = SuffixToEStem("る");
      clone.CompoundParts.SetPrimary([Note.Question.DisambiguationName, "える"]);
      return clone;
   }
}
