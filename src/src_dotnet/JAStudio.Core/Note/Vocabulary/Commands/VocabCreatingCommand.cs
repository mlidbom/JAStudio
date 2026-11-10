using System;
using JAStudio.Core.Note.Commands;

namespace JAStudio.Core.Note.Vocabulary.Commands;

/// <summary>A command of a vocab's menus that creates a vocab from it, and takes no text, such as creating its て-form (<see cref="VocabCloner"/>).</summary>
public sealed class VocabCreatingCommand : INoteCommand<VocabNote>
{
   readonly Func<VocabCloner, VocabNote> _create;

   /// <param name="name">The command's <see cref="INoteCommand{TNote}.Name"/>.</param>
   /// <param name="label">What the vocab's menu shows for the command.</param>
   /// <param name="create">Creates the vocab, with the <see cref="VocabCloner"/> of the vocab it's created from.</param>
   public VocabCreatingCommand(string name, string label, Func<VocabCloner, VocabNote> create)
   {
      Name = name;
      Label = label;
      _create = create;
   }

   public string Name { get; }
   public string Label { get; }
   public string? TextTaken => null;
   public bool CreatesAVocab => true;

   /// <summary>Creates a vocab from <paramref name="vocab"/>, and adds it to the collection: the vocab created, or the one that had its question already.</summary>
   public VocabNote CreateFrom(VocabNote vocab) => _create(vocab.Cloner);

   public void RunOn(VocabNote note, string? text)
   {
      this.AssertGivenNoText(text);
      CreateFrom(note);
   }

   public override string ToString() => Name;
}
