using Compze.Teventive.Taggregates.Tevents;
using JAStudio.Core.Note.Vocabulary.Tevents;

namespace JAStudio.Core.Note.Tevents;

/// <summary>
/// Something that happened to a note: the root of every note's tevents. A tevent is one change to a taggregate, here a note, published as<br/>
/// it happens, so that whatever follows a change subscribes to exactly the changes it concerns, such as a vocab's forms changing<br/>
/// (<see cref="IVocabTevent.Forms"/>). Each kind of note has a root of its own below this one, such as <see cref="IVocabTevent"/>.
/// </summary>
/// <remarks>
/// wip: the structure of the notes' tevents, for Magnus to look at before anything publishes them (taggregates-opus, 2026-11-16). See<br/>
/// dev_docs/taggregates/shared/the-structure-of-the-notes-tevents.md.
/// </remarks>
public interface INoteTevent : ITaggregateTevent;

/// <summary>
/// An <see cref="INoteTevent"/> as its note published it: the wrapper Compze gives each tevent a taggregate publishes, through which a subscriber<br/>
/// can tell which part of the note published it, as for the parts several notes share (<see cref="IVocabTevent.Mnemonic{TTevent}"/>).
/// </summary>
public interface INoteTevent<out TTevent> : ITaggregateTevent<TTevent> where TTevent : INoteTevent;
