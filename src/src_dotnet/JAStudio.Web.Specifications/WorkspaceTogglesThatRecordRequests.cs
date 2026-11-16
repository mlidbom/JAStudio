using JAStudio.Core.UI.Web;

namespace JAStudio.Web.Specifications;

/// <summary>An <see cref="IWorkspaceToggles"/> for specifications, which toggles nothing and counts what it was asked to toggle.</summary>
class WorkspaceTogglesThatRecordRequests : IWorkspaceToggles
{
   public int TimesTheWorkspaceWindowWasToggled { get; private set; }
   public int TimesTheNoteSearchWasToggled { get; private set; }

   public void ToggleWorkspaceWindow() => TimesTheWorkspaceWindowWasToggled++;
   public void ToggleNoteSearch() => TimesTheNoteSearchWasToggled++;
}
