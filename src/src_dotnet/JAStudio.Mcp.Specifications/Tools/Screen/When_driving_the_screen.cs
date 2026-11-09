using System;
using System.Collections.Generic;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Mcp.UI;
using ModelContextProtocol.Protocol;
using Xunit;

// ReSharper disable InconsistentNaming

namespace JAStudio.Mcp.Specifications.Tools.Screen;

/// <summary>
/// The server of a JAStudio with a headless UI, as the headless JAStudio's is, with a window open on its screen: <see cref="WindowToDrive"/>, and nothing<br/>
/// else, though a specification that failed as it set up, which then isn't disposed, may have left a window open.
/// </summary>
[Collection<SpecificationsUsingTheSharedHeadlessUI>]
public class When_driving_the_screen : McpServerSpecification
{
   protected readonly WindowToDrive Window;

   protected When_driving_the_screen() : base(HeadlessUIOfTheSpecifications.Started) => Window = TheHeadlessUI.Do(() =>
   {
      TheHeadlessUI.Screen.CloseEverything();
      return WindowToDrive.Shown();
   });

   protected static HeadlessUI TheHeadlessUI => HeadlessUIOfTheSpecifications.Started;

   /// <summary>Reads <paramref name="read"/> from the window, on the UI thread it lives on.</summary>
   protected TResult FromTheWindow<TResult>(Func<WindowToDrive, TResult> read) => FromTheUIThread(() => read(Window));

   /// <summary>Reads <paramref name="read"/> on the UI thread, which every window lives on.</summary>
   protected static TResult FromTheUIThread<TResult>(Func<TResult> read) => TheHeadlessUI.OnTheUIThread(read);

   protected string DescribeTheScreen() => TextOf(CallTool("describe_the_screen", []));

   /// <summary>The ref of the control whose line in the screen's description holds <paramref name="kindAndText"/>, such as <c>CheckBox "A check box"</c>.</summary>
   protected string RefOfTheControl(string kindAndText)
   {
      var description = DescribeTheScreen();
      var lines = description.Split('\n').Where(it => it.Contains(kindAndText)).ToList();
      return lines.Count == 1
                ? lines[0].Trim().Split(' ')[0]
                : throw new InvalidOperationException($"{lines.Count} lines of the screen's description hold {kindAndText}, not one. The description:\n{description}");
   }

   protected CallToolResult Click(string kindAndText) => CallTool("click_a_control", new Dictionary<string, object?> { ["control"] = RefOfTheControl(kindAndText) });
   protected CallToolResult TypeText(string text) => CallTool("type_text", new Dictionary<string, object?> { ["text"] = text });
   protected CallToolResult PressKeys(string keys) => CallTool("press_keys", new Dictionary<string, object?> { ["keys"] = keys });

   public override void Dispose()
   {
      TheHeadlessUI.OnTheUIThread(TheHeadlessUI.Screen.CloseEverything);
      base.Dispose();
      GC.SuppressFinalize(this);
   }

   public class describing_the_screen : When_driving_the_screen
   {
      readonly string _description;

      public describing_the_screen() => _description = DescribeTheScreen();

      [XF] public void it_lists_the_window_by_its_title() => _description.Must().Contain($"Window \"{WindowToDrive.TheTitle}\"");
      [XF] public void it_lists_the_check_box_unchecked() => _description.Must().Contain("CheckBox \"A check box\" unchecked");
      [XF] public void it_lists_the_text_box_empty() => _description.Must().Contain("TextBox empty");
      [XF] public void it_lists_the_button_by_its_text() => _description.Must().Contain("Button \"Count the clicks\"");
      [XF] public void it_doesnt_list_the_buttons_text_again_as_a_label_in_it() => _description.Split("\"Count the clicks\"").Length.Must().Be(2);
   }

   public class describing_only_the_auto_complete_box : When_driving_the_screen
   {
      readonly string _description;

      public describing_only_the_auto_complete_box() =>
         _description = TextOf(CallTool("describe_the_screen", new Dictionary<string, object?> { ["part"] = RefOfTheControl("AutoCompleteBox") }));

      [XF] public void it_lists_the_auto_complete_box_first() => _description.Must().StartWith(RefOfTheControl("AutoCompleteBox") + " AutoCompleteBox");
      [XF] public void it_lists_the_text_box_in_it() => _description.Must().Contain("placeholder \"Type a fruit\"");
      [XF] public void it_lists_nothing_else_such_as_the_check_box() => _description.Must().NotContain("CheckBox");
   }

   public class clicking_the_check_box : When_driving_the_screen
   {
      readonly string _checkBoxRef;
      readonly CallToolResult _result;

      public clicking_the_check_box()
      {
         _checkBoxRef = RefOfTheControl("CheckBox \"A check box\"");
         _result = CallTool("click_a_control", new Dictionary<string, object?> { ["control"] = _checkBoxRef });
      }

      [XF] public void the_check_box_is_checked() => FromTheWindow(it => it.CheckBox.IsChecked).Must().Be(true);
      [XF] public void the_description_lists_it_checked() => DescribeTheScreen().Must().Contain("CheckBox \"A check box\" checked");
      [XF] public void the_text_starts_by_naming_the_check_box_clicked() => TextOf(_result).Must().StartWith($"Clicked {_checkBoxRef} CheckBox \"A check box\".");
      [XF] public void the_text_lists_the_window() => TextOf(_result).Must().Contain($"Window \"{WindowToDrive.TheTitle}\"");
      [XF] public void it_gives_the_screens_image() => _result.Content.OfType<ImageContentBlock>().Count().Must().Be(1);
   }

   public class clicking_the_button : When_driving_the_screen
   {
      public clicking_the_button() => Click("Button \"Count the clicks\"");

      [XF] public void the_button_counts_one_click() => FromTheWindow(it => it.Clicks).Must().Be(1);
   }

   public class clicking_the_text_box_and_typing_Japanese : When_driving_the_screen
   {
      public clicking_the_text_box_and_typing_Japanese()
      {
         Click("TextBox empty at");
         TypeText("要する");
      }

      [XF] public void the_text_box_holds_the_text() => FromTheWindow(it => it.TextBox.Text ?? "").Must().Be("要する");
      [XF] public void the_description_lists_it_holding_the_text_and_having_the_keyboard() => DescribeTheScreen().Must().Contain("TextBox value \"要する\", has the keyboard");

      public class and_pressing_the_keys_a_and_Shift_B : clicking_the_text_box_and_typing_Japanese
      {
         public and_pressing_the_keys_a_and_Shift_B() => PressKeys("A Shift+B");

         [XF] public void the_keys_type_their_letters_after_the_text_as_a_keyboards_do() => FromTheWindow(it => it.TextBox.Text ?? "").Must().Be("要するaB");
      }

      public class and_pressing_backspace : clicking_the_text_box_and_typing_Japanese
      {
         public and_pressing_backspace() => PressKeys("Back");

         [XF] public void the_last_character_is_deleted() => FromTheWindow(it => it.TextBox.Text ?? "").Must().Be("要す");
      }
   }

   public class pressing_a_key_named_as_no_key_is : When_driving_the_screen
   {
      readonly CallToolResult _result;

      public pressing_a_key_named_as_no_key_is() => _result = PressKeys("Ctrl+Nothing");

      [XF] public void the_tool_call_fails() => _result.IsError.Must().Be(true);
      [XF] public void the_error_says_the_name_names_no_key() => TextOf(_result).Must().Contain("\"Nothing\" in \"Ctrl+Nothing\" names no key");
   }

   public class clicking_the_combo_box : When_driving_the_screen
   {
      readonly CallToolResult _result;

      public clicking_the_combo_box() => _result = Click("ComboBox");

      [XF] public void the_screen_shows_its_drop_down_as_a_popup() => TextOf(_result).Must().Contain(" Popup at ");
      [XF] public void the_description_lists_the_drop_downs_items() => DescribeTheScreen().Must().Contain("ComboBoxItem \"Second\"");

      public class and_clicking_its_second_item : clicking_the_combo_box
      {
         readonly CallToolResult _itemClicked;

         public and_clicking_its_second_item() => _itemClicked = Click("ComboBoxItem \"Second\"");

         [XF] public void the_combo_box_has_it_selected() => FromTheWindow(it => it.ComboBox.SelectedItem).Must().Be("Second");
         [XF] public void the_drop_down_is_closed() => TextOf(_itemClicked).Must().NotContain(" Popup at ");
      }
   }

   public class clicking_the_auto_complete_box_and_typing_the_start_of_two_of_its_suggestions : When_driving_the_screen
   {
      readonly CallToolResult _result;

      public clicking_the_auto_complete_box_and_typing_the_start_of_two_of_its_suggestions()
      {
         Click("TextBox empty, placeholder \"Type a fruit\"");
         _result = TypeText("ap");
      }

      [XF] public void the_screen_shows_the_suggestions_as_a_popup() => TextOf(_result).Must().Contain(" Popup at ");
      [XF] public void the_suggestions_stay_open() => FromTheWindow(it => it.AutoCompleteBox.IsDropDownOpen).Must().BeTrue();
      [XF] public void the_description_lists_the_suggestions() => DescribeTheScreen().Must().Contain("\"apricot\"");

      [XF] public void its_text_box_keeps_the_keyboard_since_a_desktop_shows_a_popup_without_activating_it() =>
         DescribeTheScreen().Must().Contain("TextBox value \"ap\", has the keyboard");
   }

   public class with_a_second_window_opened_over_it : When_driving_the_screen
   {
      protected const string TheSecondTitle = "A second window to drive";
      protected readonly WindowToDrive SecondWindow;

      public with_a_second_window_opened_over_it() => SecondWindow = TheHeadlessUI.Do(() => WindowToDrive.Shown(TheSecondTitle));

      [XF] public void the_second_window_is_active() => FromTheUIThread(() => SecondWindow.IsActive).Must().Be(true);
      [XF] public void the_first_window_is_no_longer_active() => FromTheWindow(it => it.IsActive).Must().Be(false);

      public class and_clicking_the_first_windows_button : with_a_second_window_opened_over_it
      {
         readonly CallToolResult _result;

         public and_clicking_the_first_windows_button()
         {
            var firstWindowsButton = FromTheUIThread(() => TheHeadlessUI.Screen.Refs.RefOf(Window.Button).Text);
            _result = CallTool("click_a_control", new Dictionary<string, object?> { ["control"] = firstWindowsButton });
         }

         [XF] public void the_tool_call_fails() => _result.IsError.Must().Be(true);
         [XF] public void the_error_says_the_button_is_behind_the_second_window() => TextOf(_result).Must().Contain("is behind");
         [XF] public void the_first_windows_button_counts_no_click() => FromTheWindow(it => it.Clicks).Must().Be(0);
      }

      public class and_closing_the_second_window : with_a_second_window_opened_over_it
      {
         public and_closing_the_second_window() => CallTool("close_a_window", new Dictionary<string, object?> { ["window"] = RefOfTheControl($"Window \"{TheSecondTitle}\"") });

         [XF] public void the_first_window_is_active_again() => FromTheWindow(it => it.IsActive).Must().Be(true);
      }
   }

   public class closing_the_window : When_driving_the_screen
   {
      readonly CallToolResult _result;

      public closing_the_window() => _result = CallTool("close_a_window", new Dictionary<string, object?> { ["window"] = RefOfTheControl($"Window \"{WindowToDrive.TheTitle}\"") });

      [XF] public void the_window_is_closed() => FromTheWindow(it => it.IsVisible).Must().Be(false);
      [XF] public void the_text_says_nothing_is_open() => TextOf(_result).Must().Contain("Nothing is open on the screen");
   }
}
