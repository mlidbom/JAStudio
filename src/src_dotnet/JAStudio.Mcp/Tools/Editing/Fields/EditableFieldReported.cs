using System.Text.Json.Nodes;

namespace JAStudio.Mcp.Tools.Editing.Fields;

/// <summary>One of a note's editable fields, as <see cref="NoteEditingTools.GetTheEditableFieldsOfANote"/> reports it.</summary>
/// <param name="Name">The name <c>edit_a_note</c> takes: the field's path in the note as the corpus stores it.</param>
/// <param name="Label">What the note's editor in the workspace shows beside the field.</param>
/// <param name="Takes">What the field's value is, in JSON, such as "a list of texts".</param>
/// <param name="ChangesHowSentencesParse">Whether a change to the field reparses the sentences that may contain the vocab.</param>
/// <param name="Value">The field's value now.</param>
public record EditableFieldReported(string Name, string Label, string Takes, bool ChangesHowSentencesParse, JsonNode? Value)
{
   public static EditableFieldReported Of(EditableFieldOfANote field) =>
      new(field.Name, field.Label, FieldValueJson.DescriptionOf(field.ValueType), field.ChangesHowSentencesParse, FieldValueJson.JsonOf(field.Read()));
}
