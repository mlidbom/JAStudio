using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices.JanomeEx;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.CorpusData;
using JAStudio.Core.Note.Sentences;

namespace JAStudio.Core.Note.Vocabulary;

/// <summary>
/// The compound parts of a <see cref="VocabNote"/>: the words the user has entered as what the vocab is built from.<br/>
/// A part written in brackets, like <c>[word]</c>, is an alternative compound part: another way of thinking about the vocab.<br/>
/// The parts without brackets are the primary compound parts, the ones displayed when showing what the vocab is built from.
/// </summary>
public class VocabNoteUserCompoundParts
{
   readonly NoteGuard _guard;
   List<string> _parts;

   public VocabNoteUserCompoundParts(VocabNote vocab, VocabData? data, NoteGuard guard)
   {
      Vocab = vocab;
      _guard = guard;
      _parts = data?.UserCompounds != null ? [..data.UserCompounds] : [];
   }

   VocabNote Vocab { get; }

   JPCollection Collection => Vocab.Collection;

   /// <summary>The primary compound parts: those not written in brackets.</summary>
   public List<string> Primary()
   {
      return _parts
            .Where(part => !part.StartsWith("["))
            .ToList();
   }

   /// <summary>Every compound part, alternative ones included, with the brackets stripped.</summary>
   public List<string> All() =>
      _parts
        .Select(StripBrackets)
        .ToList();

   /// Returns all compound parts with brackets preserved (e.g. "[word]" stays as-is).
   public List<string> AllRaw() => _parts.ToList();

   public void Set(List<string> value) => _guard.Update(() => _parts = [..value]);

   /// <summary>The vocab notes the primary compound parts refer to, in the order of the parts.</summary>
   public List<VocabNote> PrimaryPartsNotes() =>
      Primary()
        .SelectMany(NotesThePartRefersTo)
        .ToList();

   /// <summary>
   /// Whether any compound part, alternative ones included, refers to <paramref name="vocab"/>.<br/>
   /// Uses the same lookup as <see cref="PrimaryPartsNotes"/> to decide which vocab notes a part refers to,<br/>
   /// so going from a compound to its parts and back again always agrees.
   /// </summary>
   public bool AnyPartRefersTo(VocabNote vocab) => All().Any(part => NotesThePartRefersTo(part).Contains(vocab));

   /// <summary>
   /// Whether any compound part, alternative ones included, is exactly the <see cref="VocabNoteQuestion.DisambiguationName"/> of <paramref name="vocab"/>,<br/>
   /// rather than referring to it through one of its other forms.
   /// </summary>
   public bool AnyPartIsTheDisambiguationNameOf(VocabNote vocab) => All().Contains(vocab.Question.DisambiguationName);

   List<VocabNote> NotesThePartRefersTo(string part) => Collection.Vocab.WithFormPreferDisambiguationNameOrExactMatch(part);

   public void AutoGenerate()
   {
      var exclusions = Vocab.Forms.MatchedFormsSet()
                            .Select(form => WordExclusion.Global(form))
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

         compoundParts = allWordSubstrings
                        .Where(segment => !allWordSubstrings.Any(parent => parent.Contains(segment) && parent != segment))
                        .ToList();
      }

      var segmentsMissingVocab = compoundParts
                                .Where(segment => !Collection.Vocab.IsWord(segment))
                                .ToList();

      foreach(var missing in segmentsMissingVocab)
      {
         Vocab.Services.VocabNoteFactory.CreateWithDictionary(missing);
      }

      Set(compoundParts);
   }

   static string StripBrackets(string part) => part.Replace("[", string.Empty).Replace("]", string.Empty);

   public override string ToString() => string.Join(", ", _parts);
}
