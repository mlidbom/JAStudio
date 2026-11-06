using System;
using CommunityToolkit.Mvvm.ComponentModel;
using JAStudio.Core;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.TaskRunners;
using JAStudio.UI.Views.Workspace.Editors;

namespace JAStudio.UI.ViewModels.Workspace.Editors;

/// <summary>
/// Reparses a vocab's sentences after a change to what the vocab matches, such as its matching config or its forms,<br/>
/// so the sentences match the vocab as it is now. Shared by the tabs of a vocab's <see cref="VocabEditorSectionViewModel"/>.
/// </summary>
/// <remarks>
/// The reparse runs in the background and without the progress dialog: the <see cref="VocabEditorSection"/> shows <see cref="IsReparsingSentences"/>,<br/>
/// and a dialog popping up after every change would be in the way.
/// </remarks>
/// <remarks>
/// While the sentences are reparsed, the vocab's editors can't be changed: the reparse reads the vocab, such as its tags and forms,<br/>
/// which a change would modify at the same time from the UI thread.
/// </remarks>
partial class VocabSentencesReparserViewModel : ObservableObject
{
   readonly TemporaryServiceCollection _services;
   readonly IUIThreadDispatcher _uiThread;
   readonly Func<VocabNote> _currentVocab;
   readonly Action _sentencesReparsed;

   /// <param name="services">The application's services.</param>
   /// <param name="currentVocab">Finds the vocab being edited, as the collection holds it now.</param>
   /// <param name="sentencesReparsed">Called on the UI thread after a reparse has finished, so the vocab's page can show the sentences it now matches.</param>
   public VocabSentencesReparserViewModel(TemporaryServiceCollection services, Func<VocabNote> currentVocab, Action sentencesReparsed)
   {
      _services = services;
      _uiThread = services.Resolve<IUIThreadDispatcher>();
      _currentVocab = currentVocab;
      _sentencesReparsed = sentencesReparsed;
   }

   /// <summary>Whether the vocab's sentences are being reparsed after a change. The vocab can't be changed meanwhile.</summary>
   [ObservableProperty] bool _isReparsingSentences;

   /// <summary>Reparses the sentences that may contain the vocab, in the background. Called after a change to what the vocab matches.</summary>
   public void ReparseSentences()
   {
      IsReparsingSentences = true;
      var vocab = _currentVocab();
      _services.BackgroundTaskManager.Run(() =>
      {
         try
         {
            _services.LocalNoteUpdater.ReparseSentencesForVocabWithoutProgressDialog(vocab);
         }
         finally
         {
            _uiThread.PostToUIThread(ReparseFinished);
         }
      });
   }

   void ReparseFinished()
   {
      IsReparsingSentences = false;
      _sentencesReparsed();
   }
}
