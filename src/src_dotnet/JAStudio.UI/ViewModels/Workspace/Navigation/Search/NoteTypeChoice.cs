using System.Collections.Generic;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Search.NoteTypeProperties;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.UI.ViewModels.Workspace.Navigation.Search;

/// <summary>
/// A note type as the <see cref="NoteTypeConditionEditorViewModel"/> and the note type's submenu of the search's add menu list it, such as "Kanji",<br/>
/// with the condition that picking it makes.
/// </summary>
/// <param name="MenuKey">The key that picks this choice in the note type's submenu of the add menu, such as k for "Kanji".</param>
sealed record NoteTypeChoice(char MenuKey, string Name, NoteTypeCondition Condition)
{
   /// <summary>A choice for each note type, in the order they're listed, each making a condition on <paramref name="property"/>.</summary>
   public static IReadOnlyList<NoteTypeChoice> AllFor(NoteTypeProperty property) =>
   [
      new('v', "Vocab", property.Is<VocabNote>()),
      new('k', "Kanji", property.Is<KanjiNote>()),
      new('s', "Sentence", property.Is<SentenceNote>())
   ];

   public override string ToString() => Name;
}
