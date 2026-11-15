using System;
using System.Collections.Generic;
using System.Linq;
using Compze.Threading;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Tagging;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Studying;
using JAStudio.Core.Studying.StudyNotes;

namespace JAStudio.Core.TestUtils;

/// <summary>
/// The student's Anki in specifications, where there's no Anki: it gives the notes specifications choose a pretend study-note<br/>
/// (<see cref="GiveAStudyNoteTo"/>), and JAStudio reads the study-notes from it, and writes and deletes them in it, as in Anki<br/>
/// (<see cref="ISRSStudyNotes"/>): each study-note's corpus note, its fields as they were last written or changed here, the tags specifications or<br/>
/// JAStudio gave it, and the cards specifications suspended. It has no other cards, and no Anki notes but the study-notes. JAStudio changes its<br/>
/// tags as it changes Anki's (<see cref="ISRSTagOperations"/>).
/// </summary>
/// <remarks>
/// Unlike JAStudio inside Anki, the test environment doesn't write a note's study-note when it updates the note: a study-note here has the fields<br/>
/// its note had when it got the study-note, until it's written (<see cref="Write"/>) or a specification changes them.
/// </remarks>
public partial class PretendAnki : ISRSStudyNotes, ISRSTagOperations
{
   readonly IMonitor _monitor = IMonitor.New();
   readonly Dictionary<long, StudyNote> _studyNotesByAnkiNoteId = [];
   readonly ItsAvailability _availability = new();
   long _lastAnkiNoteId;

   /// <summary>The pretend Anki as a spaced repetition system for JAStudio to work with: there is one, which can be used unless its own sync is under way.</summary>
   public SRSAvailability Availability => _availability;

   /// <summary>Anki's own sync, with AnkiWeb, starts: until it has finished, JAStudio can't use what Anki holds.</summary>
   public void StartItsOwnSync() => _availability.ItsOwnSyncIsUnderWay(true);

   /// <summary>Anki's own sync, with AnkiWeb, has finished: JAStudio can use what Anki holds again.</summary>
   public void FinishItsOwnSync() => _availability.ItsOwnSyncIsUnderWay(false);

   public IReadOnlyList<StudyNoteInTheSRS> ReadAll() => _monitor.Locked(() => _studyNotesByAnkiNoteId.Values.Select(it => it.AsReadFromAnki()).ToList());

   public void Write(JPNote note) => _monitor.Locked(() => TheStudyNoteOf(note.GetId()).Fields = note.StudyNoteFields());

   /// <summary>Does nothing: a study-note here has no cards but those specifications suspended (<see cref="SuspendTheCard"/>).</summary>
   public void SuspendAllCardsForNote(NoteId noteId) {}

   /// <summary>Does nothing: a study-note here has no cards but those specifications suspended (<see cref="SuspendTheCard"/>).</summary>
   public void UnsuspendAllCardsForNote(NoteId noteId) {}

   public void DeleteTheStudyNoteOf(NoteId noteId) => _monitor.Locked(() =>
   {
      if(TheStudyNoteOrNoneOf(noteId) is {} studyNote) _studyNotesByAnkiNoteId.Remove(studyNote.AnkiNoteId);
   });

   /// <summary>
   /// Gives <paramref name="note"/>, a note in the collection, a study-note in Anki, as a person choosing to study it will, with the fields JAStudio<br/>
   /// writes for the note now: registers a new Anki note id for it, as a read of the study-notes would, so that the note has its study-note at once.<br/>
   /// Creating a note gives it none (Magnus, 2026-11-13).
   /// </summary>
   public void GiveAStudyNoteTo(JPNote note)
   {
      var ankiNoteId = AddAStudyNote(note.GetId(), note.StudyNoteFields());
      note.Services.StudyNoteIds.Register(ankiNoteId, note.GetId());
   }

   /// <summary>
   /// Adds an Anki note of one of JAStudio's note types, with <paramref name="fields"/>, that holds no corpus note's id, which no study-note JAStudio<br/>
   /// wrote does: as a note added in Anki itself, or one whose fields were damaged.
   /// </summary>
   /// <returns>The id of the Anki note.</returns>
   public long AddAStudyNoteThatHoldsNoCorpusNoteId(IReadOnlyDictionary<string, string> fields) => AddAStudyNote(corpusNoteId: null, fields);

   /// <summary>
   /// Gives <paramref name="note"/>'s study-note the tags <paramref name="tagsInAnki"/>, in place of those it had, as the student tagging it in Anki does.<br/>
   /// The note shows them as its Anki tags once the collection loads again.
   /// </summary>
   public void Tag(JPNote note, params string[] tagsInAnki) => _monitor.Locked(() => TheStudyNoteOf(note.GetId()).Tags = tagsInAnki);

   /// <summary>
   /// Suspends the card of <paramref name="cardType"/>, such as <see cref="CardTypes.Reading"/>, of <paramref name="note"/>'s study-note, as the student<br/>
   /// suspending it in Anki does. The note shows it once the collection loads, or reads the study-notes from Anki, again.
   /// </summary>
   public void SuspendTheCard(JPNote note, string cardType) => _monitor.Locked(() => TheStudyNoteOf(note.GetId()).SuspendTheCard(cardType, NoteTypeNameOf(note)));

   /// <summary>
   /// Gives the field <paramref name="field"/> of <paramref name="note"/>'s study-note the value <paramref name="valueInAnki"/>, as a change made in<br/>
   /// Anki does, and as a study-note JAStudio hasn't written since it changed the note has it.
   /// </summary>
   public void ChangeTheFieldOfTheStudyNoteOf(JPNote note, string field, string valueInAnki) => _monitor.Locked(() =>
   {
      var studyNote = TheStudyNoteOf(note.GetId());
      studyNote.Fields = new Dictionary<string, string>(studyNote.Fields) { [field] = valueInAnki };
   });

   /// <summary>
   /// Deletes <paramref name="note"/>'s study-note, as the student deleting it in Anki on another device, and then syncing, does: nothing tells JAStudio.<br/>
   /// The note shows it once the collection loads, or reads the study-notes from Anki, again.
   /// </summary>
   public void DeleteTheStudyNoteOf(JPNote note) => _monitor.Locked(() => _studyNotesByAnkiNoteId.Remove(TheStudyNoteOf(note.GetId()).AnkiNoteId));

   /// <summary>The tags <paramref name="note"/>'s study-note has, as Anki names them, such as <c>marked</c>, in alphabetical order.</summary>
   public IReadOnlyList<string> TagsOf(JPNote note) => _monitor.Locked(() => TheStudyNoteOf(note.GetId()).Tags.Order(StringComparer.OrdinalIgnoreCase).ToList());

   /// <summary>The fields <paramref name="note"/>'s study-note has, by their names in Anki.</summary>
   public IReadOnlyDictionary<string, string> FieldsOfTheStudyNoteOf(JPNote note) => _monitor.Locked(() => TheStudyNoteOf(note.GetId()).Fields);

   public void AddTheTagToTheStudyNotesOf(IReadOnlyCollection<NoteId> notes, string tagInAnki) => _monitor.Locked(() =>
      ChangeTheTagsOf(notes.Select(TheStudyNoteOf), tags => tags.Contains(tagInAnki, StringComparer.OrdinalIgnoreCase) ? tags : [..tags, tagInAnki]));

   public void RemoveTheTagFromTheStudyNotesOf(IReadOnlyCollection<NoteId> notes, string tagInAnki) => _monitor.Locked(() =>
      ChangeTheTagsOf(notes.Select(TheStudyNoteOf), tags => [..tags.Where(it => !it.Equals(tagInAnki, StringComparison.OrdinalIgnoreCase))]));

   public void RenameTheBranchOnEveryNoteInTheSRS(string pathInAnki, string renamedPathInAnki) => _monitor.Locked(() =>
   {
      var branch = new TagBranch(pathInAnki);
      var renamed = new TagBranch(renamedPathInAnki);
      ChangeTheTagsOf(_studyNotesByAnkiNoteId.Values, tags => [..tags.Select(it => branch.Contains(it) ? branch.TheNameRenamedTo(renamed, it) : it).Distinct(StringComparer.OrdinalIgnoreCase)]);
   });

   public void DeleteTheBranchFromEveryNoteInTheSRS(string pathInAnki) => _monitor.Locked(() =>
      ChangeTheTagsOf(_studyNotesByAnkiNoteId.Values, tags => [..tags.Where(it => !new TagBranch(pathInAnki).Contains(it))]));

   public int NumberOfNotesInTheSRSWithATagIn(string pathInAnki) => _monitor.Locked(() => _studyNotesByAnkiNoteId.Values.Count(it => it.Tags.Any(new TagBranch(pathInAnki).Contains)));

   static void ChangeTheTagsOf(IEnumerable<StudyNote> studyNotes, Func<IReadOnlyList<string>, IReadOnlyList<string>> change)
   {
      foreach(var studyNote in studyNotes.ToList())
         studyNote.Tags = change(studyNote.Tags);
   }

   /// <summary>Adds a study-note with <paramref name="fields"/>, of the corpus note with the id <paramref name="corpusNoteId"/> or of none, under a new Anki note id, which it returns.</summary>
   long AddAStudyNote(NoteId? corpusNoteId, IReadOnlyDictionary<string, string> fields) => _monitor.Locked(() =>
   {
      var studyNote = new StudyNote(++_lastAnkiNoteId, corpusNoteId, fields);
      _studyNotesByAnkiNoteId[studyNote.AnkiNoteId] = studyNote;
      return studyNote.AnkiNoteId;
   });

   StudyNote TheStudyNoteOf(NoteId note) => _studyNotesByAnkiNoteId.Values.Single(it => note.Equals(it.CorpusNoteId));

   StudyNote? TheStudyNoteOrNoneOf(NoteId note) => _studyNotesByAnkiNoteId.Values.SingleOrDefault(it => note.Equals(it.CorpusNoteId));

   static string NoteTypeNameOf(JPNote note) => note switch
   {
      VocabNote => NoteTypes.Vocab,
      KanjiNote => NoteTypes.Kanji,
      SentenceNote => NoteTypes.Sentence,
      _ => throw new ArgumentOutOfRangeException(nameof(note), note, "Not a note type with a study-note.")
   };
}
