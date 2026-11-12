using System.Text.Json.Nodes;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing;

/// <summary>
/// A <see cref="SharedCondition{TSubject}"/>, whatever its subject: a condition with a name, defined once in the corpus and used by name in any vocab's<br/>
/// match-condition, as the match-condition editor and <see cref="SharedConditions"/> work with it.
/// </summary>
public interface ISharedCondition
{
   /// <summary>Its name, as a match-condition uses it, such as <c>FollowsAPastTenseStem</c>, or <c>PrecedingToken.IsAdverbLike</c> for one on a token.</summary>
   string Name { get; }

   /// <summary>Its name with its subject's in front, as it's defined, such as <c>Word.FollowsAPastTenseStem</c>: what names its file in the corpus.</summary>
   string FullName { get; }

   /// <summary>The conditions on its subject, among which it's offered: those on a token for one on a token.</summary>
   IConditionLevel Level { get; }

   /// <summary>The condition it stands for: a subject meets the shared-condition when it meets this.</summary>
   Condition Definition { get; }

   /// <summary>The shared-condition as the condition a match-condition holds where it uses it, by name: itself.</summary>
   Condition AsACondition { get; }

   /// <summary>Gives it the definition <paramref name="json"/> holds, as the corpus stores it, its shared-conditions found by name in <paramref name="sharedConditions"/>.</summary>
   internal void DefineFrom(JsonObject json, SharedConditions sharedConditions);

   /// <summary>Whether its definition uses it, directly or through the definitions of the shared-conditions it uses: what no shared-condition may do.</summary>
   internal bool UsesItself { get; }
}
