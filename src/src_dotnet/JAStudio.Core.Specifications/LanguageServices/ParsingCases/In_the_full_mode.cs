using System;
using Compze.Must;
using Compze.xUnitBDD;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.LanguageServices.ParsingCases;

/// <summary>
/// The full mode of the parsing cases (<see cref="ParsingCase"/>), run by hand and often: the cases parsed against the whole corpus checkout<br/>
/// (<see cref="SourceCheckout.CorpusCheckout"/>), which brings the stripped-down corpus (<see cref="ParsingCasesFolder.StrippedDownCorpus"/>) up to<br/>
/// date with it, in a <see cref="FullModeRun"/>. A case that then fails shows a change to the corpus, or to the code, that changed how its sentence parses. Explicit,<br/>
/// so that only running it by name runs it: it needs the corpus checkout, takes a while, and writes the stripped-down corpus, and in accepting,<br/>
/// the case files. Review what it wrote in git's diff before committing it.
/// </summary>
public abstract class In_the_full_mode
{
   /// <summary>
   /// The mode's one run, which all its specifications read. Each mode makes it once, however many of its specifications run, and at once: two runs<br/>
   /// would each rewrite the stripped-down corpus, and running side by side, as the specifications do, write and delete each other's files.
   /// </summary>
   private protected abstract FullModeRun Run { get; }

   public class refreshing_the_stripped_down_corpus_from_the_corpus_checkout : In_the_full_mode
   {
      static readonly Lazy<FullModeRun> TheRun = new(() => new FullModeRun());
      private protected override FullModeRun Run => TheRun.Value;

      [XF(Explicit = true)] public void every_case_parses_against_it_exactly_as_against_the_corpus_checkout() => Run.CasesParsingOtherwiseAgainstTheStrippedDownCorpus.Must().BeEmpty();
      [XF(Explicit = true)] public void every_case_parses_against_the_corpus_checkout_exactly_as_the_case_expects() => Run.HowTheCasesDifferFromTheExpected.Must().BeEmpty();
   }

   public class accepting_what_each_case_parses_to_against_the_corpus_checkout : In_the_full_mode
   {
      static readonly Lazy<FullModeRun> TheRun = new(RunAcceptingWhatEachCaseParsesTo);
      private protected override FullModeRun Run => TheRun.Value;

      [XF(Explicit = true)] public void every_case_parses_against_the_stripped_down_corpus_exactly_as_against_the_corpus_checkout() => Run.CasesParsingOtherwiseAgainstTheStrippedDownCorpus.Must().BeEmpty();

      static FullModeRun RunAcceptingWhatEachCaseParsesTo()
      {
         var run = new FullModeRun();
         run.AcceptWhatEachCaseParsesTo();
         return run;
      }
   }
}
