using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.UI.WindowPlaces;

// ReSharper disable InconsistentNaming

namespace JAStudio.UI.Views.Specifications.AICreatedSpecs.WindowPlaces;

/// <summary>
/// A window whose place a <see cref="WindowPlaceKeeper"/> keeps, as the workspace window's is, shown, left by the user, then shown where it was,<br/>
/// as the workspace window is when it's brought to the front, such as when a note is opened in it from Anki.<br/>
/// The headless platform keeps nothing for a minimized window to come back to, as Windows does: it comes back as it was only if the keeper brings it back so.
/// </summary>
public class When_a_window_is_shown_where_it_was : SpecificationOpeningWindows, IAIGeneratedSpec
{
   /// <summary>The window's state, and where it is.</summary>
   private protected record WhereTheWindowIs(WindowState State, PixelPoint TopLeft, Size ClientSize);

   private protected readonly Window TheWindow;
   readonly WindowPlaceKeeper _keeper;

   public When_a_window_is_shown_where_it_was()
   {
      TheWindow = Open(() => new Window());
      _keeper = OnTheUIThread(() => WindowPlaceKeeper.KeepThePlaceOf(TheWindow, Services));
      OnTheUIThread(() =>
      {
         TheWindow.Show();
         Dispatcher.UIThread.RunJobs();
      });
   }

   private protected void ShowItWhereItWas() => OnTheUIThread(() =>
   {
      _keeper.ShowItWhereItWas();
      Dispatcher.UIThread.RunJobs();
   });

   private protected WhereTheWindowIs WhereItIs() => OnTheUIThread(() => new WhereTheWindowIs(TheWindow.WindowState, TheWindow.Position, TheWindow.ClientSize));

   public class after_being_maximized_then_minimized : When_a_window_is_shown_where_it_was
   {
      public after_being_maximized_then_minimized()
      {
         OnTheUIThread(() =>
         {
            AsWindowsDoes.Maximize(TheWindow);
            AsWindowsDoes.Minimize(TheWindow);
         });
         ShowItWhereItWas();
      }

      [XF] public void it_is_maximized() => WhereItIs().State.Must().Be(WindowState.Maximized);
   }

   public class after_being_snapped_then_minimized : When_a_window_is_shown_where_it_was
   {
      public after_being_snapped_then_minimized()
      {
         OnTheUIThread(() =>
         {
            AsWindowsDoes.SnapToTheRightHalf(TheWindow);
            AsWindowsDoes.Minimize(TheWindow);
         });
         ShowItWhereItWas();
      }

      [XF] public void it_is_where_it_was_snapped_in_its_normal_state() =>
         WhereItIs().Must().Be(new WhereTheWindowIs(WindowState.Normal, AsWindowsDoes.SnappedToTheRightHalf.TopLeft, AsWindowsDoes.SnappedToTheRightHalf.ClientSize));
   }

   public class while_it_shows_where_it_was : When_a_window_is_shown_where_it_was
   {
      readonly List<PixelPoint> _movesWhileShownWhereItWas = [];

      public while_it_shows_where_it_was()
      {
         OnTheUIThread(() =>
         {
            TheWindow.Position = new PixelPoint(300, 200);
            TheWindow.Width = 900;
            TheWindow.Height = 500;
            Dispatcher.UIThread.RunJobs();
            TheWindow.PositionChanged += (_, moved) => _movesWhileShownWhereItWas.Add(moved.Point);
         });
         ShowItWhereItWas();
      }

      [XF] public void it_is_not_moved() => _movesWhileShownWhereItWas.Must().BeEmpty();
   }
}
