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
public class In_the_full_mode
{
   readonly FullModeRun _run = new();

   public class refreshing_the_stripped_down_corpus_from_the_corpus_checkout : In_the_full_mode
   {
      [XF(Explicit = true)] public void every_case_parses_against_it_exactly_as_against_the_corpus_checkout() => _run.CasesParsingOtherwiseAgainstTheStrippedDownCorpus.Must().BeEmpty();
      [XF(Explicit = true)] public void every_case_parses_against_the_corpus_checkout_exactly_as_the_case_expects() => _run.HowTheCasesDifferFromTheExpected.Must().BeEmpty();
   }

   public class accepting_what_each_case_parses_to_against_the_corpus_checkout : In_the_full_mode
   {
      public accepting_what_each_case_parses_to_against_the_corpus_checkout() => _run.AcceptWhatEachCaseParsesTo();

      [XF(Explicit = true)] public void every_case_parses_against_the_stripped_down_corpus_exactly_as_against_the_corpus_checkout() => _run.CasesParsingOtherwiseAgainstTheStrippedDownCorpus.Must().BeEmpty();
   }
}
