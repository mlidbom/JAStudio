using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Texts;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Tokens;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Words;

/// <summary>
/// The conditions there are on a word in its sentence, the subject of a vocab's match-condition: one variant of a candidate word<br/>
/// (<see cref="CandidateWordVariant"/>), its surface or its base form, whose text is one of the vocab's forms (<see cref="VocabNoteForms"/>). They're:
/// <list type="bullet">
/// <item>conditions on its tokens: the <see cref="PrecedingToken"/>, its <see cref="FirstToken"/> and <see cref="LastToken"/>, the <see cref="FollowingToken"/>;</item>
/// <item>conditions on its texts: the <see cref="PrecedingText"/> and <see cref="FollowingText"/> in the sentence, its <see cref="Surface"/> and its<br/>
/// <see cref="BaseForm"/>;</item>
/// <item>facts about it, such as <see cref="IsASingleToken"/>.</item>
/// </list>
/// </summary>
/// <remarks>
/// The tokens are those after JAStudio's preprocessing (<see cref="LanguageServices.JanomeEx.Tokenizing.PreProcessingStage.PreProcessingStage"/>), which<br/>
/// splits some of the tokenizer's tokens, such as every dictionary-form verb into its stem and its ending. So the token preceding a word after 食べる<br/>
/// is its ending, る.
/// </remarks>
public static class WordConditions
{
   /// <summary>The word is one token, not a compound of several.</summary>
   public static readonly Fact<CandidateWordVariant> IsASingleToken = new("is a single token", "singleToken", it => !it.Word.IsCompound);

   /// <summary>The word is matched by its surface, inflected or not, rather than by its base form (<see cref="CandidateWordVariant.IsSurface"/>).</summary>
   public static readonly Fact<CandidateWordVariant> IsMatchedByItsSurface = new("is matched by its surface", "matchedByItsSurface", it => it.IsSurface);

   /// <summary>The word is matched by its surface, and the surface is inflected: it differs from the word's base form.</summary>
   public static readonly Fact<CandidateWordVariant> IsMatchedByAnInflectedSurface =
      new("is matched by an inflected surface", "matchedByAnInflectedSurface", it => it.IsSurface && it.Word.SurfaceForm != it.Word.BaseForm);

   /// <summary>The token before the word's first token. The sentence's first token has none.</summary>
   public static readonly ConditionSubject<CandidateWordVariant, IAnalysisToken> PrecedingToken =
      new("The preceding token", "precedingToken", it => it.Word.StartLocation.Previous?.Token, TokenConditions.Level);

   /// <summary>The word's first token, its only one if it's a single token.</summary>
   public static readonly ConditionSubject<CandidateWordVariant, IAnalysisToken> FirstToken =
      new("The first token", "firstToken", it => it.Word.StartLocation.Token, TokenConditions.Level);

   /// <summary>The word's last token, its only one if it's a single token.</summary>
   public static readonly ConditionSubject<CandidateWordVariant, IAnalysisToken> LastToken =
      new("The last token", "lastToken", it => it.Word.EndLocation.Token, TokenConditions.Level);

   /// <summary>The token after the word's last token. The sentence's last token has none.</summary>
   public static readonly ConditionSubject<CandidateWordVariant, IAnalysisToken> FollowingToken =
      new("The following token", "followingToken", it => it.Word.EndLocation.Next?.Token, TokenConditions.Level);

   /// <summary>The sentence's text before the word, from the sentence's start (<see cref="CandidateWord.PrecedingText"/>).</summary>
   public static readonly ConditionSubject<CandidateWordVariant, string> PrecedingText =
      new("The preceding text", "precedingText", it => it.Word.PrecedingText, TextConditions.Level);

   /// <summary>The sentence's text after the word, to the sentence's end (<see cref="CandidateWord.FollowingText"/>).</summary>
   public static readonly ConditionSubject<CandidateWordVariant, string> FollowingText =
      new("The following text", "followingText", it => it.Word.FollowingText, TextConditions.Level);

   /// <summary>The word's surface, its text as the sentence has it.</summary>
   public static readonly ConditionSubject<CandidateWordVariant, string> Surface =
      new("The surface", "surface", it => it.Word.SurfaceForm, TextConditions.Level);

   /// <summary>The word's base form: its surface, with its last token's base form in place of the last token's surface (<see cref="CandidateWord.BaseForm"/>).</summary>
   public static readonly ConditionSubject<CandidateWordVariant, string> BaseForm =
      new("The base form", "baseForm", it => it.Word.BaseForm, TextConditions.Level);

   /// <summary>The conditions on a word, and how they're read back from the corpus.</summary>
   public static readonly ConditionLevel<CandidateWordVariant> Level =
      new("a word",
          [IsASingleToken, IsMatchedByItsSurface, IsMatchedByAnInflectedSurface],
          [PrecedingToken, FirstToken, LastToken, FollowingToken, PrecedingText, FollowingText, Surface, BaseForm],
          new Dictionary<string, Func<JsonNode, Condition<CandidateWordVariant>>>());
}
