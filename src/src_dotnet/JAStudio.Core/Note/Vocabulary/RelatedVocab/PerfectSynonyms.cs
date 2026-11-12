using System.Collections.Generic;
using System.Linq;
using Compze.Contracts;
using Compze.Internals.SystemCE.ReactiveCE;
using JAStudio.Core.Note.Collection;

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

   public PerfectSynonyms(VocabNote vocab, IEnumerable<WordReference> words, NoteGuard guard)
   {
      _vocab = vocab;
      _words = new WordSet(words, guard);
      PushTheAnswerToTheOtherSynonymsWheneverItChanges();
   }

   /// <summary>
   /// The answer shown is the vocab's <see cref="VocabNote.CorpusAnswer"/>, or its source's while it has none (<see cref="VocabNote.GetAnswer"/>), so a<br/>
   /// change to either may change it.
   /// </summary>
   void PushTheAnswerToTheOtherSynonymsWheneverItChanges()
   {
      _vocab.CorpusAnswer.Subscribe(_ => PushAnswerToOtherSynonyms());
      _vocab.SourceData.Answer.Subscribe(_ => PushAnswerToOtherSynonyms());
   }

   VocabCollection Collection => _vocab.Services.Collection.Vocab;

   /// <summary>The words of the vocab's perfect synonyms.</summary>
   public IReadOnlySet<WordReference> Words => _words.Words;

   public bool Contains(WordReference word) => _words.Contains(word);

   /// <summary>Whether <paramref name="word"/> can be a perfect synonym: whether it names a vocab, since only vocab can share an answer.</summary>
   public bool Accepts(WordReference word) => Collection.NamedExactlyBy(word).Any();

   /// <summary>The vocab notes the <see cref="Words"/> name exactly: the vocab's perfect synonyms.</summary>
   public List<VocabNote> Notes() => Collection.NamedExactlyByAny(Words);

   /// <summary>Gives every perfect synonym this vocab's answer.</summary>
   void PushAnswerToOtherSynonyms()
   {
      foreach(var synonym in Notes())
         synonym.CorpusAnswer.Set(_vocab.GetAnswer());
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
         removedVocab.RelatedNotes.PerfectSynonyms.LeaveTheWeb();

      foreach(var synonym in TheWholeWeb())
         synonym.RelatedNotes.PerfectSynonyms.RemoveKeepingItAsASynonym(removed);
   }

   /// <summary>Replaces <paramref name="oldName"/>, the old name of a vocab that was renamed, with its new name, <paramref name="newName"/>, if it's a perfect synonym.</summary>
   internal void Rename(WordReference oldName, WordReference newName) => _words.Rename(oldName, newName);

   void LeaveTheWeb() => _words.Clear();

   /// <summary>
   /// Takes this vocab out of its web, as it's deleted: none of the others has it as a perfect synonym any more, but each keeps it as a synonym,<br/>
   /// which may name a word no vocab has.
   /// </summary>
   internal void LeaveTheWebAsTheVocabIsDeleted()
   {
      foreach(var synonym in Notes())
         synonym.RelatedNotes.PerfectSynonyms.RemoveKeepingItAsASynonym(_vocab.Question.Reference);
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
         foreach(var name in everyName)
            synonym.RelatedNotes.PerfectSynonyms.AddKeepingItAsASynonymToo(name);
      }
   }

   public override string ToString() => _words.ToString();
}
