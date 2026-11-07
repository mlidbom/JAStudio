namespace JAStudio.Core.Note.Search.TagProperties;

/// <summary>
/// A note's tags, as a property a search can have conditions on.<br/>
/// A condition on a tag includes the tag's whole branch: <c>source::anime</c> also matches a note tagged <c>source::anime::natsume</c>.
/// </summary>
public sealed class TagsProperty : NoteProperty
{
   internal TagsProperty() : base("Tags") {}

   /// <summary>A condition that the note has the tag <paramref name="tagPath"/>, or a tag in its branch, ignoring case.</summary>
   public TagCondition Include(string tagPath) => new(this, tagPath);
}
