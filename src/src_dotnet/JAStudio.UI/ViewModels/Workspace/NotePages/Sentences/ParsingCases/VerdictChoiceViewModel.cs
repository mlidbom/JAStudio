using CommunityToolkit.Mvvm.Input;
using Compze.Teventive.TaggregateStores;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Sentences.ParsingCases;
using JAStudio.Core.TaskRunners;
using JAStudio.UI.ViewModels.Workspace.ParsingCases;

namespace JAStudio.UI.ViewModels.Workspace.NotePages.Sentences.ParsingCases;

/// <summary>
/// One of the three verdicts a person can give on the vocab a sentence's parsing case expects the breakdown to display<br/>
/// (<see cref="ParsingCase.VerdictOnTheDisplayedVocab"/>), as a button of the page's parsing case section: chosen when it's the verdict given, and<br/>
/// giving it when clicked.
/// </summary>
sealed partial class VerdictChoiceViewModel
{
   readonly ITaggregateReference<SentenceNote> _sentence;
   readonly IFatalErrorHandler _fatalErrorHandler;

   /// <summary>The sentence as it is now.</summary>
   SentenceNote Sentence => _sentence.Current;

   public VerdictChoiceViewModel(Verdict verdict, SentenceNote sentence, IFatalErrorHandler fatalErrorHandler)
   {
      Verdict = verdict;
      _sentence = sentence.Reference();
      _fatalErrorHandler = fatalErrorHandler;
      IsChosen = SentenceEditableFields.VerdictOnTheDisplayedVocab.Read(sentence) == verdict;
   }

   public Verdict Verdict { get; }

   /// <summary>The verdict in words, such as "verified".</summary>
   public string Text => Verdict.InWords();

   /// <summary>What giving the verdict says, as the button's tooltip.</summary>
   public string Tooltip => Verdict switch
   {
      Verdict.Verified => "You have found the vocab the parsing case expects the breakdown to display right. A change to them then fails the case.",
      Verdict.Broken   => "You have found the vocab the parsing case expects the breakdown to display wrong: a bug to come back to and fix.",
      _                => "Nobody has judged the vocab the parsing case expects the breakdown to display."
   };

   /// <summary>Whether this is the verdict given.</summary>
   public bool IsChosen { get; }

   public bool IsBroken => Verdict == Verdict.Broken;

   /// <summary>Gives the verdict, unless it's the one given already.</summary>
   [RelayCommand]
   void Choose()
   {
      if(!IsChosen) _fatalErrorHandler.HandleExceptionsFrom(() => SentenceEditableFields.VerdictOnTheDisplayedVocab.Write(Sentence, Verdict));
   }
}
