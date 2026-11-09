using System.Diagnostics.CodeAnalysis;
using Compze.Contracts;

namespace JAStudio.Core.Note.Commands;

/// <summary>
/// The text given to a command run as <see cref="INoteCommand{TNote}.RunOn"/> runs it: one must be given if the command takes a text<br/>
/// (<see cref="INoteCommand{TNote}.TextTaken"/>), and none if it doesn't. Running a command given the wrong one fails, saying what the command takes.
/// </summary>
static class TextGivenToANoteCommand
{
   extension<TNote>(INoteCommand<TNote> @this) where TNote : JPNote
   {
      /// <summary>Fails unless <paramref name="text"/> is null, for a command that takes no text.</summary>
      public void AssertGivenNoText(string? text) =>
         Contract.Argument.Assert(text is null, () => $"The command {@this.Name} takes no text.");

      /// <summary>Fails unless <paramref name="text"/> is given, for a command that takes a text, saying what text it takes.</summary>
      public void AssertGivenAText([NotNull] string? text) =>
         Contract.Argument.Assert(text is not null, () => $"The command {@this.Name} takes a text: {@this.TextTaken}.");
   }
}
