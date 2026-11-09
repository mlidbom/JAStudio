using System;

namespace JAStudio.Core.Note.Commands;

/// <summary>A command of a note type's menus that changes a note with a text, such as adding a kanji with a similar meaning to a kanji's.</summary>
public sealed class NoteChangingCommandTakingAText<TNote> : INoteCommand<TNote> where TNote : JPNote
{
   readonly Action<TNote, string> _run;

   /// <param name="name">The command's <see cref="INoteCommand{TNote}.Name"/>.</param>
   /// <param name="label">What the note's menu shows for the command.</param>
   /// <param name="textTaken">What the text the command takes is, such as "the kanji with a similar meaning".</param>
   /// <param name="run">Runs the command on a note, with a text.</param>
   public NoteChangingCommandTakingAText(string name, string label, string textTaken, Action<TNote, string> run)
   {
      Name = name;
      Label = label;
      TextTaken = textTaken;
      _run = run;
   }

   public string Name { get; }
   public string Label { get; }
   public string TextTaken { get; }
   public bool CreatesAVocab => false;

   /// <summary>Runs the command on <paramref name="note"/> with <paramref name="text"/>, which saves the note.</summary>
   public void Run(TNote note, string text) => _run(note, text);

   public void RunOn(TNote note, string? text)
   {
      this.AssertGivenAText(text);
      Run(note, text);
   }

   public override string ToString() => Name;
}
