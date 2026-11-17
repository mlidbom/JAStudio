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
/// The view models follow a change once its tevents are observed, after it has committed, on a thread of the observation's: the UI thread<br/>
/// dispatcher runs what's posted to it there at once. <see cref="Workspace"/> waits for that, as a person looks at the workspace once it has caught up.
/// </remarks>
public abstract class SpecificationUsingAWorkspace : SpecificationStartingWithAnEmptyCollection
{
   /// <summary>The CardServer doesn't run in these specifications. The workspace needs its address only to build the URLs of the note pages.</summary>
   const string AddressOfACardServerThatIsNotRunning = "http://localhost";

   protected SpecificationUsingAWorkspace()
   {
      CardServerUrl.BaseUrl = AddressOfACardServerThatIsNotRunning;
      _workspace = new WorkspaceViewModel(GetService<TemporaryServiceCollection>());
   }

   readonly WorkspaceViewModel _workspace;

   /// <summary>The workspace, once it has caught up (<see cref="OnceCaughtUp{TViewModel}"/>).</summary>
   private protected WorkspaceViewModel Workspace => OnceCaughtUp(_workspace);

   /// <summary><paramref name="viewModel"/>, once every tevent published so far has been observed: it shows every change made so far.</summary>
   private protected TViewModel OnceCaughtUp<TViewModel>(TViewModel viewModel)
   {
      WaitUntilEveryTeventPublishedHasBeenObserved();
      return viewModel;
   }

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
