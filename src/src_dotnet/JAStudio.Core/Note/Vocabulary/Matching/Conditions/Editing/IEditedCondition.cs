using System.Collections.Generic;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Code;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Editing;

/// <summary>
/// What the tree editor edits, and where it stores each change: a vocab's match-condition (<see cref="EditedMatchCondition"/>), in its match-condition<br/>
/// window, or a shared-condition's definition (<see cref="EditedDefinition"/>), in the shared-condition's window. Either is edited as a tree with an<br/>
/// "all of" group at its root, as <see cref="EditableCondition"/> builds it.
/// </summary>
public interface IEditedCondition
{
   /// <summary>The conditions there are at the root: those on a word for a match-condition, those on its subject for a definition.</summary>
   IConditionLevel Level { get; }

   /// <summary>The condition as it's stored now, as a tree with an "all of" group at its root.</summary>
   Condition Stored { get; }

   /// <summary>Stores <paramref name="tree"/>, a tree as the editor built it, with an "all of" group at its root, in place of the condition.</summary>
   void Store(Condition tree);

   /// <summary>
   /// The lines of <paramref name="tree"/>, a tree as the editor builds it, as the editor shows them: a comment heading on the root's line, saying what the<br/>
   /// tree is, then each of the root's conditions a statement of its own.
   /// </summary>
   IReadOnlyList<CodeLine> LinesOf(Condition tree);

   /// <summary>The shared-conditions on <paramref name="level"/>'s subject the add menu offers: for a definition, those it can use without using itself.</summary>
   IReadOnlyList<ISharedCondition> SharedConditionsOfferedOn(IConditionLevel level);
}
