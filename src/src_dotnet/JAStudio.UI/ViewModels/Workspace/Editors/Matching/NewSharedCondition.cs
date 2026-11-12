using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing;
using JAStudio.UI.Views.Workspace.Editors.Matching;

namespace JAStudio.UI.ViewModels.Workspace.Editors.Matching;

/// <summary>A shared-condition to make, as the <see cref="NewSharedConditionDialog"/> closes with it: its definition, on its subject, and its name.</summary>
/// <param name="Definition">The condition it's made of, with the level of the subject it's on.</param>
/// <param name="Name">Its name, such as <c>FollowsAPastTenseStem</c>, without its subject's in front.</param>
sealed record NewSharedCondition(ConditionWithItsLevel Definition, string Name);
