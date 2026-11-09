using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Commands;

namespace JAStudio.Mcp.Tools.Editing.Commands;

/// <summary>
/// One of a note's <see cref="INoteCommand{TNote}"/>s, together with the note: the command run on that note, whatever its type.<br/>
/// What <see cref="NoteCommandTools"/> lists and runs.
/// </summary>
public sealed class CommandOfANote
{
   readonly Action<string?> _run;

   CommandOfANote(string name, string label, string? textTaken, bool createsAVocab, Action<string?> run)
   {
      Name = name;
      Label = label;
      TextTaken = textTaken;
      CreatesAVocab = createsAVocab;
      _run = run;
   }

   /// <summary>Each of <paramref name="commands"/> on <paramref name="note"/>.</summary>
   public static IReadOnlyList<CommandOfANote> EachOf<TNote>(IEnumerable<INoteCommand<TNote>> commands, TNote note) where TNote : JPNote =>
      [.. commands.Select(it => new CommandOfANote(it.Name, it.Label, it.TextTaken, it.CreatesAVocab, text => it.RunOn(note, text)))];

   /// <summary>See <see cref="INoteCommand{TNote}.Name"/>.</summary>
   public string Name { get; }

   /// <summary>See <see cref="INoteCommand{TNote}.Label"/>.</summary>
   public string Label { get; }

   /// <summary>See <see cref="INoteCommand{TNote}.TextTaken"/>.</summary>
   public string? TextTaken { get; }

   /// <summary>See <see cref="INoteCommand{TNote}.CreatesAVocab"/>.</summary>
   public bool CreatesAVocab { get; }

   /// <summary>Runs the command on the note, with <paramref name="text"/>: see <see cref="INoteCommand{TNote}.RunOn"/>.</summary>
   public void Run(string? text) => _run(text);
}
