using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core;
using JAStudio.Core.Note.NoteFields;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.TaskRunners;
using JAStudio.UI.Controls;
using JAStudio.UI.Views.Workspace.Editors;

namespace JAStudio.UI.ViewModels.Workspace.Editors;

/// <summary>
/// The "Text" tab of a vocab's <see cref="VocabEditorSectionViewModel"/> (<see cref="VocabTextEditor"/>): its text fields, its forms and its parts of speech.<br/>
/// Most fields are written to the note on every keystroke, and the vocab's generated data, such as its active answer, is updated with them.
/// </summary>
/// <remarks>
/// The question, the forms and the parts of speech change what the vocab matches, so every change to them reparses the vocab's sentences.<br/>
/// One per keystroke would be far too many, so the question is written when it's committed,<br/>
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
   public VocabTextEditorViewModel(IFatalErrorHandler fatalErrorHandler, Func<VocabNote> currentVocab, VocabSentencesReparserViewModel sentencesReparser)
   {
      _fatalErrorHandler = fatalErrorHandler;
      _currentVocab = currentVocab;
      _sentencesReparser = sentencesReparser;

      Question = new CommittingTextFieldEditorViewModel(fatalErrorHandler, "Question", () => _currentVocab().Question.DisambiguationName, CommitTheQuestion);
      Readings = Field("Readings", TextFieldFormat.CommaSeparated,
                       it => string.Join(", ", it.GetReadings()),
                       (it, readings) => it.SetReadings(StringExtensions.ExtractCommaSeparatedValues(readings)));
      Forms = new StringSetControlViewModel("Forms",
                                            () => _currentVocab().Forms.EnteredFormsWithBrackets(),
                                            form => _currentVocab().Forms.Add(form),
                                            form => _currentVocab().Forms.Remove(form),
                                            _sentencesReparser.ReparseSentences);
      PartsOfSpeech = new StringSetControlViewModel("Parts of speech",
                                                    () => _currentVocab().PartsOfSpeech.Get(),
                                                    partOfSpeech => ChangeThePartsOfSpeech(it => it.Append(partOfSpeech)),
                                                    partOfSpeech => ChangeThePartsOfSpeech(it => it.Where(other => other != partOfSpeech)),
                                                    _sentencesReparser.ReparseSentences);
      UserFields =
      [
         Field("Answer", TextFieldFormat.Paragraphs, it => it.User.Answer),
         Field("Explanation", TextFieldFormat.Paragraphs, it => it.User.Explanation),
         Field("Long explanation", TextFieldFormat.Paragraphs, it => it.User.ExplanationLong),
         Field("Mnemonic", TextFieldFormat.Paragraphs, it => it.User.Mnemonic),
         Field("Compound parts", TextFieldFormat.CommaSeparated,
               it => string.Join(", ", it.CompoundParts.AllRaw()),
               (it, compoundParts) => it.CompoundParts.Set(StringExtensions.ExtractCommaSeparatedValues(compoundParts)))
      ];
      AudioFields =
      [
         Field("First", TextFieldFormat.Line, it => it.Audio.First.RawValue(), (it, audio) => it.Audio.First.SetRawValue(audio)),
         Field("Second", TextFieldFormat.Line, it => it.Audio.Second.RawValue(), (it, audio) => it.Audio.Second.SetRawValue(audio)),
         Field("Text to speech", TextFieldFormat.Line, it => it.Audio.Tts.RawValue(), (it, audio) => it.Audio.Tts.SetRawValue(audio))
      ];
      TechnicalNotes = Field("Technical notes", TextFieldFormat.Paragraphs, it => it.TechnicalNotes);
      SourceFields =
      [
         Field("Answer", TextFieldFormat.Paragraphs, it => it.SourceAnswer),
         Field("Mnemonic", TextFieldFormat.Paragraphs, it => it.SourceMnemonic),
         Field("Reading mnemonic", TextFieldFormat.Paragraphs, it => it.SourceReadingMnemonic)
      ];
   }

   /// <summary>The vocab's question, with its disambiguation if it has one, as in <c>question:disambiguation</c>.</summary>
   public CommittingTextFieldEditorViewModel Question { get; }

   public TextFieldEditorViewModel Readings { get; }

   /// <summary>The forms besides the question: the question is always a form. A form written <c>[form]</c> is owned by the vocab.</summary>
   public StringSetControlViewModel Forms { get; }

   public StringSetControlViewModel PartsOfSpeech { get; }

   /// <summary>The user's own answer, explanations and mnemonic, which take precedence over the <see cref="SourceFields"/>, and the compound parts.</summary>
   public IReadOnlyList<TextFieldEditorViewModel> UserFields { get; }

   /// <summary>The vocab's audio, as Anki's <c>[sound:file]</c> references.</summary>
   public IReadOnlyList<TextFieldEditorViewModel> AudioFields { get; }

   public TextFieldEditorViewModel TechnicalNotes { get; }

   /// <summary>The answer and mnemonics the vocab came with.</summary>
   public IReadOnlyList<TextFieldEditorViewModel> SourceFields { get; }

   /// <summary>Shows everything as the note has it now, after a change made elsewhere.</summary>
   public void Refresh()
   {
      Question.Refresh();
      Forms.Refresh();
      PartsOfSpeech.Refresh();
      foreach(var field in LiveTextFields()) field.Refresh();
   }

   IEnumerable<TextFieldEditorViewModel> LiveTextFields() => [Readings, TechnicalNotes, ..UserFields, ..AudioFields, ..SourceFields];

   void CommitTheQuestion(string question)
   {
      _currentVocab().Question.Set(question.Trim());
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
      new(_fatalErrorHandler,
          label,
          format,
          () => read(_currentVocab()),
          text =>
          {
             var vocab = _currentVocab();
             write(vocab, text);
             vocab.UpdateGeneratedData();
          });
}
