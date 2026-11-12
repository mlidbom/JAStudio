using System.Collections.Generic;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Code;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Editing;

/// <summary>
/// A shared-condition's definition as the tree editor in its window edits it. The tree's "all of" group at the root is the definition, when it's an<br/>
/// "all of" group, or else holds it. A tree of one condition stores that condition as the definition, and any other tree itself. Each change is stored<br/>
/// through <see cref="SharedConditions.Redefine"/>, which saves it in its file.
/// </summary>
public sealed class EditedDefinition : IEditedCondition
{
   readonly SharedConditions _sharedConditions;

   /// <param name="sharedCondition">The shared-condition whose definition is edited.</param>
   /// <param name="sharedConditions">The corpus's shared-conditions, <paramref name="sharedCondition"/> among them.</param>
   public EditedDefinition(ISharedCondition sharedCondition, SharedConditions sharedConditions)
   {
      SharedCondition = sharedCondition;
      _sharedConditions = sharedConditions;
   }

   /// <summary>The shared-condition whose definition is edited.</summary>
   public ISharedCondition SharedCondition { get; }

   public IConditionLevel Level => SharedCondition.Level;

   public Condition Stored => SharedCondition.DefinitionAsAnAllOf;

   public void Store(Condition tree) => _sharedConditions.Redefine(SharedCondition, DefinitionIn(tree));

   public IReadOnlyList<CodeLine> LinesOf(Condition tree) => ConditionCode.LinesOfADefinitionAsStatements(SharedCondition, GroupAtTheRootOf(tree));

   /// <summary>Those the definition can use without using itself (<see cref="SharedConditions.UsableInTheDefinitionOf"/>).</summary>
   public IReadOnlyList<ISharedCondition> SharedConditionsOfferedOn(IConditionLevel level) => _sharedConditions.UsableInTheDefinitionOf(SharedCondition, level);

   /// <summary>The definition <paramref name="tree"/> stands for: the one condition of its group at the root, when it has one, else the tree itself.</summary>
   static Condition DefinitionIn(Condition tree) =>
      //bug: a tree whose root holds one "all of" group, such as one just added, AllOf(AllOf()), stores the inner group, and Stored then gives that
      //   group as the tree, AllOf(), which isn't the tree the editor holds. The next ConditionTreeEditorViewModel.Refresh, as when the window is
      //   activated again after a condition's dialog closed over it, takes it in as a change made elsewhere: the group the user just added is gone
      //   from the tree, and undo has a step more. What the definition means is the same either way.
      GroupAtTheRootOf(tree).Conditions is [var onlyCondition] ? onlyCondition : tree;

   static IConditionGroup GroupAtTheRootOf(Condition tree) => (IConditionGroup)tree;
}
