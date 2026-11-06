using System;
using JAStudio.Core;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.TaskRunners;
using JAStudio.UI.Views.Workspace.Editors;

namespace JAStudio.UI.ViewModels.Workspace.Editors;

/// <summary>
/// What the workspace's editor section shows for a vocab (<see cref="VocabEditorSection"/>): its editors, as tabs.<br/>
/// <see cref="MatchingConfig"/> for everything that affects parsing, <see cref="Metadata"/> for the rest.
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
   /// <param name="sentencesReparsed">Called on the UI thread after the sentences were reparsed for a change to the matching config.</param>
   public VocabEditorSectionViewModel(TemporaryServiceCollection services, NoteId vocabId, Action sentencesReparsed)
   {
      _services = services;
      _vocabId = vocabId;
      MatchingConfig = new VocabMatchingConfigEditorViewModel(services, CurrentVocab, sentencesReparsed);
      Metadata = new VocabMetadataEditorViewModel(services.Resolve<IFatalErrorHandler>(), CurrentVocab);
   }

   public VocabMatchingConfigEditorViewModel MatchingConfig { get; }
   public VocabMetadataEditorViewModel Metadata { get; }

   /// <summary>Shows everything as the note has it now. Called by the tab showing the vocab whenever the note is updated.</summary>
   public void Refresh()
   {
      MatchingConfig.Refresh();
      Metadata.Refresh();
   }

   /// <summary>The vocab being edited, as the collection holds it now.</summary>
   VocabNote CurrentVocab() => _services.CoreApp.Collection.Vocab.WithIdOrNone(_vocabId)
                            ?? throw new InvalidOperationException($"The vocab being edited, {_vocabId}, is no longer in the collection.");
}
