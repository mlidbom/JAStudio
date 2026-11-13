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
// - A plain click in JAStudio's workspace has the tab show the note (setupWorkspace).
// - A plain click anywhere else (the system browser) follows the link in place.
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
      } else if (postToWorkspace) {
         event.preventDefault();
         postToWorkspace(JSON.stringify({ request: 'showNote', noteId: link.dataset.noteId }));
      }
   });
}

setupNoteLinks();

// Talking to JAStudio's workspace (WorkspaceWindow), when the page is shown in one of its tabs (NoteTabView).
// Each tab hosts its page in a web view that injects a channel for posting messages to it;
// a page shown anywhere else has no such channel, and none of this applies.
// - The tab keeps its own back and forward history (NoteTabHistory), and the web view only shows the page of
//   the note the tab shows, keeping no history of its own: each page replaces the one before (NoteTabView).
//   So a note link clicked is reported for the tab to show (setupNoteLinks), and so are back and forward:
//   Alt+Left and Alt+Right, reported as keys, and the mouse's back and forward buttons.
// - Every page shown is reported too, in case the page goes to another by itself, which the tab then follows.
// - Right-click shows the same note menu as in Anki.
//   Shift+Right-click keeps the browser's own menu, for "Inspect" and the like.
// - Every key press is reported, because keys pressed on the page go to the web view, not to the workspace.
//   The workspace decides which are shortcuts (NoteViewShortcuts and its own), and the browser still
//   handles its own keys, such as F5.
// - The shown note's audio plays as soon as the page shows it (AudioPlayButton calls playWhenShown).
//   Inside Anki's reviewer, Anki plays it instead.
//   A reload shows the same note again, after an edit or a setting change, so it doesn't replay the audio.
//   The first navigation after it does.
const postToWorkspace = window.invokeCSharpAction
                      ?? (window.chrome?.webview ? message => window.chrome.webview.postMessage(message) : null);

let isShowingThePageItWasReloadedOn = performance.getEntriesByType('navigation')[0].type === 'reload';
navigation.addEventListener('currententrychange', () => isShowingThePageItWasReloadedOn = false);

window.jaStudioWorkspace = {
   playWhenShown: function(audio) {
      if (postToWorkspace && !isShowingThePageItWasReloadedOn) audio.play();
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

   document.addEventListener('keydown', function(event) {
      if (event.repeat || ['Control', 'Shift', 'Alt', 'Meta'].includes(event.key)) return;

      postToWorkspace(JSON.stringify({ request: 'keyPressed', keyGesture: keyGestureOf(event) }));
   });

   // The mouse's back and forward buttons are buttons 3 and 4.
   document.addEventListener('mouseup', function(event) {
      if (event.button !== 3 && event.button !== 4) return;

      postToWorkspace(JSON.stringify({ request: event.button === 3 ? 'goBack' : 'goForward' }));
   });

   // Written the way NoteViewShortcut.KeyGesture is: the modifiers in the order Ctrl, Shift, Alt, then the key in lower case,
   // the arrow keys as Avalonia names them (Left, Right, Up, Down).
   function keyGestureOf(event) {
      const key = event.key.toLowerCase().replace(/^arrow/, '');
      return (event.ctrlKey ? 'Ctrl+' : '') + (event.shiftKey ? 'Shift+' : '') + (event.altKey ? 'Alt+' : '') + key;
   }
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
