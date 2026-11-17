using Compze.Teventive;
using Compze.Teventive.Taggregates.BaseClasses;
using JAStudio.Core.Note.KanjiTevents;
using JAStudio.Core.Note.Tevents;
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
/// <remarks>
/// A component of its kanji: its fields publish tevents of their own, <see cref="IKanjiTevent.SourceData"/>, which its appliers apply.
/// </remarks>
public class KanjiSourceData : TeventiveComponent<KanjiNote, INoteTevent, NoteTevent, KanjiSourceData, IKanjiTevent.SourceData, KanjiTevent.SourceData>, ITeventRouted
{
   static readonly TeventRouting<KanjiSourceData, IKanjiTevent.SourceData> Appliers = DeclareAppliers(it => it
     .For<IKanjiTevent.SourceData.PropertyUpdated.Answer>(static (sourceData, tevent) => sourceData.Answer = tevent.Answer)
     .For<IKanjiTevent.SourceData.PropertyUpdated.MeaningMnemonic>(static (sourceData, tevent) => sourceData.MeaningMnemonic = tevent.MeaningMnemonic)
     .For<IKanjiTevent.SourceData.PropertyUpdated.ReadingMnemonic>(static (sourceData, tevent) => sourceData.ReadingMnemonic = tevent.ReadingMnemonic)
     .For<IKanjiTevent.SourceData.PropertyUpdated.MeaningInfo>(static (sourceData, tevent) => sourceData.MeaningInfo = tevent.MeaningInfo)
     .For<IKanjiTevent.SourceData.PropertyUpdated.ReadingInfo>(static (sourceData, tevent) => sourceData.ReadingInfo = tevent.ReadingInfo));

   TeventRouting ITeventRouted.TeventAppliers => Appliers;

   readonly KanjiNote _kanji;

   public KanjiSourceData(KanjiNote kanji, KanjiSourceDataDto? data) : base(kanji)
   {
      _kanji = kanji;
      Answer = data?.Answer ?? string.Empty;
      MeaningMnemonic = data?.MeaningMnemonic ?? string.Empty;
      ReadingMnemonic = data?.ReadingMnemonic ?? string.Empty;
      MeaningInfo = data?.MeaningInfo ?? string.Empty;
      ReadingInfo = data?.ReadingInfo ?? string.Empty;
   }

   internal KanjiSourceDataDto ToDto() => new()
   {
      Answer = Answer,
      MeaningMnemonic = MeaningMnemonic,
      ReadingMnemonic = ReadingMnemonic,
      MeaningInfo = MeaningInfo,
      ReadingInfo = ReadingInfo
   };

   /// <summary>The source's answer, shown while the kanji has no <see cref="KanjiNote.CorpusAnswer"/>.</summary>
   public string Answer { get; private set; }

   /// <summary>The source's mnemonic for the kanji's meaning. Never shown.</summary>
   public string MeaningMnemonic { get; private set; }

   /// <summary>The source's mnemonic for the kanji's reading. Never shown.</summary>
   public string ReadingMnemonic { get; private set; }

   /// <summary>The source's hints for remembering the kanji's meaning. Never shown.</summary>
   public string MeaningInfo { get; private set; }

   /// <summary>The source's hints for remembering the kanji's reading. Never shown.</summary>
   public string ReadingInfo { get; private set; }

   /// <summary>Gives the kanji <paramref name="answer"/>, trimmed, as its source's <see cref="Answer"/>.</summary>
   public void SetAnswer(string answer) => _kanji.PublishIfChanged(Answer, answer.Trim(), static it => new KanjiTevent.SourceData.SourceAnswerUpdated(it));

   /// <summary>Gives the kanji <paramref name="meaningMnemonic"/>, trimmed, as its source's <see cref="MeaningMnemonic"/>.</summary>
   public void SetMeaningMnemonic(string meaningMnemonic) =>
      _kanji.PublishIfChanged(MeaningMnemonic, meaningMnemonic.Trim(), static it => new KanjiTevent.SourceData.SourceMeaningMnemonicUpdated(it));

   /// <summary>Gives the kanji <paramref name="readingMnemonic"/>, trimmed, as its source's <see cref="ReadingMnemonic"/>.</summary>
   public void SetReadingMnemonic(string readingMnemonic) =>
      _kanji.PublishIfChanged(ReadingMnemonic, readingMnemonic.Trim(), static it => new KanjiTevent.SourceData.SourceReadingMnemonicUpdated(it));

   /// <summary>Gives the kanji <paramref name="meaningInfo"/>, trimmed, as its source's <see cref="MeaningInfo"/>.</summary>
   public void SetMeaningInfo(string meaningInfo) =>
      _kanji.PublishIfChanged(MeaningInfo, meaningInfo.Trim(), static it => new KanjiTevent.SourceData.SourceMeaningInfoUpdated(it));

   /// <summary>Gives the kanji <paramref name="readingInfo"/>, trimmed, as its source's <see cref="ReadingInfo"/>.</summary>
   public void SetReadingInfo(string readingInfo) =>
      _kanji.PublishIfChanged(ReadingInfo, readingInfo.Trim(), static it => new KanjiTevent.SourceData.SourceReadingInfoUpdated(it));
}
