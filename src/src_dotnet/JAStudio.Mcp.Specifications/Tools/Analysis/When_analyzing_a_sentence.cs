using System;
using System.Collections.Generic;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.LanguageServices.Parsing.WordExtraction;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Mcp.Tools.Analysis;
using ModelContextProtocol.Protocol;

// ReSharper disable InconsistentNaming

namespace JAStudio.Mcp.Specifications.Tools.Analysis;

/// <summary>Analyzes 明日学校に行く, "I'll go to school tomorrow", in a collection with the vocab 行く.</summary>
public class When_analyzing_a_sentence : McpServerSpecification
{
   const string Sentence = "明日学校に行く";
   readonly VocabNote _vocab行く;

   public When_analyzing_a_sentence() => _vocab行く = CreateVocab("行く", "to go", "いく");

   protected CallToolResult AnalyzeSentence(Dictionary<string, object?> arguments) => CallTool("analyze_sentence", arguments);

   /// <summary>The vocab match of the candidate word 行く made of the tokens 行 and く, as the sentence has them.</summary>
   protected static AnalyzedMatch TheMatchOf行くIn(AnalyzedSentence analysis) =>
      analysis.CandidateWords.Single(it => it is { Form: "行く", Variant: "surface" }).Matches.Single(it => it.MatchedTo == "vocab");

   static string IdOfTheVocabMatchedBy(AnalyzedMatch match) => match.VocabId ?? throw new InvalidOperationException($"{match.ParsedForm} isn't matched to a vocab.");

   public class given_as_text : When_analyzing_a_sentence
   {
      readonly AnalyzedSentence _analysis;
      public given_as_text() => _analysis = ResultOf<AnalyzedSentence>(AnalyzeSentence(new Dictionary<string, object?> { ["text"] = Sentence }));

      [XF] public void the_tokens_are_明日_学校_に_行_and_く() => _analysis.Tokens.Select(it => it.Surface).Must().SequenceEqual(["明日", "学校", "に", "行", "く"]);

      [XF] public void the_tokens_行_and_く_are_split_from_the_tokenizers_token_行く() =>
         _analysis.Tokens.Where(it => it.MadeFrom != null).Select(it => $"{it.Surface} from {it.MadeFrom}").Must().SequenceEqual(["行 from 行く", "く from 行く"]);

      [XF] public void 行く_is_among_the_displayed_words() => _analysis.DisplayedWords.Must().Contain("行く");
      [XF] public void the_candidate_word_行く_matches_the_vocab_行く() => IdOfTheVocabMatchedBy(TheMatchOf行くIn(_analysis)).Must().Be(_vocab行く.GetId().ToString());
      [XF] public void the_match_of_行く_is_valid() => TheMatchOf行くIn(_analysis).IsValid.Must().BeTrue();
      [XF] public void the_match_of_行く_is_displayed() => TheMatchOf行くIn(_analysis).IsDisplayed.Must().BeTrue();
   }

   public class given_as_a_sentence_note_whose_configuration_marks_the_match_of_行く_incorrect : When_analyzing_a_sentence
   {
      readonly AnalyzedSentence _analysis;

      public given_as_a_sentence_note_whose_configuration_marks_the_match_of_行く_incorrect()
      {
         var sentence = CreateSentence(Sentence);
         sentence.Configuration.IncorrectMatches.Add(WordExclusion.AtIndex("行く", Sentence.IndexOf("行く", StringComparison.Ordinal)));
         _analysis = ResultOf<AnalyzedSentence>(AnalyzeSentence(new Dictionary<string, object?> { ["sentenceNoteId"] = sentence.GetId().ToString() }));
      }

      [XF] public void the_match_of_行く_is_not_valid() => TheMatchOf行くIn(_analysis).IsValid.Must().BeFalse();
      [XF] public void the_failure_reason_is_that_it_is_configured_incorrect() => TheMatchOf行くIn(_analysis).FailureReasons.Must().Contain("forbids::configured_incorrect");
   }

   public class given_both_as_text_and_as_a_sentence_note : When_analyzing_a_sentence
   {
      readonly CallToolResult _result;

      public given_both_as_text_and_as_a_sentence_note() =>
         _result = AnalyzeSentence(new Dictionary<string, object?> { ["text"] = Sentence, ["sentenceNoteId"] = CreateSentence(Sentence).GetId().ToString() });

      [XF] public void the_tool_call_fails() => _result.IsError.Must().Be(true);
      [XF] public void the_error_says_to_give_one_of_them() => TextOf(_result).Must().Contain("Give either text or sentenceNoteId");
   }
}
