using System;
using System.Collections.Generic;
using JAStudio.Core;
using JAStudio.Core.Note;
using JAStudio.Core.Note.EditableFields;
using JAStudio.Core.Note.KanjiReadings;
using JAStudio.Core.Storage.Media;
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
         Field(KanjiEditableFields.Question),
         Field(KanjiEditableFields.Answer),
         Field(KanjiEditableFields.Mnemonic)
      ];
      Readings =
      [
         ReadingList(KanjiEditableFields.OnReadings),
         ReadingList(KanjiEditableFields.KunReadings),
         ReadingList(KanjiEditableFields.NanoriReadings)
      ];
      RelatedFields =
      [
         Field(KanjiEditableFields.Radicals),
         Field(KanjiEditableFields.PrimaryVocab),
         Field(KanjiEditableFields.SimilarMeaning),
         Field(KanjiEditableFields.ConfusedWith)
      ];
      SourceFields =
      [
         Field(KanjiEditableFields.SourceAnswer),
         Field(KanjiEditableFields.SourceMeaningMnemonic)
      ];
      Images = new ImageListEditorViewModel("Images", () => CurrentKanji().Images, services.Resolve<MediaFileIndex>());
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

   /// <summary>The kanji's images (<see cref="KanjiNote.Images"/>), all the corpus's own, shown with the kanji's other own data in its core section.</summary>
   public ImageListEditorViewModel Images { get; }

   public NoteTagsEditorViewModel Tags { get; }

   public void Refresh()
   {
      foreach(var field in AllFields()) field.Refresh();
      foreach(var readings in Readings) readings.Refresh();
      Images.Refresh();
      Tags.Refresh();
   }

   IEnumerable<TextFieldEditorViewModel> AllFields() => [..CoreFields, ..RelatedFields, ..SourceFields];

   TextFieldEditorViewModel Field(EditableTextField<KanjiNote> field) =>
      new(_fatalErrorHandler, field.Label, field.Format, () => field.Read(CurrentKanji()), WritingToTheCurrentKanji(field.Write));

   /// <summary>A list of texts, edited as the texts separated by commas.</summary>
   TextFieldEditorViewModel Field(EditableTextListField<KanjiNote> field) =>
      new(_fatalErrorHandler,
          field.Label,
          TextFieldFormat.CommaSeparated,
          () => string.Join(", ", field.Read(CurrentKanji())),
          WritingToTheCurrentKanji((it, text) => field.Write(it, StringExtensions.ExtractCommaSeparatedValues(text))));

   KanjiReadingListEditorViewModel ReadingList(EditableKanjiReadingsField field) => new(_fatalErrorHandler, () => field.ReadingsOn(CurrentKanji()));

   /// <summary>Writes a field's text with <paramref name="write"/> to the kanji as the collection holds it now, and updates the kanji's generated data.</summary>
   Action<string> WritingToTheCurrentKanji(Action<KanjiNote, string> write) =>
      text =>
      {
         var kanji = CurrentKanji();
         write(kanji, text);
         kanji.UpdateGeneratedData();
      };

   /// <summary>The kanji being edited, as the collection holds it now.</summary>
   KanjiNote CurrentKanji() => _services.CoreApp.Collection.Kanji.WithIdOrNone(_kanjiId)
                            ?? throw new InvalidOperationException($"The kanji being edited, {_kanjiId}, is no longer in the collection.");
}
