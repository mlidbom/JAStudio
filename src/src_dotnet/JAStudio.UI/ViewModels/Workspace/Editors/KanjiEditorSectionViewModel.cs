using System;
using System.Collections.Generic;
using JAStudio.Core;
using JAStudio.Core.Note;
using JAStudio.Core.Note.NoteFields;
using JAStudio.Core.TaskRunners;
using JAStudio.UI.Controls.NoteFields;
using JAStudio.UI.Views.Workspace.Editors;

namespace JAStudio.UI.ViewModels.Workspace.Editors;

/// <summary>
/// What the workspace's editor section shows for a kanji (<see cref="KanjiEditorSection"/>): its text fields, edited live.<br/>
/// Every keystroke is written to the note at once, and the kanji's generated data, such as its active answer, is updated with it.
/// </summary>
/// <remarks>
/// Every field reads and writes through <see cref="CurrentKanji"/>, the kanji as the collection holds it now. See <see cref="INoteEditorSectionViewModel"/>.
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
         Field("Answer", TextFieldFormat.Line, it => it.UserAnswer),
         Field("Mnemonic", TextFieldFormat.Paragraphs, it => it.UserMnemonic)
      ];
      ReadingFields =
      [
         Field("On", TextFieldFormat.CommaSeparatedReadings, it => it.ReadingOnHtml),
         Field("Kun", TextFieldFormat.CommaSeparatedReadings, it => it.ReadingKunHtml),
         Field("Nanori", TextFieldFormat.CommaSeparated, it => it.ReadingNanHtml)
      ];
      RelatedFields =
      [
         Field("Radicals", TextFieldFormat.CommaSeparated, it => it.RadicalsRaw, (it, radicals) => it.RadicalsRaw = radicals),
         Field("Primary vocab", TextFieldFormat.CommaSeparated, it => it.PrimaryVocabRaw, (it, primaryVocab) => it.PrimaryVocabRaw = primaryVocab),
         Field("Similar meaning", TextFieldFormat.CommaSeparated, it => it.UserSimilarMeaningRaw, (it, similarMeaning) => it.UserSimilarMeaningRaw = similarMeaning),
         Field("Confused with", TextFieldFormat.CommaSeparated, it => it.RelatedConfusedWithRaw, (it, confusedWith) => it.RelatedConfusedWithRaw = confusedWith)
      ];
      SourceFields =
      [
         Field("Answer", TextFieldFormat.Paragraphs, it => it.SourceAnswer),
         Field("Meaning mnemonic", TextFieldFormat.Paragraphs, it => it.SourceMeaningMnemonic)
      ];
   }

   /// <summary>The kanji itself, and the user's answer and mnemonic for it.</summary>
   public IReadOnlyList<TextFieldEditorViewModel> CoreFields { get; }

   public IReadOnlyList<TextFieldEditorViewModel> ReadingFields { get; }

   /// <summary>The radicals the kanji is made of, and the vocab and other kanji it's related to.</summary>
   public IReadOnlyList<TextFieldEditorViewModel> RelatedFields { get; }

   /// <summary>The answer and mnemonic the kanji came with. The user's own, in <see cref="CoreFields"/>, take precedence over them.</summary>
   public IReadOnlyList<TextFieldEditorViewModel> SourceFields { get; }

   public void Refresh()
   {
      foreach(var field in AllFields()) field.Refresh();
   }

   IEnumerable<TextFieldEditorViewModel> AllFields() => [..CoreFields, ..ReadingFields, ..RelatedFields, ..SourceFields];

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
