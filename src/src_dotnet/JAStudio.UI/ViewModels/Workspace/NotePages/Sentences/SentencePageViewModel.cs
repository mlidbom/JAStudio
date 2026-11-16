using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using JAStudio.Core;
using JAStudio.Core.Configuration;
using JAStudio.Core.Note.NoteFields;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Sentences.ParsingCases.Breakdowns;
using JAStudio.Core.Storage.Media;
using JAStudio.Core.UI.Web.Sentence;
using JAStudio.Core.ViewModels.KanjiList;
using JAStudio.UI.Menus.Notes.Sentence;
using JAStudio.UI.Menus.UIAgnosticMenuStructure;
using JAStudio.UI.ViewModels.Workspace.NotePages.Sentences.ParsingCases;

namespace JAStudio.UI.ViewModels.Workspace.NotePages.Sentences;

/// <summary>
/// A sentence's page in a workspace tab: what the sentence card's answer side shows (<c>SentenceCard.razor</c>), in Avalonia. Its screenshots, its<br/>
/// question with its audio, its answer and comments, and its breakdown: the words found in it, each with the vocab it matched, built from the same<br/>
/// view models in Core as the card's (<see cref="SentenceViewModel"/>), and in edit mode, its tokens.
/// </summary>
/// <remarks>
/// A sentence that is a parsing case has a section about that (<see cref="ParsingCase"/>), and when it parses otherwise than its case expects, the<br/>
/// page can show the breakdown the case expects beside the sentence's own (<see cref="RowsBesideTheExpected"/>).
/// </remarks>
sealed partial class SentencePageViewModel : NotePageViewModel
{
   readonly SentenceElementMenus _elementMenus;
   readonly SentenceNote _sentence;

   public SentencePageViewModel(SentenceNote sentence, TemporaryServiceCollection services, INotePageHost host) : base(host)
   {
      _elementMenus = new SentenceElementMenus(services, sentence, host);
      _sentence = sentence;
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
      var entryOf = breakdown.DisplayedMatches.ToDictionary(it => it, it => new BreakdownEntryViewModel(it, settings, kanjiLists));
      Entries = [.. breakdown.DisplayedMatches.Select(it => entryOf[it])];
      TokenTables = isInEditMode ? TokenTableViewModel.Of(breakdown.Analysis.Analysis) : [];

      if(sentence.ParsingCase is {} parsingCase)
      {
         var comparison = sentence.CompareWithWhatTheParsingCaseExpects();
         ParsingCase = new SentenceParsingCaseViewModel(sentence, comparison, services);
         ShowsTheExpectedBesideTheBreakdown = !comparison.ParsesAsExpected && !isInEditMode && services.CoreApp.Config.ShowWhatAParsingCaseExpectsBesideTheBreakdown.Value;
         if(ShowsTheExpectedBesideTheBreakdown)
            RowsBesideTheExpected = [.. BreakdownBesideTheExpected.Of(breakdown.Analysis.MatchesDisplayedOutsideEditMode, parsingCase.Expected.BreakdownUnder(settings.BreakdownParsingOptions()))
                                                                  .Rows
                                                                  .Select(RowBesideTheExpected)];
      }

      return;

      BreakdownRowBesideTheExpectedViewModel RowBesideTheExpected(BreakdownBesideTheExpected.Row row) =>
         new(row.Displayed is {} displayed ? entryOf[displayed] : null,
             row.Expected is {} expected ? new ExpectedBreakdownEntryViewModel(expected, services.CoreApp.Collection) : null,
             row.IsTheSameWord);

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

   /// <summary>The page's parsing case section, for a sentence that is a parsing case; none otherwise.</summary>
   public SentenceParsingCaseViewModel? ParsingCase { get; }

   public bool IsAParsingCase => ParsingCase is not null;

   /// <summary>
   /// Whether the page shows the breakdown the sentence's parsing case expects beside the sentence's own (<see cref="RowsBesideTheExpected"/>), in<br/>
   /// place of the <see cref="Entries"/> alone: the sentence parses otherwise than its case expects, in any way, the setting for it is on, and the<br/>
   /// breakdown isn't in edit mode, which lists every match.
   /// </summary>
   public bool ShowsTheExpectedBesideTheBreakdown { get; }

   /// <summary>
   /// The words of the breakdown beside those the parsing case expects under the same options, row by row, in the order they are in the sentence.<br/>
   /// Empty unless the page <see cref="ShowsTheExpectedBesideTheBreakdown"/>.
   /// </summary>
   public IReadOnlyList<BreakdownRowBesideTheExpectedViewModel> RowsBesideTheExpected { get; } = [];

   /// <summary>Makes the sentence a parsing case that expects what it parses to now, for a sentence that isn't one.</summary>
   [RelayCommand]
   void MarkAsAParsingCase() => SentenceCommands.MarkAsAParsingCase.Run(_sentence);

   public override void PlayTheNotesAudio() => Audio?.Play();

   public override List<SpecMenuItem> MenuOf(INotePageElement element, string selection) =>
      element switch
      {
         BreakdownEntryViewModel entry => _elementMenus.OfAMatch(entry.Match, selection),
         CompoundPartEntryViewModel part => _elementMenus.OfACompoundPart(part, selection),
         BreakdownKanjiViewModel kanji => _elementMenus.OfAKanji(kanji, selection),
         _ => throw new ArgumentException($"A sentence's page has no element {element}.", nameof(element))
      };
}
