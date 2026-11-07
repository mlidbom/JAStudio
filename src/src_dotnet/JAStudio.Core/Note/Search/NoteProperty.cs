namespace JAStudio.Core.Note.Search;

/// <summary>
/// Something about a note that a search can have conditions on, such as its question, its tags,<br/>
/// or whether its reading is being studied. <see cref="NoteProperties"/> lists them all.
/// </summary>
/// <remarks>
/// Each kind of property decides how conditions on it match, by making only the conditions that fit it:<br/>
/// a text property can make "contains", a true-or-false property can't.
/// </remarks>
public abstract class NoteProperty
{
   protected NoteProperty(string name) => Name = name;

   /// <summary>The property's name as the search editor shows it, such as "Question".</summary>
   public string Name { get; }

   public override string ToString() => Name;
}
