using System.Collections.Generic;
using Compze.Teventive.Taggregates.BaseClasses;
using Compze.Teventive.Taggregates.Tevents;
using JAStudio.Core.Note.Tagging;

namespace JAStudio.Core.Note.Tevents;

public abstract class NoteTevent : TaggregateTevent, INoteTevent
{
   protected NoteTevent() {}

   protected NoteTevent(NoteId noteId) : base(noteId) {}

   public class PartWithoutTeventsChanged : NoteTevent, INoteTevent.PartWithoutTeventsChanged;

   public class TagsUpdated(IReadOnlySet<Tag> tags) : NoteTevent, INoteTevent.PropertyUpdated.Tags { public IReadOnlySet<Tag> Tags { get; } = tags; }
}

public class NoteTevent<TTevent>(TTevent tevent) : TaggregateTevent<TTevent>(tevent), INoteTevent<TTevent> where TTevent : INoteTevent;
