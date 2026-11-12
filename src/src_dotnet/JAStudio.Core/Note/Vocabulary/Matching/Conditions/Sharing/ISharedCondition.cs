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

   /// <summary>
   /// Gives it <paramref name="definition"/> in place of its definition, which <paramref name="definition"/> must mean the same as<br/>
   /// (<see cref="Condition.MeansTheSameAs"/>): its definition written another way, such as with another shared-condition used in it.
   /// </summary>
   internal void RewriteItsDefinitionAs(Condition definition);

   /// <summary>Its <see cref="Definition"/> as the tree editor edits it, with an "all of" group at the root: the definition itself when it's one, else a group holding it.</summary>
   Condition DefinitionAsAnAllOf { get; }

   /// <summary>Gives it <paramref name="definition"/>, a condition on its subject, in place of its definition: what it means changes.</summary>
   internal void Redefine(Condition definition);

   /// <summary>Gives it <paramref name="name"/> in place of its name: every tree using it then uses it by that name, since they all hold it.</summary>
   internal void Rename(string name);

   /// <summary>
   /// Whether its definition uses <paramref name="other"/>, directly or through the definitions of the shared-conditions it uses: whether what it means<br/>
   /// depends on what <paramref name="other"/> means.
   /// </summary>
   internal bool IsDefinedThrough(ISharedCondition other);

   /// <summary>Whether its definition uses it, directly or through the definitions of the shared-conditions it uses: what no shared-condition may do.</summary>
   internal bool UsesItself => IsDefinedThrough(this);
}
