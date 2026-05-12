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

// Note link click handling.
// In Anki's iframe: regular click opens in the Anki previewer; Shift+Click opens in the system browser.
// In a regular browser: let the default <a> behavior proceed (same tab, or new tab on ctrl/shift+click).
function setupNoteLinks() {
   const isInIframe = window.parent !== window;
   if (!isInIframe) return;

   document.addEventListener('click', function(event) {
      const link = event.target.closest('.note-link');
      if (!link) return;

      event.preventDefault();
      const noteType = link.dataset.noteType;
      const noteId = link.dataset.noteId;
      if (noteType && noteId) {
         if (event.shiftKey) {
            fetch('/api/open-in-browser/' + encodeURIComponent(noteType) + '/' + encodeURIComponent(noteId));
         } else {
            fetch('/api/open-in-previewer/' + encodeURIComponent(noteId));
         }
      }
   });
}

setupNoteLinks();

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
