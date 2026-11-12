using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing;

namespace JAStudio.UI.ViewModels.Workspace.Navigation;

/// <summary>
/// Notes to list in the navigation pane's "List" tab (<see cref="NoteListViewModel"/>), with a title saying what they are,<br/>
/// such as the sentences a vocab matches. The "Open" items of the note menus show one.
/// </summary>
/// <param name="Title">What the notes are, such as "Sentences with 走る".</param>
/// <param name="Notes">The notes. The "List" tab orders them by their question.</param>
record NoteList(string Title, IReadOnlyList<JPNote> Notes)
{
   /// <summary>
   /// The vocab whose match-conditions depend on <paramref name="sharedCondition"/>'s definition, directly or through other shared-conditions<br/>
   /// (<see cref="VocabCollection.WhoseMatchConditionsDependOnTheDefinitionOf"/>), as <paramref name="vocab"/> holds them now: the list a shared-condition's<br/>
   /// window, and the window listing the shared-conditions, open.
   /// </summary>
   public static NoteList OfTheVocabDependingOn(ISharedCondition sharedCondition, VocabCollection vocab) =>
      new($"Vocab depending on {sharedCondition.FullName}", [.. vocab.WhoseMatchConditionsDependOnTheDefinitionOf(sharedCondition)]);

   /// <summary>The same list without <paramref name="deleted"/>.</summary>
   public NoteList Without(JPNote deleted) => this with { Notes = [.. Notes.Where(it => !it.GetId().Equals(deleted.GetId()))] };
}
