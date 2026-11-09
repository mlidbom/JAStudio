using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Core.Note.Collection;

class VocabSnapshot : CachedNote
{
   /// <summary>The word-reference naming the vocab exactly (<see cref="VocabNoteQuestion.Reference"/>): none while it's being created, without a question yet.</summary>
   public WordReference[] ReferenceNamingIt { get; }

   public string[] Forms { get; }
   public WordReference[] CompoundParts { get; }
   public string[] MainFormKanji { get; }
   public string[] AllKanji { get; }
   public string[] Readings { get; }
   /// <summary>The word the vocab is derived from, if one is given.</summary>
   public WordReference[] DerivedFrom { get; }
   public string[] Stems { get; }

   public VocabSnapshot(VocabNote note) : base(note)
   {
      ReferenceNamingIt = note.Question.HasText ? [note.Question.Reference] : [];
      Forms = [.. note.Forms.MatchedFormsList()];
      CompoundParts = [.. note.CompoundParts.All()];
      MainFormKanji = [.. note.Kanji.ExtractMainFormKanji()];
      AllKanji = [.. note.Kanji.ExtractAllKanji()];
      Readings = [.. note.GetReadings()];
      DerivedFrom = note.RelatedNotes.DerivedFrom.Word is {} word ? [word] : [];
      Stems = [.. note.Conjugator.GetStemsForPrimaryForm()];
   }
}
