using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Compze.Contracts;
using JAStudio.Core.LanguageServices.Parsing.Tokenizing;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Code;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Tokens;

/// <summary>
/// A condition that a token's part of speech is <see cref="JapaneseLevels"/>, or a kind of it: the tokenizer's part of speech, UniDic's, which has up<br/>
/// to four levels, such as 名詞・固有名詞・人名 for a person's name. 名詞・固有名詞 is met by every kind of proper noun, 名詞 by every kind of noun.<br/>
/// Written <c>PartOfSpeech.Is(...)</c>: made by <see cref="PartOfSpeechTest{TFrom}.Is"/>, on <see cref="TokenConditions.PartOfSpeech"/> or on a way to a token,<br/>
/// such as <c>FollowingToken.PartOfSpeech</c>.
/// </summary>
/// <remarks>
/// A piece JAStudio split off a token, such as a dictionary-form verb's ending, has the part of speech of the token it was split from<br/>
/// (<see cref="AnalysisToken.SourceToken"/>).
/// </remarks>
public sealed record PartOfSpeechIs : Condition<AnalysisToken>
{
   const string LevelSeparator = "・";
   const string EnglishLevelSeparator = " › ";
   readonly IReadOnlyList<string> _levels;

   /// <exception cref="Compze.Contracts.Exceptions.ArgumentAssertionFailedException">When no part of speech the tokenizer gives is, or is a kind of, <paramref name="japaneseLevels"/>.</exception>
   internal PartOfSpeechIs(IReadOnlyList<string> japaneseLevels)
   {
      _levels = japaneseLevels;
      JapaneseLevels = string.Join(LevelSeparator, japaneseLevels);
      var whyTheyAreNoPartOfSpeech = WhyTheyAreNoPartOfSpeech(japaneseLevels);
      Contract.Argument.Assert(whyTheyAreNoPartOfSpeech is null, () => $"{whyTheyAreNoPartOfSpeech}");
   }

   /// <param name="japaneseLevels">The levels joined by ・, as <see cref="JapaneseLevels"/> has them.</param>
   /// <exception cref="Compze.Contracts.Exceptions.ArgumentAssertionFailedException">As the other constructor.</exception>
   internal PartOfSpeechIs(string japaneseLevels) : this(japaneseLevels.Split(LevelSeparator)) {}

   /// <summary>The condition typed into the tree editor: the levels from the top, separated by spaces or ・, such as 名詞 固有名詞.</summary>
   /// <exception cref="Compze.Contracts.Exceptions.ArgumentAssertionFailedException">When <see cref="WhyTypedLevelsAreNoPartOfSpeech"/> says why they're none.</exception>
   internal static PartOfSpeechIs FromTyped(string typedLevels) => new(TheLevelsTyped(typedLevels));

   /// <summary>
   /// Every part of speech the tokenizer gives, and every kind of one, such as 名詞, 名詞・固有名詞 and 名詞・固有名詞・人名, each with its English, in ordinal order:<br/>
   /// what the tree editor offers to pick from.
   /// </summary>
   internal static IReadOnlyList<TokenizerValue> PickableValues()
   {
      return
      [
         ..UniDicPartOfSpeech.All
                          .SelectMany(ItAndEachPartOfSpeechItIsAKindOf)
                          .Select(ValueOf)
                          .DistinctBy(it => it.Value)
                          .OrderBy(it => it.Value, StringComparer.Ordinal)
      ];

      static IEnumerable<IReadOnlyList<string>> ItAndEachPartOfSpeechItIsAKindOf(UniDicPartOfSpeech partOfSpeech) =>
         Enumerable.Range(1, partOfSpeech.Levels.Count).Select(count => partOfSpeech.Levels.Take(count).Select(it => it.Japanese).ToList());
   }

   /// <summary>The part of speech <paramref name="japaneseLevels"/>, its levels from the top, with their English, such as 名詞・固有名詞 with noun › proper-noun.</summary>
   static TokenizerValue ValueOf(IReadOnlyList<string> japaneseLevels) =>
      new(string.Join(LevelSeparator, japaneseLevels), string.Join(EnglishLevelSeparator, japaneseLevels.Select(UniDicPartOfSpeech.EnglishNameOf)));

   /// <summary>Why the levels typed into the tree editor are no part of speech the tokenizer gives, or null when they are one.</summary>
   internal static string? WhyTypedLevelsAreNoPartOfSpeech(string typedLevels) => WhyTheyAreNoPartOfSpeech(TheLevelsTyped(typedLevels));

   static string[] TheLevelsTyped(string typedLevels) => typedLevels.Split([" ", LevelSeparator], StringSplitOptions.RemoveEmptyEntries);

   /// <summary>Why <paramref name="levels"/> are no part of speech the tokenizer gives, nor a kind of one, or null when they are.</summary>
   static string? WhyTheyAreNoPartOfSpeech(IReadOnlyList<string> levels) =>
      levels.Count == 0                    ? "A part of speech needs at least its top level, such as 名詞."
      : !UniDicPartOfSpeech.AnyIsWithin(levels) ? $"The tokenizer gives no part of speech that is, or is a kind of, {string.Join(LevelSeparator, levels)}."
                                             : null;

   /// <summary>The part of speech's levels, from the top, in Japanese, joined by ・, such as 名詞・固有名詞・人名.</summary>
   public string JapaneseLevels { get; }

   public override bool IsMetBy(AnalysisToken subject) =>
      //review:matching: a piece JAStudio split off a token reads the token's part of speech, as it reads its inflection type, inflected form
      //   and reading: so the ending る of 食べる is a verb. My call on the design document's open question 4.
      subject.SourceToken.PartsOfSpeech.IsWithin(_levels);

   /// <summary>The condition in code, with the levels' English names as a comment: <c>PartOfSpeech.Is(名詞・固有名詞・人名)  // noun › proper-noun › person-name</c>.</summary>
   internal override IEnumerable<CodeLine> CodeLines(CodeWriting writing) => writing.LinesListing("PartOfSpeech.Is", [ValueOf(_levels)]);

   /// <summary>The key this condition is stored under in the corpus.</summary>
   internal const string JsonKey = "partOfSpeech";

   internal override JsonObject ToJson() => new() { [JsonKey] = JapaneseLevels };

   public bool Equals(PartOfSpeechIs? other) => other is not null && JapaneseLevels == other.JapaneseLevels;

   public override int GetHashCode() => JapaneseLevels.GetHashCode();
}
