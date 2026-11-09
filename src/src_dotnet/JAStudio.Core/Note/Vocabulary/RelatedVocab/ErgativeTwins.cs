using System.Collections.Generic;
using System.Linq;

namespace JAStudio.Core.Note.Vocabulary.RelatedVocab;

/// <summary>
/// A vocab's ergative twins: its transitive or intransitive counterparts, such as 開ける for 開く, named by their question.<br/>
/// The relation goes both ways, so adding or removing a twin adds or removes the vocab as the twin's twin too.
/// </summary>
/// <remarks>
/// A vocab can have more than one: 起こす is the transitive counterpart of both 起きる and 起こる, and the corpus can hold a verb's kana and kanji<br/>
/// spellings as vocab of their own, such as かける and 掛ける, which are both 掛かる's twins.
/// </remarks>
public class ErgativeTwins
{
   readonly VocabNote _vocab;
   readonly RelatedVocabData _data;
   readonly NoteGuard _guard;

   public ErgativeTwins(VocabNote vocab, RelatedVocabData data, NoteGuard guard)
   {
      _vocab = vocab;
      _data = data;
      _guard = guard;
   }

   public HashSet<string> Strings() => _data.ErgativeTwins;

   public List<VocabNote> Notes() => _vocab.Services.Collection.Vocab.WithAnyFormInPreferDisambiguationNameOrExactMatch(Strings().ToList());

   /// <summary>Adds the vocab with the question <paramref name="twin"/> as an ergative twin, and this vocab as the twin's.</summary>
   public void Add(string twin) => _guard.Update(() =>
   {
      Strings().Add(twin);

      foreach(var twinVocab in _vocab.Services.Collection.Vocab.WithQuestion(twin))
      {
         if(!twinVocab.RelatedNotes.ErgativeTwins.Strings().Contains(_vocab.GetQuestion()))
         {
            twinVocab.RelatedNotes.ErgativeTwins.Add(_vocab.GetQuestion());
         }
      }
   });

   /// <summary>Removes <paramref name="twin"/> as an ergative twin, and this vocab as the twin's.</summary>
   /// <remarks>This vocab's is removed before the twin's, or the twin's <see cref="Remove"/> would find this vocab still its twin, and remove again.</remarks>
   public void Remove(string twin) => _guard.Update(() =>
   {
      Strings().Remove(twin);

      foreach(var twinVocab in _vocab.Services.Collection.Vocab.WithQuestion(twin))
      {
         if(twinVocab.RelatedNotes.ErgativeTwins.Strings().Contains(_vocab.GetQuestion()))
         {
            twinVocab.RelatedNotes.ErgativeTwins.Remove(_vocab.GetQuestion());
         }
      }
   });

   public override string ToString() => string.Join(", ", Strings());
}
