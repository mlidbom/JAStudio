namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Code;

/// <summary>A piece of a line of a condition's code (<see cref="CodeLine"/>), such as <c>PrecedingToken.</c> or <c>!</c>, with the kind of piece it is.</summary>
public sealed record CodePiece(string Text, CodePieceKind Kind)
{
   /// <summary>A piece of the code itself, such as <c>PrecedingToken.</c>.</summary>
   internal static CodePiece Of(string code) => new(code, CodePieceKind.Code);

   /// <summary>The negation of what follows it on the line, <c>!</c>.</summary>
   internal static CodePiece Negation { get; } = new("!", CodePieceKind.Negation);

   /// <summary>The name of a shared-condition, such as <c>FollowsAPastTenseStem</c>, or <c>Word.FollowsAPastTenseStem</c> where it's defined.</summary>
   internal static CodePiece SharedConditionName(string name) => new(name, CodePieceKind.SharedConditionName);

   /// <summary>A comment saying <paramref name="text"/>, such as the English of the value the line ends with.</summary>
   internal static CodePiece Comment(string text) => new($"// {text}", CodePieceKind.Comment);
}
