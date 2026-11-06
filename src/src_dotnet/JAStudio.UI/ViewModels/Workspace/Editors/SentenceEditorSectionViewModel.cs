using System;
using System.Collections.Generic;
using JAStudio.Core;
using JAStudio.Core.Note;
using JAStudio.Core.Note.NoteFields;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.TaskRunners;
using JAStudio.UI.Controls.NoteFields;
using JAStudio.UI.Views.Workspace.Editors;

namespace JAStudio.UI.ViewModels.Workspace.Editors;

/// <summary>
/// What the workspace's editor section shows for a sentence (<see cref="SentenceEditorSection"/>): its text fields, edited live.<br/>
/// Every keystroke is written to the note at once, and the sentence's generated data is updated with it:<br/>
/// a change to its question reparses the sentence, which is quick enough to do on every keystroke.
/// </summary>
/// <remarks>
/// Every field reads and writes through <see cref="CurrentSentence"/>, the sentence as the collection holds it now. See <see cref="INoteEditorSectionViewModel"/>.
/// </remarks>
class SentenceEditorSectionViewModel : INoteEditorSectionViewModel
{
   readonly TemporaryServiceCollection _services;
   readonly IFatalErrorHandler _fatalErrorHandler;
   readonly NoteId _sentenceId;

   /// <param name="services">The application's services.</param>
   /// <param name="sentenceId">The sentence to edit.</param>
   public SentenceEditorSectionViewModel(TemporaryServiceCollection services, NoteId sentenceId)
   {
      _services = services;
      _fatalErrorHandler = services.Resolve<IFatalErrorHandler>();
      _sentenceId = sentenceId;

      UserFields =
      [
         Field("Question", TextFieldFormat.Paragraphs, it => it.User.Question),
         Field("Answer", TextFieldFormat.Paragraphs, it => it.User.Answer),
         Field("Comments", TextFieldFormat.Paragraphs, it => it.User.Comments),
         Field("Reading", TextFieldFormat.Line, it => it.Reading)
      ];
      SourceFields =
      [
         Field("Question", TextFieldFormat.Paragraphs, it => it.SourceQuestion),
         Field("Answer", TextFieldFormat.Paragraphs, it => it.SourceAnswer),
         Field("Comments", TextFieldFormat.Paragraphs, it => it.SourceComments)
      ];
      OtherFields =
      [
         Field("Audio", TextFieldFormat.Line, it => it.Audio.RawValue(), (it, audio) => it.Audio.SetRawValue(audio)),
         Field("Screenshot", TextFieldFormat.Line, it => it.Screenshot.RawValue(), (it, screenshot) => it.Screenshot.SetRawValue(screenshot)),
         Field("External id", TextFieldFormat.Line, it => it.ExternalId)
      ];
   }

   /// <summary>The user's own question, answer and comments, which take precedence over the <see cref="SourceFields"/>, and the reading.</summary>
   public IReadOnlyList<TextFieldEditorViewModel> UserFields { get; }

   /// <summary>The question, answer and comments the sentence came with.</summary>
   public IReadOnlyList<TextFieldEditorViewModel> SourceFields { get; }

   /// <summary>The sentence's audio and screenshot, as Anki's <c>[sound:file]</c> and image references, and the id of where it came from.</summary>
   public IReadOnlyList<TextFieldEditorViewModel> OtherFields { get; }

   public void Refresh()
   {
      foreach(var field in AllFields()) field.Refresh();
   }

   IEnumerable<TextFieldEditorViewModel> AllFields() => [..UserFields, ..SourceFields, ..OtherFields];

   TextFieldEditorViewModel Field(string label, TextFieldFormat format, Func<SentenceNote, WritableStringValue> field) =>
      Field(label, format, it => field(it).Value, (it, text) => field(it).Set(text));

   TextFieldEditorViewModel Field(string label, TextFieldFormat format, Func<SentenceNote, string> read, Action<SentenceNote, string> write) =>
      new(_fatalErrorHandler,
          label,
          format,
          () => read(CurrentSentence()),
          text =>
          {
             var sentence = CurrentSentence();
             write(sentence, text);
             sentence.UpdateGeneratedData();
          });

   /// <summary>The sentence being edited, as the collection holds it now.</summary>
   SentenceNote CurrentSentence() => _services.CoreApp.Collection.Sentences.WithIdOrNone(_sentenceId)
                                  ?? throw new InvalidOperationException($"The sentence being edited, {_sentenceId}, is no longer in the collection.");
}
