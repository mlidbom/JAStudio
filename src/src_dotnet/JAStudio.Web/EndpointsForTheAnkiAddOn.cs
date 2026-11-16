using System;
using System.Linq;
using System.Net;
using JAStudio.Core.Anki;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Studying;
using JAStudio.Core.UI.Shortcuts;
using JAStudio.Core.UI.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace JAStudio.Web;

/// <summary>
/// The requests the Anki add-on makes of JAStudio, on the card server, under <c>/anki/</c>: what goes from Anki to JAStudio. They're the cards'<br/>
/// pages, what Anki's keys and its JA Studio menu ask of JAStudio's windows and of the note Anki shows, and what Anki's scheduling asks of<br/>
/// JAStudio's settings. Each names a note by JAStudio's own id, which the study-note holds, never by Anki's.
/// </summary>
/// <remarks>
/// They're part of the contract between the two (<see cref="AnkiAddOnContract"/>): the add-on's <c>primary_jastudio</c> package makes them. An<br/>
/// answer is JSON, and a note JAStudio hasn't is answered with 404 and <c>{"error": why}</c>: with several JAStudios connected to one Anki, a<br/>
/// JAStudio may lack a note another has.
/// </remarks>
/// <remarks>review-naming:anki-http: EndpointsForTheAnkiAddOn and its routes are Claude's (2026-11-15).</remarks>
static class EndpointsForTheAnkiAddOn
{
   /// <summary>Maps the requests on <paramref name="app"/>, which hand what they ask of JAStudio's windows to <paramref name="noteLinkOpener"/> and <paramref name="workspaceToggles"/>.</summary>
   public static void MapOn(WebApplication app, INoteLinkOpener noteLinkOpener, IWorkspaceToggles workspaceToggles)
   {
      var anki = app.MapGroup("/anki");

      anki.MapGet("/contract", () => Results.Json(new { version = AnkiAddOnContract.Version }));

      // The address of the iframe that shows the card in Anki's reviewer and previewer, which this sends on to the card's page.
      anki.MapGet("/card/{side}", (string side, string noteId, string cardType, string displayContext, JPCollection collection) =>
      {
         if(!collection.IsInitialized) return ThePageThatSaysJAStudioIsStillLoading();
         return collection.NoteFromNoteId(NoteId.Parse(noteId)) is {} note
                   ? Results.Redirect(CardServerUrl.CardPagePath(note, cardType, side, displayContext))
                   : Results.Text(ThereIsNoNoteWithTheId(noteId), "text/plain; charset=utf-8", statusCode: StatusCodes.Status404NotFound);
      });

      anki.MapPost("/workspace/toggle", () =>
      {
         workspaceToggles.ToggleWorkspaceWindow();
         return Results.NoContent();
      });

      anki.MapPost("/note-search/toggle", () =>
      {
         workspaceToggles.ToggleNoteSearch();
         return Results.NoContent();
      });

      anki.MapPost("/workspace/show-note", (string noteId, JPCollection collection) =>
         WithTheNote(collection, noteId, note =>
         {
            noteLinkOpener.OpenInWorkspace(note.GetId());
            return Results.NoContent();
         }));

      anki.MapGet("/note-view-shortcuts", (NoteViewShortcuts shortcuts) => Results.Json(new { keys = shortcuts.All.Select(it => it.KeyGesture) }));

      // The note is the note Anki shows, if it shows one.
      anki.MapPost("/note-view-shortcuts/run", (string key, string? noteId, NoteViewShortcuts shortcuts, JPCollection collection) =>
      {
         if(shortcuts.BoundTo(key) is not {} shortcut) return TheError(StatusCodes.Status404NotFound, $"No note view shortcut is bound to {key}.");
         if(noteId is null)
         {
            shortcut.Run(null);
            return Results.NoContent();
         }

         return WithTheNote(collection, noteId, note =>
         {
            shortcut.Run(note);
            return Results.NoContent();
         });
      });

      anki.MapGet("/seconds-to-show-the-question", (string noteId, double secondsTheDeckGives, int againAnswersInARowToday, HowLongToShowTheQuestion howLong, JPCollection collection) =>
         WithTheNote(collection, noteId, note => Results.Json(new { seconds = howLong.Seconds(note, secondsTheDeckGives, againAnswersInARowToday) })));

      anki.MapGet("/timebox-minutes", (string noteType, string cardType, TimeboxLengths timeboxLengths) =>
         Results.Json(new { minutes = timeboxLengths.MinutesFor(noteType, cardType) }));
   }

   static IResult WithTheNote(JPCollection collection, string noteId, Func<JPNote, IResult> answer) =>
      collection.NoteFromNoteId(NoteId.Parse(noteId)) is {} note ? answer(note) : TheError(StatusCodes.Status404NotFound, ThereIsNoNoteWithTheId(noteId));

   static string ThereIsNoNoteWithTheId(string noteId) => $"This JAStudio has no note with the id {noteId}.";

   static IResult TheError(int status, string why) => Results.Json(new { error = why }, statusCode: status);

   /// <summary>The card's page while JAStudio loads its collection: it says so, and asks for the card again each second, until the collection has loaded.</summary>
   static IResult ThePageThatSaysJAStudioIsStillLoading() =>
      Results.Content($"""<!DOCTYPE html><html><head><meta charset="utf-8"><meta http-equiv="refresh" content="1"></head><body>{WebUtility.HtmlEncode(Mine.AppStillLoadingMessage)}</body></html>""",
                      "text/html; charset=utf-8");
}
