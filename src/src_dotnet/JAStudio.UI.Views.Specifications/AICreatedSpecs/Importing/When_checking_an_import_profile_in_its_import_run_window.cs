using System;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Importing.Checking;
using JAStudio.UI.ViewModels.Importing;
using JAStudio.UI.Views.Importing;

// ReSharper disable InconsistentNaming

namespace JAStudio.UI.Views.Specifications.AICreatedSpecs.Importing;

/// <summary>
/// Three raw notes in the pretend Anki, two with their audio in Anki's media folder and one whose audio is missing, and the profile checked in its<br/>
/// import run window, opened as the import profiles window's Check opens it. The specification waits for the check to finish, and renders the window.
/// </summary>
public class When_checking_an_import_profile_in_its_import_run_window : ImportWindowsSpecification, IAIGeneratedSpec
{
   readonly ImportRunWindow _window;

   public When_checking_an_import_profile_in_its_import_run_window()
   {
      AddASentenceCard("今日はいい天気ですね。", "anime_example_000000001", "example_000000001.mp3");
      AddASentenceCard("明日も晴れるといいな。", "anime_example_000000002", "example_000000002.mp3");
      AddASentenceCardWithoutItsAudioFile("雨が降りそうだ。", "anime_example_000000003", "example_000000003.mp3");

      _window = Open(() => ImportRunWindow.CheckInItsWindow(Profile, Services));
      WaitUntil(() => !TheWindowsViewModel().IsBusy);
      Render(_window, "ImportRunWindow");
   }

   ImportRunWindowViewModel TheWindowsViewModel() => ViewModelOf<ImportRunWindowViewModel>(_window);

   RawNoteOutcome[] TheReportedOutcomes() => OnTheUIThread(() => TheWindowsViewModel().Rows.Select(it => it.Outcome).ToArray());

   bool TheColumnIsShown(string header) => OnTheUIThread(() => _window.ReportGrid.Columns.Single(it => Equals(it.Header, header)).IsVisible);

   [XF] public void the_report_has_a_row_per_raw_note() => TheReportedOutcomes().Length.Must().Be(3);
   [XF] public void the_raw_note_whose_audio_is_missing_is_a_problem() => TheReportedOutcomes().Count(it => it == RawNoteOutcome.Problem).Must().Be(1);
   [XF] public void the_column_of_the_audio_the_profile_maps_is_shown() => TheColumnIsShown("Audio").Must().BeTrue();
   [XF] public void the_column_of_the_screenshot_the_profile_does_not_map_is_hidden() => TheColumnIsShown("Screenshot").Must().BeFalse();
   [XF] public void the_window_is_titled_with_the_profile() => OnTheUIThread(() => _window.Title ?? throw new InvalidOperationException("The window has no title.")).Must().Be("Import run: Immersion Kit");
}
