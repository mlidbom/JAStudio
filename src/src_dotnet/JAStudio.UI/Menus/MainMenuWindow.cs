using System;
using Avalonia.Controls;
using JAStudio.Core;

namespace JAStudio.UI.Menus;

/// <summary>A window of its own that the workspace's main menu opens, such as the options: its name in the menu, and how the menu opens it.</summary>
/// <param name="Open">Opens the window, as the menu does, where it was last, and returns it. Called on the UI thread.</param>
public record MainMenuWindow(string Name, Func<TemporaryServiceCollection, Window> Open);
