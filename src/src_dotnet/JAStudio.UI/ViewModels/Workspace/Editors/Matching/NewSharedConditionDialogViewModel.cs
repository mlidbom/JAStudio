using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Compze.Contracts;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing;
using JAStudio.UI.Views.Workspace.Editors.Matching;

namespace JAStudio.UI.ViewModels.Workspace.Editors.Matching;

/// <summary>
/// The small modal dialog that makes a shared-condition of a line of the tree editor (<see cref="NewSharedConditionDialog"/>), as an IDE extracts a method:<br/>
/// which of the line's conditions it's made of, on its subject, and its name. It says why the typed name can't be used, and that the shared-condition is<br/>
/// then used wherever its definition is.
/// </summary>
sealed partial class NewSharedConditionDialogViewModel : ObservableObject
{
   readonly SharedConditions _sharedConditions;

   /// <param name="definitions">The conditions on the line a shared-condition can be made of, from the outermost in: at least one.</param>
   /// <param name="sharedConditions">The corpus's shared-conditions, whose names a new one can't have.</param>
   public NewSharedConditionDialogViewModel(IReadOnlyList<ConditionWithItsLevel> definitions, SharedConditions sharedConditions)
   {
      Contract.Argument.Assert(definitions.Count > 0, () => "A shared-condition is made of one of a line's conditions, and this line has none to make one of.");
      _sharedConditions = sharedConditions;
      Definitions = [.. definitions.Select(it => new OfferedDefinitionViewModel(it))];
      Chosen = Definitions[0];
   }

   public string Title => "Make a shared-condition";

   /// <summary>The conditions on the line it can be made of, from the outermost in, the first chosen to start with.</summary>
   public IReadOnlyList<OfferedDefinitionViewModel> Definitions { get; }

   /// <summary>The condition it's made of.</summary>
   [ObservableProperty]
   [NotifyPropertyChangedFor(nameof(SubjectInFront), nameof(WhyItCannotBeMade), nameof(CanMake))]
   public partial OfferedDefinitionViewModel Chosen { get; set; }

   /// <summary>Its name, as typed, without its subject's in front.</summary>
   [ObservableProperty]
   [NotifyPropertyChangedFor(nameof(WhyItCannotBeMade), nameof(CanMake))]
   public partial string TypedName { get; set; } = "";

   /// <summary>Its subject's name as code names it, shown in front of the typed name, such as <c>Word.</c>.</summary>
   public string SubjectInFront => $"{Chosen.Definition.Level.SubjectNameInCode}.";

   /// <summary>What making it does besides.</summary>
   public string WhatItDoes => "It's then used wherever its definition is: in every vocab's match-condition, and every other shared-condition's definition, that holds it. "
                             + "None of them means anything else, so no vocab matches other words.";

   /// <summary>Why the typed name can't be used, or null when it can.</summary>
   public string? WhyItCannotBeMade => TypedName.Length == 0 ? "Type its name." : _sharedConditions.WhyItCannotNameANewOne(Chosen.Definition.Level, TypedName);

   public bool CanMake => WhyItCannotBeMade is null;

   /// <summary>The shared-condition the dialog closes with when it's accepted.</summary>
   public NewSharedCondition ToMake => new(Chosen.Definition, TypedName);

   /// <summary>Makes it. Set by the <see cref="NewSharedConditionDialog"/>, which closes with <see cref="ToMake"/>.</summary>
   public IRelayCommand? AcceptCommand { get; set; }

   /// <summary>Gives up on it. Set by the <see cref="NewSharedConditionDialog"/>, which closes without one.</summary>
   public IRelayCommand? CancelCommand { get; set; }
}
