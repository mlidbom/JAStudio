using Compze.Contracts;
using JAStudio.Core.Storage.Dto;

namespace JAStudio.Core.Note.Vocabulary;

/// <summary>
/// A <see cref="VocabNote"/>'s question: its <see cref="Text"/>, the word itself, and, when another vocab has the same text,<br/>
/// a <see cref="Disambiguation"/> that tells them apart, such as <c>masu</c> for the <c>入る</c> that follows a masu stem.
/// </summary>
/// <remarks>
/// Other notes refer to a vocab by its <see cref="DisambiguationName"/>, <c>入る:masu</c>, until references between notes get a type of their own.
/// </remarks>
public class VocabNoteQuestion
{
   /// <summary>Separates the <see cref="Text"/> from the <see cref="Disambiguation"/> in a <see cref="DisambiguationName"/>, so neither may contain it.</summary>
   public const string DisambiguationMarker = ":";

   readonly VocabNote _vocab;
   readonly NoteGuard _guard;

   public VocabNoteQuestion(VocabNote vocab, VocabDto? data, NoteGuard guard)
   {
      _vocab = vocab;
      _guard = guard;
      Text = data?.Question.Text ?? string.Empty;
      Disambiguation = data?.Question.Disambiguation ?? string.Empty;
   }

   /// <summary>The question itself: the vocab's main form, as <c>入る</c>. Empty only on a vocab that's being created and hasn't been given its question yet.</summary>
   public string Text { get; private set; }

   /// <summary>What tells this vocab apart from others with the same <see cref="Text"/>, as <c>masu</c> in <c>入る:masu</c>. Empty for most vocab.</summary>
   public string Disambiguation { get; private set; }

   /// <summary>Whether the vocab has a <see cref="Disambiguation"/>.</summary>
   public bool IsDisambiguated => Disambiguation != string.Empty;

   /// <summary>
   /// The name other notes refer to the vocab by: the <see cref="Text"/>, followed by the <see cref="DisambiguationMarker"/> and the <see cref="Disambiguation"/><br/>
   /// if the vocab has one, as <c>入る:masu</c>.
   /// </summary>
   public string DisambiguationName => IsDisambiguated ? Text + DisambiguationMarker + Disambiguation : Text;

   public string WithoutNoiseCharacters => Text.Replace(Mine.VocabPrefixSuffixMarker, "");

   public VocabStems Stems() => new(_vocab);

   /// <summary>
   /// Sets the <see cref="Text"/>, keeping the <see cref="Disambiguation"/>. The question is always one of the vocab's forms, so the old one stops being one.
   /// </summary>
   public void SetText(string text)
   {
      Contract.Argument.Assert(text != string.Empty, () => "A vocab's question can't be empty.");
      AssertHasNoDisambiguationMarker(text);
      _guard.Update(() =>
      {
         Text = text;
         _vocab.Forms.OnQuestionChanged();
      });
   }

   /// <summary>Sets the <see cref="Disambiguation"/>: empty for none.</summary>
   public void SetDisambiguation(string disambiguation)
   {
      AssertHasNoDisambiguationMarker(disambiguation);
      _guard.Update(() => Disambiguation = disambiguation);
   }

   internal VocabQuestionDto ToDto() => new() { Text = Text, Disambiguation = Disambiguation };

   static void AssertHasNoDisambiguationMarker(string text) =>
      Contract.Argument.Assert(!text.Contains(DisambiguationMarker),
                               () => $"'{text}' contains '{DisambiguationMarker}', which separates a vocab's question from its disambiguation when other notes refer to it."
                                   + " The question and the disambiguation are set separately.");

   public override string ToString() => Text;
}
