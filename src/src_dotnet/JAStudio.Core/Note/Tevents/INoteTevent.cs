using Compze.Teventive.Taggregates.Tevents;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Note.Tevents;

public interface INoteTevent : ITaggregateTevent
{
   /// <summary>
   /// The note's stored data changed in a part that publishes no tevents of its own yet. So that a taggregate store counts the note as changed,<br/>
   /// and saves it.
   /// </summary>
   /// <remarks>wip: goes when the last part of the notes publishes tevents of its own (step 10 of dev_docs/taggregates/shared/the-migration.md).</remarks>
   public interface PartWithoutTeventsChanged : INoteTevent;
}

public interface INoteTevent<out TTevent> : ITaggregateTevent<TTevent> where TTevent : INoteTevent;
