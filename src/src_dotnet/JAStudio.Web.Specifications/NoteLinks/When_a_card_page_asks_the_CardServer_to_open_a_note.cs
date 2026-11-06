using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core;
using JAStudio.Core.Note;
using JAStudio.Core.Specifications.Fixtures;

// ReSharper disable InconsistentNaming

namespace JAStudio.Web.Specifications.NoteLinks;

public class When_a_card_page_asks_the_CardServer_to_open_a_note : IAsyncDisposable
{
   readonly CollectionFactory.AppScope _appScope = CollectionFactory.InjectCollectionWithSelectData(DataNeeded.Vocabulary);
   readonly NoteLinkOpenerThatRecordsRequests _noteLinkOpener = new();
   readonly HttpClient _http = new();
   readonly CardServer _server;
   readonly NoteId _noteId;

   public When_a_card_page_asks_the_CardServer_to_open_a_note()
   {
      _server = new CardServer(TemporaryServiceCollection.Instance.Services, _noteLinkOpener);
      _server.Start();
      _noteId = _appScope.CoreApp.Collection.Vocab.WithQuestion("入れる")[0].GetId();
   }

   void RequestAsACardPage(string path) =>
      _http.Send(new HttpRequestMessage(HttpMethod.Get, $"{_server.BaseUrl}{path}")).EnsureSuccessStatusCode();

   public async ValueTask DisposeAsync()
   {
      _http.Dispose();
      await _server.StopAsync();
      _appScope.Dispose();
   }

   public class in_the_note_viewer : When_a_card_page_asks_the_CardServer_to_open_a_note
   {
      public in_the_note_viewer() => RequestAsACardPage($"/api/open-in-note-viewer/{_noteId}");

      [XF] public void the_note_link_opener_opens_that_note_in_the_note_viewer() => _noteLinkOpener.NotesOpenedInNoteViewer.Must().SequenceEqual(new List<NoteId> { _noteId });
      [XF] public void the_note_link_opener_opens_nothing_in_the_Anki_previewer() => _noteLinkOpener.NotesOpenedInAnkiPreviewer.Must().BeEmpty();
   }

   public class in_the_Anki_previewer : When_a_card_page_asks_the_CardServer_to_open_a_note
   {
      public in_the_Anki_previewer() => RequestAsACardPage($"/api/open-in-previewer/{_noteId}");

      [XF] public void the_note_link_opener_opens_that_note_in_the_Anki_previewer() => _noteLinkOpener.NotesOpenedInAnkiPreviewer.Must().SequenceEqual(new List<NoteId> { _noteId });
      [XF] public void the_note_link_opener_opens_nothing_in_the_note_viewer() => _noteLinkOpener.NotesOpenedInNoteViewer.Must().BeEmpty();
   }
}
