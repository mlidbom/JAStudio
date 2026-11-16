using System;
using System.Collections.Generic;
using System.Linq;
using Compze.Contracts;
using Compze.DependencyInjection.Abstractions;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing;

namespace JAStudio.Core.Note.Collection;

/// <summary>
/// Every vocab of the corpus, and the ways to look them up: by form, by reading, by what a <see cref="WordReference"/> refers to, by the compounds a vocab<br/>
/// is part of, and so on.
/// </summary>
public class VocabCollection
{
   readonly StoredNotes<VocabNote> _notes;
   readonly NoteUnitsOfWork _unitsOfWork;
   readonly NoteIndex<VocabNote, VocabIndexes.Question> _byQuestion;
   readonly NoteIndex<VocabNote, VocabIndexes.ReferenceNamingIt> _byReferenceNamingIt;
   readonly NoteIndex<VocabNote, VocabIndexes.Form> _byForm;
   readonly NoteIndex<VocabNote, VocabIndexes.FormStart> _byFormStart;
   readonly NoteIndex<VocabNote, VocabIndexes.CompoundPart> _byCompoundPart;
   readonly NoteIndex<VocabNote, VocabIndexes.DerivedFrom> _byDerivedFrom;
   readonly NoteIndex<VocabNote, VocabIndexes.KanjiInTheMainForm> _byKanjiInTheMainForm;
   readonly NoteIndex<VocabNote, VocabIndexes.KanjiInAnyForm> _byKanjiInAnyForm;
   readonly NoteIndex<VocabNote, VocabIndexes.Reading> _byReading;
   readonly NoteIndex<VocabNote, VocabIndexes.Stem> _byStem;

   internal VocabCollection(NoteServices noteServices, IRootResolver resolver, NoteUnitsOfWork unitsOfWork)
   {
      _notes = new StoredNotes<VocabNote>(noteServices, resolver);
      _unitsOfWork = unitsOfWork;
      _byQuestion = _notes.IndexBy<VocabIndexes.Question>();
      _byReferenceNamingIt = _notes.IndexBy<VocabIndexes.ReferenceNamingIt>();
      _byForm = _notes.IndexBy<VocabIndexes.Form>();
      _byFormStart = _notes.IndexBy<VocabIndexes.FormStart>();
      _byCompoundPart = _notes.IndexBy<VocabIndexes.CompoundPart>();
      _byDerivedFrom = _notes.IndexBy<VocabIndexes.DerivedFrom>();
      _byKanjiInTheMainForm = _notes.IndexBy<VocabIndexes.KanjiInTheMainForm>();
      _byKanjiInAnyForm = _notes.IndexBy<VocabIndexes.KanjiInAnyForm>();
      _byReading = _notes.IndexBy<VocabIndexes.Reading>();
      _byStem = _notes.IndexBy<VocabIndexes.Stem>();
   }

   /// <summary>Starts recording every vocab this collection's lookups return, such as the vocab parsing a sentence looks at, until the recording is disposed.</summary>
   public LookupRecording<VocabNote> StartRecordingTheVocabLookedUp() => _notes.LookupRecorder.StartRecording();

   public bool IsWord(string form) => WithForm(form).Count > 0;
   public List<VocabNote> All() => _notes.All();

   /// <summary>
   /// The vocab whose match-conditions depend on <paramref name="sharedCondition"/>'s definition (<see cref="Condition.DependsOnTheDefinitionOf"/>): those a<br/>
   /// change to it can make match other words, whose sentences it reparses.
   /// </summary>
   public List<VocabNote> WhoseMatchConditionsDependOnTheDefinitionOf(ISharedCondition sharedCondition) =>
      [.. All().Where(it => it.MatchingConfiguration.MatchCondition.DependsOnTheDefinitionOf(sharedCondition))];

   /// <summary>
   /// The vocab whose match-conditions use <paramref name="sharedCondition"/> by name (<see cref="Condition.Uses"/>): those deleting it writes its definition<br/>
   /// out in.
   /// </summary>
   public List<VocabNote> WhoseMatchConditionsUse(ISharedCondition sharedCondition) =>
      [.. All().Where(it => it.MatchingConfiguration.MatchCondition.Uses(sharedCondition))];

   public VocabNote? WithIdOrNone(NoteId noteId) => _notes.WithIdOrNone(noteId);
   public VocabNote? WithTheStudyNoteOrNone(long studyNoteId) => _notes.WithTheStudyNoteOrNone(studyNoteId);
   public List<VocabNote> WithForm(string form) => _byForm.NotesWith(new(form));

   /// <summary>Whether any vocab has a form that starts with <paramref name="text"/>, or is it.</summary>
   public bool HasAFormStartingWith(string text) => _byFormStart.HasNotesWith(new(text));

   /// <summary>The vocab <paramref name="reference"/> names exactly: whose question has its text and its disambiguation. At most one, as no two vocab share a question.</summary>
   public List<VocabNote> NamedExactlyBy(WordReference reference) => _byReferenceNamingIt.NotesWith(new(reference));

   /// <summary>The vocab <paramref name="reference"/> names exactly (<see cref="NamedExactlyBy"/>), or null when no vocab has that question.</summary>
   public VocabNote? NamedExactlyByOrNone(WordReference reference) => NamedExactlyBy(reference).SingleOrDefault();

   /// <summary>
   /// The vocab <paramref name="question"/> names exactly, if there is one, or else the vocab <paramref name="create"/> creates with that question:<br/>
   /// no two vocab share a question, so a vocab that has it already is given back, unchanged, rather than a duplicate created.
   /// </summary>
   public CreatedOrExistingVocab NamedExactlyByOrCreated(WordReference question, Func<VocabNote> create) =>
      NamedExactlyByOrNone(question) is {} existing ? CreatedOrExistingVocab.Existing(existing) : CreatedOrExistingVocab.Created(create());

   /// <summary>Whether a vocab other than <paramref name="vocab"/> has <paramref name="question"/> as its question, which no two vocab may share.</summary>
   public bool AnotherVocabHasTheQuestion(VocabNote vocab, WordReference question) => NamedExactlyBy(question).Any(it => !it.Equals(vocab));

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
      return namedExactly.Count > 0 || reference.IsDisambiguated
                ? namedExactly
                : WithTheFormPreferringItsQuestion(reference.Text);
   }

   /// <summary>The vocab any of <paramref name="references"/> refers to (<see cref="ReferredToBy"/>).</summary>
   public List<VocabNote> ReferredToByAny(IEnumerable<WordReference> references) => [.. references.SelectMany(ReferredToBy).Distinct()];

   /// <summary>
   /// Renames every word-reference that names <paramref name="oldName"/> exactly to <paramref name="newName"/>, in every vocab's related vocab and compound<br/>
   /// parts, after the vocab that had the one name got the other.
   /// </summary>
   /// <remarks>
   /// A reference that only refers to the vocab through one of its forms is a spelling, not the vocab's name, and stays: the vocab may well keep that form.
   /// </remarks>
   internal void RenameTheReferences(WordReference oldName, WordReference newName)
   {
      foreach(var vocab in All())
         vocab.Edit(it => it.RenameTheReferences(oldName, newName));
   }

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
   public List<VocabNote> CompoundsContainingCompoundPart(WordReference compoundPart) => IncludingTheCompoundsTheyArePartOf(WithCompoundPart(compoundPart));

   /// <summary>
   /// The vocab derived from <paramref name="vocab"/>: those whose <see cref="Vocabulary.RelatedVocab.RelatedVocab.DerivedFrom"/> refers to it, the same lookup<br/>
   /// a vocab uses to show what it's derived from, so the two directions agree.
   /// </summary>
   public List<VocabNote> DerivedFrom(VocabNote vocab) =>
   [
      .. WordsThatCouldReferTo(vocab)
        .SelectMany(DerivedFromTheWord)
        .Distinct()
        .Where(it => it.RelatedNotes.DerivedFrom.Notes().Contains(vocab))
   ];
   public List<VocabNote> WithKanjiInMainForm(KanjiNote kanji) => _byKanjiInTheMainForm.NotesWith(new(kanji.GetQuestion()));
   public List<VocabNote> WithKanjiInAnyForm(KanjiNote kanji) => _byKanjiInAnyForm.NotesWith(new(kanji.GetQuestion()));
   public List<VocabNote> WithQuestion(string question) => _byQuestion.NotesWith(new(question));

   /// <summary>
   /// The vocab <paramref name="disambiguationName"/> names, as a person selects it in a note or types it: with a disambiguation, as <c>入る:masu</c>,<br/>
   /// the vocab it names exactly; without one, every vocab with it as its question's text, as <c>入る</c> for both <c>入る</c> and <c>入る:masu</c>.<br/>
   /// None when it names no word (<see cref="WordReference.FromDisambiguationNameOrNone"/>).
   /// </summary>
   public List<VocabNote> WithQuestionOrNamedBy(string disambiguationName) =>
      WordReference.FromDisambiguationNameOrNone(disambiguationName) switch
      {
         { IsDisambiguated: true } reference => NamedExactlyBy(reference),
         {} reference                         => WithQuestion(reference.Text),
         null                                 => []
      };

   public List<VocabNote> WithReading(string reading) => _byReading.NotesWith(new(reading));
   public List<VocabNote> WithStem(string stem) => _byStem.NotesWith(new(stem));

   public List<VocabNote> WithAnyFormIn(List<string> forms) => [.. forms.SelectMany(WithForm).Distinct()];

   /// <inheritdoc cref="SentenceCollection.Add"/>
   /// <remarks>Its question must be one no vocab has.</remarks>
   public void Add(VocabNote note)
   {
      Contract.Argument.Assert(NamedExactlyBy(note.Question.Reference).Count == 0, () => $"A vocab named {note.Question.DisambiguationName} exists already, and no two vocab may share a question.");
      _unitsOfWork.Run(it => it.Add(note));
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

   /// <summary>
   /// The vocab with <paramref name="form"/> as a form, only those with it as their question's text if there are any: <c>掛ける</c> finds かける, which has it<br/>
   /// as a form, only while no vocab has <c>掛ける</c> as its question.
   /// </summary>
   List<VocabNote> WithTheFormPreferringItsQuestion(string form)
   {
      var withTheForm = WithForm(form);
      var withItAsTheirQuestion = withTheForm.Where(it => it.Question.Text == form).ToList();
      return withItAsTheirQuestion.Count > 0 ? withItAsTheirQuestion : withTheForm;
   }

   /// <summary>The vocab with exactly <paramref name="compoundPart"/> among their compound parts.</summary>
   List<VocabNote> WithCompoundPart(WordReference compoundPart) => _byCompoundPart.NotesWith(new(compoundPart));

   /// <summary>The vocab whose derived-from word is exactly <paramref name="word"/>.</summary>
   List<VocabNote> DerivedFromTheWord(WordReference word) => _byDerivedFrom.NotesWith(new(word));

   /// <summary>The vocab notes with a compound part that refers to <paramref name="vocab"/>, not following compounds of compounds.</summary>
   List<VocabNote> CompoundsWithAPartReferringTo(VocabNote vocab) =>
   [
      .. WordsThatCouldReferTo(vocab)
        .SelectMany(WithCompoundPart)
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
