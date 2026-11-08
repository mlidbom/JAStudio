using JAStudio.Core.Importing.RawNotes;

namespace JAStudio.Core.Importing.Checking;

/// <summary>
/// One thing an import check found about a <see cref="RawNote"/>, such as "A sentence in the corpus has the external id …"<br/>
/// or "The file … isn't in Anki's media folder". Notes and media files live apart, so each is stated on its own.
/// </summary>
public class RawNoteFinding(RawNoteFindingKind kind, string description)
{
   public RawNoteFindingKind Kind { get; } = kind;
   public string Description { get; } = description;

   public static RawNoteFinding Information(string description) => new(RawNoteFindingKind.Information, description);
   public static RawNoteFinding AlreadyInJAStudio(string description) => new(RawNoteFindingKind.AlreadyInJAStudio, description);
   public static RawNoteFinding Problem(string description) => new(RawNoteFindingKind.Problem, description);

   public override string ToString() => Description;
}
