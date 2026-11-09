using System;
using System.Collections.Generic;
using JAStudio.Core;
using JAStudio.Core.Note;
using JAStudio.Core.Note.EditableFields;
using JAStudio.Core.Note.NoteFields;
using JAStudio.Core.TaskRunners;
using JAStudio.UI.Controls.NoteFields;
using JAStudio.UI.ViewModels.Workspace.Tagging;
using JAStudio.UI.Views.Workspace.Editors;

namespace JAStudio.UI.ViewModels.Workspace.Editors;

/// <summary>
/// What the workspace's editor section shows for a kanji (<see cref="KanjiEditorSection"/>): its text fields and its readings, edited live.<br/>
/// Every keystroke, and every change to the readings, is written to the note at once.
/// </summary>
/// <remarks>
/// Every field and the readings read and write through <see cref="CurrentKanji"/>, the kanji as the collection holds it now. See <see cref="INoteEditorSectionViewModel"/>.
/// </remarks>
class KanjiEditorSectionViewModel : INoteEditorSectionViewModel
{
   readonly TemporaryServiceCollection _services;
   readonly IFatalErrorHandler _fatalErrorHandler;
   readonly NoteId _kanjiId;

   /// <param name="services">The application's services.</param>
   /// <param name="kanjiId">The kanji to edit.</param>
   public KanjiEditorSectionViewModel(TemporaryServiceCollection services, NoteId kanjiId)
   {
      _services = services;
      _fatalErrorHandler = services.Resolve<IFatalErrorHandler>();
      _kanjiId = kanjiId;

      CoreFields =
      [
         Field("Question", TextFieldFormat.Line, it => it.GetQuestion(), (it, question) => it.SetQuestion(question)),
         Field("Answer", TextFieldFormat.Line, it => it.CorpusAnswer),
         Field("Mnemonic", TextFieldFormat.Paragraphs, it => it.Mnemonic)
      ];
      Readings =
      [
         new KanjiReadingListEditorViewModel(_fatalErrorHandler, () => CurrentKanji().Readings.On),
         new KanjiReadingListEditorViewModel(_fatalErrorHandler, () => CurrentKanji().Readings.Kun),
         new KanjiReadingListEditorViewModel(_fatalErrorHandler, () => CurrentKanji().Readings.Nanori)
      ];
      RelatedFields =
      [
         Field("Radicals", TextFieldFormat.CommaSeparated, it => it.RadicalsRaw, (it, radicals) => it.RadicalsRaw = radicals),
         Field("Primary vocab", TextFieldFormat.CommaSeparated, it => it.PrimaryVocabRaw, (it, primaryVocab) => it.PrimaryVocabRaw = primaryVocab),
         Field("Similar meaning", TextFieldFormat.CommaSeparated, it => it.SimilarMeaningRaw, (it, similarMeaning) => it.SimilarMeaningRaw = similarMeaning),
         Field("Confused with", TextFieldFormat.CommaSeparated, it => it.RelatedConfusedWithRaw, (it, confusedWith) => it.RelatedConfusedWithRaw = confusedWith)
      ];
      SourceFields =
      [
         Field("Answer", TextFieldFormat.Paragraphs, it => it.SourceData.Answer),
         Field("Meaning mnemonic", TextFieldFormat.Paragraphs, it => it.SourceData.MeaningMnemonic)
      ];
      OtherFields =
      [
         Field("Image", TextFieldFormat.Line, it => it.Image.RawValue(), (it, image) => it.Image.SetRawValue(image))
      ];
      Tags = new NoteTagsEditorViewModel(services, CurrentKanji);
   }

   /// <summary>The kanji itself, and the corpus's own answer, which takes the place of the source's, and mnemonic for it.</summary>
   public IReadOnlyList<TextFieldEditorViewModel> CoreFields { get; }

   /// <summary>The kanji's on, kun and nanori readings (<see cref="KanjiNote.Readings"/>), each primary or secondary.</summary>
   public IReadOnlyList<KanjiReadingListEditorViewModel> Readings { get; }

   /// <summary>The radicals the kanji is made of, and the vocab and other kanji it's related to.</summary>
   public IReadOnlyList<TextFieldEditorViewModel> RelatedFields { get; }

   /// <summary>
   /// What the kanji's source gave it (<see cref="KanjiNote.SourceData"/>): its answer, which the corpus's own in <see cref="CoreFields"/> takes the place of,<br/>
   /// and its meaning mnemonic, which is never shown on a card.
   /// </summary>
   public IReadOnlyList<TextFieldEditorViewModel> SourceFields { get; }

   /// <summary>The kanji's image, as Anki's <c>&lt;img src="…"&gt;</c> markup (<see cref="KanjiNote.Image"/>).</summary>
   public IReadOnlyList<TextFieldEditorViewModel> OtherFields { get; }

   public NoteTagsEditorViewModel Tags { get; }

   public void Refresh()
   {
      foreach(var field in AllFields()) field.Refresh();
      foreach(var readings in Readings) readings.Refresh();
      Tags.Refresh();
   }

   IEnumerable<TextFieldEditorViewModel> AllFields() => [..CoreFields, ..RelatedFields, ..SourceFields, ..OtherFields];

   TextFieldEditorViewModel Field(string label, TextFieldFormat format, Func<KanjiNote, WritableStringValue> field) =>
      Field(label, format, it => field(it).Value, (it, text) => field(it).Set(text));

   TextFieldEditorViewModel Field(string label, TextFieldFormat format, Func<KanjiNote, string> read, Action<KanjiNote, string> write) =>
      new(_fatalErrorHandler,
          label,
          format,
          () => read(CurrentKanji()),
          text =>
          {
             var kanji = CurrentKanji();
             write(kanji, text);
             kanji.UpdateGeneratedData();
          });

   /// <summary>The kanji being edited, as the collection holds it now.</summary>
   KanjiNote CurrentKanji() => _services.CoreApp.Collection.Kanji.WithIdOrNone(_kanjiId)
                            ?? throw new InvalidOperationException($"The kanji being edited, {_kanjiId}, is no longer in the collection.");
}
