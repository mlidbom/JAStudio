using System.Collections.Generic;

namespace JAStudio.Mcp.Tools.ParsingCases;

/// <summary>What <see cref="ParsingCaseTools.ListTheParsingCases"/> reports: how many parsing cases the corpus has of each outcome, and the cases listed.</summary>
/// <param name="ParsingCases">How many parsing cases the corpus has, listed or not.</param>
/// <param name="Failing">How many of them fail.</param>
/// <param name="ToReview">How many of them are to review.</param>
/// <param name="ParsingAsExpected">How many of them parse as expected.</param>
/// <param name="Cases">The cases listed, at most as many as asked for, those that fail first.</param>
public record ParsingCasesListed(int ParsingCases, int Failing, int ToReview, int ParsingAsExpected, IReadOnlyList<ParsingCaseListed> Cases);
