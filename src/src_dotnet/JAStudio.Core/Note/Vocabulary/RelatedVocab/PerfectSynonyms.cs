using System;
using System.Collections.Generic;
using System.Linq;
using Compze.Contracts;
using Compze.Tessaging.TessageBus;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Tevents;
using JAStudio.Core.Note.Vocabulary.Tevents;

namespace JAStudio.Core.Note.Vocabulary.RelatedVocab;

/// <summary>
/// A vocab's perfect synonyms: words that mean exactly the same, so they share one answer. They form a web: each one is a perfect synonym of every other,<br/>
/// and a synonym too (<see cref="Synonyms"/>), and a change to any one's answer is pushed to the others.
/// </summary>
/// <remarks>
/// Each perfect synonym is a vocab that exists, since only vocab can share an answer: adding one requires it, and a deleted vocab leaves its<br/>
/// web (<see cref="LeaveTheWebAsTheVocabIsDeleted"/>).
/// </remarks>
/// <remarks>A perfect synonym is the vocab its word names exactly (<see cref="VocabCollection.NamedExactlyBy"/>), never one it would only match by a form.</remarks>
public class PerfectSynonyms
{
   /// <summary>
   /// Declares, through <paramref name="handle"/>, the participation handlers by which perfect synonyms share one answer: a change to a vocab's<br/>
   /// <see cref="VocabNote.CorpusAnswer"/>, or to its source's (<see cref="VocabSourceData.Answer"/>), gives each of its perfect synonyms the answer the<br/>
   /// vocab then shows, in the unit of work that changed it (<see cref="PushAnswerToOtherSynonyms"/>).
   /// </summary>
   internal static void ShareTheAnswerThrough(TessageBusHandlerRegistrar handle) => handle
     .ForTevent(static (IVocabTevent.PropertyUpdated.CorpusAnswer it, JPCollection collection) => PushTheAnswerOfTheVocabThatPublished(it, collection))
     .ForTevent(static (IVocabTevent.SourceData.PropertyUpdated.Answer it, JPCollection collection) => PushTheAnswerOfTheVocabThatPublished(it, collection));

   static void PushTheAnswerOfTheVocabThatPublished(INoteTevent tevent, JPCollection collection) =>
      PerfectSynonymsOfTheVocabThatPublished(tevent, collection).PushAnswerToOtherSynonyms();

   /// <summary>
   /// Declares, through <paramref name="handle"/>, the participation handlers by which the perfect synonyms stay one web, in the unit of work that changed<br/>
   /// a vocab's: a perfect synonym added joins its web and the vocab's into one, and gives every vocab in it the vocab's answer (<see cref="JoinTheWebs"/>),<br/>
   /// one removed leaves the web, staying a synonym of each vocab in it (<see cref="TakeOutOfTheWeb"/>), and a vocab deleted leaves its web the same<br/>
   /// way (<see cref="LeaveTheWebAsTheVocabIsDeleted"/>).
   /// </summary>
   internal static void KeepTheWebWholeThrough(TessageBusHandlerRegistrar handle) => handle
     .ForTevent(static (IVocabTevent.RelatedVocab.PropertyUpdated.PerfectSynonyms.Added it, JPCollection collection) => PerfectSynonymsOfTheVocabThatPublished(it, collection).JoinTheWebs())
     .ForTevent(static (IVocabTevent.RelatedVocab.PropertyUpdated.PerfectSynonyms.Removed it, JPCollection collection) => PerfectSynonymsOfTheVocabThatPublished(it, collection).TakeOutOfTheWeb(it.Word))
     .ForTevent(static (IVocabTevent.Deleted it, JPCollection collection) => PerfectSynonymsOfTheVocabThatPublished(it, collection).LeaveTheWebAsTheVocabIsDeleted());

   static PerfectSynonyms PerfectSynonymsOfTheVocabThatPublished(INoteTevent tevent, JPCollection collection) =>
      collection.TheRunningUnitOfWork.NoteThatPublished<VocabNote>(tevent).RelatedNotes.PerfectSynonyms;

   readonly VocabNote _vocab;
   readonly WordSet _words;

   public PerfectSynonyms(VocabNote vocab, IEnumerable<WordReference> words)
   {
      _vocab = vocab;
      _words = new WordSet(vocab, words, static it => new VocabTevent.RelatedVocab.PerfectSynonymsUpdated(it));
   }

   VocabCollection Collection => _vocab.Services.Collection.Vocab;

   /// <summary>The words of the vocab's perfect synonyms.</summary>
   public IReadOnlySet<WordReference> Words => _words.Words;

   public bool Contains(WordReference word) => _words.Contains(word);

   /// <summary>Whether <paramref name="word"/> can be a perfect synonym: whether it names a vocab, since only vocab can share an answer.</summary>
   public bool Accepts(WordReference word) => Collection.NamedExactlyBy(word).Any();

   /// <summary>The vocab notes the <see cref="Words"/> name exactly: the vocab's perfect synonyms.</summary>
   public List<VocabNote> Notes() => Collection.NamedExactlyByAny(Words);

   /// <summary>
   /// Gives every perfect synonym this vocab's answer, each in an edit that joins the unit of work running. The answer shown is the vocab's<br/>
   /// <see cref="VocabNote.CorpusAnswer"/>, or its source's while it has none (<see cref="VocabNote.GetAnswer"/>), so a change to either pushes it<br/>
   /// (<see cref="ShareTheAnswerThrough"/>).
   /// </summary>
   void PushAnswerToOtherSynonyms()
   {
      foreach(var synonym in Notes())
         synonym.Edit(it => it.SetCorpusAnswer(_vocab.GetAnswer()));
   }

   /// <summary>
   /// Adds <paramref name="added"/>, which must name a vocab, as a perfect synonym and a synonym, and so joins the two webs of perfect synonyms into one,<br/>
   /// and gives every vocab in it this vocab's answer (<see cref="KeepTheWebWholeThrough"/>).
   /// </summary>
   public void AddOverwritingTheAnswerOfTheAddedSynonym(WordReference added)
   {
      Contract.Argument.Assert(Accepts(added),
                               () => $"No vocab is named {added.DisambiguationName}, and a perfect synonym must be a vocab: perfect synonyms share one answer.");
      if(_vocab.Question.IsNamedBy(added)) return;
      _words.Add(added, words => new VocabTevent.RelatedVocab.PerfectSynonymAdded(added, words));
      _vocab.RelatedNotes.Synonyms.Add(added);
   }

   /// <summary>
   /// Removes <paramref name="removed"/>, keeping it as a synonym, and so takes it out of the web: it's no perfect synonym of any of the others any more,<br/>
   /// but stays a synonym of each (<see cref="KeepTheWebWholeThrough"/>).
   /// </summary>
   public void Remove(WordReference removed)
   {
      _words.Remove(removed, words => new VocabTevent.RelatedVocab.PerfectSynonymRemoved(removed, words));
      _vocab.RelatedNotes.Synonyms.Add(removed);
   }

   /// <summary>What a participation handler of a perfect synonym added does: connects the whole web, and gives every vocab in it this vocab's answer.</summary>
   void JoinTheWebs()
   {
      ConnectTheWholeWeb();
      PushAnswerToOtherSynonyms();
   }

   /// <summary>
   /// What a participation handler of the removing of <paramref name="removed"/> does: the vocab it names leaves the web, and every vocab in the web<br/>
   /// keeps it only as a synonym.
   /// </summary>
   void TakeOutOfTheWeb(WordReference removed)
   {
      foreach(var removedVocab in Collection.NamedExactlyBy(removed))
         EditThePerfectSynonymsOf(removedVocab, it => it.LeaveTheWeb());

      foreach(var synonym in TheWholeWeb())
         EditThePerfectSynonymsOf(synonym, it => it.RemoveKeepingItAsASynonym(removed));
   }

   /// <summary>Replaces <paramref name="oldName"/>, the old name of a vocab that was renamed, with its new name, <paramref name="newName"/>, if it's a perfect synonym.</summary>
   internal void Rename(WordReference oldName, WordReference newName) => _words.Rename(oldName, newName);

   /// <summary>Makes <paramref name="words"/> the words of the vocab's perfect synonyms: what their tevent's applier does.</summary>
   internal void Apply(IReadOnlySet<WordReference> words) => _words.Apply(words);

   void LeaveTheWeb() => _words.Clear();

   /// <summary>
   /// What a participation handler of the vocab's deletion does: takes it out of its web, so that none of the others has it as a perfect synonym any<br/>
   /// more, but each keeps it as a synonym, which may name a word no vocab has.
   /// </summary>
   void LeaveTheWebAsTheVocabIsDeleted()
   {
      foreach(var synonym in Notes())
         EditThePerfectSynonymsOf(synonym, it => it.RemoveKeepingItAsASynonym(_vocab.Question.Reference));
   }

   void RemoveKeepingItAsASynonym(WordReference synonym)
   {
      _words.Remove(synonym);
      _vocab.RelatedNotes.Synonyms.Add(synonym);
   }

   void AddKeepingItAsASynonymToo(WordReference synonym)
   {
      if(_vocab.Question.IsNamedBy(synonym)) return;
      _words.Add(synonym);
      _vocab.RelatedNotes.Synonyms.Add(synonym);
   }

   /// <summary>This vocab and every vocab it's linked to through perfect synonyms, directly or through others.</summary>
   HashSet<VocabNote> TheWholeWeb()
   {
      var found = new HashSet<VocabNote>();
      RecurseInto(_vocab);
      return found;

      void RecurseInto(VocabNote synonym)
      {
         if(!found.Add(synonym)) return;
         foreach(var related in synonym.RelatedNotes.PerfectSynonyms.Notes())
            RecurseInto(related);
      }
   }

   /// <summary>Makes every vocab in <see cref="TheWholeWeb"/> a perfect synonym, and a synonym, of every other.</summary>
   void ConnectTheWholeWeb()
   {
      var wholeWeb = TheWholeWeb();
      var everyName = wholeWeb.Select(it => it.Question.Reference).ToList();

      foreach(var synonym in wholeWeb)
      {
         EditThePerfectSynonymsOf(synonym, it =>
         {
            foreach(var name in everyName)
               it.AddKeepingItAsASynonymToo(name);
         });
      }
   }

   /// <summary>Changes <paramref name="vocab"/>'s perfect synonyms with <paramref name="change"/>, in an edit of <paramref name="vocab"/> (<see cref="EditingANote"/>).</summary>
   static void EditThePerfectSynonymsOf(VocabNote vocab, Action<PerfectSynonyms> change) => vocab.Edit(it => change(it.RelatedNotes.PerfectSynonyms));

   public override string ToString() => _words.ToString();
}
