using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Core.Note.Collection;

public class VocabCollection
{
   readonly IBackendNoteCreator _backendNoteCreator;
   internal readonly VocabCache Cache;
   // ReSharper disable once UnusedMember.Global used from python
   public IExternalNoteUpdateHandler ExternalSyncHandler => Cache;

   public VocabCollection(IBackendNoteCreator backendNoteCreator, NoteServices noteServices)
   {
      _backendNoteCreator = backendNoteCreator;
      Cache = new VocabCache(noteServices);
   }

   /// <summary>Starts recording every vocab this collection's lookups return, such as the vocab parsing a sentence looks at, until the recording is disposed.</summary>
   public LookupRecording<VocabNote> StartRecordingTheVocabLookedUp() => Cache.StartRecordingTheNotesLookedUp();

   public bool IsWord(string form) => Cache.WithForm(form).Any();
   public List<VocabNote> All() => Cache.All();
   public VocabNote? WithIdOrNone(NoteId noteId) => Cache.WithIdOrNone(noteId);
   public VocabNote? WithExternalIdOrNone(long externalNoteId) => Cache.WithExternalIdOrNone(externalNoteId);
   public List<VocabNote> WithDisambiguationName(string name) => Cache.WithDisambiguationName(name);
   public List<VocabNote> WithForm(string form) => Cache.WithForm(form);

   /// <summary>
   /// The vocab notes <paramref name="vocab"/> is part of: every vocab with a compound part that refers to it,<br/>
   /// and, recursively, every vocab those in turn are part of. Alphabetical by question.
   /// </summary>
   /// <remarks>
   /// A compound part refers to <paramref name="vocab"/> as decided by <see cref="VocabNoteCompoundParts.AnyPartRefersTo"/>,<br/>
   /// the same lookup a compound uses to show its own parts, so the two directions of navigation always agree.
   /// </remarks>
   public List<VocabNote> CompoundsContaining(VocabNote vocab) => IncludingTheCompoundsTheyArePartOf(CompoundsWithAPartReferringTo(vocab));

   /// <summary>
   /// The vocab notes with exactly <paramref name="compoundPart"/> as one of their compound parts,<br/>
   /// and, recursively, every vocab those in turn are part of. Alphabetical by question.
   /// </summary>
   public List<VocabNote> CompoundsContainingCompoundPart(string compoundPart) => IncludingTheCompoundsTheyArePartOf(Cache.WithCompoundPart(compoundPart));
   public List<VocabNote> DerivedFrom(string derivedFrom) => Cache.DerivedFrom(derivedFrom);
   public List<VocabNote> WithKanjiInMainForm(KanjiNote kanji) => Cache.WithKanjiInMainForm(kanji.GetQuestion());
   public List<VocabNote> WithKanjiInAnyForm(KanjiNote kanji) => Cache.WithKanjiInAnyForm(kanji.GetQuestion());
   public List<VocabNote> WithQuestion(string question) => Cache.WithQuestion(question);

   public IEnumerable<VocabNote> WithQuestionPreferDisambiguationName(string question) =>
      question.Contains(VocabNoteQuestion.DisambiguationMarker)
         ? Cache.WithDisambiguationName(question)
         : Cache.WithQuestion(question);

   public List<VocabNote> WithReading(string reading) => Cache.WithReading(reading);
   public List<VocabNote> WithStem(string stem) => Cache.WithStem(stem);

   public List<VocabNote> WithFormPreferDisambiguationNameOrExactMatch(string form)
   {
      var withDisambiguationName = WithDisambiguationName(form).ToList();
      if(withDisambiguationName.Any())
         return withDisambiguationName;

      var matches = WithForm(form);
      var exactMatch = matches.Where(voc => voc.Question.Text == form).ToList();
      var sequence = exactMatch.Any() ? exactMatch : matches;
      return sequence.Distinct().ToList();
   }

   public List<VocabNote> WithAnyFormInPreferDisambiguationNameOrExactMatch(List<string> forms) =>
      forms
        .SelectMany(WithFormPreferDisambiguationNameOrExactMatch)
        .Distinct()
        .ToList();

   public List<VocabNote> WithAnyFormIn(List<string> forms) =>
      forms
        .SelectMany(WithForm)
        .Distinct()
        .ToList();

   public List<VocabNote> WithAnyDisambiguationNameIn(IEnumerable<string> questions) =>
      questions
        .SelectMany(WithDisambiguationName)
        .ToList();

   public void Add(VocabNote note)
   {
      _backendNoteCreator.CreateVocab(note, () => Cache.AddToCache(note));
   }

   List<VocabNote> IncludingTheCompoundsTheyArePartOf(List<VocabNote> compounds)
   {
      var result = new HashSet<VocabNote>();
      foreach(var compound in compounds)
      {
         Include(compound);
      }

      return result.OrderBy(it => it.GetQuestion()).ToList();

      void Include(VocabNote compound)
      {
         if(!result.Add(compound)) return;

         foreach(var compoundItIsPartOf in CompoundsWithAPartReferringTo(compound))
         {
            Include(compoundItIsPartOf);
         }
      }
   }

   /// <summary>The vocab notes with a compound part that refers to <paramref name="vocab"/>, not following compounds of compounds.</summary>
   /// <remarks>
   /// Only a compound part that is the <see cref="VocabNoteQuestion.DisambiguationName"/> of <paramref name="vocab"/>, or one of its forms,<br/>
   /// can refer to it, so the compounds with such a part are the only candidates worth asking.
   /// </remarks>
   List<VocabNote> CompoundsWithAPartReferringTo(VocabNote vocab)
   {
      return CompoundsWithAPartThatCouldReferToIt()
            .Where(compound => compound.CompoundParts.AnyPartRefersTo(vocab))
            .ToList();

      IEnumerable<VocabNote> CompoundsWithAPartThatCouldReferToIt() =>
         vocab.Forms.MatchedFormsSet()
              .Append(vocab.Question.DisambiguationName)
              .SelectMany(Cache.WithCompoundPart)
              .Distinct();
   }
}
