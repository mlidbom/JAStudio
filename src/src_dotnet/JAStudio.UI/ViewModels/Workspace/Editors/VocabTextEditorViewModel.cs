using System;
using System.Collections.Generic;
using JAStudio.Core;
using JAStudio.Core.Note.EditableFields;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Note.Vocabulary.EditableFields;
using JAStudio.Core.Storage.Media;
using JAStudio.Core.TaskRunners;
using JAStudio.UI.Controls.NoteFields;
using JAStudio.UI.Views.TextToSpeech;
using JAStudio.UI.Views.Workspace.Editors;

namespace JAStudio.UI.ViewModels.Workspace.Editors;

/// <summary>
/// The "Text" tab of a vocab's <see cref="VocabEditorSectionViewModel"/> (<see cref="VocabTextEditor"/>): its text fields, its forms and its parts of speech,<br/>
/// each one of the <see cref="VocabEditableFields"/>.<br/>
/// Most fields are written to the note on every keystroke, and the vocab's generated data, such as its active answer, is updated with them.
/// </summary>
/// <remarks>
/// The question, the forms and the parts of speech change what the vocab matches, so every change to them reparses the vocab's sentences.<br/>
/// One per keystroke would be far too many, so the question and its disambiguation are written when they're committed,<br/>
/// and the forms and the parts of speech are lists, where each added or removed entry is one change.
/// </remarks>
class VocabTextEditorViewModel
{
   readonly IFatalErrorHandler _fatalErrorHandler;
   readonly Func<VocabNote> _currentVocab;
   readonly VocabSentencesReparserViewModel _sentencesReparser;

   /// <param name="fatalErrorHandler">Shows the user an exception thrown while writing a change.</param>
   /// <param name="currentVocab">Finds the vocab being edited, as the collection holds it now.</param>
   /// <param name="sentencesReparser">Reparses the vocab's sentences after a change to what it matches.</param>
   /// <param name="openTheTextToSpeechDialog">Opens the <see cref="TextToSpeechDialog"/> for the vocab as the collection holds it now.</param>
   /// <param name="media">Finds the vocab's audio and images in the media layer, to play and show them.</param>
   public VocabTextEditorViewModel(IFatalErrorHandler fatalErrorHandler, Func<VocabNote> currentVocab, VocabSentencesReparserViewModel sentencesReparser, Action openTheTextToSpeechDialog, MediaFileIndex media)
   {
      _fatalErrorHandler = fatalErrorHandler;
      _currentVocab = currentVocab;
      _sentencesReparser = sentencesReparser;

      Question = CommittingField(VocabEditableFields.Question);
      Disambiguation = CommittingField(VocabEditableFields.Disambiguation);
      Readings = Field(VocabEditableFields.Readings);
      Forms = new EnteredFormsEditorViewModel(_currentVocab, _sentencesReparser.ReparseSentences);
      PartsOfSpeech = new PartsOfSpeechEditorViewModel(() => _currentVocab().PartsOfSpeech, _sentencesReparser.ReparseSentences);
      CorpusFields =
      [
         Field(VocabEditableFields.Answer),
         Field(VocabEditableFields.Explanation),
         Field(VocabEditableFields.LongExplanation),
         Field(VocabEditableFields.Mnemonic),
         Field(VocabEditableFields.CompoundParts),
         Field(VocabEditableFields.AlternativeCompoundParts)
      ];
      CorpusMedia =
      [
         new AudioListEditorViewModel("Audio", () => _currentVocab().Audio, media, EditorAudioPlayer.Instance, openTheTextToSpeechDialog),
         new ImageListEditorViewModel("Images", () => _currentVocab().Images, media)
      ];
      SourceMedia =
      [
         new AudioListEditorViewModel("Audio", () => _currentVocab().SourceData.Audio, media, EditorAudioPlayer.Instance),
         new ImageListEditorViewModel("Images", () => _currentVocab().SourceData.Images, media)
      ];
      TechnicalNotes = Field(VocabEditableFields.TechnicalNotes);
      SourceFields =
      [
         Field(VocabEditableFields.SourceAnswer),
         Field(VocabEditableFields.SourceMnemonic),
         Field(VocabEditableFields.SourceReadingMnemonic)
      ];
   }

   /// <summary>The vocab's question: its <see cref="VocabNoteQuestion.Text"/>.</summary>
   public CommittingTextFieldEditorViewModel Question { get; }

   /// <summary>The <see cref="VocabNoteQuestion.Disambiguation"/> that tells the vocab apart from others with the same question, if it has one.</summary>
   public CommittingTextFieldEditorViewModel Disambiguation { get; }

   public TextFieldEditorViewModel Readings { get; }

   /// <summary>The forms besides the question, which is always a form, each owned by the vocab or not.</summary>
   public EnteredFormsEditorViewModel Forms { get; }

   /// <summary>The vocab's parts of speech, of which only those JAStudio knows can be added.</summary>
   public PartsOfSpeechEditorViewModel PartsOfSpeech { get; }

   /// <summary>
   /// The corpus's own answer, which takes the place of the source's, its explanations and mnemonic,<br/>
   /// and the <see cref="VocabNoteCompoundParts.Primary"/> and <see cref="VocabNoteCompoundParts.Alternative"/> compound parts.
   /// </summary>
   public IReadOnlyList<TextFieldEditorViewModel> CorpusFields { get; }

   /// <summary>The corpus's own audio and images of the vocab, added since its source. The audio has a button for generating new TTS audio.</summary>
   public IReadOnlyList<MediaListEditorViewModel> CorpusMedia { get; }

   public TextFieldEditorViewModel TechnicalNotes { get; }

   /// <summary>What the vocab's source gave it (<see cref="VocabNote.SourceData"/>): its answer and mnemonics. The mnemonics are never shown on a card.</summary>
   public IReadOnlyList<TextFieldEditorViewModel> SourceFields { get; }

   /// <summary>The audio and images the vocab's source gave it (<see cref="VocabNote.SourceData"/>).</summary>
   public IReadOnlyList<MediaListEditorViewModel> SourceMedia { get; }

   /// <summary>Shows everything as the note has it now, after a change made elsewhere.</summary>
   public void Refresh()
   {
      Question.Refresh();
      Disambiguation.Refresh();
      Forms.Refresh();
      PartsOfSpeech.Refresh();
      foreach(var field in LiveTextFields()) field.Refresh();
      foreach(var list in (IEnumerable<MediaListEditorViewModel>)[..CorpusMedia, ..SourceMedia]) list.Refresh();
   }

   IEnumerable<TextFieldEditorViewModel> LiveTextFields() => [Readings, TechnicalNotes, ..CorpusFields, ..SourceFields];

   /// <summary>A text field that changes how sentences parse, written when it's committed, after which the vocab's sentences are reparsed.</summary>
   CommittingTextFieldEditorViewModel CommittingField(EditableTextField<VocabNote> field) =>
      new(_fatalErrorHandler, field.Label, () => field.Read(_currentVocab()), text =>
      {
         field.Write(_currentVocab(), text);
         _sentencesReparser.ReparseSentences();
      });

   TextFieldEditorViewModel Field(EditableTextField<VocabNote> field) =>
      new(_fatalErrorHandler, field.Label, field.Format, () => field.Read(_currentVocab()), WritingToTheCurrentVocab(field.Write));

   /// <summary>A list of texts, edited as the texts separated by commas.</summary>
   TextFieldEditorViewModel Field(EditableTextListField<VocabNote> field) =>
      new(_fatalErrorHandler,
          field.Label,
          TextFieldFormat.CommaSeparated,
          () => string.Join(", ", field.Read(_currentVocab())),
          WritingToTheCurrentVocab((it, text) => field.Write(it, StringExtensions.ExtractCommaSeparatedValues(text))));

   /// <summary>Writes a field's text with <paramref name="write"/> to the vocab as the collection holds it now, and updates the vocab's generated data.</summary>
   Action<string> WritingToTheCurrentVocab(Action<VocabNote, string> write) =>
      text =>
      {
         var vocab = _currentVocab();
         write(vocab, text);
         vocab.UpdateGeneratedData();
      };
}
