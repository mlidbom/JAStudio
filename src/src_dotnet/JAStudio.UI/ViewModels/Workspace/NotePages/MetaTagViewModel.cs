using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.UI.ViewModels.Workspace.NotePages;

/// <summary>
/// One of a vocab's meta tags as its page and the sentence breakdowns show it (<see cref="VocabMetaTag"/>), in a row of them separated by bars: what it<br/>
/// shows, such as 名, what it means on hover, and how it's coloured, as the cards' styles colour it by its name.
/// </summary>
sealed class MetaTagViewModel
{
   readonly VocabMetaTag _tag;

   MetaTagViewModel(VocabMetaTag tag, bool isTheLast)
   {
      _tag = tag;
      IsTheLast = isTheLast;
   }

   /// <summary><paramref name="tags"/>, in a row.</summary>
   public static List<MetaTagViewModel> RowOf(IReadOnlyList<VocabMetaTag> tags) => [.. tags.Select((tag, index) => new MetaTagViewModel(tag, index == tags.Count - 1))];

   public string Display => _tag.Display;

   public string Tooltip => _tag.Tooltip;

   /// <summary>Whether it's the last of its row, which no bar follows.</summary>
   public bool IsTheLast { get; }

   /// <summary>Whether it warns of something to fix, such as a vocab in no sentence, or an ichidan verb hiding a godan verb's potential form.</summary>
   public bool IsAWarning => _tag.Name is "in_no_sentences" or "is-ichidan-hiding-godan-potential";

   /// <summary>Whether it's of little note, such as how many sentences a vocab not being studied is in.</summary>
   public bool IsFaint => _tag.Name == "in_sentences";
}
