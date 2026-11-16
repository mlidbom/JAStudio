using Compze.Teventive.Taggregates.Tevents;
using JAStudio.Core.Note.Tevents;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Note.Sentences.Tevents;

public interface ISentenceTevent : INoteTevent
{
   public interface Created : ISentenceTevent, ITaggregateCreatedTevent;
}

/// <summary>Takes any <see cref="INoteTevent"/>: a sentence also publishes the tevents every note has.</summary>
public interface ISentenceTevent<out TTevent> : INoteTevent<TTevent> where TTevent : INoteTevent;
