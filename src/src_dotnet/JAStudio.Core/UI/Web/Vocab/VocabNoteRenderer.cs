using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Core.UI.Web.Vocab;

/// <summary>
/// Creates an AppendingPrerenderer that renders the Blazor iframe for vocab cards.
/// </summary>
public class VocabNoteRenderer
{
   // ReSharper disable once UnusedMember.Global called from python
   public AppendingPrerenderer<VocabNote> CreateRenderer() => new(RenderIframe);

   static string RenderIframe(VocabNote note, string cardTemplateName, string side, string displayContext)
   {
      var baseUrl = CardServerUrl.BaseUrl;
      if(baseUrl == null) return "<!-- CardServer not running -->";
      return $"""<iframe src="{baseUrl}{CardServerUrl.CardPagePath(note, cardTemplateName, side, displayContext)}" style="position:fixed;inset:0;width:100%;height:100%;border:none;" frameborder="0"></iframe>""";
   }
}
