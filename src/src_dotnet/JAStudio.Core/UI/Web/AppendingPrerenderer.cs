using System;
using System.Threading.Tasks;
using Compze.Internals.SystemCE.ThreadingCE.TasksCE;
using JAStudio.Core.Note;
using JAStudio.Core.SysUtils;

namespace JAStudio.Core.UI.Web;

/// <summary>
/// Prerenderer that returns a Blazor iframe for both front and back card displays.
/// On review questions, starts prerendering the back side in the background.
/// </summary>
public class AppendingPrerenderer<TNote> where TNote : JPNote
{
   readonly Func<TNote, string, string, string, string> _renderIframe;
   Task<string>? _prerenderedTask;

   static string HostStylesheet => CardServerUrl.BaseUrl is {} baseUrl
      ? $"""<link rel="stylesheet" href="{baseUrl}/css/jas_anki_card_template_styles.css">"""
      : "";

   /// <param name="renderIframe">Given a note, card template name, side (front/back), and display context (review/preview), returns the iframe HTML.</param>
   public AppendingPrerenderer(Func<TNote, string, string, string, string> renderIframe) => _renderIframe = renderIframe;

   // ReSharper disable once UnusedMember.Global used from python
   public string Render(TNote note, string html, string typeOfDisplay, string cardTemplateName)
   {
      if(!note.Collection.IsInitialized)
         return Mine.AppStillLoadingMessage;

      var displayContext = DisplayType.IsReview(typeOfDisplay) ? "review" : "preview";

      if(DisplayType.IsDisplayingQuestion(typeOfDisplay))
      {
         if(DisplayType.IsDisplayingReviewQuestion(typeOfDisplay))
            SchedulePrerender(note, cardTemplateName, displayContext);
         return RenderIframeWithTiming(note, cardTemplateName, "front", displayContext);
      }

      if(DisplayType.IsDisplayingReviewAnswer(typeOfDisplay) && _prerenderedTask != null)
      {
         html = ApplyPrerendered(html);
      } else if(DisplayType.IsDisplayingAnswer(typeOfDisplay))
      {
         html = RenderSynchronously(note, html, cardTemplateName, displayContext);
      }

      return html;
   }

   void SchedulePrerender(TNote note, string cardTemplateName, string displayContext)
   {
      _prerenderedTask = TaskCE.Run(() => RenderIframeWithTiming(note, cardTemplateName, "back", displayContext));
   }

   string RenderIframeWithTiming(TNote note, string cardTemplateName, string side, string displayContext)
   {
      using(StopWatch.LogWarningIfSlowerThan(0.5, "prerendering"))
      {
         return HostStylesheet + _renderIframe(note, cardTemplateName, side, displayContext);
      }
   }

   string ApplyPrerendered(string html)
   {
      using(StopWatch.LogWarningIfSlowerThan(0.01, "fetching_prerendered"))
      {
         var result = html + _prerenderedTask!.Result;
         _prerenderedTask = null;
         return result;
      }
   }

   string RenderSynchronously(TNote note, string html, string cardTemplateName, string displayContext)
   {
      using(StopWatch.LogWarningIfSlowerThan(0.5, "live_rendering"))
      {
         return html + RenderIframeWithTiming(note, cardTemplateName, "back", displayContext);
      }
   }
}
