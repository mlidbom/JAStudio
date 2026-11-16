using Compze.Teventive.Taggregates.BaseClasses;
using Compze.Teventive.Taggregates.Tevents;

namespace JAStudio.Core.Note.Tevents;

public abstract class NoteTevent : TaggregateTevent, INoteTevent
{
   protected NoteTevent() {}

   protected NoteTevent(NoteId noteId) : base(noteId) {}

   public class PartWithoutTeventsChanged : NoteTevent, INoteTevent.PartWithoutTeventsChanged;
}

public class NoteTevent<TTevent>(TTevent tevent) : TaggregateTevent<TTevent>(tevent), INoteTevent<TTevent> where TTevent : INoteTevent;
