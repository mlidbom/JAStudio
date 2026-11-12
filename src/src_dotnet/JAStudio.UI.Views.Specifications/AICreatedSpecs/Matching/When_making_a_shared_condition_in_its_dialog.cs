using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.ConditionTrees;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Editing;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Texts;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Tokens;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Words;
using JAStudio.UI.ViewModels.Workspace.Editors.Matching;
using JAStudio.UI.Views.Workspace.Editors.Matching;
using static JAStudio.Core.Note.Vocabulary.Matching.Conditions.Words.WordConditions;

// ReSharper disable InconsistentNaming

namespace JAStudio.UI.Views.Specifications.AICreatedSpecs.Matching;

/// <summary>
/// The dialog that makes a shared-condition of the line <c>!PrecedingToken.Surface.EndsWithOneOf("ん")</c> of a tree, shown over a window, with the name<br/>
/// <c>EndsInN</c> typed. The specification renders the dialog.
/// </summary>
public class When_making_a_shared_condition_in_its_dialog : SpecificationOpeningWindows, IAIGeneratedSpec
{
   readonly NewSharedConditionDialogViewModel _dialog;
   readonly Window _owner;

   public When_making_a_shared_condition_in_its_dialog()
   {
      var tree = new EditableCondition(ConditionGroup.AllOf(!PrecedingToken.Meets(TokenConditions.Surface.Meets(TextConditions.EndsWithOneOf("ん")))), WordConditions.Level);
      _dialog = new NewSharedConditionDialogViewModel(tree.ConditionsToMakeASharedConditionOfAt(ConditionPosition.Root.Child(0)), GetService<SharedConditions>())
                {
                   TypedName = "EndsInN"
                };
      _owner = Open(() =>
      {
         var window = new Window { Width = 800, Height = 600 };
         window.Show();
         return window;
      });
      OnTheUIThread(() => _ = NewSharedConditionDialog.ShowAsync(_owner, _dialog, new PixelPoint(0, 0)));
      Render(TheDialog, "NewSharedConditionDialog");
   }

   Window TheDialog => OnTheUIThread(() => _owner.OwnedWindows.Single());

   [XF] public void the_dialog_is_shown() => OnTheUIThread(() => TheDialog.IsVisible).Must().BeTrue();

   [XF] public void the_typed_name_can_make_one() => _dialog.CanMake.Must().BeTrue();
}
