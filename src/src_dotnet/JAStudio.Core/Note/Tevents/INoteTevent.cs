using System.Collections.Generic;
using Compze.Teventive.Taggregates.Tevents;
using JAStudio.Core.Note.Tagging;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Note.Tevents;

/// <summary>A tevent of any note. Each note class's tevents extend it, and every note also publishes these.</summary>
public interface INoteTevent : ITaggregateTevent
{
   public static class PropertyUpdated
   {
      public interface Tags : INoteTevent { IReadOnlySet<Tag> Tags { get; } }
   }

   /// <summary>
   /// The note is deleted: the unit of work that publishes this holds it no more, and removes it from the collection when it commits<br/>
   /// (<see cref="JPNote.Delete"/>). What a deleted note leaves in other notes is changed by participation handlers of it.
   /// </summary>
   public interface Deleted : INoteTevent, ITaggregateDeletedTevent;
}

public interface INoteTevent<out TTevent> : ITaggregateTevent<TTevent> where TTevent : INoteTevent;
