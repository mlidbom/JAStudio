namespace JAStudio.Core.Reservoir;

/// <summary>
/// A sentence of the reservoir as its id there and its text: what analyzing it anew, or finding it by a text it contains, needs.<br/>
/// <see cref="StoredReservoirSentence"/> is the sentence with all the reservoir keeps of it.
/// </summary>
/// <remarks>A value type, as the value tuple it replaces was, so that the millions of them held in memory cost no more than their id and text.</remarks>
/// <param name="Id">The sentence's id in the reservoir.</param>
/// <param name="Text">The sentence, cleaned as <see cref="ReservoirSentenceText"/> cleans it.</param>
public readonly record struct ReservoirSentence(long Id, string Text);
