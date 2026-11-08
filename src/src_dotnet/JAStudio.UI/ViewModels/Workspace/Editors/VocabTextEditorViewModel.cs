using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core;
using JAStudio.Core.Note.NoteFields;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.TaskRunners;
using JAStudio.UI.Controls.NoteFields;
using JAStudio.UI.Views.TextToSpeech;
using JAStudio.UI.Views.Workspace.Editors;

namespace JAStudio.UI.ViewModels.Workspace.Editors;

/// <summary>
/// The "Text" tab of a vocab's <see cref="VocabEditorSectionViewModel"/> (<see cref="VocabTextEditor"/>): its text fields, its forms and its parts of speech.<br/>
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
   public VocabTextEditorViewModel(IFatalErrorHandler fatalErrorHandler, Func<VocabNote> currentVocab, VocabSentencesReparserViewModel sentencesReparser, Action openTheTextToSpeechDialog)
   {
      _fatalErrorHandler = fatalErrorHandler;
      _currentVocab = currentVocab;
      _sentencesReparser = sentencesReparser;

      Question = new CommittingTextFieldEditorViewModel(fatalErrorHandler, "Question", () => _currentVocab().Question.Text, CommitTheQuestion);
      Disambiguation = new CommittingTextFieldEditorViewModel(fatalErrorHandler, "Disambiguation", () => _currentVocab().Question.Disambiguation, CommitTheDisambiguation);
      Readings = Field("Readings", TextFieldFormat.CommaSeparated,
                       it => string.Join(", ", it.GetReadings()),
                       (it, readings) => it.SetReadings(StringExtensions.ExtractCommaSeparatedValues(readings)));
      Forms = new EnteredFormsEditorViewModel(_currentVocab, _sentencesReparser.ReparseSentences);
      PartsOfSpeech = new StringSetEditorViewModel("Parts of speech",
                                                    () => _currentVocab().PartsOfSpeech.Get(),
                                                    partOfSpeech => ChangeThePartsOfSpeech(it => it.Append(partOfSpeech)),
                                                    partOfSpeech => ChangeThePartsOfSpeech(it => it.Where(other => other != partOfSpeech)),
                                                    _sentencesReparser.ReparseSentences);
      CorpusFields =
      [
         Field("Answer", TextFieldFormat.Paragraphs, it => it.CorpusAnswer),
         Field("Explanation", TextFieldFormat.Paragraphs, it => it.Explanation),
         Field("Long explanation", TextFieldFormat.Paragraphs, it => it.ExplanationLong),
         Field("Mnemonic", TextFieldFormat.Paragraphs, it => it.Mnemonic),
         Field("Compound parts", TextFieldFormat.CommaSeparated,
               it => string.Join(", ", it.CompoundParts.Primary()),
               (it, compoundParts) => it.CompoundParts.SetPrimary(StringExtensions.ExtractCommaSeparatedValues(compoundParts))),
         Field("Alternative compound parts", TextFieldFormat.CommaSeparated,
               it => string.Join(", ", it.CompoundParts.Alternative()),
               (it, compoundParts) => it.CompoundParts.SetAlternative(StringExtensions.ExtractCommaSeparatedValues(compoundParts)))
      ];
      AudioFields =
      [
         Field("First", TextFieldFormat.Line, it => it.Audio.First.RawValue(), (it, audio) => it.Audio.First.SetRawValue(audio)),
         Field("Second", TextFieldFormat.Line, it => it.Audio.Second.RawValue(), (it, audio) => it.Audio.Second.SetRawValue(audio)),
         new TextToSpeechAudioFieldEditorViewModel(fatalErrorHandler,
                                                   "Text to speech",
                                                   () => _currentVocab().TextToSpeechAudio.RawValue(),
                                                   WritingToTheCurrentVocab((it, audio) => it.TextToSpeechAudio.SetRawValue(audio)),
                                                   openTheTextToSpeechDialog)
      ];
      TechnicalNotes = Field("Technical notes", TextFieldFormat.Paragraphs, it => it.TechnicalNotes);
      SourceFields =
      [
         Field("Answer", TextFieldFormat.Paragraphs, it => it.SourceData.Answer),
         Field("Mnemonic", TextFieldFormat.Paragraphs, it => it.SourceData.Mnemonic),
         Field("Reading mnemonic", TextFieldFormat.Paragraphs, it => it.SourceData.ReadingMnemonic)
      ];
   }

   /// <summary>The vocab's question: its <see cref="VocabNoteQuestion.Text"/>.</summary>
   public CommittingTextFieldEditorViewModel Question { get; }

   /// <summary>The <see cref="VocabNoteQuestion.Disambiguation"/> that tells the vocab apart from others with the same question, if it has one.</summary>
   public CommittingTextFieldEditorViewModel Disambiguation { get; }

   public TextFieldEditorViewModel Readings { get; }

   /// <summary>The forms besides the question, which is always a form, each owned by the vocab or not.</summary>
   public EnteredFormsEditorViewModel Forms { get; }

   public StringSetEditorViewModel PartsOfSpeech { get; }

   /// <summary>
   /// The corpus's own answer, which takes the place of the source's, its explanations and mnemonic,<br/>
   /// and the <see cref="VocabNoteCompoundParts.Primary"/> and <see cref="VocabNoteCompoundParts.Alternative"/> compound parts.
   /// </summary>
   public IReadOnlyList<TextFieldEditorViewModel> CorpusFields { get; }

   /// <summary>
   /// The vocab's audio, as Anki's <c>[sound:file]</c> references. The last is its TTS audio, with a button for generating new<br/>
   /// (<see cref="TextToSpeechAudioFieldEditorViewModel"/>).
   /// </summary>
   public IReadOnlyList<TextFieldEditorViewModel> AudioFields { get; }

   public TextFieldEditorViewModel TechnicalNotes { get; }

   /// <summary>What the vocab's source gave it (<see cref="VocabNote.SourceData"/>): its answer and mnemonics. The mnemonics are never shown on a card.</summary>
   public IReadOnlyList<TextFieldEditorViewModel> SourceFields { get; }

   /// <summary>Shows everything as the note has it now, after a change made elsewhere.</summary>
   public void Refresh()
   {
      Question.Refresh();
      Disambiguation.Refresh();
      Forms.Refresh();
      PartsOfSpeech.Refresh();
      foreach(var field in LiveTextFields()) field.Refresh();
   }

   IEnumerable<TextFieldEditorViewModel> LiveTextFields() => [Readings, TechnicalNotes, ..CorpusFields, ..AudioFields, ..SourceFields];

   void CommitTheQuestion(string question)
   {
      _currentVocab().Question.SetText(question.Trim());
      _sentencesReparser.ReparseSentences();
   }

   void CommitTheDisambiguation(string disambiguation)
   {
      _currentVocab().Question.SetDisambiguation(disambiguation.Trim());
      _sentencesReparser.ReparseSentences();
   }

   void ChangeThePartsOfSpeech(Func<IEnumerable<string>, IEnumerable<string>> change)
   {
      var partsOfSpeech = _currentVocab().PartsOfSpeech;
      partsOfSpeech.Set(change(partsOfSpeech.Get()));
   }

   TextFieldEditorViewModel Field(string label, TextFieldFormat format, Func<VocabNote, WritableStringValue> field) =>
      Field(label, format, it => field(it).Value, (it, text) => field(it).Set(text));

   TextFieldEditorViewModel Field(string label, TextFieldFormat format, Func<VocabNote, string> read, Action<VocabNote, string> write) =>
      new(_fatalErrorHandler, label, format, () => read(_currentVocab()), WritingToTheCurrentVocab(write));

   /// <summary>Writes a field's text with <paramref name="write"/> to the vocab as the collection holds it now, and updates the vocab's generated data.</summary>
   Action<string> WritingToTheCurrentVocab(Action<VocabNote, string> write) =>
      text =>
      {
         var vocab = _currentVocab();
         write(vocab, text);
         vocab.UpdateGeneratedData();
      };
}
