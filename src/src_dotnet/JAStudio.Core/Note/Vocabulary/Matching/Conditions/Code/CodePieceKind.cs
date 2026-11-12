namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Code;

/// <summary>What a piece of a condition's code is (<see cref="CodePiece"/>), so that the tree editor can show each kind as it should be.</summary>
public enum CodePieceKind
{
   /// <summary>The code itself, such as <c>PrecedingToken.IsAGodanVerb</c>.</summary>
   Code,

   /// <summary>A negation, <c>!</c>, which the editor shows in a color that stands out, since one at the start of a line is easy to miss.</summary>
   Negation,

   /// <summary>
   /// The name of a shared-condition (<see cref="Sharing.SharedCondition{TSubject}"/>), such as <c>FollowsAPastTenseStem</c>, which the editor shows in a color of<br/>
   /// its own, as an IDE colors the methods you wrote: it stands for a definition of the corpus's, not for one of JAStudio's conditions.
   /// </summary>
   SharedConditionName,

   /// <summary>A comment, such as the English of a part of speech after its Japanese, <c>// noun › numeric</c>, which the editor shows dimmed.</summary>
   Comment
}
