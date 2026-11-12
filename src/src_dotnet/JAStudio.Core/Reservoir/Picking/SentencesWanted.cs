namespace JAStudio.Core.Reservoir.Picking;

/// <summary>
/// How many sentences the corpus wants of each kind of feature a sentence can exercise (<see cref="SentenceFeatures"/>): the floor picking<br/>
/// from the reservoir fills, counting the sentences the corpus has already.
/// </summary>
/// <param name="ForAVocab">The sentences wanted that a vocab matches.</param>
/// <param name="ForARejection">The sentences wanted where a reason rejects a vocab's match.</param>
/// <param name="ForADictionaryWord">The sentences wanted that show a word the dictionary has and no vocab.</param>
/// <param name="ForAMissingWord">The sentences wanted that show a word neither a vocab nor the dictionary has.</param>
public record SentencesWanted(int ForAVocab, int ForARejection, int ForADictionaryWord, int ForAMissingWord)
{
   /// <summary>A few of each, as the reservoir's report picks: three for a vocab, two for a rejection, one for a word no vocab has.</summary>
   public static readonly SentencesWanted AFewOfEach = new(ForAVocab: 3, ForARejection: 2, ForADictionaryWord: 1, ForAMissingWord: 1);

   /// <summary><paramref name="sentences"/> for each vocab, and none for the other features: what giving every vocab a floor of sentences takes.</summary>
   public static SentencesWanted ForEachVocab(int sentences) => new(ForAVocab: sentences, ForARejection: 0, ForADictionaryWord: 0, ForAMissingWord: 0);

   /// <summary>
   /// <paramref name="sentences"/> for each vocab and each word the dictionary has, and none for the other features: what giving every word<br/>
   /// worth a vocab a floor of sentences takes.
   /// </summary>
   public static SentencesWanted ForEachVocabAndDictionaryWord(int sentences) =>
      new(ForAVocab: sentences, ForARejection: 0, ForADictionaryWord: sentences, ForAMissingWord: 0);

   /// <summary>
   /// <paramref name="sentences"/> for each word the sentence breakdown displays, vocab or not, and none for rejections: what giving every word a<br/>
   /// floor of sentences takes.
   /// </summary>
   public static SentencesWanted ForEveryWord(int sentences) => new(ForAVocab: sentences, ForARejection: 0, ForADictionaryWord: sentences, ForAMissingWord: sentences);
}
