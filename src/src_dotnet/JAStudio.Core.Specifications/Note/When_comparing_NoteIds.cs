using System;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.Note;

public class When_comparing_NoteIds
{
   static readonly Guid SharedGuid = Guid.Parse("10000000-0000-0000-0000-000000000000");

   public class given_a_NoteId_and_a_VocabId_with_the_same_guid : When_comparing_NoteIds
   {
      readonly NoteId _noteId = new(SharedGuid);
      readonly VocabId _vocabId = new(SharedGuid);

      [XF] public void they_are_equal() => _noteId.Equals(_vocabId).Must().BeTrue();
      [XF] public void equals_operator_returns_true() => (_noteId == _vocabId).Must().BeTrue();
      [XF] public void they_have_the_same_hash_code() => _noteId.GetHashCode().Must().Be(_vocabId.GetHashCode());
   }

   public class given_a_VocabId_and_a_KanjiId_with_the_same_guid : When_comparing_NoteIds
   {
      readonly VocabId _vocabId = new(SharedGuid);
      readonly KanjiId _kanjiId = new(SharedGuid);

      [XF] public void they_are_not_equal() => _vocabId.Equals(_kanjiId).Must().BeFalse();
      [XF] public void equals_operator_returns_false() => (_vocabId == _kanjiId).Must().BeFalse();
   }

   public class given_a_VocabId_and_a_SentenceId_with_the_same_guid : When_comparing_NoteIds
   {
      readonly VocabId _vocabId = new(SharedGuid);
      readonly SentenceId _sentenceId = new(SharedGuid);

      [XF] public void they_are_not_equal() => _vocabId.Equals(_sentenceId).Must().BeFalse();
   }

   public class given_a_NoteId_parsed_from_string : When_comparing_NoteIds
   {
      readonly NoteId _original = new(SharedGuid);
      readonly NoteId _parsed = NoteId.Parse(SharedGuid.ToString());

      [XF] public void it_equals_the_original() => _parsed.Equals(_original).Must().BeTrue();
      [XF] public void it_has_the_correct_value() => _parsed.Value.Must().Be(SharedGuid);
   }
}
