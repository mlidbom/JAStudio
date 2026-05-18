using System.Linq;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Core.Note.Collection;

class VocabSnapshot : CachedNote
{
   public string DisambiguationName { get; }
   public string[] Forms { get; }
   public string[] CompoundParts { get; }
   public string[] MainFormKanji { get; }
   public string[] AllKanji { get; }
   public string[] Readings { get; }
   public string DerivedFrom { get; }
   public string[] Stems { get; }

   public VocabSnapshot(VocabNote note) : base(note)
   {
      DisambiguationName = note.Question.DisambiguationName;
      Forms = note.Forms.AllList().ToArray();
      CompoundParts = note.CompoundParts.All().ToArray();
      MainFormKanji = note.Kanji.ExtractMainFormKanji().ToArray();
      AllKanji = note.Kanji.ExtractAllKanji().ToArray();
      Readings = note.GetReadings().ToArray();
      DerivedFrom = note.RelatedNotes.DerivedFrom.Get();
      Stems = note.Conjugator.GetStemsForPrimaryForm().ToArray();
   }
}
