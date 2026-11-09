using System.Collections.Generic;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Mcp.Tools.Reloading;
using ModelContextProtocol.Protocol;

// ReSharper disable InconsistentNaming

namespace JAStudio.Mcp.Specifications.Tools.Reloading;

/// <summary>
/// The corpus repository has committed files for the vocab 行く and 来る on the branch checked out. The branch <c>ahead</c> is one commit ahead of<br/>
/// it, in which 行く's file changed.
/// </summary>
public abstract class When_fast_forwarding_the_corpus_checkout : CorpusRepositorySpecification
{
   const string BranchAhead = "ahead";
   protected readonly string _commitBefore;
   protected readonly string _commitAhead;
   protected readonly VocabNote _vocab来る;

   protected When_fast_forwarding_the_corpus_checkout()
   {
      var vocab行く = CreateVocab("行く", "to go", "いく");
      _vocab来る = CreateVocab("来る", "to come", "くる");
      Write(FileOf(vocab行く), "{}");
      Write(FileOf(_vocab来る), "{}");
      _commitBefore = CommitEverything();

      Git("checkout", "--quiet", "-b", BranchAhead);
      Write(FileOf(vocab行く), """{"changed": true}""");
      _commitAhead = CommitEverything("行く changed");
      Git("checkout", "--quiet", "-");
   }

   protected CallToolResult FastForwardTo(string commit) =>
      CallTool("fast_forward_the_corpus_checkout", new Dictionary<string, object?> { ["commit"] = commit });

   public class to_a_branch_ahead_of_the_checkouts_head : When_fast_forwarding_the_corpus_checkout
   {
      readonly FastForwardResult _result;

      public to_a_branch_ahead_of_the_checkouts_head() => _result = ResultOf<FastForwardResult>(FastForwardTo(BranchAhead));

      [XF] public void the_checkout_is_at_the_commit_of_the_branch() => HeadCommit().Must().Be(_commitAhead);
      [XF] public void the_commit_before_is_reported() => _result.CommitBefore.Must().Be(_commitBefore);
      [XF] public void the_commit_after_is_reported() => _result.CommitAfter.Must().Be(_commitAhead);
      [XF] public void the_vocab_whose_file_changed_is_the_one_note_listed() => _result.ChangedNotes.Vocab.Notes.Select(it => it.Question).Must().SequenceEqual(["行く"]);
   }

   /// <summary>Since the branch <c>ahead</c> was made, 来る's file changed in a commit on the branch checked out, so <c>ahead</c> isn't ahead of it.</summary>
   public class to_a_branch_that_is_not_ahead_of_the_checkouts_head : When_fast_forwarding_the_corpus_checkout
   {
      readonly string _commitOfTheCheckout;
      readonly CallToolResult _result;

      public to_a_branch_that_is_not_ahead_of_the_checkouts_head()
      {
         Write(FileOf(_vocab来る), """{"changed": true}""");
         _commitOfTheCheckout = CommitEverything("来る changed");

         _result = FastForwardTo(BranchAhead);
      }

      [XF] public void the_tool_fails() => _result.IsError.Must().Be(true);
      [XF] public void the_failure_is_gits_refusal_to_fast_forward() => TextOf(_result).Must().Contain("Not possible to fast-forward");
      [XF] public void the_checkout_stays_at_its_commit() => HeadCommit().Must().Be(_commitOfTheCheckout);
      [XF] public void the_collection_holds_its_notes_again() => GetService<JPCollection>().Vocab.All().Count.Must().Be(2);
   }
}
