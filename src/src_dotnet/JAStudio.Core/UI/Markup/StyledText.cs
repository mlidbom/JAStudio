using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace JAStudio.Core.UI.Markup;

/// <summary>
/// A note's text as JAStudio's own pages show it: the text, in runs each styled as the markup around it in the note says (<see cref="TextStyle"/>).<br/>
/// The notes' texts hold HTML, such as <c>&lt;b&gt;</c> in a sentence's source question, and JAStudio's own tags, such as <c>&lt;kanji&gt;</c> in a<br/>
/// kanji's mnemonic, which the Blazor cards show with their styles, and the workspace's Avalonia pages with these runs.
/// </summary>
/// <remarks>
/// Line breaks are kept: those in the text itself, as the cards keep them, <c>&lt;br&gt;</c>, and those around blocks, such as a <c>&lt;div&gt;</c><br/>
/// or a list's <c>&lt;li&gt;</c>, which starts with a bullet. Line breaks at the start or end of the whole text are left out. Elements that show no<br/>
/// text, such as an image, are left out, and elements of no known style, such as a link, show their text unstyled.
/// </remarks>
public sealed class StyledText
{
   StyledText(IReadOnlyList<StyledRun> runs) => Runs = runs;

   public IReadOnlyList<StyledRun> Runs { get; }

   /// <summary>Reads <paramref name="markup"/>, a note's text, into the runs it shows.</summary>
   public static StyledText FromMarkup(string markup)
   {
      var document = new HtmlDocument();
      document.LoadHtml(markup);
      var runs = new RunsBeingRead();
      runs.AddTheContentOf(document.DocumentNode, TextStyle.Plain);
      return new StyledText(runs.WithoutLineBreaksAtTheEnds());
   }

   /// <summary>The text without its styles, such as for a tooltip, or to compare in a specification.</summary>
   public override string ToString() => string.Concat(Runs.Select(it => it.Text));

   /// <summary>The runs of a text being read, with the line breaks blocks need, and adjacent runs of the same style joined.</summary>
   sealed class RunsBeingRead
   {
      const char LineBreak = '\n';

      static readonly HashSet<string> Blocks = ["div", "p", "li", "ul", "ol", "hr", "center", "h1", "h2", "h3", "h4", "h5", "h6", "blockquote", "table", "tr"];
      static readonly HashSet<string> ElementsShowingNoText = ["img", "audio", "video", "script", "style", "wbr", "rt", "rp"];

      readonly List<StyledRun> _runs = [];

      /// <summary>Whether a block started or ended, so that text that follows starts on a line of its own.</summary>
      bool _theNextTextStartsANewLine;

      public void AddTheContentOf(HtmlNode node, TextStyle style)
      {
         foreach(var child in node.ChildNodes)
            Add(child, style);
      }

      void Add(HtmlNode node, TextStyle style)
      {
         switch(node)
         {
            case HtmlTextNode text:
               Add(HtmlEntity.DeEntitize(text.Text), style);
               return;
            case { NodeType: HtmlNodeType.Element } element:
               AddTheElement(element, style);
               return;
         }
      }

      void AddTheElement(HtmlNode element, TextStyle style)
      {
         var name = element.Name.ToLowerInvariant();
         if(ElementsShowingNoText.Contains(name)) return;

         if(name == "br")
         {
            Add(LineBreak.ToString(), style);
            return;
         }

         var isABlock = Blocks.Contains(name);
         if(isABlock) _theNextTextStartsANewLine = true;
         if(name == "li") Add("• ", style);
         AddTheContentOf(element, style | StyleOf(element, name));
         if(isABlock) _theNextTextStartsANewLine = true;
      }

      static TextStyle StyleOf(HtmlNode element, string name) =>
         name switch
         {
            "b" or "strong" => TextStyle.Bold,
            "i" or "em" => TextStyle.Italic,
            "u" => TextStyle.Underlined,
            "s" or "strike" or "del" or "obsolete" => TextStyle.StruckThrough,
            "ja" => TextStyle.Japanese,
            "tag" => TextStyle.Label,
            "kanji" or "kan" => TextStyle.Kanji,
            "radical" or "rad" => TextStyle.Radical,
            "vocabulary" or "vocab" or "voc" => TextStyle.Vocabulary,
            "reading" or "read" => TextStyle.Reading,
            "primary" => TextStyle.Primary,
            "span" when element.HasClass("readingsSeparator") => TextStyle.ReadingsSeparator,
            _ => TextStyle.Plain
         };

      void Add(string text, TextStyle style)
      {
         if(text.Length == 0) return;

         if(_theNextTextStartsANewLine && _runs.Count > 0 && !_runs[^1].Text.EndsWith(LineBreak))
            Append(LineBreak.ToString(), TextStyle.Plain);
         _theNextTextStartsANewLine = false;
         Append(text.Replace("\r", ""), style);
      }

      void Append(string text, TextStyle style)
      {
         if(_runs.Count > 0 && _runs[^1].Style == style)
            _runs[^1] = _runs[^1] with { Text = _runs[^1].Text + text };
         else
            _runs.Add(new StyledRun(text, style));
      }

      public List<StyledRun> WithoutLineBreaksAtTheEnds()
      {
         var runs = new List<StyledRun>(_runs);
         while(runs.Count > 0 && runs[0].Text.TrimStart(LineBreak) is var rest && rest != runs[0].Text)
         {
            if(rest.Length == 0) runs.RemoveAt(0);
            else runs[0] = runs[0] with { Text = rest };
         }

         while(runs.Count > 0 && runs[^1].Text.TrimEnd(LineBreak) is var rest && rest != runs[^1].Text)
         {
            if(rest.Length == 0) runs.RemoveAt(runs.Count - 1);
            else runs[^1] = runs[^1] with { Text = rest };
         }

         return runs;
      }
   }
}
