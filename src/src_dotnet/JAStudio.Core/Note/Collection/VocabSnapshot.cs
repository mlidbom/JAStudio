using System.Linq;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Core.Note.Collection;

class VocabSnapshot : CachedNote
{
   /// <summary>The word-reference naming the vocab exactly (<see cref="VocabNoteQuestion.Reference"/>): none while it's being created, without a question yet.</summary>
   public WordReference[] ReferenceNamingIt { get; }

   public string[] Forms { get; }

   /// <summary>Every start of each of its <see cref="Forms"/>: its first character, its first two, and so on to the whole form. 行く gives 行 and 行く.</summary>
   public string[] FormPrefixes { get; }

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
      FormPrefixes = [.. Forms.SelectMany(form => Enumerable.Range(1, form.Length).Select(length => form[..length])).Distinct()];
      CompoundParts = [.. note.CompoundParts.All()];
      MainFormKanji = [.. note.Kanji.ExtractMainFormKanji()];
      AllKanji = [.. note.Kanji.ExtractAllKanji()];
      Readings = [.. note.GetReadings()];
      DerivedFrom = note.RelatedNotes.DerivedFrom.Word is {} word ? [word] : [];
      Stems = [.. note.Conjugator.GetStemsForPrimaryForm()];
   }
}
