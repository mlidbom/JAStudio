using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using JAStudio.Core;
using JAStudio.Core.LanguageServices.JanomeEx;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Editing;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing;
using JAStudio.Core.TaskRunners;
using JAStudio.UI.ViewModels.Workspace.Editors.Matching.Checking;
using JAStudio.UI.Views.Workspace.Editors.Matching;

namespace JAStudio.UI.ViewModels.Workspace.Editors.Matching;

/// <summary>
/// A vocab's match-condition window (<see cref="MatchConditionWindow"/>): its match-condition, edited in the tree editor, with all the room a window<br/>
/// gives, as its Matching tab only shows it. Every change is saved in the vocab, and its sentences are then reparsed, as after a change on the tab.<br/>
/// The tree can be checked against a word in a sentence (<see cref="WordToCheckAgainst"/>), each line showing whether the word meets its condition.
/// </summary>
/// <remarks>
/// It holds the vocab's id, not the note object, since the collection replaces a note's object when Anki edits the note: the tree reads and stores the<br/>
/// vocab as the collection holds it now, as the vocab's editors do (<see cref="VocabEditorSectionViewModel"/>).
/// </remarks>
sealed class MatchConditionWindowViewModel : ObservableObject, IEditorWindowViewModel
{
   readonly TemporaryServiceCollection _services;
   readonly NoteId _vocabId;

   /// <param name="services">The application's services.</param>
   /// <param name="vocab">The vocab whose match-condition the window edits.</param>
   /// <param name="openTheDefinitionOf">Opens a shared-condition's window, such as one the match-condition uses.</param>
   /// <param name="sentencesReparsedFor">
   /// Called on the UI thread after the vocab's sentences were reparsed for a change, with the vocab, so its pages can show what it now matches.
   /// </param>
   public MatchConditionWindowViewModel(TemporaryServiceCollection services,
                                        VocabNote vocab,
                                        Action<ISharedCondition> openTheDefinitionOf,
                                        Action<IReadOnlyCollection<VocabNote>> sentencesReparsedFor)
   {
      _services = services;
      _vocabId = vocab.GetId();
      WordToCheckAgainst = new WordToCheckAgainstViewModel(services.Resolve<AnalysisServices>(), CurrentVocab, CheckTheTreeAgainst);
      //review:matching: this reparse and the vocab's editors' are separate (VocabEditorSectionViewModel.SentencesReparser), so a flag or rule changed
      //   on the Matching tab, or a form on the Text tab, while this one reads the vocab, or the other way around, changes the vocab under it, as a
      //   shared-condition's window's reparse already could. One reparser per vocab, which both disable their editors on, would close it.
      SentencesReparser = new VocabSentencesReparserViewModel(services,
                                                              () => [CurrentVocab()],
                                                              reparsedVocab =>
                                                              {
                                                                 sentencesReparsedFor(reparsedVocab);
                                                                 WordToCheckAgainst.ShowTheWordsOf(reparsedVocab.Single());
                                                              });
      MatchConditionEditor = new ConditionTreeEditorViewModel(services.Resolve<IFatalErrorHandler>(),
                                                              new EditedMatchCondition(CurrentVocab),
                                                              services.NoteServices,
                                                              SentencesReparser.ReparseSentences,
                                                              openTheDefinitionOf);
   }

   /// <summary>The window's title: the vocab's question, such as <c>えた - match-condition</c>.</summary>
   public string Title => $"{CurrentVocab().GetQuestion()} - match-condition";

   /// <summary>The tree editor of the vocab's match-condition.</summary>
   public ConditionTreeEditorViewModel MatchConditionEditor { get; }

   /// <summary>
   /// The word in a sentence the tree is checked against: each line shows whether the word meets its condition. Its words are found anew after each<br/>
   /// change's reparse, so that each says whether it matches the vocab now.
   /// </summary>
   public WordToCheckAgainstViewModel WordToCheckAgainst { get; }

   /// <summary>Reparses the vocab's sentences after a change to its match-condition. The tree can't be changed while it runs.</summary>
   public VocabSentencesReparserViewModel SentencesReparser { get; }

   /// <summary>Whether the vocab is no longer in the collection, as after it was deleted. The window then closes, since every change to it would fail.</summary>
   public bool IsGone => TheVocabInTheCollection is null;

   /// <summary>
   /// Shows the match-condition as it is now, when something else may have changed it, such as a rule's editor on the vocab's Matching tab, or the MCP<br/>
   /// server: a change like the tree editor's own, which undo takes back.
   /// </summary>
   public void ShowItAsItIsNow()
   {
      OnPropertyChanged(nameof(Title));
      MatchConditionEditor.Refresh();
      WordToCheckAgainst.ShowTheWordsOf(CurrentVocab());
   }

   /// <summary>Checks the tree against <paramref name="word"/>, a word in a sentence, from now on, or stops checking it when there's none.</summary>
   void CheckTheTreeAgainst(CandidateWordVariant? word) =>
      MatchConditionEditor.CheckTheTreeWith(word is null ? null : tree => EditedMatchCondition.CheckedAgainst(tree, word));

   /// <summary>The vocab whose match-condition the window edits, as the collection holds it now.</summary>
   VocabNote CurrentVocab() =>
      TheVocabInTheCollection ?? throw new InvalidOperationException($"The vocab whose match-condition is edited, {_vocabId}, is no longer in the collection.");

   /// <summary>The vocab whose match-condition the window edits, as the collection holds it now, or none when it's gone.</summary>
   VocabNote? TheVocabInTheCollection => _services.CoreApp.Collection.Vocab.WithIdOrNone(_vocabId);
}
