// Card interaction scripts for Blazor iframe.
// Uses event delegation on `document` so handlers survive Blazor Server DOM re-renders
// without needing re-initialization.

// Right-click on clipboard/highlight elements selects their text for easy copying.
document.addEventListener('mousedown', function (event) {
   if (event.button !== 2) return;

   const element = event.target.closest('.clipboard, ja, .headword-term, rad, radical, voc, vocab, kan, kanji, read, reading');
   if (!element) return;

   const selection = window.getSelection();
   const selectedText = selection.toString();
   if (!selectedText || !element.contains(selection.anchorNode)) {
      const range = document.createRange();
      range.selectNodeContents(element);
      selection.removeAllRanges();
      selection.addRange(range);
   }
});

// Play/pause audio via the play-button span next to <audio> elements.
document.addEventListener('click', function (e) {
   const button = e.target.closest('.play-button');
   if (!button) return;

   e.preventDefault();
   e.stopPropagation();
   const audio = button.previousElementSibling;
   if (!audio || audio.tagName !== 'AUDIO') return;

   if (audio.paused) {
      audio.play();
      button.textContent = '⏸︎';
   } else {
      audio.pause();
      button.textContent = '▶';
   }
});

// Reset play button text when audio finishes.
document.addEventListener('ended', function (e) {
   if (e.target.tagName !== 'AUDIO') return;
   const button = e.target.nextElementSibling;
   if (button && button.classList.contains('play-button')) {
      button.textContent = '▶';
   }
}, true); // useCapture: 'ended' doesn't bubble

// Note link click handling, the same wherever a card page is shown:
// - Ctrl+Click opens the note in Anki's previewer.
// - Shift+Click opens the note in the system browser.
// - A plain click inside Anki's reviewer (the page is in an iframe) opens the note in JAStudio's workspace.
// - A plain click anywhere else (the workspace, the system browser) follows the link in place.
function setupNoteLinks() {
   const isInIframe = window.parent !== window;

   document.addEventListener('click', function(event) {
      const link = event.target.closest('.note-link');
      if (!link) return;

      const noteType = link.dataset.noteType;
      const noteId = encodeURIComponent(link.dataset.noteId);
      if (event.ctrlKey) {
         event.preventDefault();
         fetch('/api/open-in-previewer/' + noteId);
      } else if (event.shiftKey) {
         event.preventDefault();
         fetch('/api/open-in-browser/' + encodeURIComponent(noteType) + '/' + noteId);
      } else if (isInIframe) {
         event.preventDefault();
         fetch('/api/open-in-workspace/' + noteId);
      }
   });
}

setupNoteLinks();

// Talking to JAStudio's workspace (WorkspaceWindow), when the page is shown there.
// The workspace hosts the page in a web view that injects a channel for posting messages to it;
// a page shown anywhere else has no such channel, and none of this applies.
// - Every page shown is reported, because following a link is Blazor client-side routing, which the
//   workspace can't see by itself. The Navigation API reports those, and Back and Forward too.
// - Right-click shows the same note menu as in Anki.
//   Shift+Right-click keeps the browser's own menu, for "Inspect" and the like.
// - The shown note's audio plays as soon as the page shows it (AudioPlayButton calls playWhenShown).
//   Inside Anki's reviewer, Anki plays it instead.
const postToWorkspace = window.invokeCSharpAction
                      ?? (window.chrome?.webview ? message => window.chrome.webview.postMessage(message) : null);

window.jaStudioWorkspace = {
   playWhenShown: function(audio) {
      if (postToWorkspace) audio.play();
   }
};

function setupWorkspace() {
   if (!postToWorkspace) return;

   const reportPageShown = () => postToWorkspace(JSON.stringify({ request: 'pageShown', url: location.href }));
   reportPageShown();
   navigation.addEventListener('currententrychange', reportPageShown);

   document.addEventListener('contextmenu', function(event) {
      if (event.shiftKey) return;

      event.preventDefault();
      postToWorkspace(JSON.stringify({
         request: 'showNoteContextMenu',
         selection: window.getSelection().toString().trim()
      }));
   });
}

setupWorkspace();

// Shell toggle: press 'm' to show/hide the app shell.
// Stores a .NET object reference set by MainLayout via JS interop.
window.jaStudioShell = {
   _dotNetRef: null,
   registerToggle: function(dotNetRef) {
      this._dotNetRef = dotNetRef;
   },
   unregisterToggle: function() {
      this._dotNetRef = null;
   }
};

document.addEventListener('keydown', function(e) {
   if (e.key === 'm' && !e.ctrlKey && !e.altKey && !e.metaKey
       && e.target.tagName !== 'INPUT' && e.target.tagName !== 'TEXTAREA' && !e.target.isContentEditable) {
      if (window.jaStudioShell._dotNetRef) {
         window.jaStudioShell._dotNetRef.invokeMethodAsync('ToggleShellFromJs');
      }
   }
});
