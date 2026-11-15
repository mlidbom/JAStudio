namespace JAStudio.Core.Note.Sentences.ParsingCases;

/// <summary>
/// What a person has found about one aspect of what a parsing case expects its sentence to parse to (<see cref="ParsingCase.Expected"/>), such as<br/>
/// the vocab its breakdown displays (<see cref="DisplayedVocab"/>): that it's right, that it's wrong, or nothing yet.
/// </summary>
public enum Verdict
{
   /// <summary>Nobody has judged it: it's what the sentence parsed to when it was recorded, right or wrong.</summary>
   Unverified,

   /// <summary>A person has found it right. The verified parsing cases are the gold standard that a change to parsing is measured against.</summary>
   Verified,

   /// <summary>A person has found it wrong: a bug to come back to and fix.</summary>
   Broken
}
