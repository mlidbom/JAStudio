using System.Collections.Generic;
using JAStudio.Core.Reservoir.Storage.Media;
using JAStudio.Core.Reservoir.Sources;

namespace JAStudio.Core.Reservoir.Storage.Format;

/// <summary>
/// The sentence a stored note gives, shaped as a sentence note's source data in the corpus (<c>sourceData</c>), each media reference with the<br/>
/// hash its file is stored under.
/// </summary>
/// <param name="Question">The sentence, cleaned as the reservoir keeps it (<see cref="ReservoirSentenceText"/>).</param>
/// <param name="Answer">Its translation, or empty when the deck has none.</param>
/// <param name="ExternalId">Where in the deck it is: its note (<see cref="SourceSentence.Position"/>).</param>
/// <param name="Audio">Its audio file, when it has one.</param>
/// <param name="Screenshots">Its image file, such as a screenshot of the scene, when it has one.</param>
public sealed record SentenceSourceData(string Question, string Answer, string ExternalId, IReadOnlyList<MediaReference> Audio, IReadOnlyList<MediaReference> Screenshots)
{
   /// <summary>The source data of <paramref name="sentence"/>, its media referred to by the hashes they're stored under in <paramref name="media"/>.</summary>
   public static SentenceSourceData Of(SourceSentence sentence, StoredDeckMedia media)
   {
      return new SentenceSourceData(sentence.Text.Text, sentence.Translation ?? "", sentence.Position, ReferencesTo(sentence.AudioFile), ReferencesTo(sentence.ImageFile));

      IReadOnlyList<MediaReference> ReferencesTo(string? fileName) => fileName is null ? [] : [MediaReference.To(fileName, media)];
   }
}
