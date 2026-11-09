using System.Collections.Generic;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.UI.Web;
using ModelContextProtocol.Protocol;
using Xunit;

// ReSharper disable InconsistentNaming

namespace JAStudio.Mcp.Specifications.Tools.Cards;

/// <summary>
/// The server of a JAStudio with a headless UI rendering a card of the vocab 行く: the card server's card page, rendered by the headless UI's<br/>
/// browser, which here records the pages asked for. The card server's address is one that isn't running.
/// </summary>
[Collection<SpecificationsUsingTheSharedHeadlessUI>]
public class When_rendering_a_card : McpServerSpecification
{
   const string AddressOfACardServerThatIsNotRunning = "http://localhost";

   protected readonly VocabNote Vocab;

   protected When_rendering_a_card() : base(HeadlessUIOfTheSpecifications.Started)
   {
      CardServerUrl.BaseUrl = AddressOfACardServerThatIsNotRunning;
      Vocab = CreateVocab("行く", "to go", "いく");
   }

   protected CallToolResult RenderCard(Dictionary<string, object?> arguments) =>
      CallTool("render_card", new Dictionary<string, object?>(arguments) { ["noteId"] = Vocab.GetId().ToString() });

   private protected static WebPageImagesThatRecordThePagesAsked.PageAsked TheLastPageAsked => HeadlessUIOfTheSpecifications.WebPageImages.PagesAsked[^1];

   public class given_only_the_note : When_rendering_a_card
   {
      readonly CallToolResult _result;

      public given_only_the_note() => _result = RenderCard([]);

      [XF] public void the_tool_call_succeeds() => _result.IsError.Must().NotBe(true);

      [XF] public void the_page_rendered_is_the_back_of_the_reading_card_as_the_reviewer_shows_it() =>
         TheLastPageAsked.Page.ToString().Must().Be($"{AddressOfACardServerThatIsNotRunning}/card/vocab/back?NoteId={Vocab.GetId()}&CardType=Reading&DisplayContext=review");

      [XF] public void the_whole_page_is_rendered() => TheLastPageAsked.WholePage.Must().BeTrue();
      [XF] public void it_is_laid_out_1000_pixels_wide() => TheLastPageAsked.Viewport.Width.Must().Be(1000);
      [XF] public void it_gives_the_image_rendered() => _result.Content.OfType<ImageContentBlock>().Count().Must().Be(1);
      [XF] public void the_text_says_what_the_image_shows() => _result.Content.OfType<TextContentBlock>().Single().Text.Must().StartWith("The back of the Reading card of 行く, ");
   }

   public class given_the_front_of_the_listening_card_in_lower_case : When_rendering_a_card
   {
      public given_the_front_of_the_listening_card_in_lower_case() => RenderCard(new Dictionary<string, object?> { ["cardType"] = "listening", ["side"] = "front" });

      [XF] public void the_page_rendered_is_the_front_of_the_listening_card() =>
         TheLastPageAsked.Page.ToString().Must().Be($"{AddressOfACardServerThatIsNotRunning}/card/vocab/front?NoteId={Vocab.GetId()}&CardType=Listening&DisplayContext=review");
   }

   public class given_a_card_type_there_is_not : When_rendering_a_card
   {
      readonly CallToolResult _result;

      public given_a_card_type_there_is_not() => _result = RenderCard(new Dictionary<string, object?> { ["cardType"] = "Writing" });

      [XF] public void the_tool_call_fails() => _result.IsError.Must().Be(true);
      [XF] public void the_error_names_the_card_types_there_are() => TextOf(_result).Must().Contain("There are: \"Reading\", \"Listening\"");
   }
}
