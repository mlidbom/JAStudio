using Compze.Teventive.Taggregates.BaseClasses;
using Compze.Teventive.Taggregates.Tevents;

namespace JAStudio.Core.Note.Tevents;

/// <summary>The base class of every note tevent's class: each <see cref="INoteTevent"/> interface has a class that implements it.</summary>
public abstract class NoteTevent : TaggregateTevent, INoteTevent
{
   protected NoteTevent() {}

   /// <summary>For the tevent that creates a note, which gives the note its id.</summary>
   protected NoteTevent(NoteId noteId) : base(noteId) {}
}

/// <summary>The class of the wrapper <see cref="INoteTevent{TTevent}"/>.</summary>
public class NoteTevent<TTevent>(TTevent tevent) : TaggregateTevent<TTevent>(tevent), INoteTevent<TTevent> where TTevent : INoteTevent;
