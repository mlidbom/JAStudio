using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Sentences;
using JAStudio.Mcp.Tools.Editing.Fields;
using JAStudio.Mcp.Tools.ParsingCases;

namespace JAStudio.Mcp.Tools.Editing;

/// <summary>
/// The value of each editable field of a note (<see cref="EditableNote"/>) at one moment, in JSON: what an edit or a command changed in the note is<br/>
/// the difference between its values before and after (<see cref="ChangesTo"/>).
/// </summary>
/// <remarks>
/// A sentence has one value more, <c>parsingCase</c>, which no editable field holds: null when the sentence isn't a parsing case, or else how it<br/>
/// compares with what its case expects (<see cref="ParsingCaseStateReported"/>). So a command that makes it a parsing case, accepts what it parses<br/>
/// to or makes it an ordinary sentence again reports a change, and so does an edit that changes how a parsing case's sentence parses.
/// </remarks>
sealed class NoteFieldValues
{
   readonly IReadOnlyList<FieldValue> _values;

   NoteFieldValues(IReadOnlyList<FieldValue> values) => _values = values;

   /// <summary>The value of each editable field of <paramref name="note"/> now.</summary>
   public static NoteFieldValues Of(EditableNote note) =>
      new([.. note.Fields.Select(it => new FieldValue(it.Name, FieldValueJson.JsonOf(it.Read()))), .. WhatNoEditableFieldHoldsOf(note.Note)]);

   /// <summary>The values of <paramref name="note"/> that an edit or a command can change and no editable field holds: a sentence's <c>parsingCase</c>.</summary>
   static IEnumerable<FieldValue> WhatNoEditableFieldHoldsOf(JPNote note) =>
      note is SentenceNote sentence
         ? [new FieldValue("parsingCase", sentence.ParsingCase != null ? FieldValueJson.JsonOf(ParsingCaseStateReported.Of(sentence)) : null)]
         : [];

   /// <summary>Each field whose value in <paramref name="later"/>, the values of the same note at a later moment, differs from its value here, in the order of the note's fields.</summary>
   public IReadOnlyList<FieldChange> ChangesTo(NoteFieldValues later) =>
      [.. _values.Zip(later._values)
                 .Where(it => !JsonNode.DeepEquals(it.First.Value, it.Second.Value))
                 .Select(it => new FieldChange(it.First.Field, it.First.Value, it.Second.Value))];

   /// <summary>One field's value, in JSON.</summary>
   record FieldValue(string Field, JsonNode? Value);
}
