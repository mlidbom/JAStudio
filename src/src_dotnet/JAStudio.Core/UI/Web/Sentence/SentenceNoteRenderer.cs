using JAStudio.Core.Note.Sentences;

namespace JAStudio.Core.UI.Web.Sentence;

/// <summary>
/// Creates an AppendingPrerenderer that renders the Blazor iframe for sentence cards.
/// </summary>
public class SentenceNoteRenderer
{
   // ReSharper disable once UnusedMember.Global called from python
   public AppendingPrerenderer<SentenceNote> CreateRenderer() => new(RenderIframe);

   static string RenderIframe(SentenceNote note, string cardTemplateName, string side, string displayContext)
   {
      var baseUrl = CardServerUrl.BaseUrl;
      if(baseUrl == null) return "<!-- CardServer not running -->";
      return $"""<iframe src="{baseUrl}{CardServerUrl.CardPagePath(note, cardTemplateName, side, displayContext)}" style="position:fixed;inset:0;width:100%;height:100%;border:none;" frameborder="0"></iframe>""";
   }
}
