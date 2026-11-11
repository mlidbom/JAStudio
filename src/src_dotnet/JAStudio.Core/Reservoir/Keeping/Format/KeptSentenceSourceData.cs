using System.Collections.Generic;

namespace JAStudio.Core.Reservoir.Keeping.Format;

/// <summary>
/// The sentence a kept note gives, shaped as a sentence note's source data in the corpus (<c>sourceData</c>), each media reference with the<br/>
/// hash its file is kept under.
/// </summary>
/// <param name="Question">The sentence, cleaned as the reservoir keeps it (<see cref="ReservoirSentenceText"/>).</param>
/// <param name="Answer">Its translation, or empty when the deck has none.</param>
/// <param name="ExternalId">Where in the deck it is: its note.</param>
public sealed record KeptSentenceSourceData(string Question, string Answer, string ExternalId, IReadOnlyList<KeptMediaReference> Audio, IReadOnlyList<KeptMediaReference> Screenshots);
