using System;
using JAStudio.Core.Note.Commands;

namespace JAStudio.Core.Note.Vocabulary.Commands;

/// <summary>
/// A command of a vocab's menus that creates a vocab from it with a text, such as one with the text suffixed onto the vocab's い-stem<br/>
/// (<see cref="VocabCloner"/>).
/// </summary>
public sealed class VocabCreatingCommandTakingAText : INoteCommand<VocabNote>
{
   readonly Func<VocabCloner, string, VocabNote> _create;

   /// <param name="name">The command's <see cref="INoteCommand{TNote}.Name"/>.</param>
   /// <param name="label">What the vocab's menu shows for the command.</param>
   /// <param name="textTaken">What the text the command takes is, such as "the text to suffix onto the vocab's い-stem".</param>
   /// <param name="create">Creates the vocab with a text, with the <see cref="VocabCloner"/> of the vocab it's created from.</param>
   public VocabCreatingCommandTakingAText(string name, string label, string textTaken, Func<VocabCloner, string, VocabNote> create)
   {
      Name = name;
      Label = label;
      TextTaken = textTaken;
      _create = create;
   }

   public string Name { get; }
   public string Label { get; }
   public string TextTaken { get; }
   public bool CreatesAVocab => true;

   /// <summary>
   /// Creates a vocab from <paramref name="vocab"/> with <paramref name="text"/>, and adds it to the collection: the vocab created, or the one that had its<br/>
   /// question already.
   /// </summary>
   public VocabNote CreateFrom(VocabNote vocab, string text) => _create(vocab.Cloner, text);

   public void RunOn(VocabNote note, string? text)
   {
      this.AssertGivenAText(text);
      CreateFrom(note, text);
   }

   public override string ToString() => Name;
}
