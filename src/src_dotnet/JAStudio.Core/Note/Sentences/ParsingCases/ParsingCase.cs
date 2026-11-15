using JAStudio.Core.Note.Collection;

namespace JAStudio.Core.Note.Sentences.ParsingCases;

/// <summary>
/// What makes a sentence of the corpus a parsing case (<see cref="SentenceNote.ParsingCase"/>): the exact result the sentence is expected to parse<br/>
/// to, and a person's verdict on it. The parsing cases are what a change to parsing is measured against: a case that parses otherwise than<br/>
/// expected shows what the change did to its sentence (<see cref="ParsingCaseComparison"/>).
/// </summary>
/// <remarks>
/// <see cref="Expected"/> records everything the sentence parsed to, and nobody vouches for all of it. A person's verdict is on one aspect of it,<br/>
/// the vocab the breakdown displays (<see cref="ParsingCaseResult.DisplayedVocab"/>). Further aspects get a verdict of their own when it's clear<br/>
/// what verifying them means.
/// </remarks>
/// <param name="Name">What the case is about, such as <c>ichidan imperative</c>. Empty until a person names it.</param>
/// <param name="Why">Why the case exists, such as the misparse it caught, or what is wrong with how it parses. Empty until a person says.</param>
/// <param name="VerdictOnTheDisplayedVocab">What a person has found about the vocab <see cref="Expected"/> has the breakdown display.</param>
/// <param name="Expected">The result the sentence is expected to parse to, without the questions of the vocab, which a vocab's id stands for.</param>
public sealed record ParsingCase(string Name, string Why, Verdict VerdictOnTheDisplayedVocab, ParsingCaseResult Expected)
{
   /// <summary>A parsing case nobody has named or judged, expecting <paramref name="whatTheSentenceParsesTo"/>.</summary>
   public static ParsingCase Expecting(ParsingCaseResult whatTheSentenceParsesTo) => new("", "", Verdict.Unverified, whatTheSentenceParsesTo.WithoutTheQuestionsOfTheVocab());

   /// <summary>This case expecting <paramref name="whatTheSentenceParsesTo"/> instead: a person's verdict stays as it is.</summary>
   public ParsingCase Accepting(ParsingCaseResult whatTheSentenceParsesTo) => this with { Expected = whatTheSentenceParsesTo.WithoutTheQuestionsOfTheVocab() };

   /// <summary>How <paramref name="whatTheSentenceParsesTo"/> compares with what this case expects, naming each vocab by its question in <paramref name="collection"/>.</summary>
   public ParsingCaseComparison ComparedWith(ParsingCaseResult whatTheSentenceParsesTo, JPCollection collection) =>
      new(this, whatTheSentenceParsesTo, Expected.HowItDiffersFromThisExpectedResult(whatTheSentenceParsesTo, collection));
}
