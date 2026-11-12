using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Code;

namespace JAStudio.UI.ViewModels.Workspace.Editors.MatchConditions;

/// <summary>
/// A piece of a line of code in the match-condition's tree editor (<see cref="CodeLineViewModel"/>), shown as its kind should be: a negation in a color<br/>
/// that stands out, since a <c>!</c> at the start of a line is easy to miss, and a comment dimmed.
/// </summary>
class CodePieceViewModel
{
   readonly CodeTextKind _kind;

   CodePieceViewModel(string text, CodeTextKind kind)
   {
      Text = text;
      _kind = kind;
   }

   /// <summary>The piece of the line from a condition's code.</summary>
   public static CodePieceViewModel Of(CodeText text) => new(text.Text, text.Kind);

   /// <summary>Spaces of code, such as those that indent a line under the line it continues, or that keep a comment apart from the code before it.</summary>
   public static CodePieceViewModel Spaces(string spaces) => new(spaces, CodeTextKind.Code);

   public string Text { get; }

   public bool IsNegation => _kind == CodeTextKind.Negation;

   public bool IsComment => _kind == CodeTextKind.Comment;
}
