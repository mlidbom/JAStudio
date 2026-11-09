using System.Collections.Generic;
using JAStudio.Core.Note.EditableFields;
using JAStudio.Core.Note.KanjiReadings;

namespace JAStudio.Core.Note;

/// <summary>
/// Every field of a kanji that a person edits in the kanji's editor in the workspace (<see cref="IEditableField{TNote}"/>), in the editor's order:<br/>
/// the kanji, its answer and mnemonic; its readings; its radicals and related vocab and kanji; what its source gave it; its image; its tags.
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

   /// <summary>The radicals the kanji is made of.</summary>
   public static readonly EditableTextListField<KanjiNote> Radicals =
      new("radicals", "Radicals", it => StringExtensions.ExtractCommaSeparatedValues(it.RadicalsRaw), (it, radicals) => it.RadicalsRaw = string.Join(", ", radicals));

   /// <summary>The vocab the kanji's card shows as its primary vocab.</summary>
   public static readonly EditableTextListField<KanjiNote> PrimaryVocab =
      new("primaryVocab", "Primary vocab", it => StringExtensions.ExtractCommaSeparatedValues(it.PrimaryVocabRaw), (it, vocab) => it.PrimaryVocabRaw = string.Join(", ", vocab));

   /// <summary>Other kanji with a similar meaning.</summary>
   public static readonly EditableTextListField<KanjiNote> SimilarMeaning =
      //todo: Harmonize with the menu (Magnus, 2026-11-09): the editor writes this list as free text, one way only, while the menu's "Similar meaning"
      //   (KanjiCommands.AddSimilarMeaning) adds a kanji both ways. The free-text field should go, the editor should add and remove kanji as the menu does,
      //   and the menu item should then probably go.
      new("similarMeaning", "Similar meaning", it => StringExtensions.ExtractCommaSeparatedValues(it.SimilarMeaningRaw), (it, kanji) => it.SimilarMeaningRaw = string.Join(", ", kanji));

   /// <summary>Other kanji the kanji is confused with.</summary>
   public static readonly EditableTextListField<KanjiNote> ConfusedWith =
      new("confusedWith", "Confused with", it => StringExtensions.ExtractCommaSeparatedValues(it.RelatedConfusedWithRaw), (it, kanji) => it.RelatedConfusedWithRaw = string.Join(", ", kanji));

   /// <summary>The answer the kanji's source gave it (<see cref="KanjiNote.SourceData"/>).</summary>
   public static readonly EditableTextField<KanjiNote> SourceAnswer = new("sourceData.answer", "Answer", TextFieldFormat.Paragraphs, it => it.SourceData.Answer);

   /// <summary>The meaning mnemonic the kanji's source gave it, never shown on a card.</summary>
   public static readonly EditableTextField<KanjiNote> SourceMeaningMnemonic =
      new("sourceData.meaningMnemonic", "Meaning mnemonic", TextFieldFormat.Paragraphs, it => it.SourceData.MeaningMnemonic);

   /// <summary>The kanji's image, as Anki's <c>&lt;img src="…"&gt;</c> markup (<see cref="KanjiNote.Image"/>).</summary>
   public static readonly EditableTextField<KanjiNote> Image = new("image", "Image", TextFieldFormat.Line, it => it.Image.RawValue(), (it, image) => it.Image.SetRawValue(image));

   public static readonly EditableTagsField<KanjiNote> Tags = new();

   /// <summary>Every field above, in the same order.</summary>
   public static IReadOnlyList<IEditableField<KanjiNote>> All { get; } =
   [
      Question, Answer, Mnemonic, OnReadings, KunReadings, NanoriReadings, Radicals, PrimaryVocab, SimilarMeaning, ConfusedWith, SourceAnswer, SourceMeaningMnemonic, Image, Tags
   ];
}
