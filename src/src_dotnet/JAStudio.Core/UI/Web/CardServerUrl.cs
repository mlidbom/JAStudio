using System;
using System.Web;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Core.UI.Web;

/// <summary>
/// Provides the base URL for the CardServer (Kestrel/Blazor), and the URLs of the pages it serves.
/// Set by JAStudio.Web.CardServer on startup.
/// Used by renderers to inject iframe references.
/// </summary>
public static class CardServerUrl
{
   /// <summary>
   /// The base URL of the running CardServer, e.g. "http://127.0.0.1:54321".
   /// Null if the server has not been started.
   /// </summary>
   public static string? BaseUrl { get; set; }

   public static string MediaUrl(string fileName) => $"/media?name={Uri.EscapeDataString(fileName)}";

   /// <summary>
   /// The server-relative path of the page that shows the whole of <paramref name="note"/>: its card's answer side,
   /// with links to the notes it relates to.
   /// </summary>
   public static string NotePagePath(JPNote note) => $"/card/{CardPageNoteType(note)}/back?{NoteIdQueryParameter}={note.GetId()}";

   /// <summary>The absolute URL of <see cref="NotePagePath"/> on the running CardServer.</summary>
   public static Uri NotePageUrl(JPNote note) => new($"{RunningServersBaseUrl}{NotePagePath(note)}");

   /// <summary>
   /// The server-relative path of the page that shows a side of one of <paramref name="note"/>'s cards, as Anki shows it in an iframe.
   /// </summary>
   /// <param name="cardType">The card's type, the name of its card template in Anki, such as <see cref="CardTypes.Reading"/>.</param>
   /// <param name="side"><c>front</c> or <c>back</c>.</param>
   /// <param name="displayContext"><c>review</c> when Anki's reviewer shows the card, <c>preview</c> when its previewer does.</param>
   public static string CardPagePath(JPNote note, string cardType, string side, string displayContext) =>
      $"/card/{CardPageNoteType(note)}/{side}?{NoteIdQueryParameter}={note.GetId()}&CardType={cardType}&DisplayContext={displayContext}";

   /// <summary>The absolute URL of <see cref="CardPagePath"/> on the running CardServer.</summary>
   public static Uri CardPageUrl(JPNote note, string cardType, string side, string displayContext) =>
      new($"{RunningServersBaseUrl}{CardPagePath(note, cardType, side, displayContext)}");

   static string RunningServersBaseUrl => BaseUrl ?? throw new InvalidOperationException("The CardServer has not been started.");

   /// <summary>
   /// The id of the note a card page shows, read from the page's URL.<br/>
   /// Null for a page that shows no particular note.
   /// </summary>
   public static NoteId? NoteIdOfPage(Uri cardServerPageUrl) =>
      HttpUtility.ParseQueryString(cardServerPageUrl.Query)[NoteIdQueryParameter] is {} noteId ? NoteId.Parse(noteId) : null;

   /// <summary>The note type segment of a card page path: <c>/card/{type}/…</c>.</summary>
   public static string CardPageNoteType(JPNote note) =>
      note switch
      {
         VocabNote => "vocab",
         KanjiNote => "kanji",
         SentenceNote => "sentence",
         _ => throw new ArgumentOutOfRangeException(nameof(note), note.GetType(), "Card pages exist only for vocab, kanji and sentences.")
      };

   const string NoteIdQueryParameter = "NoteId";
}
