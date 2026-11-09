using System.Collections.Generic;
using Compze.Contracts;
using JAStudio.Core.Storage.Dto;

namespace JAStudio.Core.Note.Vocabulary;

/// <summary>
/// A word-reference: how a note refers to a word, such as a vocab's synonym or one of its compound parts. It names the word by its <see cref="Text"/>,<br/>
/// and, when the corpus has more than one vocab with that text, by the <see cref="Disambiguation"/> of the one meant, as a vocab's question does<br/>
/// (<see cref="VocabNoteQuestion"/>).
/// </summary>
/// <remarks>
/// It names the word, not the note: a word exists whether or not the corpus has a vocab for it, so a reference to a word without a vocab is valid,<br/>
/// and resolves again once the vocab is created. Finding the vocab it refers to is a lookup (<see cref="Collection.VocabCollection.ReferredToBy"/>).<br/>
/// Renaming a vocab rewrites the references that name it (<see cref="VocabNoteQuestion"/>).
/// </remarks>
public sealed record WordReference
{
   /// <summary>A reference to the word <paramref name="text"/>, with <paramref name="disambiguation"/> to tell apart vocab sharing that text: empty for none.</summary>
   public WordReference(string text, string disambiguation = "")
   {
      Contract.Argument.Assert(text != string.Empty, () => "A word-reference names a word, so its text can't be empty.");
      AssertHasNoDisambiguationMarker(text);
      AssertHasNoDisambiguationMarker(disambiguation);
      Text = text;
      Disambiguation = disambiguation;
   }

   /// <summary>The word itself, as <c>入る</c>.</summary>
   public string Text { get; }

   /// <summary>What tells the vocab meant apart from others with the same <see cref="Text"/>, as <c>masu</c> for <c>入る:masu</c>. Empty for most references.</summary>
   public string Disambiguation { get; }

   /// <summary>Whether the reference has a <see cref="Disambiguation"/>.</summary>
   public bool IsDisambiguated => Disambiguation != string.Empty;

   /// <summary>Whether the reference is one of <paramref name="words"/>, without a disambiguation: <c>え</c> is one of [<c>える</c>, <c>え</c>], <c>え:imperative</c> isn't.</summary>
   public bool IsAPlainWordAmong(IReadOnlySet<string> words) => !IsDisambiguated && words.Contains(Text);

   /// <summary>
   /// The <see cref="VocabNoteQuestion.DisambiguationName"/> of the vocab the reference names: the <see cref="Text"/>, followed by<br/>
   /// <see cref="VocabNoteQuestion.DisambiguationMarker"/> and the <see cref="Disambiguation"/> if it has one, as <c>入る:masu</c>. It's how a reference is shown, and typed.
   /// </summary>
   public string DisambiguationName => IsDisambiguated ? Text + VocabNoteQuestion.DisambiguationMarker + Disambiguation : Text;

   /// <summary>The reference that <paramref name="disambiguationName"/> is the <see cref="DisambiguationName"/> of, as a person types or copies it: <c>入る:masu</c> or <c>入る</c>.</summary>
   public static WordReference FromDisambiguationName(string disambiguationName)
   {
      var parts = disambiguationName.Split(VocabNoteQuestion.DisambiguationMarker);
      Contract.Argument.Assert(parts.Length <= 2, () => $"'{disambiguationName}' has more than one '{VocabNoteQuestion.DisambiguationMarker}', which separates a word's text from its disambiguation.");
      return parts.Length == 2 ? new WordReference(parts[0], parts[1]) : new WordReference(disambiguationName);
   }

   /// <summary>
   /// The reference <paramref name="text"/> is the <see cref="DisambiguationName"/> of, as <see cref="FromDisambiguationName"/> gives it, or null when <paramref name="text"/><br/>
   /// can't be one, as text a person selected in a note can be: empty, starting with the marker, or holding it more than once.
   /// </summary>
   public static WordReference? FromDisambiguationNameOrNone(string text)
   {
      var parts = text.Split(VocabNoteQuestion.DisambiguationMarker);
      return parts.Length <= 2 && parts[0] != string.Empty ? FromDisambiguationName(text) : null;
   }

   internal static WordReference FromDto(WordReferenceDto dto) => new(dto.Text, dto.Disambiguation);

   internal WordReferenceDto ToDto() => new() { Text = Text, Disambiguation = Disambiguation };

   static void AssertHasNoDisambiguationMarker(string text) =>
      Contract.Argument.Assert(!text.Contains(VocabNoteQuestion.DisambiguationMarker),
                               () => $"'{text}' contains '{VocabNoteQuestion.DisambiguationMarker}', which separates a word's text from its disambiguation."
                                   + " They are given separately.");

   public override string ToString() => DisambiguationName;
}
