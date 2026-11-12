using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Code;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing;
using JAStudio.UI.Views.Workspace.Editors.Matching;

namespace JAStudio.UI.ViewModels.Workspace.Editors.Matching;

/// <summary>
/// One of the conditions the <see cref="NewSharedConditionDialog"/> offers to make a shared-condition of, as its definition, on its subject: such as<br/>
/// "On a token: Surface.EndsWithOneOf("ん")".
/// </summary>
/// <param name="definition">The condition, with the level of the subject it's on.</param>
sealed class OfferedDefinitionViewModel(ConditionWithItsLevel definition)
{
   /// <summary>The condition, with the level of the subject it's on.</summary>
   public ConditionWithItsLevel Definition { get; } = definition;

   /// <summary>The subject and the condition as code on one line, such as "On a token: Surface.EndsWithOneOf("ん")".</summary>
   public string Text { get; } = $"On {definition.Level.SubjectName}: {ConditionCode.OnOneLine(definition.Condition)}";
}
