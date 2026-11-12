namespace JAStudio.Core.Reservoir.Picking;

/// <summary>What a feature of reservoir sentences is about (<see cref="SentenceFeatures"/>).</summary>
public enum SentenceFeatureKind : byte
{
   /// <summary>A vocab's match: a sentence the vocab matches.</summary>
   Vocab,

   /// <summary>A word the sentence breakdown displays that the dictionary has and no vocab, shown in enough works to be no name.</summary>
   DictionaryWord,

   /// <summary>A word the sentence breakdown displays that neither a vocab nor the dictionary has, shown in enough works to be no name.</summary>
   MissingWord,

   /// <summary>A failure reason rejecting a vocab's match.</summary>
   Rejection
}
