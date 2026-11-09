using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices.JanomeEx;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Storage.Dto;
using JAStudio.Core.Note.Sentences;

namespace JAStudio.Core.Note.Vocabulary;

/// <summary>
/// The compound parts of a <see cref="VocabNote"/>: the words the vocab is built from.<br/>
/// The <see cref="Primary"/> parts are the vocab split into words, in order, and are what's shown when showing what the vocab is built from.<br/>
/// The <see cref="Alternative"/> parts are words from other ways of understanding the vocab, such as <c>くれる</c> for <c>くれよ</c>,<br/>
/// which is also <c>くれ</c> followed by <c>よ</c>.<br/>
/// Each part is a <see cref="WordReference"/>: the word, with a disambiguation when the vocab it's part of has to be told apart, as <c>出す:masu</c> in <c>見出す</c>.
/// </summary>
public class VocabNoteCompoundParts
{
   readonly NoteGuard _guard;
   List<WordReference> _primary;
   HashSet<WordReference> _enteredAlternatives;

   public VocabNoteCompoundParts(VocabNote vocab, VocabDto? data, NoteGuard guard)
   {
      Vocab = vocab;
      _guard = guard;
      _primary = data != null ? [.. data.CompoundParts.Primary.Select(WordReference.FromDto)] : [];
      _enteredAlternatives = data != null ? [.. data.CompoundParts.Alternative.Select(WordReference.FromDto)] : [];
   }

   VocabNote Vocab { get; }

   JPCollection Collection => Vocab.Collection;

   /// <summary>The vocab split into the words it's built from, in order. A word can occur more than once, as in <c>どいつもこいつも</c>.</summary>
   public List<WordReference> Primary() => [.. _primary];

   /// <summary>
   /// The words from other ways of understanding the vocab: a set, in a fixed order.<br/>
   /// A word that's also a <see cref="Primary"/> part is never one of them, since it would add nothing.
   /// </summary>
   /// <remarks>
   /// That rule is applied here rather than when the alternatives are set, so that typing the primary parts in the editor can't drop an alternative<br/>
   /// part on the way: typing <c>よう</c> passes through <c>よ</c>.
   /// </remarks>
   public List<WordReference> Alternative() =>
   [
      .. _enteredAlternatives
        .Except(_primary)
        .OrderBy(it => it.DisambiguationName, StringComparer.Ordinal)
   ];

   /// <summary>Every compound part: the <see cref="Primary"/> parts, followed by the <see cref="Alternative"/> ones.</summary>
   public List<WordReference> All() => [.. _primary, .. Alternative()];

   /// <summary>Replaces the <see cref="Primary"/> parts with <paramref name="parts"/>, in their order.</summary>
   public void SetPrimary(IEnumerable<WordReference> parts) => _guard.Update(() => _primary = [.. parts]);

   /// <summary>Replaces the <see cref="Alternative"/> parts with <paramref name="parts"/>.</summary>
   public void SetAlternative(IEnumerable<WordReference> parts) => _guard.Update(() => _enteredAlternatives = [.. parts]);

   /// <summary>Replaces <paramref name="oldName"/>, the old name of a vocab that was renamed, with its new name, <paramref name="newName"/>, wherever it's a part.</summary>
   internal void Rename(WordReference oldName, WordReference newName)
   {
      if(!_primary.Contains(oldName) && !_enteredAlternatives.Contains(oldName)) return;

      _guard.Update(() =>
      {
         _primary = [.. _primary.Select(it => it == oldName ? newName : it)];
         _enteredAlternatives = [.. _enteredAlternatives.Select(it => it == oldName ? newName : it)];
      });
   }

   /// <summary>The vocab notes the primary compound parts refer to, in the order of the parts.</summary>
   public List<VocabNote> PrimaryPartsNotes() =>
   [
      .. Primary()
        .SelectMany(NotesThePartRefersTo)
   ];

   /// <summary>
   /// Whether any compound part, alternative ones included, refers to <paramref name="vocab"/>.<br/>
   /// Uses the same lookup as <see cref="PrimaryPartsNotes"/> to decide which vocab notes a part refers to,<br/>
   /// so going from a compound to its parts and back again always agrees.
   /// </summary>
   public bool AnyPartRefersTo(VocabNote vocab) => All().Any(part => NotesThePartRefersTo(part).Contains(vocab));

   /// <summary>
   /// Whether any compound part, alternative ones included, names <paramref name="vocab"/> exactly (<see cref="VocabNoteQuestion.Reference"/>),<br/>
   /// rather than referring to it through one of its other forms.
   /// </summary>
   public bool AnyPartNamesExactly(VocabNote vocab) => All().Any(vocab.Question.IsNamedBy);

   List<VocabNote> NotesThePartRefersTo(WordReference part) => Collection.Vocab.ReferredToBy(part);

   /// <summary>
   /// Replaces the <see cref="Primary"/> parts with the words the vocab's question splits into, creating a vocab from the dictionary for each word<br/>
   /// that has none. The <see cref="Alternative"/> parts are kept.
   /// </summary>
   public void AutoGenerate()
   {
      var exclusions = Vocab.Forms.MatchedFormsSet()
                            .Select(WordExclusion.Global)
                            .ToList();
      var config = SentenceConfiguration.FromIncorrectMatches(exclusions);
      var analysisServices = new AnalysisServices(Vocab.Services.Collection.Vocab, Vocab.Services.DictLookup, Vocab.Services.Settings);
      var analysis = new TextAnalysis(analysisServices, Vocab.GetQuestion(), config);
      var compoundParts = analysis.DisplayWordVariants
                                  .Where(a => !Vocab.Forms.MatchedFormsSet().Contains(a.Form))
                                  .Select(a => a.Form)
                                  .ToList();

      if(compoundParts.Count <= 1) // time to brute force it
      {
         var word = Vocab.GetQuestion();
         var allSubstrings = new List<string>();
         for(var i = 0; i < word.Length; i++)
         {
            for(var j = i + 1; j <= word.Length; j++)
            {
               var substring = word.Substring(i, j - i);
               if(substring != word)
               {
                  allSubstrings.Add(substring);
               }
            }
         }

         var allWordSubstrings = allSubstrings
                                .Where(w => Vocab.Services.DictLookup.IsDictionaryOrCollectionWord(w))
                                .ToList();

         compoundParts =
         [
            .. allWordSubstrings
              .Where(segment => !allWordSubstrings.Any(parent => parent.Contains(segment) && parent != segment))
         ];
      }

      var segmentsMissingVocab = compoundParts
                                .Where(segment => !Collection.Vocab.IsWord(segment))
                                .ToList();

      foreach(var missing in segmentsMissingVocab)
      {
         Vocab.Services.VocabNoteFactory.CreateWithDictionary(missing);
      }

      SetPrimary(compoundParts.Select(it => new WordReference(it)));
   }

   public override string ToString() => string.Join(", ", All().Select(it => it.DisambiguationName));
}
