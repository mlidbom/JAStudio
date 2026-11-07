using JAStudio.Core.Note.Sentences;
using JAStudio.UI.Utils;

namespace JAStudio.UI.ViewModels.Workspace.Navigation.SearchResults;

/// <summary>A sentence a search found, as a row of the results grid shows it.</summary>
/// <remarks>
/// <see cref="Source"/> and <see cref="ExternalId"/> sort with the numbers in them compared as numbers,<br/>
/// so episode 2 comes before episode 11, and sorting by the external id puts an episode's sentences in the order their lines are spoken.
/// </remarks>
sealed class SentenceSearchResultViewModel(SentenceNote sentence) : SearchResultViewModel(sentence)
{
   /// <summary>The sentence's source tag, such as <c>source::anime::natsume::s4::11</c>.</summary>
   public TextSortedWithNumbersAsNumbers Source { get; } = new(sentence.SourceTag.ToString());

   /// <summary>
   /// The id the sentence had in the deck it was imported from, such as the subs2srs id <c>ny4-nodup_11_0.15.23.540-0.15.25.620</c>:<br/>
   /// the deck, episode 11, and the line's start and end times. Empty for many sentences.
   /// </summary>
   public TextSortedWithNumbersAsNumbers ExternalId { get; } = new(sentence.ExternalId.Value);
}
