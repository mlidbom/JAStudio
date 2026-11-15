using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JAStudio.Core.Note;
using JAStudio.Core.UI.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace JAStudio.Web.Components.Layout;

/// <summary>
/// The right-click menu of a page that shows a note, wherever the page isn't shown in JAStudio's workspace, which has a menu of its own: in Anki's<br/>
/// reviewer and previewer, and in a web browser. <c>card.js</c> opens it at the cursor, saying which note link, if any, was clicked, and its<br/>
/// items run in JAStudio, through the page's own connection to the card server. Shift+Right-click keeps the browser's own menu.
/// </summary>
/// <remarks>
/// Anki's own windows had a menu of the note until 2026-11-15, which C# described and the Anki add-on built with Qt. This is JAStudio's own page's,<br/>
/// so it knows what was clicked, and needs neither Qt nor the add-on (Magnus, 2026-11-15: "Let's do a spike to prove the blazor menu works, if so<br/>
/// that is the way to go.").
/// </remarks>
/// <remarks>
/// wip: a spike's items, which prove that the menu works: opening the page's note, or the note of the link clicked, in JAStudio's workspace.<br/>
/// review-naming:menus: RightClickMenu is Claude's (2026-11-15).
/// </remarks>
public partial class RightClickMenu : IDisposable
{
   [Inject] public IJSRuntime JS { get; set; } = null!;
   [Inject] public NavigationManager Navigation { get; set; } = null!;
   [Inject] public INoteLinkOpener NoteLinkOpener { get; set; } = null!;

   DotNetObjectReference<RightClickMenu>? _thisForTheScript;
   PointOnThePage? _shownAt;
   IReadOnlyList<Item> _items = [];

   /// <summary>Makes itself known to <c>card.js</c> once the page is shown in the browser: <see cref="OnAfterRenderAsync"/> isn't called while the server only prerenders the page.</summary>
   protected override async Task OnAfterRenderAsync(bool firstRender)
   {
      if(!firstRender) return;

      _thisForTheScript = DotNetObjectReference.Create(this);
      await JS.InvokeVoidAsync("jaStudioRightClickMenu.register", _thisForTheScript);
   }

   /// <summary>
   /// Opens the menu at (<paramref name="x"/>, <paramref name="y"/>), in CSS pixels from the top left of the page's window, for a right-click on<br/>
   /// the note link to the note with the id <paramref name="noteIdOfTheLinkClicked"/>, whose text is <paramref name="textOfTheLinkClicked"/>, or<br/>
   /// on no link when both are null.
   /// </summary>
   [JSInvokable]
   public void ShowAt(double x, double y, string? noteIdOfTheLinkClicked, string? textOfTheLinkClicked)
   {
      _items = TheItemsFor(noteIdOfTheLinkClicked, textOfTheLinkClicked);
      _shownAt = _items.Count > 0 ? new PointOnThePage(x, y) : null;
      StateHasChanged();
   }

   [JSInvokable]
   public void Close()
   {
      _shownAt = null;
      StateHasChanged();
   }

   List<Item> TheItemsFor(string? noteIdOfTheLinkClicked, string? textOfTheLinkClicked)
   {
      List<Item> items = [];
      if(noteIdOfTheLinkClicked != null)
         items.Add(new Item($"Open {textOfTheLinkClicked} in JAStudio's workspace", () => NoteLinkOpener.OpenInWorkspace(NoteId.Parse(noteIdOfTheLinkClicked))));
      if(CardServerUrl.NoteIdOfPage(new Uri(Navigation.Uri)) is {} theNoteOfThePage)
         items.Add(new Item("Open this note in JAStudio's workspace", () => NoteLinkOpener.OpenInWorkspace(theNoteOfThePage)));
      return items;
   }

   void Run(Item item)
   {
      Close();
      item.Run();
   }

   /// <summary>Lets go of the reference <c>card.js</c> holds, as the page goes: the script goes with the page, so it's told nothing.</summary>
   public void Dispose() => _thisForTheScript?.Dispose();

   /// <summary>A point on the page, in CSS pixels from the top left of the page's window.</summary>
   sealed record PointOnThePage(double X, double Y);

   /// <summary>An item of the menu: its text, and what it does when picked.</summary>
   sealed record Item(string Name, Action Run);
}
