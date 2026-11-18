namespace JAStudio.Core.Specifications.LanguageServices.ParsingCases;

/// <summary>A JAStudio holding the parsing cases' corpus, which <see cref="Each_parsing_case"/> parses every case against.</summary>
public sealed class JAStudioWithTheParsingCasesCorpus() : JAStudioWithTheVocabOf(ParsingCasesFolder.Corpus);
