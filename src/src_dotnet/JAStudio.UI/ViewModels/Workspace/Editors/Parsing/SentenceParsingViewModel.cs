using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using JAStudio.Core;
using JAStudio.Core.Configuration;
using JAStudio.Core.LanguageServices.Parsing.WordExtraction.MatchTrees;
using JAStudio.Core.Note.Sentences;
using JAStudio.UI.Menus.Notes.Sentence;
using JAStudio.UI.Menus.UIAgnosticMenuStructure;
using JAStudio.UI.ViewModels.Workspace.NotePages;

namespace JAStudio.UI.ViewModels.Workspace.Editors.Parsing;

/// <summary>
/// How a sentence parsed, as its sentence breakdown's analysis makes it, the Parsing tab of its editor section: every match, the shown ones in sentence<br/>
/// order, each with the matches it covers that aren't shown indented under it (<see cref="MatchTree"/>), and each with the reasons it doesn't match or<br/>
/// isn't shown. Each match's right-click menu marks it, as a breakdown entry's does. See <c>dev_docs/jastudio-workspace.md</c>, "Edit mode becomes<br/>
/// the sentence's Parsing tab".
/// </summary>
/// <remarks>It reads the sentence and the settings as they are when it's made and whenever it's refreshed.</remarks>
sealed partial class SentenceParsingViewModel : ObservableObject
{
   readonly TemporaryServiceCollection _services;
   readonly Func<SentenceNote> _currentSentence;
   readonly INotePageHost _tab;

   /// <param name="services">The application's services.</param>
   /// <param name="currentSentence">Finds the sentence, as the collection holds it now.</param>
   /// <param name="tab">The workspace tab showing the sentence, which shows the notes the menus open.</param>
   public SentenceParsingViewModel(TemporaryServiceCollection services, Func<SentenceNote> currentSentence, INotePageHost tab)
   {
      _services = services;
      _currentSentence = currentSentence;
      _tab = tab;
      Matches = TheMatchesAsTheSentenceParsesNow();
   }

   /// <summary>The sentence's matches, as rows: each shown match, then the matches it covers, indented.</summary>
   [ObservableProperty] public partial IReadOnlyList<ParsedMatchViewModel> Matches { get; private set; }

   /// <summary>Shows the matches as the sentence parses now, after a change to the sentence or to the settings of its breakdown.</summary>
   public void Refresh() => Matches = TheMatchesAsTheSentenceParsesNow();

   /// <summary>The right-click menu of <paramref name="match"/>, a row, with <paramref name="selection"/>, the text selected on it, if any: a breakdown entry's.</summary>
   public List<SpecMenuItem> MenuOf(ParsedMatchViewModel match, string selection) =>
      new SentenceElementMenus(_services, _currentSentence(), _tab).OfAMatch(match.Match, selection);

   IReadOnlyList<ParsedMatchViewModel> TheMatchesAsTheSentenceParsesNow()
   {
      var analysis = _currentSentence().CreateBreakdownAnalysis(_services.Resolve<Settings>().BreakdownParsingOptions());
      return [.. MatchTree.Of(analysis).Roots.SelectMany(TheRowsOf)];

      static IEnumerable<ParsedMatchViewModel> TheRowsOf(MatchTreeNode node) =>
         [ParsedMatchViewModel.AtTheTop(node), .. node.Covered.Select(ParsedMatchViewModel.Covered)];
   }
}
