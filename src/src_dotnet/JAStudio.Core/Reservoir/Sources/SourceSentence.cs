namespace JAStudio.Core.Reservoir.Sources;

/// <summary>A sentence as a <see cref="ReservoirSource"/> gives it, before it joins the reservoir: its text, and what came with it.</summary>
/// <param name="Text">The sentence, cleaned.</param>
/// <param name="Position">Where in its source it is, such as <c>note 1588140539432</c> in a deck, or <c>Natsume 01.srt 00:10:01</c> in subtitles.</param>
/// <param name="Translation">Its translation, usually into English, when the source has one.</param>
/// <param name="AudioFile">The name of its audio file in a deck, when it has one.</param>
/// <param name="ImageFile">The name of its image file in a deck, such as a screenshot of the scene, when it has one.</param>
public sealed record SourceSentence(ReservoirSentenceText Text, string Position, string? Translation, string? AudioFile, string? ImageFile);
