using System;
using Compze.Contracts;
using JAStudio.Core.Storage.Dto;

namespace JAStudio.Core.Note.Vocabulary;

/// <summary>
/// A <see cref="VocabNote"/>'s question: its <see cref="Text"/>, the word itself, and, when another vocab has the same text,<br/>
/// a <see cref="Disambiguation"/> that tells them apart, such as <c>masu</c> for the <c>入る</c> that follows a masu stem.
/// </summary>
/// <remarks>
/// Other notes refer to a vocab by a <see cref="WordReference"/> that names it (<see cref="Reference"/>), such as a synonym or a compound part.
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

   /// <summary>Whether the vocab has been given its question: false only while it's being created.</summary>
   public bool HasText => Text != string.Empty;

   /// <summary>Whether the vocab has a <see cref="Disambiguation"/>.</summary>
   public bool IsDisambiguated => Disambiguation != string.Empty;

   /// <summary>
   /// The name the vocab is shown and typed by where it has to be told apart from others: the <see cref="Text"/>, followed by the <see cref="DisambiguationMarker"/><br/>
   /// and the <see cref="Disambiguation"/> if the vocab has one, as <c>入る:masu</c>.
   /// </summary>
   public string DisambiguationName => IsDisambiguated ? Text + DisambiguationMarker + Disambiguation : Text;

   /// <summary>The <see cref="WordReference"/> that names this vocab exactly: its <see cref="Text"/> and its <see cref="Disambiguation"/>. Other notes refer to it by it.</summary>
   public WordReference Reference => new(Text, Disambiguation);

   /// <summary>Whether <paramref name="reference"/> names this vocab exactly, its <see cref="Disambiguation"/> included.</summary>
   public bool IsNamedBy(WordReference reference) => reference.Text == Text && reference.Disambiguation == Disambiguation;

   public string WithoutNoiseCharacters => Text.Replace(Mine.VocabPrefixSuffixMarker, "");

   public VocabStems Stems() => new(_vocab);

   /// <summary>
   /// Sets the <see cref="Text"/>, keeping the <see cref="Disambiguation"/>. The question is always one of the vocab's forms, so the old one stops being one.<br/>
   /// The references that named the vocab by its old name are renamed with it (<see cref="RenamingTheReferencesToIt"/>).
   /// </summary>
   public void SetText(string text)
   {
      Contract.Argument.Assert(text != string.Empty, () => "A vocab's question can't be empty.");
      AssertHasNoDisambiguationMarker(text);
      RenamingTheReferencesToIt(() => _guard.Update(() =>
      {
         Text = text;
         _vocab.Forms.OnQuestionChanged();
      }));
   }

   /// <summary>
   /// Sets the <see cref="Disambiguation"/>: empty for none. The references that named the vocab by its old name are renamed with it<br/>
   /// (<see cref="RenamingTheReferencesToIt"/>).
   /// </summary>
   public void SetDisambiguation(string disambiguation)
   {
      AssertHasNoDisambiguationMarker(disambiguation);
      RenamingTheReferencesToIt(() => _guard.Update(() => Disambiguation = disambiguation));
   }

   /// <summary>
   /// Renames the vocab with <paramref name="rename"/>, then renames the word-references that named it by its old name, so they keep naming it<br/>
   /// (<see cref="Collection.VocabCollection.RenameTheReferences"/>). A vocab given its first question, as it's created, had no name to be referred to by.
   /// </summary>
   void RenamingTheReferencesToIt(Action rename)
   {
      var oldName = HasText ? Reference : null;
      rename();
      if(oldName is not null && !IsNamedBy(oldName))
         _vocab.Services.Collection.Vocab.RenameTheReferences(oldName, Reference);
   }

   internal VocabQuestionDto ToDto() => new() { Text = Text, Disambiguation = Disambiguation };

   static void AssertHasNoDisambiguationMarker(string text) =>
      Contract.Argument.Assert(!text.Contains(DisambiguationMarker),
                               () => $"'{text}' contains '{DisambiguationMarker}', which separates a vocab's question from its disambiguation when other notes refer to it."
                                   + " The question and the disambiguation are set separately.");

   public override string ToString() => Text;
}
