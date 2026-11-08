using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.UI.ViewModels.Importing;
using JAStudio.UI.Views.Importing;

// ReSharper disable InconsistentNaming

namespace JAStudio.UI.Views.Specifications.AICreatedSpecs.Importing;

/// <summary>The import profiles window, opened as from the main menu with one saved profile. The specification renders the window.</summary>
public class When_opening_the_import_profiles_window : ImportWindowsSpecification, IAIGeneratedSpec
{
   readonly ImportProfilesWindow _window;

   public When_opening_the_import_profiles_window()
   {
      _window = Open(() => ImportProfilesWindow.OpenNearTheCursor(Services));
      Render(_window, "ImportProfilesWindow");
   }

   [XF] public void the_saved_profile_is_selected_for_editing() =>
      OnTheUIThread(() => ViewModelOf<ImportProfilesWindowViewModel>(_window).SelectedProfile?.Profile).Must().Be(Profile);
}
