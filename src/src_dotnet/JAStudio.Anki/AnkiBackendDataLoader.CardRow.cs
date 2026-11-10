using JAStudio.Anki.CollectionDatabase;
using JAStudio.Core.Note.Collection;

namespace JAStudio.Anki;

public sealed partial class AnkiBackendDataLoader
{
   /// <summary>A card as the load reads it: the id of its Anki note, its ordinal, which says which of its note type's templates it's of, and its queue in Anki.</summary>
   sealed record CardRow(long AnkiNoteId, int Ordinal, int Queue)
   {
      /// <summary>
      /// Every card in the collection, not only the study-notes': leaving the others out would take a join with the notes, which takes longer<br/>
      /// than reading them. So would a join with the templates, for their names: 200 of 285 ms on Magnus's collection on 2026-11-10.
      /// </summary>
      public const string SelectEvery = "select json_group_array(json_object('ankiNoteId', nid, 'ordinal', ord, 'queue', queue)) from cards";

      /// <summary>The queue Anki puts a suspended card in.</summary>
      const int QueueOfASuspendedCard = -1;

      bool IsSuspended => Queue == QueueOfASuspendedCard;

      /// <summary>Its studying status, as a card of a note of <paramref name="noteType"/>, its note's note type.</summary>
      public CardStudyingStatus StudyingStatusAsACardOf(NoteTypeInAnki noteType) => new(AnkiNoteId, noteType.TemplateNameOfTheCardWith(Ordinal), IsSuspended, noteType.Name);
   }
}
