using System.Collections.Generic;
using MemoryPack;

namespace JAStudio.Core.Note.CorpusData;

[MemoryPackable]
public partial class KanjiData : CorpusObjectData
{
   public string Kanji { get; init; } = string.Empty;
   public string SourceAnswer { get; init; } = string.Empty;
   public string UserAnswer { get; init; } = string.Empty;
   public string ActiveAnswer { get; init; } = string.Empty;
   public string ReadingOnHtml { get; init; } = string.Empty;
   public string ReadingKunHtml { get; init; } = string.Empty;
   public string ReadingNanHtml { get; init; } = string.Empty;
   public List<string> Radicals { get; init; } = [];
   public string SourceMeaningMnemonic { get; init; } = string.Empty;
   public string MeaningInfo { get; init; } = string.Empty;
   public string ReadingMnemonic { get; init; } = string.Empty;
   public string ReadingInfo { get; init; } = string.Empty;
   public List<string> PrimaryVocab { get; init; } = [];
   public string References { get; init; } = string.Empty;
   public string UserMnemonic { get; init; } = string.Empty;
   public List<string> SimilarMeaning { get; init; } = [];
   public List<string> ConfusedWith { get; init; } = [];

   /// <summary>
   /// An image for the kanji, as Anki's <c>&lt;img src="…"&gt;</c> markup, or empty. Only a few kanji have one: JAStudio only carries it, kept from<br/>
   /// the Anki notes before they were trimmed, since an image can't be made again the way the kanji's audio can be computed.
   /// </summary>
   /// <remarks>
   /// Declared last on purpose: the corpus's snapshot cache (MemoryPack) reads members by their position, and old snapshots read correctly only while<br/>
   /// new members come after the existing ones.
   /// </remarks>
   public string Image { get; init; } = string.Empty;

   protected override NoteId CreateTypedId() => new KanjiId(Id);

   protected override void PopulateAnkiNoteFields(Dictionary<string, string> fields)
   {
      fields[NoteFieldsConstants.Kanji.Question] = Kanji;
      fields[NoteFieldsConstants.Kanji.SourceAnswer] = SourceAnswer;
      fields[NoteFieldsConstants.Kanji.UserAnswer] = UserAnswer;
      fields[NoteFieldsConstants.Kanji.ActiveAnswer] = ActiveAnswer;
      fields[NoteFieldsConstants.Kanji.ReadingOn] = ReadingOnHtml;
      fields[NoteFieldsConstants.Kanji.ReadingKun] = ReadingKunHtml;
      fields[NoteFieldsConstants.Kanji.ReadingNan] = ReadingNanHtml;
      fields[NoteFieldsConstants.Kanji.Radicals] = string.Join(", ", Radicals);
      fields[NoteFieldsConstants.Kanji.SourceMeaningMnemonic] = SourceMeaningMnemonic;
      fields[NoteFieldsConstants.Kanji.MeaningInfo] = MeaningInfo;
      fields[NoteFieldsConstants.Kanji.ReadingMnemonic] = ReadingMnemonic;
      fields[NoteFieldsConstants.Kanji.ReadingInfo] = ReadingInfo;
      fields[NoteFieldsConstants.Kanji.PrimaryVocab] = string.Join(", ", PrimaryVocab);
      fields[NoteFieldsConstants.Kanji.References] = References;
      fields[NoteFieldsConstants.Kanji.UserMnemonic] = UserMnemonic;
      fields[NoteFieldsConstants.Kanji.UserSimilarMeaning] = string.Join(", ", SimilarMeaning);
      fields[NoteFieldsConstants.Kanji.RelatedConfusedWith] = string.Join(", ", ConfusedWith);

   }

}
