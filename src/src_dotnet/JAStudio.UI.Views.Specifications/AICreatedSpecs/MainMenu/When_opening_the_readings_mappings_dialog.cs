using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.UI.WindowPlaces;

// ReSharper disable InconsistentNaming

namespace JAStudio.UI.Views.Specifications.AICreatedSpecs.MainMenu;

/// <summary>
/// The readings mappings dialog, opened as from the main menu's Readings mappings, and rendered. A view that doesn't load, such as one using a resource it doesn't define,<br/>
/// throws when the window opens, so it fails here rather than in Anki.
/// </summary>
public class When_opening_the_readings_mappings_dialog : SpecificationOpeningWindows, IAIGeneratedSpec
{
   readonly ReadingsMappingsDialog _dialog;

   public When_opening_the_readings_mappings_dialog()
   {
      _dialog = Open(() => new ReadingsMappingsDialog(Services).ShowWhereItWasLast(Services));
      Render(_dialog, "ReadingsMappingsDialog");
   }

   [XF] public void the_dialog_is_shown() => OnTheUIThread(() => _dialog.IsVisible).Must().BeTrue();
}
