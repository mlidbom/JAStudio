using System;
using System.Collections.Generic;
using JAStudio.Core;
using JAStudio.Core.Note;
using JAStudio.Core.Note.EditableFields;
using JAStudio.Core.Note.NoteFields;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.TaskRunners;
using JAStudio.UI.Controls.NoteFields;
using JAStudio.UI.ViewModels.Workspace.Tagging;
using JAStudio.UI.Views.TextToSpeech;
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

      CorpusFields =
      [
         Field("Question", TextFieldFormat.Paragraphs, it => it.CorpusQuestion),
         Field("Answer", TextFieldFormat.Paragraphs, it => it.CorpusAnswer),
         Field("Comments", TextFieldFormat.Paragraphs, it => it.Comments),
         Field("Reading", TextFieldFormat.Line, it => it.Reading)
      ];
      SourceFields =
      [
         Field("Question", TextFieldFormat.Paragraphs, it => it.SourceData.Question),
         Field("Answer", TextFieldFormat.Paragraphs, it => it.SourceData.Answer),
         Field("Comments", TextFieldFormat.Paragraphs, it => it.SourceData.Comments),
         Field("External id", TextFieldFormat.Line, it => it.SourceData.ExternalId)
      ];
      OtherFields =
      [
         new TextToSpeechAudioFieldEditorViewModel(_fatalErrorHandler,
                                                   "Audio",
                                                   () => CurrentSentence().TextToSpeechAudio.RawValue(),
                                                   WritingToTheCurrentSentence((it, audio) => it.TextToSpeechAudio.SetRawValue(audio)),
                                                   () => TextToSpeechDialog.OpenFor(CurrentSentence(), _services)),
         Field("Screenshot", TextFieldFormat.Line, it => it.Screenshot.RawValue(), (it, screenshot) => it.Screenshot.SetRawValue(screenshot)),
         Field("Source name", TextFieldFormat.Line, it => it.SourceName)
      ];
      Tags = new NoteTagsEditorViewModel(services, CurrentSentence);
   }

   /// <summary>
   /// The corpus's own question and answer, which take the place of the source's, its comments, shown beside the source's, and the reading.
   /// </summary>
   public IReadOnlyList<TextFieldEditorViewModel> CorpusFields { get; }

   /// <summary>What the sentence's source gave it (<see cref="SentenceNote.SourceData"/>): its question, answer and comments, and the id it has there.</summary>
   public IReadOnlyList<TextFieldEditorViewModel> SourceFields { get; }

   /// <summary>
   /// The sentence's audio and screenshot, as Anki's <c>[sound:file]</c> and image references, and the name of the work or deck it came from<br/>
   /// (<see cref="SentenceNote.SourceName"/>).<br/>
   /// The audio is its TTS audio, with a button for generating new (<see cref="TextToSpeechAudioFieldEditorViewModel"/>).
   /// </summary>
   public IReadOnlyList<TextFieldEditorViewModel> OtherFields { get; }

   public NoteTagsEditorViewModel Tags { get; }

   public void Refresh()
   {
      foreach(var field in AllFields()) field.Refresh();
      Tags.Refresh();
   }

   IEnumerable<TextFieldEditorViewModel> AllFields() => [..CorpusFields, ..SourceFields, ..OtherFields];

   TextFieldEditorViewModel Field(string label, TextFieldFormat format, Func<SentenceNote, WritableStringValue> field) =>
      Field(label, format, it => field(it).Value, (it, text) => field(it).Set(text));

   TextFieldEditorViewModel Field(string label, TextFieldFormat format, Func<SentenceNote, string> read, Action<SentenceNote, string> write) =>
      new(_fatalErrorHandler, label, format, () => read(CurrentSentence()), WritingToTheCurrentSentence(write));

   /// <summary>Writes a field's text with <paramref name="write"/> to the sentence as the collection holds it now, and updates the sentence's generated data.</summary>
   Action<string> WritingToTheCurrentSentence(Action<SentenceNote, string> write) =>
      text =>
      {
         var sentence = CurrentSentence();
         write(sentence, text);
         sentence.UpdateGeneratedData();
      };

   /// <summary>The sentence being edited, as the collection holds it now.</summary>
   SentenceNote CurrentSentence() => _services.CoreApp.Collection.Sentences.WithIdOrNone(_sentenceId)
                                  ?? throw new InvalidOperationException($"The sentence being edited, {_sentenceId}, is no longer in the collection.");
}
