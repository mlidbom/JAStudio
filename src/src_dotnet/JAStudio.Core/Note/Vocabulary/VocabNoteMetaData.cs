using System.Collections.Generic;
using Compze.Teventive;
using Compze.Teventive.Taggregates.BaseClasses;
using JAStudio.Core.Note.Tevents;
using JAStudio.Core.Note.Vocabulary.Tevents;
using JAStudio.Core.Storage.Dto;

namespace JAStudio.Core.Note.Vocabulary;

/// <summary>
/// What JAStudio works out about a vocab from the corpus and keeps with it, its <see cref="SentenceCount"/>, and the meta tags its pages show.<br/>
/// A component of its vocab, whose stored value publishes a tevent of its own, <see cref="IVocabTevent.MetaData"/>, which its applier applies.
/// </summary>
public class VocabNoteMetaData : TeventiveComponent<VocabNote, INoteTevent, NoteTevent, VocabNoteMetaData, IVocabTevent.MetaData, VocabTevent.MetaData>, ITeventRouted
{
   static readonly TeventRouting<VocabNoteMetaData, IVocabTevent.MetaData> Appliers = DeclareAppliers(it => it
     .For<IVocabTevent.MetaData.PropertyUpdated.SentenceCount>(static (metaData, tevent) => metaData.SentenceCount = tevent.SentenceCount));

   TeventRouting ITeventRouted.TeventAppliers => Appliers;

   readonly VocabNote _vocab;

   public VocabNoteMetaData(VocabNote vocab, VocabDto? data) : base(vocab)
   {
      _vocab = vocab;
      SentenceCount = data?.SentenceCount ?? 0;
   }

   /// <summary>How many sentences the vocab is matched in: generated data, which updating the vocab's generated data counts.</summary>
   public int SentenceCount { get; private set; }

   public void SetSentenceCount(int value) => _vocab.PublishIfChanged(SentenceCount, value, static it => new VocabTevent.MetaData.SentenceCountUpdated(it));

   public string MetaTagsHtml(bool displayExtendedSentenceStatistics = true, bool noSentenceStatistics = false) => VocabNoteMetaTagFormatter.GetMetaTagsHtml(_vocab, displayExtendedSentenceStatistics, noSentenceStatistics);

   /// <summary>The vocab's meta tags, as <see cref="MetaTagsHtml"/> shows them, for a page that isn't HTML.</summary>
   public List<VocabMetaTag> MetaTags(bool displayExtendedSentenceStatistics = true, bool noSentenceStatistics = false) => VocabNoteMetaTagFormatter.GetMetaTags(_vocab, displayExtendedSentenceStatistics, noSentenceStatistics);
}
