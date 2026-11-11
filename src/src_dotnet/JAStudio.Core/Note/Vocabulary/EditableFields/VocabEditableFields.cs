using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.EditableFields;
using JAStudio.Core.Note.NoteFields;
using JAStudio.Core.Note.Vocabulary.Matching;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Rules;
using JAStudio.Core.Note.Vocabulary.RelatedVocab;

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

   /// <summary>The vocab's <see cref="VocabNoteCompoundParts.Primary"/> compound parts, each by its <see cref="WordReference.DisambiguationName"/>, as <c>出す:masu</c>.</summary>
   public static readonly EditableTextListField<VocabNote> CompoundParts =
      new("compoundParts.primary", "Compound parts", it => DisambiguationNamesOf(it.CompoundParts.Primary()), (it, parts) => it.CompoundParts.SetPrimary(WordReference.TheWordsNamedBy(parts)));

   /// <summary>The vocab's <see cref="VocabNoteCompoundParts.Alternative"/> compound parts, each by its <see cref="WordReference.DisambiguationName"/>.</summary>
   public static readonly EditableTextListField<VocabNote> AlternativeCompoundParts =
      new("compoundParts.alternative",
          "Alternative compound parts",
          it => DisambiguationNamesOf(it.CompoundParts.Alternative()),
          (it, parts) => it.CompoundParts.SetAlternative(WordReference.TheWordsNamedBy(parts)));

   public static readonly EditableTextField<VocabNote> TechnicalNotes = new("technicalNotes", "Technical notes", TextFieldFormat.Paragraphs, it => it.TechnicalNotes);

   /// <summary>The answer the vocab's source gave it (<see cref="VocabNote.SourceData"/>).</summary>
   public static readonly EditableTextField<VocabNote> SourceAnswer = new("sourceData.answer", "Answer", TextFieldFormat.Paragraphs, it => it.SourceData.Answer);

   /// <summary>The mnemonic the vocab's source gave it, never shown on a card.</summary>
   public static readonly EditableTextField<VocabNote> SourceMnemonic = new("sourceData.mnemonic", "Mnemonic", TextFieldFormat.Paragraphs, it => it.SourceData.Mnemonic);

   /// <summary>The reading mnemonic the vocab's source gave it, never shown on a card.</summary>
   public static readonly EditableTextField<VocabNote> SourceReadingMnemonic =
      new("sourceData.readingMnemonic", "Reading mnemonic", TextFieldFormat.Paragraphs, it => it.SourceData.ReadingMnemonic);

   public static readonly EditableRequireForbidField YieldLastToken = RequireForbidRule("yieldLastTokenToOverlappingCompound", "Yield to overlapping following compound", it => it.YieldLastToken);
   public static readonly EditableRequireForbidField Surface = MatchingRule(MatchingRules.Surface, "Surface");
   public static readonly EditableRequireForbidField SentenceEnd = MatchingRule(MatchingRules.SentenceEnd, "Sentence end");
   public static readonly EditableRequireForbidField SentenceStart = MatchingRule(MatchingRules.SentenceStart, "Sentence start");
   public static readonly EditableRequireForbidField SingleToken = MatchingRule(MatchingRules.SingleToken, "Single token");
   public static readonly EditableRequireForbidField GodanImperative = MatchingRule(MatchingRules.GodanImperative, "Godan imperative");
   public static readonly EditableRequireForbidField GodanImperativeHead = MatchingRule(MatchingRules.GodanImperativeHead, "Godan imperative (head)");
   public static readonly EditableRequireForbidField GodanPotential = MatchingRule(MatchingRules.GodanPotential, "Godan potential");
   public static readonly EditableRequireForbidField IchidanImperative = MatchingRule(MatchingRules.IchidanImperative, "Ichidan imperative");
   public static readonly EditableRequireForbidField MasuStem = MatchingRule(MatchingRules.MasuStem, "Masu stem");
   public static readonly EditableRequireForbidField Godan = MatchingRule(MatchingRules.Godan, "Godan");
   public static readonly EditableRequireForbidField Ichidan = MatchingRule(MatchingRules.Ichidan, "Ichidan");
   public static readonly EditableRequireForbidField Irrealis = MatchingRule(MatchingRules.Irrealis, "Irrealis");
   public static readonly EditableRequireForbidField PastTenseStem = MatchingRule(MatchingRules.PastTenseStem, "Past tense stem");
   public static readonly EditableRequireForbidField DictionaryFormStart = RequireForbidRule("dictionaryFormStart", "Dictionary form (start)", it => it.DictionaryFormStart);
   public static readonly EditableRequireForbidField DictionaryFormHead = MatchingRule(MatchingRules.DictionaryFormHead, "Dictionary form (head)");
   public static readonly EditableRequireForbidField AdverbHead = MatchingRule(MatchingRules.AdverbHead, "Adverb (head)");
   public static readonly EditableRequireForbidField TeFormStemStart = MatchingRule(MatchingRules.TeFormStemStart, "て-form stem (start)");
   public static readonly EditableRequireForbidField TeFormStemHead = MatchingRule(MatchingRules.TeFormStemHead, "て-form stem (head)");

   public static readonly EditableFlagField<VocabNote> IsInflectingWord = MatchingFlag("isInflectingWord", "Inflecting word", it => it.IsInflectingWord, changesHowSentencesParse: true);
   public static readonly EditableFlagField<VocabNote> IsPoisonWord = MatchingFlag("isPoisonWord", "Poison word", it => it.IsPoisonWord, changesHowSentencesParse: true);

   public static readonly EditableFlagField<VocabNote> MatchWithPrecedingVowel =
      MatchingFlag("matchWithPrecedingVowel", "Match with preceding vowel", it => it.MatchWithPrecedingVowel, changesHowSentencesParse: true);

   public static readonly EditableFlagField<VocabNote> QuestionOverridesForm =
      MatchingFlag("questionOverridesForm",
                   "Question overrides form: Show the question in results even if the match was another form",
                   it => it.QuestionOverridesForm,
                   changesHowSentencesParse: true);

   public static readonly EditableTextSetField<VocabNote> PrefixIsNot = MatchingRule(MatchingRules.PrefixIsNot, "Prefix is not");
   public static readonly EditableTextSetField<VocabNote> RequiredPrefix = MatchingRule(MatchingRules.RequiredPrefix, "Required prefix");
   public static readonly EditableTextSetField<VocabNote> SuffixIsNot = MatchingRule(MatchingRules.SuffixIsNot, "Suffix is not");
   public static readonly EditableTextSetField<VocabNote> SurfaceIsNot = MatchingRule(MatchingRules.SurfaceIsNot, "Surface is not");

   /// <summary>The vocab's <see cref="VocabNoteMatchingConfiguration.YieldToSurface"/> rule, a rule on display.</summary>
   public static readonly EditableTextSetField<VocabNote> YieldToSurface =
      new("matching.yieldToSurface",
          "Yield to surface",
          it => it.MatchingConfiguration.YieldToSurface.Surfaces,
          (it, surface) => it.MatchingConfiguration.YieldToSurface.Add(surface),
          (it, surface) => it.MatchingConfiguration.YieldToSurface.Remove(surface),
          changesHowSentencesParse: true);

   /// <summary>
   /// The vocab's whole <see cref="VocabNoteMatchingConfiguration.MatchCondition"/>, as JSON text (<see cref="MatchConditionJson.AsJsonText"/>): empty for none.<br/>
   /// It can hold any condition, where the matching rules above edit only one made of rules. A condition that isn't an "all of" is made the only one in one.
   /// </summary>
   public static readonly EditableTextField<VocabNote> MatchCondition =
      new("matching.condition",
          "Match-condition",
          TextFieldFormat.Paragraphs,
          it => MatchConditionJson.AsJsonText(it.MatchingConfiguration.MatchCondition),
          (it, text) => it.MatchingConfiguration.SetMatchCondition(MatchConditionJson.VocabsMatchConditionFromJsonText(text)),
          changesHowSentencesParse: true);

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
   public static readonly EditableSetField<VocabNote, WordReference> Synonyms = Relationship("relatedVocab.synonyms", "Synonyms", it => it.Synonyms);

   /// <summary>
   /// Perfect synonyms share one answer: adding one gives it the vocab's answer, and links every vocab of the two webs of perfect synonyms to every other,<br/>
   /// as perfect synonyms and as synonyms (<see cref="RelatedVocab.PerfectSynonyms"/>).
   /// </summary>
   public static readonly EditableSetField<VocabNote, WordReference> PerfectSynonyms =
      new("relatedVocab.perfectSynonyms",
          "Perfect synonyms, which share the answer",
          it => it.RelatedNotes.PerfectSynonyms.Words,
          (it, synonym) => it.RelatedNotes.PerfectSynonyms.AddOverwritingTheAnswerOfTheAddedSynonym(synonym),
          (it, synonym) => it.RelatedNotes.PerfectSynonyms.Remove(synonym),
          WordReference.InAFixedOrder);

   /// <summary>The vocab's <see cref="Vocabulary.RelatedVocab.RelatedVocab.Antonyms"/>, linked both ways as synonyms are.</summary>
   public static readonly EditableSetField<VocabNote, WordReference> Antonyms = Relationship("relatedVocab.antonyms", "Antonyms", it => it.Antonyms);

   /// <summary>The words the vocab is easily confused with (<see cref="Vocabulary.RelatedVocab.RelatedVocab.ConfusedWith"/>): only this vocab knows of them.</summary>
   public static readonly EditableSetField<VocabNote, WordReference> ConfusedWith = Relationship("relatedVocab.confusedWith", "Confused with", it => it.ConfusedWith);

   /// <summary>The words worth looking at alongside the vocab (<see cref="Vocabulary.RelatedVocab.RelatedVocab.SeeAlso"/>), linked both ways as synonyms are.</summary>
   public static readonly EditableSetField<VocabNote, WordReference> SeeAlso = Relationship("relatedVocab.seeAlso", "See also", it => it.SeeAlso);

   /// <summary>The vocab's transitive or intransitive counterparts (<see cref="Vocabulary.RelatedVocab.RelatedVocab.ErgativeTwins"/>), such as 開ける for 開く: adding or removing one<br/>
   /// adds or removes the vocab as the twin's twin too.</summary>
   public static readonly EditableSetField<VocabNote, WordReference> ErgativeTwins = Relationship("relatedVocab.ergativeTwins", "Ergative twins", it => it.ErgativeTwins);

   /// <summary>
   /// The word the vocab is derived from, by its <see cref="WordReference.DisambiguationName"/>: empty for none. Text that names no word, as <c>:</c> typed<br/>
   /// alone does, is none too, since the editor writes every keystroke.
   /// </summary>
   public static readonly EditableTextField<VocabNote> DerivedFrom =
      new("relatedVocab.derivedFrom",
          "Derived from",
          TextFieldFormat.Line,
          it => it.RelatedNotes.DerivedFrom.Word?.DisambiguationName ?? "",
          (it, name) => it.RelatedNotes.DerivedFrom.Set(WordReference.FromDisambiguationNameOrNone(name.Trim())));

   /// <summary>Every field above, in the same order.</summary>
   public static IReadOnlyList<IEditableField<VocabNote>> All { get; } =
   [
      Question, Disambiguation, Readings, Forms, PartsOfSpeech, Answer, Explanation, LongExplanation, Mnemonic, CompoundParts, AlternativeCompoundParts,
      TechnicalNotes, SourceAnswer, SourceMnemonic, SourceReadingMnemonic,
      YieldLastToken, Surface, SentenceEnd, SentenceStart, SingleToken, GodanImperative, GodanImperativeHead, GodanPotential, IchidanImperative, MasuStem,
      Godan, Ichidan, Irrealis, PastTenseStem, DictionaryFormStart, DictionaryFormHead, AdverbHead, TeFormStemStart, TeFormStemHead,
      IsInflectingWord, IsPoisonWord, MatchWithPrecedingVowel, QuestionOverridesForm, PrefixIsNot, RequiredPrefix, SuffixIsNot, SurfaceIsNot, YieldToSurface,
      MatchCondition, UsuallyWrittenInKana, IsCompositionallyTransparentCompound,
      Archaic, Childish, Derogatory, Formal, Honorific, Humble, Informal, Literary, Polite, RoughMasculine, Sensitive, Slang, SoftFeminine, Vulgar,
      Tags,
      Synonyms, PerfectSynonyms, Antonyms, ConfusedWith, SeeAlso, ErgativeTwins, DerivedFrom
   ];

   /// <summary>One of the vocab's relationships to other words (<see cref="WordRelationship"/>), named <paramref name="name"/> in the corpus.</summary>
   static EditableSetField<VocabNote, WordReference> Relationship(string name, string label, Func<RelatedVocab.RelatedVocab, WordRelationship> relationship) =>
      new(name,
          label,
          it => relationship(it.RelatedNotes).Words,
          (it, word) => relationship(it.RelatedNotes).Add(word),
          (it, word) => relationship(it.RelatedNotes).Remove(word),
          WordReference.InAFixedOrder);

   static List<string> DisambiguationNamesOf(IEnumerable<WordReference> words) => [.. words.Select(it => it.DisambiguationName)];

   /// <summary>One of the vocab's require/forbid rules that aren't in its match-condition (<see cref="VocabMatchingRequireForbidFlags"/>), named <paramref name="name"/> in the corpus.</summary>
   static EditableRequireForbidField RequireForbidRule(string name, string label, Func<VocabMatchingRequireForbidFlags, RequireForbidFlagField> rule) =>
      new($"matching.requiredOrForbidden.{name}",
          label,
          it => rule(it.MatchingConfiguration.RequiresForbids).Choice,
          (it, choice) => rule(it.MatchingConfiguration.RequiresForbids).Choose(choice));

   /// <summary>A matching rule of the vocab's match-condition that a vocab requires or forbids (<see cref="MatchConditionAsRules"/>).</summary>
   static EditableRequireForbidField MatchingRule(RequireForbidMatchingRule rule, string label) =>
      new($"matching.rules.{rule.Name}", label, it => it.MatchingConfiguration.Rules.ChoiceOf(rule), (it, choice) => it.MatchingConfiguration.Rules.Choose(rule, choice));

   /// <summary>One of the vocab's matching flags (<see cref="VocabMatchingFlags"/>), named <paramref name="name"/> in the corpus.</summary>
   static EditableFlagField<VocabNote> MatchingFlag(string name, string label, Func<VocabMatchingFlags, FlagField> flag, bool changesHowSentencesParse) =>
      new($"matching.flags.{name}", label, it => flag(it.MatchingConfiguration.Flags), changesHowSentencesParse);

   /// <summary>A matching rule of the vocab's match-condition that a vocab gives texts (<see cref="MatchConditionAsRules"/>).</summary>
   static EditableTextSetField<VocabNote> MatchingRule(TextsMatchingRule rule, string label) =>
      new($"matching.rules.{rule.Name}",
          label,
          it => it.MatchingConfiguration.Rules.TextsOf(rule),
          (it, text) => it.MatchingConfiguration.Rules.AddText(rule, text),
          (it, text) => it.MatchingConfiguration.Rules.RemoveText(rule, text),
          changesHowSentencesParse: true);

   /// <summary>One of the vocab's register flags (<see cref="VocabNoteRegister"/>), such as whether it's formal, named <paramref name="name"/> in the corpus.</summary>
   static EditableFlagField<VocabNote> RegisterFlag(string name, string label, Func<VocabNoteRegister, FlagField> flag) =>
      new($"register.{name}", label, it => flag(it.Register));
}
