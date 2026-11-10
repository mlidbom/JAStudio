using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.EditableFields;
using JAStudio.Core.Note.KanjiReadings;
using JAStudio.Core.Note.KanjiRelationships;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Core.Note;

/// <summary>
/// Every field of a kanji that a person edits in the kanji's editor in the workspace (<see cref="IEditableField{TNote}"/>), in the editor's order:<br/>
/// the kanji, its answer and mnemonic; its readings; its radicals, the kanji it's related to and its primary vocab; what its source gave it; its tags.
/// </summary>
/// <remarks>
/// The kanji's editor is built from these fields, and JAStudio's MCP server edits <see cref="All"/> of them, so the two edit the same fields.
/// </remarks>
public static class KanjiEditableFields
{
   public static readonly EditableTextField<KanjiNote> Question = new("kanji", "Question", TextFieldFormat.Line, it => it.GetQuestion(), (it, question) => it.SetQuestion(question));

   /// <summary>The corpus's own answer, which takes the place of the source's (<see cref="SourceAnswer"/>).</summary>
   public static readonly EditableTextField<KanjiNote> Answer = new("corpusAnswer", "Answer", TextFieldFormat.Line, it => it.CorpusAnswer);

   public static readonly EditableTextField<KanjiNote> Mnemonic = new("mnemonic", "Mnemonic", TextFieldFormat.Paragraphs, it => it.Mnemonic);

   public static readonly EditableKanjiReadingsField OnReadings = new("readings.on", KanjiReadingKind.On);
   public static readonly EditableKanjiReadingsField KunReadings = new("readings.kun", KanjiReadingKind.Kun);
   public static readonly EditableKanjiReadingsField NanoriReadings = new("readings.nanori", KanjiReadingKind.Nanori);

   /// <summary>The radicals the kanji is made of (<see cref="KanjiNote.Radicals"/>), in their order, each as often as the kanji has it.</summary>
   public static readonly EditableListField<KanjiNote, string> Radicals =
      new("radicals",
          "Radicals",
          it => it.Radicals.All,
          (it, radical) => it.Radicals.Add(radical),
          (it, radical) => it.Radicals.Remove(radical),
          (it, radicals) => it.Radicals.Set(radicals));

   /// <summary>Other kanji with a similar meaning (<see cref="KanjiNote.SimilarMeaning"/>): adding or removing one changes that kanji's too.</summary>
   public static readonly EditableSetField<KanjiNote, string> SimilarMeaning = Relationship("similarMeaning", "Similar meaning", it => it.SimilarMeaning);

   /// <summary>Other kanji the kanji is easily confused with (<see cref="KanjiNote.ConfusedWith"/>): adding or removing one changes that kanji's too.</summary>
   public static readonly EditableSetField<KanjiNote, string> ConfusedWith = Relationship("confusedWith", "Confused with", it => it.ConfusedWith);

   /// <summary>
   /// The vocab the kanji's page shows first (<see cref="KanjiNote.PrimaryVocab"/>), in their order, each typed by its name, such as <c>入る:masu</c>,<br/>
   /// as a vocab's compound parts are.
   /// </summary>
   public static readonly EditableTextListField<KanjiNote> PrimaryVocab =
      new("primaryVocab", "Primary vocab", it => [.. it.PrimaryVocab.Words.Select(word => word.DisambiguationName)], (it, names) => it.PrimaryVocab.Set(WordReference.TheWordsNamedBy(names)));

   /// <summary>The answer the kanji's source gave it (<see cref="KanjiNote.SourceData"/>).</summary>
   public static readonly EditableTextField<KanjiNote> SourceAnswer = new("sourceData.answer", "Answer", TextFieldFormat.Paragraphs, it => it.SourceData.Answer);

   /// <summary>The meaning mnemonic the kanji's source gave it, never shown on a card.</summary>
   public static readonly EditableTextField<KanjiNote> SourceMeaningMnemonic =
      new("sourceData.meaningMnemonic", "Meaning mnemonic", TextFieldFormat.Paragraphs, it => it.SourceData.MeaningMnemonic);

   public static readonly EditableTagsField<KanjiNote> Tags = new();

   /// <summary>Every field above, in the same order.</summary>
   public static IReadOnlyList<IEditableField<KanjiNote>> All { get; } =
   [
      Question, Answer, Mnemonic, OnReadings, KunReadings, NanoriReadings, Radicals, SimilarMeaning, ConfusedWith, PrimaryVocab, SourceAnswer, SourceMeaningMnemonic, Tags
   ];

   /// <summary>
   /// One of the kanji's relationships to other kanji (<see cref="MutualKanjiRelationship"/>), named <paramref name="name"/> in the corpus, edited through the<br/>
   /// relationship's own operations.
   /// </summary>
   static EditableSetField<KanjiNote, string> Relationship(string name, string label, Func<KanjiNote, MutualKanjiRelationship> relationship) =>
      new(name,
          label,
          it => relationship(it).Kanji,
          (it, kanji) => relationship(it).Add(kanji),
          (it, kanji) => relationship(it).Remove(kanji),
          InTheOrderTheyWereAdded);

   /// <summary><paramref name="kanji"/> as a <see cref="MutualKanjiRelationship"/> holds them: in the order they were added (<see cref="MutualKanjiRelationship.Kanji"/>).</summary>
   static IEnumerable<string> InTheOrderTheyWereAdded(IEnumerable<string> kanji) => kanji;
}
