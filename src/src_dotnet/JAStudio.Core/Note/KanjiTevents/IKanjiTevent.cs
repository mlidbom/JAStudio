using System.Collections.Generic;
using Compze.Teventive.Taggregates.Tevents;
using JAStudio.Core.Note.Media;
using JAStudio.Core.Note.Tevents;
using JAStudio.Core.Note.Vocabulary;

// ReSharper disable InconsistentNaming
// ReSharper disable PossibleInterfaceMemberAmbiguity

namespace JAStudio.Core.Note.KanjiTevents;

/// <summary>
/// The tevent appliers apply only <c>PropertyUpdated</c> tevents, each the whole new value of one property, as a vocab's do
/// (<see cref="Vocabulary.Tevents.IVocabTevent"/>).
/// </summary>
public interface IKanjiTevent : INoteTevent
{
   public new static class PropertyUpdated
   {
      public interface CorpusAnswer : IKanjiTevent { string CorpusAnswer { get; } }
      public interface Mnemonic : IKanjiTevent { string Mnemonic { get; } }
      public interface KanjiReferences : IKanjiTevent { string KanjiReferences { get; } }
      public interface Radicals : IKanjiTevent { IReadOnlyList<string> Radicals { get; } }
      public interface PrimaryVocab : IKanjiTevent { IReadOnlyList<WordReference> PrimaryVocab { get; } }
      public interface SimilarMeaning : IKanjiTevent { IReadOnlyList<string> SimilarMeaning { get; } }
      public interface ConfusedWith : IKanjiTevent { IReadOnlyList<string> ConfusedWith { get; } }
      public interface Images : IKanjiTevent { IReadOnlyList<ImageReference> Images { get; } }
   }

   /// <remarks>Carries the kanji's question, its character, which no tevent changes: a kanji with another character is another kanji.</remarks>
   public interface Created : IKanjiTevent, ITaggregateCreatedTevent { string Question { get; } }

   public interface SourceData : IKanjiTevent
   {
      public new static class PropertyUpdated
      {
         public interface Answer : SourceData { string Answer { get; } }
         public interface MeaningMnemonic : SourceData { string MeaningMnemonic { get; } }
         public interface ReadingMnemonic : SourceData { string ReadingMnemonic { get; } }
         public interface MeaningInfo : SourceData { string MeaningInfo { get; } }
         public interface ReadingInfo : SourceData { string ReadingInfo { get; } }
      }
   }
}

/// <summary>Takes any <see cref="INoteTevent"/>: a kanji also publishes the tevents every note has.</summary>
public interface IKanjiTevent<out TTevent> : INoteTevent<TTevent> where TTevent : INoteTevent;
