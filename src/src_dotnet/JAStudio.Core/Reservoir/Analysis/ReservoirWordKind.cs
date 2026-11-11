namespace JAStudio.Core.Reservoir.Analysis;

/// <summary>What a word in a reservoir sentence's analysis matched, as the match kinds of the sentence breakdown.</summary>
public enum ReservoirWordKind : byte
{
   /// <summary>A vocab: the word is one of JAStudio's vocab notes.</summary>
   Vocab,

   /// <summary>A dictionary entry, with no vocab for the word.</summary>
   Dictionary,

   /// <summary>Nothing: neither a vocab nor the dictionary has the word, as the tokenizer gave it.</summary>
   Missing
}
