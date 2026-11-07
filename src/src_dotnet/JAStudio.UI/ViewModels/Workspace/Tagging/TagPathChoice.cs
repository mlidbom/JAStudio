namespace JAStudio.UI.ViewModels.Workspace.Tagging;

/// <summary>
/// A tag path a <see cref="TagPickerViewModel"/> lists to pick from: one of the paths it offers,<br/>
/// or, when <see cref="IsNew"/>, the typed text itself as a tag no note has yet.
/// </summary>
sealed record TagPathChoice(string TagPath, bool IsNew)
{
   public static TagPathChoice Offered(string tagPath) => new(tagPath, IsNew: false);
   public static TagPathChoice NewTag(string tagPath) => new(tagPath, IsNew: true);

   /// <summary>The choice as the list shows it: the path, marked when it's a new tag.</summary>
   public override string ToString() => IsNew ? $"{TagPath}   (a new tag)" : TagPath;
}
