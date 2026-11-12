namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Code;

/// <summary>A piece of a line of a condition's code (<see cref="CodeLine"/>), such as <c>PrecedingToken.</c> or <c>!</c>, with the kind of piece it is.</summary>
public sealed record CodeText(string Text, CodeTextKind Kind)
{
   /// <summary>A piece of the code itself, such as <c>PrecedingToken.</c>.</summary>
   internal static CodeText Of(string code) => new(code, CodeTextKind.Code);

   /// <summary>The negation of what follows it on the line, <c>!</c>.</summary>
   internal static CodeText Negation { get; } = new("!", CodeTextKind.Negation);

   /// <summary>A comment saying <paramref name="text"/>, such as the English of the value the line ends with.</summary>
   internal static CodeText Comment(string text) => new($"// {text}", CodeTextKind.Comment);
}
