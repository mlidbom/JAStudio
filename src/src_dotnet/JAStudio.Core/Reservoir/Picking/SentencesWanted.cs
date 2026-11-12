namespace JAStudio.Core.Reservoir.Picking;

/// <summary>
/// How many sentences the corpus wants of each kind of feature a sentence can exercise (<see cref="SentenceFeatures"/>): the floor picking<br/>
/// from the reservoir fills, counting the sentences the corpus has already.
/// </summary>
/// <param name="ForAVocab">The sentences wanted that a vocab matches.</param>
/// <param name="ForARejection">The sentences wanted where a reason rejects a vocab's match.</param>
/// <param name="ForAWordNoVocabHas">The sentences wanted that show a word no vocab has.</param>
public record SentencesWanted(int ForAVocab, int ForARejection, int ForAWordNoVocabHas)
{
   /// <summary>A few of each, as the reservoir's report picks: three for a vocab, two for a rejection, one for a word no vocab has.</summary>
   public static readonly SentencesWanted AFewOfEach = new(ForAVocab: 3, ForARejection: 2, ForAWordNoVocabHas: 1);

   /// <summary><paramref name="sentences"/> for each vocab, and none for the other features: what giving every vocab a floor of sentences takes.</summary>
   public static SentencesWanted ForEachVocab(int sentences) => new(ForAVocab: sentences, ForARejection: 0, ForAWordNoVocabHas: 0);
}
