using System;
using JAStudio.Core;
using JAStudio.Core.Note;
using JAStudio.Core.Specifications;
using JAStudio.Core.UI.Web;
using JAStudio.UI.ViewModels.Workspace;
using JAStudio.UI.ViewModels.Workspace.Editors;

namespace JAStudio.UI.Specifications;

/// <summary>
/// A specification of the workspace: the application's real services with an empty collection, as for
/// <see cref="SpecificationStartingWithAnEmptyCollection"/>, and a <see cref="WorkspaceViewModel"/> to open its notes in.
/// </summary>
/// <remarks>
/// There's no window and no web view: the specifications drive the view models, as the views do.<br/>
/// The UI thread dispatcher runs what's posted to it at once, so a refresh the app would post runs inside the change that caused it.
/// </remarks>
public abstract class SpecificationUsingAWorkspace : SpecificationStartingWithAnEmptyCollection
{
   /// <summary>The CardServer doesn't run in these specifications. The workspace needs its address only to build the URLs of the note pages.</summary>
   const string AddressOfACardServerThatIsNotRunning = "http://localhost";

   protected SpecificationUsingAWorkspace()
   {
      CardServerUrl.BaseUrl = AddressOfACardServerThatIsNotRunning;
      Workspace = new WorkspaceViewModel(GetService<TemporaryServiceCollection>());
   }

   private protected WorkspaceViewModel Workspace { get; }

   /// <summary>
   /// Opens <paramref name="note"/> in the workspace with the editor section open, as "Edit" in its note menu does,<br/>
   /// and returns the editors the section shows for it.
   /// </summary>
   private protected TEditorSection Edit<TEditorSection>(JPNote note) where TEditorSection : INoteEditorSectionViewModel
   {
      Workspace.EditNote(note);
      return Workspace.SelectedTab?.EditorSection is TEditorSection editorSection
                ? editorSection
                : throw new InvalidOperationException($"The editor section shows no {typeof(TEditorSection).Name} for the note.");
   }
}
