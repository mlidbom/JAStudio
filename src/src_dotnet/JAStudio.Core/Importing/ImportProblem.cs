using JAStudio.Core.Importing.RawNotes;

namespace JAStudio.Core.Importing;

/// <summary>
/// Something that stops an <see cref="ImportRun"/>, found when it's prepared: a fault in its <see cref="ImportProfile"/>, such as an unmapped question,<br/>
/// or in one of its <see cref="RawNote"/>s, such as a media file missing from Anki's media folder. Any one problem stops the whole run.
/// </summary>
public class ImportProblem(RawNote? rawNote, string description)
{
   /// <summary>The raw note with the problem, or null for a problem with the import profile itself.</summary>
   public RawNote? RawNote { get; } = rawNote;

   public string Description { get; } = description;

   public override string ToString() => RawNote == null ? Description : $"{RawNote}: {Description}";
}
