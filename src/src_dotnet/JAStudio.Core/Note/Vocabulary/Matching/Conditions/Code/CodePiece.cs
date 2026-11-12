using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Code;

/// <summary>A piece of a line of a condition's code (<see cref="CodeLine"/>), such as <c>PrecedingToken.</c> or <c>!</c>, with the kind of piece it is.</summary>
public sealed record CodePiece(string Text, CodePieceKind Kind)
{
   /// <summary>The shared-condition the piece names, when it's a <see cref="CodePieceKind.SharedConditionName"/>: what clicking the name opens.</summary>
   public ISharedCondition? SharedCondition { get; private init; }

   /// <summary>A piece of the code itself, such as <c>PrecedingToken.</c>.</summary>
   internal static CodePiece Of(string code) => new(code, CodePieceKind.Code);

   /// <summary>The negation of what follows it on the line, <c>!</c>.</summary>
   internal static CodePiece Negation { get; } = new("!", CodePieceKind.Negation);

   /// <summary>The name of <paramref name="sharedCondition"/>, as a condition uses it, such as <c>FollowsAPastTenseStem</c>.</summary>
   internal static CodePiece NameOf(ISharedCondition sharedCondition) => new(sharedCondition.Name, CodePieceKind.SharedConditionName) { SharedCondition = sharedCondition };

   /// <summary>The full name of <paramref name="sharedCondition"/>, as where it's defined, such as <c>Word.FollowsAPastTenseStem</c>.</summary>
   internal static CodePiece FullNameOf(ISharedCondition sharedCondition) =>
      new(sharedCondition.FullName, CodePieceKind.SharedConditionName) { SharedCondition = sharedCondition };

   /// <summary>A comment saying <paramref name="text"/>, such as the English of the value the line ends with.</summary>
   internal static CodePiece Comment(string text) => new($"// {text}", CodePieceKind.Comment);
}
