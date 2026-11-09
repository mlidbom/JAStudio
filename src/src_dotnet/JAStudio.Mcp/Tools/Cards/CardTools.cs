using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.UI.Web;
using JAStudio.Mcp.UI;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace JAStudio.Mcp.Tools.Cards;

/// <summary>
/// The tools that show Claude a note's cards as Anki shows them: the card server's card pages, rendered by the headless browser of the<br/>
/// <see cref="HeadlessUI"/>. Only a JAStudio with a headless UI offers them, since only it has a browser to render pages with.
/// </summary>
[McpServerToolType]
public class CardTools
{
   /// <summary>How wide <see cref="RenderCard"/> lays a card out unless told otherwise: about as wide as Anki's reviewer on Magnus's screen.</summary>
   const int ViewportWidthByDefault = 1000;

   /// <summary>The viewport's height a card is laid out for; the image is of the whole card, however tall.</summary>
   const int ViewportHeight = 800;

   static readonly string[] CardTypesThereAre = [CardTypes.Reading, CardTypes.Listening];
   static readonly string[] SidesThereAre = ["front", "back"];

   readonly HeadlessUI _headlessUI;
   readonly JPCollection _collection;

   public CardTools(HeadlessUI headlessUI, JPCollection collection)
   {
      _headlessUI = headlessUI;
      _collection = collection;
   }

   [McpServerTool(Name = "render_card", ReadOnly = true, Idempotent = true)]
   [Description("""
                Renders a side of one of a note's cards as Anki's reviewer shows it: the card server's page for it, laid out as wide as asked,
                rendered whole, however tall, by a headless browser.
                """)]
   public IEnumerable<ContentBlock> RenderCard(
      [Description("The note's id, a GUID, as search_notes lists it.")] string noteId,
      [Description("The card's type: \"Reading\" or \"Listening\".")] string cardType = CardTypes.Reading,
      [Description("The card's side: \"front\" or \"back\".")] string side = "back",
      [Description("How wide to lay the card out, in pixels.")] int width = ViewportWidthByDefault)
   {
      var note = _collection.NoteFromNoteId(NoteId.Parse(noteId)) ?? throw new ArgumentException($"No note has the id {noteId}.", nameof(noteId));
      var cardTypeAsNamed = OneOf(CardTypesThereAre, cardType, nameof(cardType));
      var sideAsNamed = OneOf(SidesThereAre, side, nameof(side));

      var page = CardServerUrl.CardPageUrl(note, cardTypeAsNamed, sideAsNamed, "review");
      return ImageResult.Of($"The {sideAsNamed} of the {cardTypeAsNamed} card of {note.GetQuestion()}",
                            _headlessUI.WebPageImages.OfTheWholePage(page, new ViewportSize(width, ViewportHeight)));

      static string OneOf(string[] valuesThereAre, string value, string parameterName) =>
         valuesThereAre.SingleOrDefault(it => string.Equals(it, value, StringComparison.OrdinalIgnoreCase))
      ?? throw new ArgumentException($"There is no {parameterName} '{value}'. There are: {string.Join(", ", valuesThereAre.Select(it => $"\"{it}\""))}.", parameterName);
   }
}
