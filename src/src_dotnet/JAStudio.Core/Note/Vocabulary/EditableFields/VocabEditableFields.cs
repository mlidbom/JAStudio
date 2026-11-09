using System;
using System.Collections.Generic;
using JAStudio.Core.Note.EditableFields;
using JAStudio.Core.Note.NoteFields;
using JAStudio.Core.Note.Vocabulary.Matching;

namespace JAStudio.Core.Note.Vocabulary.EditableFields;

/// <summary>
/// Every field of a vocab that a person edits (<see cref="IEditableField{TNote}"/>): first those of the vocab's editor in the workspace, in the order of<br/>
/// its tabs, the text fields, forms and parts of speech, the matching rules and flags, whether it's usually written in kana, its register and its tags;<br/>
/// then its related vocab, which the vocab's menus edit.
/// </summary>
/// <remarks>
/// The vocab's editors are built from these fields, and JAStudio's MCP server edits <see cref="All"/> of them, so the two edit the same fields.<br/>
/// The editors of the <see cref="Forms"/>, the <see cref="PartsOfSpeech"/>, <see cref="UsuallyWrittenInKana"/> and the <see cref="Tags"/> are not built from<br/>
/// them yet: they write through the same operations of the vocab, and show labels of their own.
/// </remarks>
public static class VocabEditableFields
{
   public static readonly EditableTextField<VocabNote> Question =
      new("question.text", "Question", TextFieldFormat.Line, it => it.Question.Text, (it, question) => it.Question.SetText(question.Trim()), changesHowSentencesParse: true);

   /// <summary>The <see cref="VocabNoteQuestion.Disambiguation"/> that tells the vocab apart from others with the same question, if it has one.</summary>
   public static readonly EditableTextField<VocabNote> Disambiguation =
      new("question.disambiguation", "Disambiguation", TextFieldFormat.Line, it => it.Question.Disambiguation, (it, disambiguation) => it.Question.SetDisambiguation(disambiguation.Trim()), changesHowSentencesParse: true);

   /// <summary>The vocab's readings, in kana, the primary one first.</summary>
   public static readonly EditableTextListField<VocabNote> Readings = new("readings", "Readings", it => it.GetReadings(), (it, readings) => it.SetReadings(readings));

   public static readonly EditableFormsField Forms = new();
   public static readonly EditablePartsOfSpeechField PartsOfSpeech = new();

   /// <summary>The corpus's own answer, which takes the place of the source's (<see cref="SourceAnswer"/>).</summary>
   public static readonly EditableTextField<VocabNote> Answer = new("corpusAnswer", "Answer", TextFieldFormat.Paragraphs, it => it.CorpusAnswer);

   public static readonly EditableTextField<VocabNote> Explanation = new("explanation", "Explanation", TextFieldFormat.Paragraphs, it => it.Explanation);
   public static readonly EditableTextField<VocabNote> LongExplanation = new("explanationLong", "Long explanation", TextFieldFormat.Paragraphs, it => it.ExplanationLong);
   public static readonly EditableTextField<VocabNote> Mnemonic = new("mnemonic", "Mnemonic", TextFieldFormat.Paragraphs, it => it.Mnemonic);

   /// <summary>The vocab's <see cref="VocabNoteCompoundParts.Primary"/> compound parts.</summary>
   public static readonly EditableTextListField<VocabNote> CompoundParts =
      new("compoundParts.primary", "Compound parts", it => it.CompoundParts.Primary(), (it, parts) => it.CompoundParts.SetPrimary(parts));

   /// <summary>The vocab's <see cref="VocabNoteCompoundParts.Alternative"/> compound parts.</summary>
   public static readonly EditableTextListField<VocabNote> AlternativeCompoundParts =
      new("compoundParts.alternative", "Alternative compound parts", it => it.CompoundParts.Alternative(), (it, parts) => it.CompoundParts.SetAlternative(parts));

   /// <summary>The vocab's first audio, as Anki's <c>[sound:file]</c> reference.</summary>
   public static readonly EditableTextField<VocabNote> FirstAudio =
      new("audioB", "First", TextFieldFormat.Line, it => it.Audio.First.RawValue(), (it, audio) => it.Audio.First.SetRawValue(audio));

   /// <summary>The vocab's second audio, as Anki's <c>[sound:file]</c> reference.</summary>
   public static readonly EditableTextField<VocabNote> SecondAudio =
      new("audioG", "Second", TextFieldFormat.Line, it => it.Audio.Second.RawValue(), (it, audio) => it.Audio.Second.SetRawValue(audio));

   /// <summary>The vocab's text-to-speech audio, as Anki's <c>[sound:file]</c> reference.</summary>
   public static readonly EditableTextField<VocabNote> TextToSpeechAudio =
      new("audioTTS", "Text to speech", TextFieldFormat.Line, it => it.TextToSpeechAudio.RawValue(), (it, audio) => it.TextToSpeechAudio.SetRawValue(audio));

   public static readonly EditableTextField<VocabNote> TechnicalNotes = new("technicalNotes", "Technical notes", TextFieldFormat.Paragraphs, it => it.TechnicalNotes);

   /// <summary>The answer the vocab's source gave it (<see cref="VocabNote.SourceData"/>).</summary>
   public static readonly EditableTextField<VocabNote> SourceAnswer = new("sourceData.answer", "Answer", TextFieldFormat.Paragraphs, it => it.SourceData.Answer);

   /// <summary>The mnemonic the vocab's source gave it, never shown on a card.</summary>
   public static readonly EditableTextField<VocabNote> SourceMnemonic = new("sourceData.mnemonic", "Mnemonic", TextFieldFormat.Paragraphs, it => it.SourceData.Mnemonic);

   /// <summary>The reading mnemonic the vocab's source gave it, never shown on a card.</summary>
   public static readonly EditableTextField<VocabNote> SourceReadingMnemonic =
      new("sourceData.readingMnemonic", "Reading mnemonic", TextFieldFormat.Paragraphs, it => it.SourceData.ReadingMnemonic);

   public static readonly EditableRequireForbidField YieldLastToken = RequireForbidRule("yieldLastTokenToOverlappingCompound", "Yield to overlapping following compound", it => it.YieldLastToken);
   public static readonly EditableRequireForbidField Surface = RequireForbidRule("surface", "Surface", it => it.Surface);
   public static readonly EditableRequireForbidField SentenceEnd = RequireForbidRule("sentenceEnd", "Sentence end", it => it.SentenceEnd);
   public static readonly EditableRequireForbidField SentenceStart = RequireForbidRule("sentenceStart", "Sentence start", it => it.SentenceStart);
   public static readonly EditableRequireForbidField SingleToken = RequireForbidRule("singleToken", "Single token", it => it.SingleToken);
   public static readonly EditableRequireForbidField GodanImperative = RequireForbidRule("godanImperative", "Godan imperative", it => it.GodanImperative);
   public static readonly EditableRequireForbidField GodanImperativeHead = RequireForbidRule("godanImperativeHead", "Godan imperative (head)", it => it.GodanImperativeHead);
   public static readonly EditableRequireForbidField GodanPotential = RequireForbidRule("godanPotential", "Godan potential", it => it.GodanPotential);
   public static readonly EditableRequireForbidField IchidanImperative = RequireForbidRule("ichidanImperative", "Ichidan imperative", it => it.IchidanImperative);
   public static readonly EditableRequireForbidField MasuStem = RequireForbidRule("masuStem", "Masu stem", it => it.MasuStem);
   public static readonly EditableRequireForbidField Godan = RequireForbidRule("godan", "Godan", it => it.Godan);
   public static readonly EditableRequireForbidField Ichidan = RequireForbidRule("ichidan", "Ichidan", it => it.Ichidan);
   public static readonly EditableRequireForbidField Irrealis = RequireForbidRule("irrealis", "Irrealis", it => it.Irrealis);
   public static readonly EditableRequireForbidField PastTenseStem = RequireForbidRule("pastTenseStem", "Past tense stem", it => it.PastTenseStem);
   public static readonly EditableRequireForbidField DictionaryFormStart = RequireForbidRule("dictionaryFormStart", "Dictionary form (start)", it => it.DictionaryFormStart);
   public static readonly EditableRequireForbidField DictionaryFormHead = RequireForbidRule("dictionaryFormHead", "Dictionary form (head)", it => it.DictionaryFormHead);
   public static readonly EditableRequireForbidField AdverbHead = RequireForbidRule("adverbHead", "Adverb (head)", it => it.AdverbHead);
   public static readonly EditableRequireForbidField TeFormStemStart = RequireForbidRule("teFormStemStart", "て-form stem (start)", it => it.TeFormStemStart);
   public static readonly EditableRequireForbidField TeFormStemHead = RequireForbidRule("teFormStemHead", "て-form stem (head)", it => it.TeFormStemHead);

   public static readonly EditableFlagField<VocabNote> IsInflectingWord = MatchingFlag("isInflectingWord", "Inflecting word", it => it.IsInflectingWord, changesHowSentencesParse: true);
   public static readonly EditableFlagField<VocabNote> IsPoisonWord = MatchingFlag("isPoisonWord", "Poison word", it => it.IsPoisonWord, changesHowSentencesParse: true);

   public static readonly EditableFlagField<VocabNote> MatchWithPrecedingVowel =
      MatchingFlag("matchWithPrecedingVowel", "Match with preceding vowel", it => it.MatchWithPrecedingVowel, changesHowSentencesParse: true);

   public static readonly EditableFlagField<VocabNote> QuestionOverridesForm =
      MatchingFlag("questionOverridesForm",
                   "Question overrides form: Show the question in results even if the match was another form",
                   it => it.QuestionOverridesForm,
                   changesHowSentencesParse: true);

   public static readonly EditableTextSetField<VocabNote> PrefixIsNot = TextSetRule("prefixIsNot", "Prefix is not", it => it.PrefixIsNot);
   public static readonly EditableTextSetField<VocabNote> RequiredPrefix = TextSetRule("requiredPrefix", "Required prefix", it => it.RequiredPrefix);
   public static readonly EditableTextSetField<VocabNote> SuffixIsNot = TextSetRule("suffixIsNot", "Suffix is not", it => it.SuffixIsNot);
   public static readonly EditableTextSetField<VocabNote> SurfaceIsNot = TextSetRule("surfaceIsNot", "Surface is not", it => it.SurfaceIsNot);
   public static readonly EditableTextSetField<VocabNote> YieldToSurface = TextSetRule("yieldToSurface", "Yield to surface", it => it.YieldToSurface);

   public static readonly EditableUsuallyWrittenInKanaField UsuallyWrittenInKana = new();

   /// <summary>Affects only how the sentence breakdown displays the vocab, not how sentences parse: see <see cref="VocabMatchingFlags.IsCompositionallyTransparentCompound"/>.</summary>
   public static readonly EditableFlagField<VocabNote> IsCompositionallyTransparentCompound =
      MatchingFlag("isCompositionallyTransparentCompound", "Compositionally transparent compound", it => it.IsCompositionallyTransparentCompound, changesHowSentencesParse: false);

   public static readonly EditableFlagField<VocabNote> Archaic = RegisterFlag("archaic", "Archaic", it => it.Archaic);
   public static readonly EditableFlagField<VocabNote> Childish = RegisterFlag("childish", "Childish", it => it.Childish);
   public static readonly EditableFlagField<VocabNote> Derogatory = RegisterFlag("derogatory", "Derogatory", it => it.Derogatory);
   public static readonly EditableFlagField<VocabNote> Formal = RegisterFlag("formal", "Formal", it => it.Formal);
   public static readonly EditableFlagField<VocabNote> Honorific = RegisterFlag("honorific", "Honorific", it => it.Honorific);
   public static readonly EditableFlagField<VocabNote> Humble = RegisterFlag("humble", "Humble", it => it.Humble);
   public static readonly EditableFlagField<VocabNote> Informal = RegisterFlag("informal", "Informal", it => it.Informal);
   public static readonly EditableFlagField<VocabNote> Literary = RegisterFlag("literary", "Literary", it => it.Literary);
   public static readonly EditableFlagField<VocabNote> Polite = RegisterFlag("polite", "Polite", it => it.Polite);
   public static readonly EditableFlagField<VocabNote> RoughMasculine = RegisterFlag("roughMasculine", "Rough, traditionally considered male", it => it.RoughMasculine);
   public static readonly EditableFlagField<VocabNote> Sensitive = RegisterFlag("sensitive", "Sensitive", it => it.Sensitive);
   public static readonly EditableFlagField<VocabNote> Slang = RegisterFlag("slang", "Slang", it => it.Slang);
   public static readonly EditableFlagField<VocabNote> SoftFeminine = RegisterFlag("softFeminine", "Soft, traditionally considered feminine", it => it.SoftFeminine);
   public static readonly EditableFlagField<VocabNote> Vulgar = RegisterFlag("vulgar", "Vulgar, usually offensive", it => it.Vulgar);

   public static readonly EditableTagsField<VocabNote> Tags = new();

   /// <summary>Synonyms are linked both ways: adding one to the vocab adds the vocab to the synonym's synonyms, and removing one removes it.</summary>
   public static readonly EditableTextSetField<VocabNote> Synonyms =
      new("relatedVocab.synonyms", "Synonyms", it => it.RelatedNotes.Synonyms.Strings(), (it, synonym) => it.RelatedNotes.Synonyms.Add(synonym), (it, synonym) => it.RelatedNotes.Synonyms.Remove(synonym));

   /// <summary>
   /// Perfect synonyms share one answer: adding one gives it the vocab's answer, and links every vocab of the two webs of perfect synonyms to every other,<br/>
   /// as perfect synonyms and as synonyms (<see cref="RelatedVocab.PerfectSynonyms"/>).
   /// </summary>
   public static readonly EditableTextSetField<VocabNote> PerfectSynonyms =
      new("relatedVocab.perfectSynonyms",
          "Perfect synonyms, which share the answer",
          it => it.RelatedNotes.PerfectSynonyms.Get(),
          (it, synonym) => it.RelatedNotes.PerfectSynonyms.AddOverwritingTheAnswerOfTheAddedSynonym(synonym),
          (it, synonym) => it.RelatedNotes.PerfectSynonyms.Remove(synonym));

   public static readonly EditableTextSetField<VocabNote> Antonyms =
      new("relatedVocab.antonyms", "Antonyms", it => it.RelatedNotes.Antonyms.Strings(), (it, antonym) => it.RelatedNotes.Antonyms.Add(antonym), (it, antonym) => it.RelatedNotes.Antonyms.Remove(antonym));

   public static readonly EditableTextSetField<VocabNote> ConfusedWith =
      new("relatedVocab.confusedWith", "Confused with", it => it.RelatedNotes.ConfusedWith.Get(), (it, vocab) => it.RelatedNotes.ConfusedWith.Add(vocab), (it, vocab) => it.RelatedNotes.ConfusedWith.Remove(vocab));

   public static readonly EditableTextSetField<VocabNote> SeeAlso =
      new("relatedVocab.seeAlso", "See also", it => it.RelatedNotes.SeeAlso.Strings(), (it, vocab) => it.RelatedNotes.SeeAlso.Add(vocab), (it, vocab) => it.RelatedNotes.SeeAlso.Remove(vocab));

   /// <summary>The vocab's transitive or intransitive counterpart, such as 開ける for 開く. Setting it sets the twin's ergative twin to the vocab too.</summary>
   public static readonly EditableTextField<VocabNote> ErgativeTwin =
      new("relatedVocab.ergativeTwin", "Ergative twin", TextFieldFormat.Line, it => it.RelatedNotes.ErgativeTwin.Get(), (it, twin) => it.RelatedNotes.ErgativeTwin.Set(twin));

   public static readonly EditableTextField<VocabNote> DerivedFrom =
      new("relatedVocab.derivedFrom", "Derived from", TextFieldFormat.Line, it => it.RelatedNotes.DerivedFrom.Get(), (it, vocab) => it.RelatedNotes.DerivedFrom.Set(vocab));

   /// <summary>Every field above, in the same order.</summary>
   public static IReadOnlyList<IEditableField<VocabNote>> All { get; } =
   [
      Question, Disambiguation, Readings, Forms, PartsOfSpeech, Answer, Explanation, LongExplanation, Mnemonic, CompoundParts, AlternativeCompoundParts,
      FirstAudio, SecondAudio, TextToSpeechAudio, TechnicalNotes, SourceAnswer, SourceMnemonic, SourceReadingMnemonic,
      YieldLastToken, Surface, SentenceEnd, SentenceStart, SingleToken, GodanImperative, GodanImperativeHead, GodanPotential, IchidanImperative, MasuStem,
      Godan, Ichidan, Irrealis, PastTenseStem, DictionaryFormStart, DictionaryFormHead, AdverbHead, TeFormStemStart, TeFormStemHead,
      IsInflectingWord, IsPoisonWord, MatchWithPrecedingVowel, QuestionOverridesForm, PrefixIsNot, RequiredPrefix, SuffixIsNot, SurfaceIsNot, YieldToSurface,
      UsuallyWrittenInKana, IsCompositionallyTransparentCompound,
      Archaic, Childish, Derogatory, Formal, Honorific, Humble, Informal, Literary, Polite, RoughMasculine, Sensitive, Slang, SoftFeminine, Vulgar,
      Tags,
      Synonyms, PerfectSynonyms, Antonyms, ConfusedWith, SeeAlso, ErgativeTwin, DerivedFrom
   ];

   /// <summary>One of the vocab's require/forbid matching rules (<see cref="VocabMatchingRequireForbidFlags"/>), named <paramref name="name"/> in the corpus.</summary>
   static EditableRequireForbidField RequireForbidRule(string name, string label, Func<VocabMatchingRequireForbidFlags, RequireForbidFlagField> rule) =>
      new($"matching.requiredOrForbidden.{name}", label, it => rule(it.MatchingConfiguration.RequiresForbids));

   /// <summary>One of the vocab's matching flags (<see cref="VocabMatchingFlags"/>), named <paramref name="name"/> in the corpus.</summary>
   static EditableFlagField<VocabNote> MatchingFlag(string name, string label, Func<VocabMatchingFlags, FlagField> flag, bool changesHowSentencesParse) =>
      new($"matching.flags.{name}", label, it => flag(it.MatchingConfiguration.Flags), changesHowSentencesParse);

   /// <summary>One of the vocab's configurable matching rules (<see cref="VocabNoteMatchingRules"/>), named <paramref name="name"/> in the corpus.</summary>
   static EditableTextSetField<VocabNote> TextSetRule(string name, string label, Func<VocabNoteMatchingRules, HashSet<string>> rule) =>
      new($"matching.rules.{name}", label, it => rule(it.MatchingConfiguration.ConfigurableRules), it => it.MatchingConfiguration.ConfigurableRules.Save(), changesHowSentencesParse: true);

   /// <summary>One of the vocab's register flags (<see cref="VocabNoteRegister"/>), such as whether it's formal, named <paramref name="name"/> in the corpus.</summary>
   static EditableFlagField<VocabNote> RegisterFlag(string name, string label, Func<VocabNoteRegister, FlagField> flag) =>
      new($"register.{name}", label, it => flag(it.Register));
}
