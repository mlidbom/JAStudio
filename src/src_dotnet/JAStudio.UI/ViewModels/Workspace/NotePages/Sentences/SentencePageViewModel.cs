using System.Collections.Generic;
using System.Linq;
using JAStudio.Core;
using JAStudio.Core.Configuration;
using JAStudio.Core.Note.NoteFields;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Storage.Media;
using JAStudio.Core.UI.Web.Sentence;
using JAStudio.Core.ViewModels.KanjiList;

namespace JAStudio.UI.ViewModels.Workspace.NotePages.Sentences;

/// <summary>
/// A sentence's page in a workspace tab: what the sentence card's answer side shows (<c>SentenceCard.razor</c>), in Avalonia. Its screenshots, its<br/>
/// question with its audio, its answer and comments, and its breakdown: the words found in it, each with the vocab it matched, built from the same<br/>
/// view models in Core as the card's (<see cref="SentenceViewModel"/>), and in edit mode, its tokens.
/// </summary>
sealed class SentencePageViewModel : NotePageViewModel
{
   public SentencePageViewModel(SentenceNote sentence, TemporaryServiceCollection services, INotePageHost host) : base(host)
   {
      var settings = services.Resolve<Settings>();
      var media = services.Resolve<MediaFileIndex>();
      var isInEditMode = settings.ShowBreakdownInEditMode();
      var hasACorpusQuestion = sentence.CorpusQuestion.HasValue();

      ScreenshotFiles = [.. sentence.AllScreenshots.Select(it => media.TryGetByOriginalFileName(it.FileName)?.FilePath).OfType<string>()];
      Audio = AudioButtonViewModel.For(sentence.PreferredAudio);
      CorpusQuestion = hasACorpusQuestion ? QuestionAsShown(sentence.CorpusQuestion.Value) : null;
      SourceQuestion = QuestionAsShown(sentence.SourceData.Question.Value);
      CorpusAnswer = sentence.CorpusAnswer.HasValue() ? sentence.CorpusAnswer.Value : null;
      SourceAnswer = sentence.SourceData.Answer.Value;
      Comments = sentence.Comments.HasValue() ? sentence.Comments.Value : null;
      SourceComments = sentence.SourceData.Comments.HasValue() ? sentence.SourceData.Comments.Value : null;
      ViewToggles = SentenceViewToggleViewModel.TheOnesOnIn(services.CoreApp.Config);

      var breakdown = new SentenceViewModel(sentence, settings, services.CoreApp.Collection.Vocab);
      var kanjiLists = services.Resolve<SentenceKanjiListViewModel>();
      Entries = [.. breakdown.DisplayedMatches.Select(it => new BreakdownEntryViewModel(it, settings, kanjiLists))];
      TokenTables = isInEditMode ? TokenTableViewModel.Of(breakdown.Analysis.Analysis) : [];
      return;

      string QuestionAsShown(string question) =>
         isInEditMode ? question.Replace(SentenceQuestionField.WordBreakTag, ShownWordBreakTag) : question;
   }

   /// <summary>A word break tag in the question, shown in edit mode, as the cards show it: in the bold colour.</summary>
   const string ShownWordBreakTag = "<b>&lt;wbr&gt;</b>";

   /// <summary>The files of the sentence's screenshots, those of its source first.</summary>
   public IReadOnlyList<string> ScreenshotFiles { get; }

   /// <summary>The sentence's own audio, which its ▶ button in front of the first question shown plays, and the page plays when shown.</summary>
   public AudioButtonViewModel? Audio { get; }

   /// <summary>The corpus's own question, with its markup, which overrides the source's, when it has one.</summary>
   public string? CorpusQuestion { get; }

   /// <summary>The question the sentence came with from its source, with its markup, shown smaller when the corpus overrides it.</summary>
   public string SourceQuestion { get; }

   public bool HasACorpusQuestion => CorpusQuestion is not null;

   /// <summary>The audio button in front of the corpus's question, the first one shown, when there is one.</summary>
   public AudioButtonViewModel? AudioOfTheCorpusQuestion => HasACorpusQuestion ? Audio : null;

   /// <summary>The audio button in front of the source's question, when it's the first one shown.</summary>
   public AudioButtonViewModel? AudioOfTheSourceQuestion => HasACorpusQuestion ? null : Audio;

   /// <summary>The corpus's own answer, with its markup, which overrides the source's, when it has one.</summary>
   public string? CorpusAnswer { get; }

   /// <summary>The answer the sentence came with from its source, with its markup, shown smaller when the corpus overrides it.</summary>
   public string SourceAnswer { get; }

   public bool HasACorpusAnswer => CorpusAnswer is not null;

   /// <summary>The user's comments on the sentence, with their markup, when there are any.</summary>
   public string? Comments { get; }

   /// <summary>The comments the sentence came with from its source, with their markup, when there are any.</summary>
   public string? SourceComments { get; }

   /// <summary>The sentence view toggles that are on, which the breakdown's title shows.</summary>
   public IReadOnlyList<SentenceViewToggleViewModel> ViewToggles { get; }

   /// <summary>The words of the breakdown, in the order they are in the sentence.</summary>
   public IReadOnlyList<BreakdownEntryViewModel> Entries { get; }

   /// <summary>In edit mode, the tables of the sentence's tokens; none otherwise.</summary>
   public IReadOnlyList<TokenTableViewModel> TokenTables { get; }

   public override void PlayTheNotesAudio() => Audio?.Play();
}
