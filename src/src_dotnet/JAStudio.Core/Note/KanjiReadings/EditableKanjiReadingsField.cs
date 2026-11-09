using System.Collections.Generic;
using JAStudio.Core.Note.EditableFields;

namespace JAStudio.Core.Note.KanjiReadings;

/// <summary>
/// A kanji's readings of one <see cref="KanjiReadingKind"/> (<see cref="KanjiReadingList"/>), in order, each primary or secondary (<see cref="KanjiReading"/>).<br/>
/// On readings are kept in hiragana, however they're written.
/// </summary>
public sealed class EditableKanjiReadingsField : EditableField<KanjiNote, IReadOnlyList<KanjiReading>>
{
   readonly KanjiReadingKind _kind;

   /// <param name="name">The field's <see cref="IEditableField{TNote}.Name"/>: its path in the kanji as the corpus stores it.</param>
   /// <param name="kind">Which of the kanji's readings it is.</param>
   public EditableKanjiReadingsField(string name, KanjiReadingKind kind) : base(name, kind.ToString(), changesHowSentencesParse: false) => _kind = kind;

   /// <summary>Finds the readings on a kanji, for the editor's list, which reads and writes them there.</summary>
   public KanjiReadingList ReadingsOn(KanjiNote kanji) => kanji.Readings.OfKind(_kind);

   public override IReadOnlyList<KanjiReading> Read(KanjiNote note) => ReadingsOn(note).All();
   public override void Write(KanjiNote note, IReadOnlyList<KanjiReading> value) => ReadingsOn(note).ReplaceWith(value);
}
