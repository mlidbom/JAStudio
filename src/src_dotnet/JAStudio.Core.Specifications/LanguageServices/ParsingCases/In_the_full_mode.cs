using System;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Sentences;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.LanguageServices.ParsingCases;

/// <summary>
/// The full mode of the parsing cases, run by hand and often: it generates the case files (<see cref="ParsingCaseFile"/>) from the sentences of the corpus<br/>
/// checkout that are parsing cases (<see cref="SentenceNote.ParsingCase"/>), and brings the parsing cases' corpus<br/>
/// (<see cref="ParsingCasesFolder.Corpus"/>) up to date with the corpus checkout, in a <see cref="FullModeRun"/>. The corpus is the master: a<br/>
/// case is made, judged and accepted on its sentence in JAStudio, and this brings the specifications in line with it.
/// </summary>
/// <remarks>
/// Explicit, so that only running it by name runs it: it needs the corpus checkout (<see cref="SourceCheckout.CorpusCheckout"/>), takes a while, and<br/>
/// writes the case files and the parsing cases' corpus. Review what it wrote in git's diff before committing it.<br/>
/// In a linked worktree, check the corpus out first with <c>dev_scripts/check-out-the-corpus.ps1</c>: a new worktree has none.
/// </remarks>
public abstract class In_the_full_mode
{
   public class generating_the_case_files_and_the_parsing_cases_corpus_from_the_corpus_checkout : In_the_full_mode
   {
      /// <summary>
      /// The mode's one run, which both specifications read: two runs would each rewrite the case files and the parsing cases' corpus, and running side<br/>
      /// by side, as the specifications do, write and delete each other's files.
      /// </summary>
      static readonly Lazy<FullModeRun> TheRun = new(() => new FullModeRun());

      [XF(Explicit = true)] public void every_parsing_case_of_the_corpus_checkout_parses_there_exactly_as_it_expects() =>
         TheRun.Value.HowTheParsingCasesOfTheCorpusCheckoutDifferFromWhatTheyExpect.Must().BeEmpty();

      [XF(Explicit = true)] public void every_case_parses_against_the_parsing_cases_corpus_exactly_as_its_sentence_does_against_the_corpus_checkout() =>
         TheRun.Value.CasesParsingOtherwiseAgainstTheParsingCasesCorpus.Must().BeEmpty();
   }
}
