using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using JAStudio.Core;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.TaskRunners;
using JAStudio.UI.ViewModels.Workspace.Editors.Matching;
using JAStudio.UI.Views.Workspace.Editors;

namespace JAStudio.UI.ViewModels.Workspace.Editors;

/// <summary>
/// Reparses the sentences of some vocab after a change to what they match, so the sentences match the vocab as they are now: a vocab's after a change<br/>
/// to its matching config or its forms, shared by the tabs of its <see cref="VocabEditorSectionViewModel"/>, or those of the vocab depending on a<br/>
/// shared-condition after a change to its definition, in its <see cref="SharedConditionWindowViewModel"/>.
/// </summary>
/// <remarks>
/// The reparse runs in the background and without the progress dialog: the <see cref="VocabEditorSection"/> shows <see cref="IsReparsingSentences"/>,<br/>
/// and a dialog popping up after every change would be in the way.
/// </remarks>
/// <remarks>
/// While the sentences are reparsed, the editors of what they match can't be changed: the reparse reads the vocab, such as its tags and forms,<br/>
/// which a change would modify at the same time from the UI thread.
/// </remarks>
partial class VocabSentencesReparserViewModel : ObservableObject
{
   readonly TemporaryServiceCollection _services;
   readonly IUIThreadDispatcher _uiThread;
   readonly Func<IReadOnlyCollection<VocabNote>> _vocab;
   readonly Action _sentencesReparsed;

   /// <param name="services">The application's services.</param>
   /// <param name="vocab">Finds the vocab whose sentences to reparse, as the collection holds them now: the vocab being edited, for a vocab's editors.</param>
   /// <param name="sentencesReparsed">Called on the UI thread after a reparse has finished, so the vocab's page can show the sentences it now matches.</param>
   public VocabSentencesReparserViewModel(TemporaryServiceCollection services, Func<IReadOnlyCollection<VocabNote>> vocab, Action sentencesReparsed)
   {
      //review:matching: SharedConditionWindowViewModel reparsed the sentences of the vocab depending on its shared-condition as this does a vocab's,
      //   in a copy of ReparseSentences of its own. This reparses the sentences of the vocab it's given, now one or several, for both.
      _services = services;
      _uiThread = services.Resolve<IUIThreadDispatcher>();
      _vocab = vocab;
      _sentencesReparsed = sentencesReparsed;
   }

   /// <summary>Whether the vocab's sentences are being reparsed after a change. What they match can't be changed meanwhile.</summary>
   [ObservableProperty] bool _isReparsingSentences;

   /// <summary>Reparses the sentences that may contain the vocab, each once, in the background. Called after a change to what the vocab match.</summary>
   public void ReparseSentences()
   {
      IsReparsingSentences = true;
      var vocab = _vocab();
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
