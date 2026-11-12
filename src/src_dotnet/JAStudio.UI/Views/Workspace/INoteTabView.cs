namespace JAStudio.UI.Views.Workspace;

/// <summary>
/// The view of one workspace tab, the one the workspace window shows the tab's page in: <see cref="NoteTabView"/>, in a web view, or<br/>
/// <see cref="NoteTabImageView"/>, as an image, where there is no screen to show a web view on, as in the headless JAStudio.
/// </summary>
interface INoteTabView
{
   /// <summary>Gives the keyboard to the page, for its keys to act on the note being shown, such as the note view shortcuts.</summary>
   void FocusPage();
}
