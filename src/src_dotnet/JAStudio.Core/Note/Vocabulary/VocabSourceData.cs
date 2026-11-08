using JAStudio.Core.Note.NoteFields;
using JAStudio.Core.Storage.Dto;

namespace JAStudio.Core.Note.Vocabulary;

/// <summary>
/// What a <see cref="VocabNote"/>'s source, such as WaniKani or the dictionary, gave it when it was imported: its answer and its mnemonics.
/// </summary>
/// <remarks>
/// The source's answer is shown while the vocab has no <see cref="VocabNote.CorpusAnswer"/> of its own.<br/>
/// The source's mnemonics are never shown: those from WaniKani aren't ours to show, so they're only kept.
/// </remarks>
public class VocabSourceData
{
   public VocabSourceData(VocabDto? data, NoteGuard guard)
   {
      Answer = new WritableStringValue(data?.SourceAnswer ?? string.Empty, guard);
      Mnemonic = new WritableStringValue(data?.SourceMnemonic ?? string.Empty, guard);
      ReadingMnemonic = new WritableStringValue(data?.SourceReadingMnemonic ?? string.Empty, guard);
   }

   /// <summary>The source's answer, shown while the vocab has no <see cref="VocabNote.CorpusAnswer"/>.</summary>
   public WritableStringValue Answer { get; }

   /// <summary>The source's mnemonic for the vocab's meaning. Never shown.</summary>
   public WritableStringValue Mnemonic { get; }

   /// <summary>The source's mnemonic for the vocab's reading. Never shown.</summary>
   public WritableStringValue ReadingMnemonic { get; }
}
