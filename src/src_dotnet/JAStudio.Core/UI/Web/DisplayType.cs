using Compze.Internals.SystemCE;
using System.Collections.Generic;

namespace JAStudio.Core.UI.Web;

/// <summary>
/// Display type constants and checks.
/// </summary>
static class DisplayType
{
   public const string ReviewQuestion = "reviewQuestion";
   public const string ReviewAnswer = "reviewAnswer";

   static readonly HashSet<string> QuestionTypes = ["reviewQuestion", "previewQuestion", "clayoutQuestion"];
   static readonly HashSet<string> AnswerTypes = ["reviewAnswer", "previewAnswer", "clayoutAnswer"];

   public static bool IsDisplayingQuestion(string displayType) => QuestionTypes.Contains(displayType);

   public static bool IsDisplayingAnswer(string displayType) => AnswerTypes.Contains(displayType);

   public static bool IsDisplayingReviewQuestion(string displayType) => displayType == ReviewQuestion;

   public static bool IsDisplayingReviewAnswer(string displayType) => displayType == ReviewAnswer;

   public static bool IsReview(string displayType) => displayType.StartsWithOrdinal("review");
}
