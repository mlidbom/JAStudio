using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Sentences;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Storage;

/// <summary>
/// The corpus stores a sentence's incorrect and hidden matches in a fixed order (<see cref="WordExclusion.InAFixedOrder"/>), so that the same matches<br/>
/// are stored the same way however the edits that made them went. In the order of the sentence's set, as they once were, two ways of making the<br/>
/// same edits could store the same matches in two orders.
/// </summary>
public class When_two_sentences_are_given_the_same_incorrect_and_hidden_matches_in_opposite_orders : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   const string TheSentence = "猫がいる。犬を見る。";
   static readonly IReadOnlyList<WordExclusion> TheMatches = [WordExclusion.AtIndex("見る", 7), WordExclusion.AtIndex("いる", 2), WordExclusion.Global("いる")];
   static readonly IReadOnlyList<StoredMatch> InTheFixedOrder = [new("いる", -1), new("いる", 2), new("見る", 7)];

   readonly IReadOnlyList<StoredMatches> _storedByEach;

   public When_two_sentences_are_given_the_same_incorrect_and_hidden_matches_in_opposite_orders() =>
      _storedByEach = [TheStoredMatchesOf(ASentenceGiven(TheMatches)), TheStoredMatchesOf(ASentenceGiven([.. TheMatches.Reverse()]))];

   [XF] public void each_stores_its_incorrect_matches_by_word_in_ordinal_order_then_by_index() =>
      _storedByEach.Must().Satisfy(it => it.All(stored => stored.IncorrectMatches.SequenceEqual(InTheFixedOrder)));

   [XF] public void each_stores_its_hidden_matches_by_word_in_ordinal_order_then_by_index() =>
      _storedByEach.Must().Satisfy(it => it.All(stored => stored.HiddenMatches.SequenceEqual(InTheFixedOrder)));

   SentenceNote ASentenceGiven(IReadOnlyList<WordExclusion> matches)
   {
      var sentence = CreateSentence(TheSentence);
      sentence.Edit(it =>
      {
         foreach(var match in matches)
         {
            it.Configuration.IncorrectMatches.Add(match);
            it.Configuration.HiddenMatches.Add(match);
         }
      });
      return sentence;
   }

   StoredMatches TheStoredMatchesOf(SentenceNote sentence)
   {
      using var stored = JsonDocument.Parse(TheStoredFileOf(sentence));
      var configuration = stored.RootElement.GetProperty("configuration");
      return new StoredMatches(TheStored(configuration.GetProperty("incorrectMatches")), TheStored(configuration.GetProperty("hiddenMatches")));
   }

   static IReadOnlyList<StoredMatch> TheStored(JsonElement matches) =>
      [.. matches.EnumerateArray().Select(it => new StoredMatch(it.GetProperty("word").GetString(), it.GetProperty("index").GetInt32()))];

   /// <summary>A sentence's incorrect and hidden matches, as its file holds them.</summary>
   record StoredMatches(IReadOnlyList<StoredMatch> IncorrectMatches, IReadOnlyList<StoredMatch> HiddenMatches);

   /// <summary>An incorrect or hidden match as a sentence's file holds it: its word, and its index, or -1 for the word everywhere in the sentence.</summary>
   record StoredMatch(string? Word, int Index);
}
