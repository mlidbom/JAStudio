using JAStudio.Core.Note.NoteFields;
using JAStudio.Core.Storage.Dto;

namespace JAStudio.Core.Note;

/// <summary>
/// What a <see cref="KanjiNote"/>'s source, WaniKani for most kanji, gave it when it was imported: its answer, and its mnemonics and hints<br/>
/// for the kanji's meaning and reading.
/// </summary>
/// <remarks>
/// The source's answer is shown while the kanji has no <see cref="KanjiNote.CorpusAnswer"/> of its own.<br/>
/// The source's mnemonics and hints are never shown: those from WaniKani aren't ours to show, so they're only kept.
/// </remarks>
public class KanjiSourceData
{
   public KanjiSourceData(KanjiSourceDataDto? data, NoteGuard guard)
   {
      Answer = new WritableStringValue(data?.Answer ?? string.Empty, guard);
      MeaningMnemonic = new WritableStringValue(data?.MeaningMnemonic ?? string.Empty, guard);
      ReadingMnemonic = new WritableStringValue(data?.ReadingMnemonic ?? string.Empty, guard);
      MeaningInfo = new WritableStringValue(data?.MeaningInfo ?? string.Empty, guard);
      ReadingInfo = new WritableStringValue(data?.ReadingInfo ?? string.Empty, guard);
   }

   internal KanjiSourceDataDto ToDto() => new()
   {
      Answer = Answer.Value,
      MeaningMnemonic = MeaningMnemonic.Value,
      ReadingMnemonic = ReadingMnemonic.Value,
      MeaningInfo = MeaningInfo.Value,
      ReadingInfo = ReadingInfo.Value
   };

   /// <summary>The source's answer, shown while the kanji has no <see cref="KanjiNote.CorpusAnswer"/>.</summary>
   public WritableStringValue Answer { get; }

   /// <summary>The source's mnemonic for the kanji's meaning. Never shown.</summary>
   public WritableStringValue MeaningMnemonic { get; }

   /// <summary>The source's mnemonic for the kanji's reading. Never shown.</summary>
   public WritableStringValue ReadingMnemonic { get; }

   /// <summary>The source's hints for remembering the kanji's meaning. Never shown.</summary>
   public WritableStringValue MeaningInfo { get; }

   /// <summary>The source's hints for remembering the kanji's reading. Never shown.</summary>
   public WritableStringValue ReadingInfo { get; }
}
