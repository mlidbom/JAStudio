using JAStudio.Core.Note;

namespace JAStudio.Core.UI.Web.Kanji;

/// <summary>
/// Creates an AppendingPrerenderer that renders the Blazor iframe for kanji cards.
/// </summary>
public class KanjiNoteRenderer
{
   // ReSharper disable once UnusedMember.Global called from python
   public AppendingPrerenderer<KanjiNote> CreateRenderer() => new(RenderIframe);

   static string RenderIframe(KanjiNote note, string cardTemplateName, string side, string displayContext)
   {
      var baseUrl = CardServerUrl.BaseUrl;
      if(baseUrl == null) return "<!-- CardServer not running -->";
      return $"""<iframe src="{baseUrl}{CardServerUrl.CardPagePath(note, cardTemplateName, side, displayContext)}" style="position:fixed;inset:0;width:100%;height:100%;border:none;" frameborder="0"></iframe>""";
   }
}
