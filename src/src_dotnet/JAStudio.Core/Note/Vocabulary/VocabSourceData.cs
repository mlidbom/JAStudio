using System.Linq;
using Compze.Teventive;
using Compze.Teventive.Taggregates.BaseClasses;
using JAStudio.Core.Note.Media;
using JAStudio.Core.Note.Tevents;
using JAStudio.Core.Note.Vocabulary.Tevents;
using JAStudio.Core.Storage.Dto;

namespace JAStudio.Core.Note.Vocabulary;

/// <summary>
/// What a <see cref="VocabNote"/>'s source, such as WaniKani or the dictionary, gave it when it was imported: its answer, its mnemonics, its audio and its<br/>
/// images.
/// </summary>
/// <remarks>
/// The source's answer is shown while the vocab has no <see cref="VocabNote.CorpusAnswer"/> of its own.<br/>
/// The source's mnemonics are never shown: those from WaniKani aren't ours to show, so they're only kept.
/// </remarks>
/// <remarks>
/// A component of its vocab: its text fields publish tevents of their own, <see cref="IVocabTevent.SourceData"/>, which its appliers apply. Its audio<br/>
/// and images still change through the vocab's <see cref="NoteGuard"/>.
/// </remarks>
public class VocabSourceData : TeventiveComponent<VocabNote, INoteTevent, NoteTevent, VocabSourceData, IVocabTevent.SourceData, VocabTevent.SourceData>, ITeventRouted
{
   static readonly TeventRouting<VocabSourceData> Appliers = TeventRouting<VocabSourceData>.DeclareAppliers(it => it
     .For<IVocabTevent.SourceData.PropertyUpdated.Answer>(static (sourceData, tevent) => sourceData.Answer = tevent.Answer)
     .For<IVocabTevent.SourceData.PropertyUpdated.Mnemonic>(static (sourceData, tevent) => sourceData.Mnemonic = tevent.Mnemonic)
     .For<IVocabTevent.SourceData.PropertyUpdated.ReadingMnemonic>(static (sourceData, tevent) => sourceData.ReadingMnemonic = tevent.ReadingMnemonic));

   TeventRouting ITeventRouted.TeventAppliers => Appliers;

   readonly VocabNote _vocab;

   public VocabSourceData(VocabNote vocab, VocabSourceDataDto? data, NoteGuard guard) : base(vocab)
   {
      _vocab = vocab;
      Answer = data?.Answer ?? string.Empty;
      Mnemonic = data?.Mnemonic ?? string.Empty;
      ReadingMnemonic = data?.ReadingMnemonic ?? string.Empty;
      Audio = new AudioReferenceList(data?.Audio.Select(it => it.ToReference()) ?? [], guard);
      Images = new MediaReferenceList<ImageReference>(data?.Images.Select(it => it.ToReference()) ?? [], guard);
   }

   internal VocabSourceDataDto ToDto() =>
      new()
      {
         Answer = Answer,
         Mnemonic = Mnemonic,
         ReadingMnemonic = ReadingMnemonic,
         Audio = [..Audio.All.Select(AudioReferenceDto.From)],
         Images = [..Images.All.Select(ImageReferenceDto.From)],
      };

   /// <summary>The source's answer, shown while the vocab has no <see cref="VocabNote.CorpusAnswer"/>.</summary>
   public string Answer { get; private set; }

   /// <summary>The source's mnemonic for the vocab's meaning. Never shown.</summary>
   public string Mnemonic { get; private set; }

   /// <summary>The source's mnemonic for the vocab's reading. Never shown.</summary>
   public string ReadingMnemonic { get; private set; }

   /// <summary>
   /// Gives the vocab <paramref name="answer"/>, trimmed, as its source's <see cref="Answer"/>, and its perfect synonyms the answer the vocab then shows<br/>
   /// (<see cref="RelatedVocab.PerfectSynonyms"/>), which is this one while the vocab has no <see cref="VocabNote.CorpusAnswer"/>.
   /// </summary>
   public void SetAnswer(string answer)
   {
      //wip: step 11 of dev_docs/taggregates/shared/the-migration.md moves the push to a participation handler of IVocabTevent.SourceData.PropertyUpdated.Answer.
      if(_vocab.PublishIfChanged(Answer, answer.Trim(), static it => new VocabTevent.SourceData.SourceAnswerUpdated(it)))
         _vocab.RelatedNotes.PerfectSynonyms.PushAnswerToOtherSynonyms();
   }

   /// <summary>Gives the vocab <paramref name="mnemonic"/>, trimmed, as its source's <see cref="Mnemonic"/>.</summary>
   public void SetMnemonic(string mnemonic) => _vocab.PublishIfChanged(Mnemonic, mnemonic.Trim(), static it => new VocabTevent.SourceData.SourceMnemonicUpdated(it));

   /// <summary>Gives the vocab <paramref name="readingMnemonic"/>, trimmed, as its source's <see cref="ReadingMnemonic"/>.</summary>
   public void SetReadingMnemonic(string readingMnemonic) =>
      _vocab.PublishIfChanged(ReadingMnemonic, readingMnemonic.Trim(), static it => new VocabTevent.SourceData.SourceReadingMnemonicUpdated(it));

   /// <summary>The audio the source gave the vocab, such as WaniKani's recordings of the word in a male and a female voice.</summary>
   public AudioReferenceList Audio { get; }

   /// <summary>The images the source gave the vocab, such as Core 2000's. Shown nowhere yet.</summary>
   public MediaReferenceList<ImageReference> Images { get; }
}
