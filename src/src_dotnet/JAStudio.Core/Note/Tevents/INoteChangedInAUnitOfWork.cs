using Compze.Tessaging.TessageTypes;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Note.Tevents;

/// <summary>
/// A note was added or changed in a unit of work: one for each such note, whatever and however many tevents it published, so that what follows<br/>
/// the changes to notes, such as a page that shows a note, hears once of each note a unit of work changed. A note deleted in the unit of work has<br/>
/// none: it published its <see cref="INoteTevent.Deleted"/>.
/// </summary>
/// <remarks>
/// The unit of work publishes them once its work is done, inside it (<see cref="Collection.NoteUnitOfWork"/>), so its observers get them once it<br/>
/// has committed, and never if it rolls back.
/// </remarks>
/// <remarks>review-naming: INoteChangedInAUnitOfWork, and Vocab, Kanji and Sentence nested in it, are names Claude chose (2026-11-17).</remarks>
public interface INoteChangedInAUnitOfWork : ITevent
{
   NoteId NoteId { get; }

   public interface Vocab : INoteChangedInAUnitOfWork;
   public interface Kanji : INoteChangedInAUnitOfWork;
   public interface Sentence : INoteChangedInAUnitOfWork;
}
