using System;
using JAStudio.Core;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Storage.Media;
using JAStudio.Core.TaskRunners;
using JAStudio.UI.Views.TextToSpeech;
using JAStudio.UI.Views.Workspace.Editors.Matching;
using JAStudio.UI.Views.Workspace.Editors;

namespace JAStudio.UI.ViewModels.Workspace.Editors;

/// <summary>
/// What the workspace's editor section shows for a vocab (<see cref="VocabEditorSection"/>): its editors, as tabs.<br/>
/// <see cref="Text"/> for its text fields, forms and parts of speech, <see cref="MatchingConfig"/> for the rest of what affects parsing,<br/>
/// <see cref="Metadata"/> for what doesn't, and <see cref="Related"/> for its relationships to other words.
/// </summary>
/// <remarks>
/// It holds the vocab's id, not the note object, because the collection replaces a note's object when Anki edits the note<br/>
/// and when it reloads, and an editor holding the old object would edit an orphan.<br/>
/// So every editor reads and writes through <see cref="CurrentVocab"/>, and the tab showing the vocab calls <see cref="Refresh"/>
/// whenever the note is updated, whoever updated it.
/// </remarks>
class VocabEditorSectionViewModel : INoteEditorSectionViewModel
{
   readonly TemporaryServiceCollection _services;
   readonly NoteId _vocabId;

   /// <param name="services">The application's services.</param>
   /// <param name="vocabId">The vocab to edit.</param>
   /// <param name="sentencesReparsed">Called on the UI thread after the sentences were reparsed for a change to what the vocab matches.</param>
   /// <param name="editNote">Opens a note in the workspace with its editors showing, such as a vocab the related tab created.</param>
   public VocabEditorSectionViewModel(TemporaryServiceCollection services, NoteId vocabId, Action sentencesReparsed, Action<JPNote> editNote)
   {
      _services = services;
      _vocabId = vocabId;
      var fatalErrorHandler = services.Resolve<IFatalErrorHandler>();
      SentencesReparser = new VocabSentencesReparserViewModel(services, CurrentVocab, sentencesReparsed);
      Text = new VocabTextEditorViewModel(fatalErrorHandler, CurrentVocab, SentencesReparser, () => TextToSpeechDialog.OpenFor(CurrentVocab(), services), services.Resolve<MediaFileIndex>());
      MatchingConfig = new VocabMatchingConfigEditorViewModel(fatalErrorHandler, CurrentVocab, SentencesReparser, sharedCondition => SharedConditionWindow.Open(sharedCondition, services));
      Metadata = new VocabMetadataEditorViewModel(services, CurrentVocab);
      Related = new VocabRelatedEditorViewModel(services, CurrentVocab, editNote);
   }

   /// <summary>Reparses the vocab's sentences after a change to what it matches. No editor can be changed while it runs.</summary>
   public VocabSentencesReparserViewModel SentencesReparser { get; }

   public VocabTextEditorViewModel Text { get; }
   public VocabMatchingConfigEditorViewModel MatchingConfig { get; }
   public VocabMetadataEditorViewModel Metadata { get; }
   public VocabRelatedEditorViewModel Related { get; }

   /// <summary>Shows everything as the note has it now. Called by the tab showing the vocab whenever the note is updated.</summary>
   public void Refresh()
   {
      Text.Refresh();
      MatchingConfig.Refresh();
      Metadata.Refresh();
      Related.Refresh();
   }

   /// <summary>The vocab being edited, as the collection holds it now.</summary>
   VocabNote CurrentVocab() => _services.CoreApp.Collection.Vocab.WithIdOrNone(_vocabId)
                            ?? throw new InvalidOperationException($"The vocab being edited, {_vocabId}, is no longer in the collection.");
}
