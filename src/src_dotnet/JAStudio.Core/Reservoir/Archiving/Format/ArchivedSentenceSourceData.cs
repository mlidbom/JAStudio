using System.Collections.Generic;
using JAStudio.Core.Reservoir.Archiving.Media;
using JAStudio.Core.Reservoir.Sources;

namespace JAStudio.Core.Reservoir.Archiving.Format;

/// <summary>
/// The sentence an archived note gives, shaped as a sentence note's source data in the corpus (<c>sourceData</c>), each media reference with the<br/>
/// hash its file is stored under.
/// </summary>
/// <param name="Question">The sentence, cleaned as the reservoir keeps it (<see cref="ReservoirSentenceText"/>).</param>
/// <param name="Answer">Its translation, or empty when the deck has none.</param>
/// <param name="ExternalId">Where in the deck it is: its note (<see cref="SourceSentence.Position"/>).</param>
/// <param name="Audio">Its audio file, when it has one.</param>
/// <param name="Screenshots">Its image file, such as a screenshot of the scene, when it has one.</param>
public sealed record ArchivedSentenceSourceData(string Question, string Answer, string ExternalId, IReadOnlyList<ArchivedMediaReference> Audio, IReadOnlyList<ArchivedMediaReference> Screenshots)
{
   /// <summary>The source data of <paramref name="sentence"/>, its media referred to by the hashes they're stored under in <paramref name="media"/>.</summary>
   public static ArchivedSentenceSourceData Of(SourceSentence sentence, StoredDeckMedia media)
   {
      return new ArchivedSentenceSourceData(sentence.Text.Text, sentence.Translation ?? "", sentence.Position, ReferencesTo(sentence.AudioFile), ReferencesTo(sentence.ImageFile));

      IReadOnlyList<ArchivedMediaReference> ReferencesTo(string? fileName) => fileName is null ? [] : [ArchivedMediaReference.To(fileName, media)];
   }
}
