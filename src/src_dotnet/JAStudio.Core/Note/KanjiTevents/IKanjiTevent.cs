using System.Collections.Generic;
using Compze.Teventive.Taggregates.Tevents;
using JAStudio.Core.Note.KanjiReadings;
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
/// <remarks>
/// A kanji's relationship to other kanji goes both ways (<see cref="KanjiRelationships.MutualKanjiRelationship"/>), and publishes the kanji it changed,<br/>
/// with its whole new list: <c>Added</c> and <c>Removed</c> when an edit of the kanji changed it, which a participation handler mirrors in the other<br/>
/// kanji, and <c>AddedAsTheMirrorOf</c> and <c>RemovedAsTheMirrorOf</c> when that mirror changed it, which nothing mirrors back. The kanji of a<br/>
/// mirror's tevent is the one whose edit it mirrors.
/// </remarks>
public interface IKanjiTevent : INoteTevent
{
   public new static class PropertyUpdated
   {
      public interface CorpusAnswer : IKanjiTevent { string CorpusAnswer { get; } }
      public interface Mnemonic : IKanjiTevent { string Mnemonic { get; } }
      public interface KanjiReferences : IKanjiTevent { string KanjiReferences { get; } }
      public interface Radicals : IKanjiTevent { IReadOnlyList<string> Radicals { get; } }
      public interface PrimaryVocab : IKanjiTevent { IReadOnlyList<WordReference> PrimaryVocab { get; } }

      public interface SimilarMeaning : IKanjiTevent
      {
         IReadOnlyList<string> SimilarMeaning { get; }

         public interface Added : SimilarMeaning { string Kanji { get; } }
         public interface Removed : SimilarMeaning { string Kanji { get; } }
         public interface AddedAsTheMirrorOf : SimilarMeaning { string Kanji { get; } }
         public interface RemovedAsTheMirrorOf : SimilarMeaning { string Kanji { get; } }
      }

      public interface ConfusedWith : IKanjiTevent
      {
         IReadOnlyList<string> ConfusedWith { get; }

         public interface Added : ConfusedWith { string Kanji { get; } }
         public interface Removed : ConfusedWith { string Kanji { get; } }
         public interface AddedAsTheMirrorOf : ConfusedWith { string Kanji { get; } }
         public interface RemovedAsTheMirrorOf : ConfusedWith { string Kanji { get; } }
      }

      public interface Images : IKanjiTevent { IReadOnlyList<ImageReference> Images { get; } }
   }

   /// <remarks>Carries the kanji's question, its character, which no tevent changes: a kanji with another character is another kanji.</remarks>
   public interface Created : IKanjiTevent, ITaggregateCreatedTevent { string Question { get; } }

   public interface Readings : IKanjiTevent
   {
      public new static class PropertyUpdated
      {
         public interface On : Readings { IReadOnlyList<KanjiReading> On { get; } }
         public interface Kun : Readings { IReadOnlyList<KanjiReading> Kun { get; } }
         public interface Nanori : Readings { IReadOnlyList<KanjiReading> Nanori { get; } }
      }
   }

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
