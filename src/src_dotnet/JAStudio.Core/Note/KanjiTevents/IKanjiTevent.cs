using Compze.Teventive.Taggregates.Tevents;
using JAStudio.Core.Note.Tevents;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Note.KanjiTevents;

public interface IKanjiTevent : INoteTevent
{
   /// <remarks>Carries the kanji's question, its character, which no tevent changes: a kanji with another character is another kanji.</remarks>
   public interface Created : IKanjiTevent, ITaggregateCreatedTevent { string Question { get; } }
}

/// <summary>Takes any <see cref="INoteTevent"/>: a kanji also publishes the tevents every note has.</summary>
public interface IKanjiTevent<out TTevent> : INoteTevent<TTevent> where TTevent : INoteTevent;
