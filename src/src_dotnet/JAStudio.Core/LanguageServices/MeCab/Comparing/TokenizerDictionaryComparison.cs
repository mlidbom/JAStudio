using System.Collections.Generic;
using System.Linq;

namespace JAStudio.Core.LanguageServices.MeCab.Comparing;

/// <summary>
/// How two tokenizer dictionaries tokenize the same sentences: where they cut them into tokens differently, and, for the tokens both cut alike,<br/>
/// how what each says of them corresponds: part of speech, inflection, base form and word. For switching JAStudio's tokenizer from IPADIC to<br/>
/// UniDic, it tells what parsing would have to be taught, and how two UniDic releases differ. A report of it is written by<br/>
/// <see cref="TokenizerDictionaryComparisonReport"/>.
/// </summary>
public sealed class TokenizerDictionaryComparison
{
   TokenizerDictionaryComparison(TokenizerDictionary first, TokenizerDictionary second)
   {
      First = first;
      Second = second;
   }

   /// <summary>The dictionary compared with <see cref="Second"/>, such as IPADIC, JAStudio's.</summary>
   public TokenizerDictionary First { get; }

   /// <summary>The dictionary compared with <see cref="First"/>.</summary>
   public TokenizerDictionary Second { get; }

   /// <summary>How many sentences were compared.</summary>
   public long Sentences { get; private set; }

   /// <summary>How many sentences both dictionaries cut into the same tokens.</summary>
   public long SentencesCutAlike { get; private set; }

   /// <summary>How many tokens the first dictionary cut the sentences into.</summary>
   public long FirstsTokens { get; private set; }

   /// <summary>How many tokens the second dictionary cut the sentences into.</summary>
   public long SecondsTokens { get; private set; }

   /// <summary>How many tokens both dictionaries cut alike.</summary>
   public long TokensCutAlike { get; private set; }

   /// <summary>How many of the first dictionary's tokens it doesn't have the word of (<see cref="RawMeCabToken.IsUnknownToTheDictionary"/>).</summary>
   public long FirstsUnknownTokens { get; private set; }

   /// <summary>How many of the second dictionary's tokens it doesn't have the word of (<see cref="RawMeCabToken.IsUnknownToTheDictionary"/>).</summary>
   public long SecondsUnknownTokens { get; private set; }

   /// <summary>Each difference in where the two cut tokens, with the sentences it's found in.</summary>
   public Tally<CuttingDifference> CuttingDifferences { get; } = new();

   /// <summary>The parts of speech of the tokens cut alike, the two dictionaries' side by side, with the tokens' surfaces.</summary>
   public Tally<Correspondence> PartsOfSpeech { get; } = new();

   /// <summary>The inflection types of the tokens cut alike that either dictionary gives one, with the tokens' surfaces.</summary>
   public Tally<Correspondence> InflectionTypes { get; } = new();

   /// <summary>The inflection forms of the tokens cut alike that either dictionary gives one, with the tokens' surfaces.</summary>
   public Tally<Correspondence> InflectionForms { get; } = new();

   /// <summary>The base forms of the tokens cut alike that both dictionaries know and give different base forms, with the tokens' surfaces.</summary>
   public Tally<Correspondence> BaseForms { get; } = new();

   /// <summary>The words (<see cref="RawMeCabToken.Word"/>) of the tokens cut alike that both dictionaries know and give different words, with the tokens' surfaces.</summary>
   public Tally<Correspondence> Words { get; } = new();

   /// <summary>The tokens cut alike that the first dictionary doesn't know and the second does, by surface and the second's part of speech.</summary>
   public Tally<Correspondence> UnknownToTheFirstOnly { get; } = new();

   /// <summary>The tokens cut alike that the second dictionary doesn't know and the first does, by surface and the first's part of speech.</summary>
   public Tally<Correspondence> UnknownToTheSecondOnly { get; } = new();

   /// <summary>The comparison of <paramref name="first"/> and <paramref name="second"/> over <paramref name="sentences"/>, tokenized on many threads at once.</summary>
   public static TokenizerDictionaryComparison Of(TokenizerDictionary first, TokenizerDictionary second, IReadOnlyList<string> sentences) =>
      sentences.AsParallel()
               .Aggregate(() => new TokenizerDictionaryComparison(first, second),
                          (comparison, sentence) => comparison.WithTheSentenceCompared(sentence),
                          (comparison, another) => comparison.WithEverythingIn(another),
                          comparison => comparison);

   TokenizerDictionaryComparison WithTheSentenceCompared(string sentence)
   {
      var firstsTokens = First.Tokenize(sentence);
      var secondsTokens = Second.Tokenize(sentence);
      var spans = AlignedSpan.Of(firstsTokens, secondsTokens);

      Sentences++;
      if(spans.All(it => it.IsCutAlike)) SentencesCutAlike++;
      FirstsTokens += firstsTokens.Count;
      SecondsTokens += secondsTokens.Count;
      FirstsUnknownTokens += firstsTokens.Count(it => it.IsUnknownToTheDictionary);
      SecondsUnknownTokens += secondsTokens.Count(it => it.IsUnknownToTheDictionary);

      foreach(var span in spans)
      {
         if(span.IsCutAlike) CountTheTokenCutAlike(span.First[0], span.Second[0]);
         else CuttingDifferences.Add(CuttingDifference.In(span), sentence);
      }

      return this;
   }

   void CountTheTokenCutAlike(RawMeCabToken firsts, RawMeCabToken seconds)
   {
      TokensCutAlike++;
      PartsOfSpeech.Add(new Correspondence(firsts.PartOfSpeech, seconds.PartOfSpeech), firsts.Surface);
      if(IsInflected(firsts) || IsInflected(seconds))
      {
         InflectionTypes.Add(new Correspondence(firsts.InflectionType, seconds.InflectionType), firsts.Surface);
         InflectionForms.Add(new Correspondence(firsts.InflectionForm, seconds.InflectionForm), firsts.Surface);
      }

      if(firsts.IsUnknownToTheDictionary && !seconds.IsUnknownToTheDictionary) UnknownToTheFirstOnly.Add(new Correspondence(firsts.Surface, seconds.PartOfSpeech), firsts.Surface);
      if(seconds.IsUnknownToTheDictionary && !firsts.IsUnknownToTheDictionary) UnknownToTheSecondOnly.Add(new Correspondence(seconds.Surface, firsts.PartOfSpeech), seconds.Surface);
      if(firsts.IsUnknownToTheDictionary || seconds.IsUnknownToTheDictionary) return;

      if(firsts.BaseForm != seconds.BaseForm) BaseForms.Add(new Correspondence(firsts.BaseForm, seconds.BaseForm), firsts.Surface);
      if(firsts.Word != seconds.Word) Words.Add(new Correspondence(firsts.Word, seconds.Word), firsts.Surface);

      static bool IsInflected(RawMeCabToken token) => token.InflectionType != RawMeCabToken.Nothing;
   }

   TokenizerDictionaryComparison WithEverythingIn(TokenizerDictionaryComparison another)
   {
      Sentences += another.Sentences;
      SentencesCutAlike += another.SentencesCutAlike;
      FirstsTokens += another.FirstsTokens;
      SecondsTokens += another.SecondsTokens;
      TokensCutAlike += another.TokensCutAlike;
      FirstsUnknownTokens += another.FirstsUnknownTokens;
      SecondsUnknownTokens += another.SecondsUnknownTokens;
      CuttingDifferences.AddEverythingIn(another.CuttingDifferences);
      PartsOfSpeech.AddEverythingIn(another.PartsOfSpeech);
      InflectionTypes.AddEverythingIn(another.InflectionTypes);
      InflectionForms.AddEverythingIn(another.InflectionForms);
      BaseForms.AddEverythingIn(another.BaseForms);
      Words.AddEverythingIn(another.Words);
      UnknownToTheFirstOnly.AddEverythingIn(another.UnknownToTheFirstOnly);
      UnknownToTheSecondOnly.AddEverythingIn(another.UnknownToTheSecondOnly);
      return this;
   }
}
