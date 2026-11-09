using System;

namespace JAStudio.Core.Note.Commands;

/// <summary>A command of a note type's menus that changes a note, and takes no text, such as accepting a vocab's meaning.</summary>
public sealed class NoteChangingCommand<TNote> : INoteCommand<TNote> where TNote : JPNote
{
   readonly Action<TNote> _run;

   /// <param name="name">The command's <see cref="INoteCommand{TNote}.Name"/>.</param>
   /// <param name="label">What the note's menu shows for the command.</param>
   /// <param name="run">Runs the command on a note.</param>
   public NoteChangingCommand(string name, string label, Action<TNote> run)
   {
      Name = name;
      Label = label;
      _run = run;
   }

   public string Name { get; }
   public string Label { get; }
   public string? TextTaken => null;
   public bool CreatesAVocab => false;

   /// <summary>Runs the command on <paramref name="note"/>, which saves the note.</summary>
   public void Run(TNote note) => _run(note);

   public void RunOn(TNote note, string? text)
   {
      this.AssertGivenNoText(text);
      Run(note);
   }

   public override string ToString() => Name;
}
