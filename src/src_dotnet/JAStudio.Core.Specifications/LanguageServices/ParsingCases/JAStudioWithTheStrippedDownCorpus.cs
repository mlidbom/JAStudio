namespace JAStudio.Core.Specifications.LanguageServices.ParsingCases;

/// <summary>A JAStudio holding the parsing cases' stripped-down corpus, which <see cref="Each_parsing_case"/> parses every case against.</summary>
public sealed class JAStudioWithTheStrippedDownCorpus() : JAStudioWithTheVocabOf(ParsingCasesFolder.StrippedDownCorpus);
