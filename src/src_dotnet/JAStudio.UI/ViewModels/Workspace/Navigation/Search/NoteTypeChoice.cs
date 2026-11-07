using JAStudio.Core.Note.Search.NoteTypeProperties;

namespace JAStudio.UI.ViewModels.Workspace.Navigation.Search;

/// <summary>A note type as the <see cref="NoteTypeConditionEditorViewModel"/> lists it, such as "Kanji", with the condition that picking it makes.</summary>
sealed record NoteTypeChoice(string Name, NoteTypeCondition Condition)
{
   public override string ToString() => Name;
}
