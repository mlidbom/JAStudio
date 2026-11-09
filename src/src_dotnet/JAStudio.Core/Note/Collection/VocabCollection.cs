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

   public bool IsWord(string form) => Cache.WithForm(form).Count > 0;
   public List<VocabNote> All() => Cache.All();
   public VocabNote? WithIdOrNone(NoteId noteId) => Cache.WithIdOrNone(noteId);
   public VocabNote? WithExternalIdOrNone(long externalNoteId) => Cache.WithExternalIdOrNone(externalNoteId);
   public List<VocabNote> WithForm(string form) => Cache.WithForm(form);

   /// <summary>The vocab <paramref name="reference"/> names exactly: whose question has its text and its disambiguation. One, unless the corpus has duplicates.</summary>
   public List<VocabNote> NamedExactlyBy(WordReference reference) => Cache.NamedExactlyBy(reference);

   /// <summary>The vocab any of <paramref name="references"/> names exactly (<see cref="NamedExactlyBy"/>).</summary>
   public List<VocabNote> NamedExactlyByAny(IEnumerable<WordReference> references) => [.. references.SelectMany(NamedExactlyBy).Distinct()];

   /// <summary>
   /// The vocab <paramref name="reference"/> refers to: the vocab it names exactly (<see cref="NamedExactlyBy"/>). A reference without a disambiguation that names<br/>
   /// no vocab exactly refers to the vocab with its text as a form instead, only those whose question has that text if there are any, as <c>んじゃ</c> refers to<br/>
   /// <c>んじゃ:s.start</c> when there is no plain <c>んじゃ</c>.
   /// </summary>
   public List<VocabNote> ReferredToBy(WordReference reference)
   {
      var namedExactly = NamedExactlyBy(reference);
      if(namedExactly.Count > 0 || reference.IsDisambiguated)
         return namedExactly;

      var withTheForm = WithForm(reference.Text);
      var withTheQuestion = withTheForm.Where(it => it.Question.Text == reference.Text).ToList();
      return withTheQuestion.Count > 0 ? withTheQuestion : withTheForm;
   }

   /// <summary>
   /// Renames every word-reference that names <paramref name="oldName"/> exactly to <paramref name="newName"/>, in every vocab's related vocab and compound<br/>
   /// parts, after the vocab that had the one name got the other. References that still name a vocab exactly, a duplicate of the renamed one, stay.
   /// </summary>
   /// <remarks>
   /// A reference that only refers to the vocab through one of its forms is a spelling, not the vocab's name, and stays: the vocab may well keep that form.
   /// </remarks>
   internal void RenameTheReferences(WordReference oldName, WordReference newName)
   {
      if(NamedExactlyBy(oldName).Count > 0) return;

      foreach(var vocab in All())
         vocab.RenameTheReferences(oldName, newName);
   }

   /// <summary>The vocab any of <paramref name="references"/> refers to (<see cref="ReferredToBy"/>).</summary>
   public List<VocabNote> ReferredToByAny(IEnumerable<WordReference> references) => [.. references.SelectMany(ReferredToBy).Distinct()];

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
   public List<VocabNote> CompoundsContainingCompoundPart(WordReference compoundPart) => IncludingTheCompoundsTheyArePartOf(Cache.WithCompoundPart(compoundPart));

   /// <summary>
   /// The vocab derived from <paramref name="vocab"/>: those whose <see cref="Vocabulary.RelatedVocab.RelatedVocab.DerivedFrom"/> refers to it, the same lookup<br/>
   /// a vocab uses to show what it's derived from, so the two directions agree.
   /// </summary>
   public List<VocabNote> DerivedFrom(VocabNote vocab) =>
   [
      .. WordsThatCouldReferTo(vocab)
        .SelectMany(Cache.DerivedFrom)
        .Distinct()
        .Where(it => it.RelatedNotes.DerivedFrom.Notes().Contains(vocab))
   ];
   public List<VocabNote> WithKanjiInMainForm(KanjiNote kanji) => Cache.WithKanjiInMainForm(kanji.GetQuestion());
   public List<VocabNote> WithKanjiInAnyForm(KanjiNote kanji) => Cache.WithKanjiInAnyForm(kanji.GetQuestion());
   public List<VocabNote> WithQuestion(string question) => Cache.WithQuestion(question);

   /// <summary>
   /// The vocab <paramref name="disambiguationName"/> names, as a person selects it in a note or types it: with a disambiguation, as <c>入る:masu</c>,<br/>
   /// the vocab it names exactly; without one, every vocab with it as its question's text, as <c>入る</c> for both <c>入る</c> and <c>入る:masu</c>.<br/>
   /// None when it names no word (<see cref="WordReference.FromDisambiguationNameOrNone"/>).
   /// </summary>
   public List<VocabNote> WithQuestionOrNamedBy(string disambiguationName) =>
      WordReference.FromDisambiguationNameOrNone(disambiguationName) switch
      {
         { IsDisambiguated: true } reference => NamedExactlyBy(reference),
         {} reference                         => Cache.WithQuestion(reference.Text),
         null                                 => []
      };

   public List<VocabNote> WithReading(string reading) => Cache.WithReading(reading);
   public List<VocabNote> WithStem(string stem) => Cache.WithStem(stem);

   public List<VocabNote> WithAnyFormIn(List<string> forms) => [.. forms.SelectMany(WithForm).Distinct()];

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

      return [.. result.OrderBy(it => it.GetQuestion())];

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
   List<VocabNote> CompoundsWithAPartReferringTo(VocabNote vocab) =>
   [
      .. WordsThatCouldReferTo(vocab)
        .SelectMany(Cache.WithCompoundPart)
        .Distinct()
        .Where(compound => compound.CompoundParts.AnyPartRefersTo(vocab))
   ];

   /// <summary>
   /// The only word-references that can refer to <paramref name="vocab"/> (<see cref="ReferredToBy"/>): the one naming it exactly, and one for each of its forms,<br/>
   /// so the notes holding one of them are the only candidates worth asking.
   /// </summary>
   static IEnumerable<WordReference> WordsThatCouldReferTo(VocabNote vocab) =>
      vocab.Forms.MatchedFormsSet()
           .Select(form => new WordReference(form))
           .Append(vocab.Question.Reference)
           .Distinct();
}
