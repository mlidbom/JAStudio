using System;
using JAStudio.Core.UI.Web;
using Microsoft.AspNetCore.Components;

namespace JAStudio.Web.Components.Layout;

/// <summary>
/// The layout of the pages that show a note: the page of one of its cards, which Anki shows in an iframe, and the note's own page.<br/>
/// A card's page in Anki reloads itself after each change that outdates it (<see cref="ChangesThatOutdateNotePages"/>), so the card under review<br/>
/// shows its note as it is now without Anki being asked to draw the card again.
/// </summary>
/// <remarks>
/// A note's own page isn't reloaded here: in JAStudio's workspace its tab reloads it (<c>NoteTabViewModel.PageOutdated</c>), which leaves a<br/>
/// hidden tab alone until it's shown.
/// </remarks>
public partial class CardLayout : IDisposable
{
   [Inject] public NavigationManager Navigation { get; set; } = null!;
   [Inject] public ChangesThatOutdateNotePages ChangesThatOutdateNotePages { get; set; } = null!;

   IDisposable? _subscriptionToTheChangesThatOutdateThePage;

   /// <summary>Starts following the changes once the page is shown in the browser: <see cref="OnAfterRender"/> isn't called while the server only prerenders the page.</summary>
   protected override void OnAfterRender(bool firstRender)
   {
      if(!firstRender) return;

      var page = new Uri(Navigation.Uri);
      if(CardServerUrl.IsACardsPageInAnki(page) && CardServerUrl.NoteIdOfPage(page) is {} shownNote)
         _subscriptionToTheChangesThatOutdateThePage = ChangesThatOutdateNotePages.Subscribe(shownNote, ReloadThePage);
   }

   /// <summary>Has the browser load the page again. Called on the thread that made the change, so the reload is handed to the page's own thread.</summary>
   void ReloadThePage()
   {
      // One reload shows every change made until the page has loaded again, and the page loaded then follows the changes after it.
      _subscriptionToTheChangesThatOutdateThePage?.Dispose();
      _ = InvokeAsync(() => Navigation.Refresh(forceReload: true));
   }

   public void Dispose() => _subscriptionToTheChangesThatOutdateThePage?.Dispose();
}
