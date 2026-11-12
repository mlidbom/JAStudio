using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.ConditionTrees;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Code;

/// <summary>
/// One line of a condition written as code (<see cref="ConditionCode"/>), made of pieces of each kind (<see cref="Texts"/>), with the <see cref="Position"/><br/>
/// in the tree of the condition it's a line of, and how deep in the code it is.
/// </summary>
/// <remarks>
/// A condition takes more than one line when it's a group, whose conditions go on lines of their own under it, or when it lists values with their<br/>
/// English, one to a line, such as <c>InflectedForm.In(</c> with <c>連用形,</c> and <c>連用タ接続)</c> under it.
/// </remarks>
public sealed class CodeLine
{
   const string BeforeTheComment = "   ";

   internal CodeLine(ConditionPosition position, int depth, IReadOnlyList<CodeText> texts)
   {
      Position = position;
      Depth = depth;
      Texts = texts;
   }

   /// <summary>
   /// Where the condition this is a line of is in the tree: the whole condition the match-condition editor shows on one line of its own, with the<br/>
   /// negations and subjects in front of it, such as <c>!PrecedingToken.IsAGodanVerb</c>.
   /// </summary>
   public ConditionPosition Position { get; }

   /// <summary>How deep the line is in the code: a group's conditions are one deeper than the group, and so are the values under a condition's first line.</summary>
   public int Depth { get; }

   /// <summary>The line's pieces, in order, such as <c>!</c>, <c>PrecedingToken.</c> and <c>IsAGodanVerb</c>.</summary>
   public IReadOnlyList<CodeText> Texts { get; }

   /// <summary>The line's code, without its comment.</summary>
   public string Code => string.Concat(Texts.Where(it => it.Kind != CodeTextKind.Comment).Select(it => it.Text));

   /// <summary>The line as text, its comment after its code.</summary>
   public override string ToString() =>
      string.Join(BeforeTheComment, new[] { Code, string.Concat(Texts.Where(it => it.Kind == CodeTextKind.Comment).Select(it => it.Text)) }.Where(it => it.Length > 0));
}
