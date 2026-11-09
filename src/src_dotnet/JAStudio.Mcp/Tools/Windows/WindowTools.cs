using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using JAStudio.Core;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Mcp.UI;
using JAStudio.UI.Menus;
using JAStudio.UI.Views.Workspace;
using JAStudio.UI.Views.Workspace.Editors;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace JAStudio.Mcp.Tools.Windows;

/// <summary>
/// The tools that show Claude JAStudio's windows: they open them in the headless JAStudio's <see cref="HeadlessUI"/>, on the real corpus, and<br/>
/// render them. Only a JAStudio with a headless UI offers them: in the live JAStudio, a window opened would open on Magnus's screen.
/// </summary>
[McpServerToolType]
public class WindowTools
{
   /// <summary>
   /// The windows <see cref="RenderWindow"/> renders. Not <see cref="MainMenuWindows.TextToSpeechVoices"/>: opening it lists the voices of the<br/>
   /// text-to-speech services online, with Magnus's keys, and opens an audio player.
   /// </summary>
   static readonly MainMenuWindow[] WindowsRendered = [MainMenuWindows.Options, MainMenuWindows.ReadingsMappings, MainMenuWindows.ImportProfiles];

   readonly HeadlessUI _headlessUI;
   readonly TemporaryServiceCollection _services;
   readonly JPCollection _collection;

   public WindowTools(HeadlessUI headlessUI, TemporaryServiceCollection services, JPCollection collection)
   {
      _headlessUI = headlessUI;
      _services = services;
      _collection = collection;
   }

   [McpServerTool(Name = "render_window", ReadOnly = true, Idempotent = true)]
   [Description("""
                Renders one of the windows of their own that the workspace's main menu opens: opens it in the headless JAStudio as the menu
                does, with the corpus and the add-on's configuration, draws it as on a screen, closes it, and gives its image. The windows:
                "Options", "Readings mappings" and "Import profiles".
                """)]
   public IEnumerable<ContentBlock> RenderWindow([Description("The window's name, as the main menu names it, such as \"Options\".")] string window)
   {
      var windowToRender = WindowsRendered.SingleOrDefault(it => string.Equals(it.Name, window, StringComparison.OrdinalIgnoreCase))
                        ?? throw new ArgumentException($"There is no window named '{window}' to render. The windows are: {string.Join(", ", WindowsRendered.Select(it => $"\"{it.Name}\""))}.", nameof(window));

      return ImageResult.Of(windowToRender.Name, _headlessUI.Render(() => windowToRender.Open(_services)));
   }

   [McpServerTool(Name = "render_the_workspace", ReadOnly = true)]
   [Description("""
                Renders the headless JAStudio's workspace window as it is: its navigation, its editor and its tabs, the shown tab's page rendered by a
                headless browser. Given a note, it shows the note first, in a tab of its own as a click on a note link does, or in the editor. The
                workspace keeps its tabs from one call to the next, as the workspace does, and starts as Magnus last left his, from a copy of his
                workspace's saved state. The editor shows its first tab, such as a vocab's "Text", unless told which.
                """)]
   public IEnumerable<ContentBlock> RenderTheWorkspace(
      [Description("The id of a note to show first, a GUID, as search_notes lists it. None to render the workspace as it is.")] string? noteId = null,
      [Description("Whether to show the note in the editor, as editing it does, rather than its page in a tab.")] bool inTheEditor = false,
      [Description("The tab of the editor to show, by its header, such as \"Matching\" or \"Metadata\" for a vocab. The editor's first tab if none.")] string? editorTab = null)
   {
      var note = noteId == null ? null : NoteWithTheId(noteId);
      return ImageResult.Of(note == null ? "The workspace" : $"The workspace showing {note.GetQuestion()}", _headlessUI.Render(() =>
      {
         if(note != null) ShowTheNote(note);
         return WorkspaceWindow.Shown();
      }, workspace =>
      {
         if(editorTab != null) ShowTheEditorsTab(workspace, editorTab);
      }));

      void ShowTheNote(JPNote noteToShow)
      {
         if(inTheEditor) WorkspaceWindow.EditNote(noteToShow);
         else WorkspaceWindow.OpenNote(noteToShow);
      }

      JPNote NoteWithTheId(string id) => _collection.NoteFromNoteId(NoteId.Parse(id)) ?? throw new ArgumentException($"No note has the id {id}.", nameof(noteId));
   }

   /// <summary>Selects the tab of the <paramref name="workspace"/>'s editor whose header is <paramref name="header"/>, or, when the editor has none, fails with those it has.</summary>
   static void ShowTheEditorsTab(Window workspace, string header)
   {
      var editor = workspace.GetLogicalDescendants().OfType<EditorSection>().SingleOrDefault()
                ?? throw new ArgumentException("The workspace shows no editor: give a note to show in it.", nameof(header));
      var tabs = editor.GetLogicalDescendants().OfType<TabItem>().ToList();
      var tab = tabs.SingleOrDefault(it => it.Header as string == header)
             ?? throw new ArgumentException($"The editor has no tab \"{header}\". Its tabs are: {string.Join(", ", tabs.Select(it => $"\"{it.Header}\""))}.", nameof(header));
      tab.IsSelected = true;
   }
}
