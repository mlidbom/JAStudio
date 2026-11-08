using System;
using System.Linq;
using System.Threading;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Search;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Tagging;
using JAStudio.UI.ViewModels.Workspace.Navigation;

// ReSharper disable InconsistentNaming

namespace JAStudio.UI.Specifications.AICreatedSpecs;

/// <summary>
/// Two sentences tagged <c>source::anime</c>, both open in tabs of their own, found by a search for the tag, and on the List tab.<br/>
/// Then one of them is deleted, as "Delete note" in its menu does.
/// </summary>
public class When_a_note_shown_in_the_workspace_is_deleted : SpecificationUsingAWorkspace, IAIGeneratedSpec
{
   const string DeletedSentence = "家に帰る";
   const string KeptSentence = "学校に行く";
   static readonly TimeSpan TimeToWaitForTheSearch = TimeSpan.FromSeconds(10);

   readonly NoteSearchViewModel _search;

   public When_a_note_shown_in_the_workspace_is_deleted()
   {
      var deleted = CreateTaggedSentence(DeletedSentence);
      var kept = CreateTaggedSentence(KeptSentence);
      Workspace.OpenNote(deleted);
      Workspace.OpenNote(kept);
      Workspace.Navigation.ShowNoteList(new NoteList("Sentences about going", [deleted, kept]));

      _search = Workspace.Navigation.Search;
      _search.AddCondition(NoteProperties.Tags.Include("source::anime"));
      SpinWait.SpinUntil(() => !_search.IsSearching && _search.Results.Sentences.Count == 2, TimeToWaitForTheSearch).Must().BeTrue();

      GetService<JPCollection>().Delete(deleted);
   }

   SentenceNote CreateTaggedSentence(string question)
   {
      var sentence = CreateTestSentence(question, "an answer");
      sentence.Tags.Set(Tag.FromName("source::anime"));
      return sentence;
   }

   [XF] public void its_tab_closes() => Workspace.Tabs.Select(it => it.Header).Must().SequenceEqual([KeptSentence]);
   [XF] public void the_search_results_no_longer_show_it() => _search.Results.Sentences.Results.Select(it => it.Question).Must().SequenceEqual([KeptSentence]);
   [XF] public void the_List_tab_no_longer_lists_it() =>
      Workspace.Navigation.NoteList.Notes.OfEachType.SelectMany(it => it.Notes).Select(it => it.Question).Must().SequenceEqual([KeptSentence]);
   [XF] public void the_List_tabs_title_counts_only_the_note_that_is_left() => Workspace.Navigation.NoteList.Title.Must().Be("Sentences about going (1)");
}
