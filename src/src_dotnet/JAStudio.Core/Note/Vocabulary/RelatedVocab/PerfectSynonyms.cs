using System;
using System.Collections.Generic;
using System.Linq;
using Compze.Contracts;
using JAStudio.Core.Note.Collection;
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
   /// Gives every perfect synonym this vocab's answer. The answer shown is the vocab's <see cref="VocabNote.CorpusAnswer"/>, or its source's while it has<br/>
   /// none (<see cref="VocabNote.GetAnswer"/>), so a change to either pushes it: <see cref="VocabNote.SetCorpusAnswer"/> and <see cref="VocabSourceData.SetAnswer"/>.
   /// </summary>
   internal void PushAnswerToOtherSynonyms()
   {
      foreach(var synonym in Notes())
         synonym.Edit(it => it.SetCorpusAnswer(_vocab.GetAnswer()));
   }

   /// <summary>
   /// Adds <paramref name="added"/>, which must name a vocab, joining the two webs of perfect synonyms into one, and gives every vocab in it this<br/>
   /// vocab's answer.
   /// </summary>
   public void AddOverwritingTheAnswerOfTheAddedSynonym(WordReference added)
   {
      Contract.Argument.Assert(Accepts(added),
                               () => $"No vocab is named {added.DisambiguationName}, and a perfect synonym must be a vocab: perfect synonyms share one answer.");
      if(_vocab.Question.IsNamedBy(added)) return;
      AddKeepingItAsASynonymToo(added);
      ConnectTheWholeWeb();
      PushAnswerToOtherSynonyms();
   }

   /// <summary>Removes <paramref name="removed"/> from the web: it's no perfect synonym of any of the others any more, but stays a synonym of each.</summary>
   public void Remove(WordReference removed)
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
   /// Takes this vocab out of its web, as it's deleted: none of the others has it as a perfect synonym any more, but each keeps it as a synonym,<br/>
   /// which may name a word no vocab has.
   /// </summary>
   internal void LeaveTheWebAsTheVocabIsDeleted()
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
