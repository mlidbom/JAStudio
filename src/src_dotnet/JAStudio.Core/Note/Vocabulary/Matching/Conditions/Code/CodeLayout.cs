using System.Collections.Generic;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Code;

/// <summary>
/// How a condition's code is laid out as it's shown (<see cref="ShownCodeLine"/>). What a line opens with a parenthesis, a group's conditions or a list's<br/>
/// values, each start at the column right after the parenthesis, as code is indented in a programming language (Magnus, 2026-11-12: "What makes it<br/>
/// read well to me is mostly the indentation"). Where the code is only read, the first of them starts on the opening line; in the tree editor, where<br/>
/// each condition is a line of its own, it starts on the next line.
/// </summary>
/// <remarks>
/// The columns are counted in characters, which a monospace font shows as one cell each. What comes before a parenthesis is always code, names and<br/>
/// <c>!</c>, so a Japanese character, wider than a cell, never shifts a column.
/// </remarks>
public sealed class CodeLayout
{
   /// <summary>What a line deeper than one that opens nothing is indented by for each level, such as a definition under its shared-condition's name.</summary>
   public const int IndentationPerLevel = 3;

   /// <summary>
   /// Where the code is only read: <c>AnyOf(IsAGodanVerb,</c>, then <c>IsAnIchidanVerb)</c> under <c>IsAGodanVerb</c>. As the window listing the<br/>
   /// shared-conditions, a vocab's Matching tab and the MCP server show it.
   /// </summary>
   public static CodeLayout ForReading { get; } = new(theFirstStartsOnTheOpeningLine: true);

   /// <summary>
   /// In the tree editor, where each condition is a line of its own: <c>AnyOf(</c>, then <c>IsAGodanVerb,</c> and <c>IsAnIchidanVerb)</c> under it, each<br/>
   /// starting at the column right after its parenthesis.
   /// </summary>
   public static CodeLayout ForTheTreeEditor { get; } = new(theFirstStartsOnTheOpeningLine: false);

   readonly bool _theFirstStartsOnTheOpeningLine;

   CodeLayout(bool theFirstStartsOnTheOpeningLine) => _theFirstStartsOnTheOpeningLine = theFirstStartsOnTheOpeningLine;

   /// <summary>
   /// <paramref name="lines"/>, the lines of a condition, laid out as they're shown, the first starting at <paramref name="column"/>, as a line in the tree<br/>
   /// editor starts after the parenthesis of its group. A line deeper than one that opens nothing, such as the definition under<br/>
   /// <c>Word.FollowsAVerb =</c>, is indented under it.
   /// </summary>
   public IReadOnlyList<ShownCodeLine> LinesShowing(IReadOnlyList<CodeLine> lines, int column = 0)
   {
      List<ShownCodeLine> shown = [];
      Dictionary<int, int> columnOfEachDepth = [];
      CodeLine? previous = null;
      foreach(var line in lines)
      {
         if(previous is { OpensWhatFollows: true })
            StartWhatThePreviousLineOpens(line);
         else
            shown.Add(ShownCodeLine.StartingAt(ColumnOnALineOfItsOwn(line), line));
         previous = line;
      }

      return shown;

      void StartWhatThePreviousLineOpens(CodeLine line)
      {
         var columnAfterTheParenthesis = shown[^1].Width;
         columnOfEachDepth[line.Depth] = columnAfterTheParenthesis;
         if(_theFirstStartsOnTheOpeningLine)
            shown[^1] = shown[^1].FollowedBy(line);
         else
            shown.Add(ShownCodeLine.StartingAt(columnAfterTheParenthesis, line));
      }

      int ColumnOnALineOfItsOwn(CodeLine line) =>
         columnOfEachDepth.GetValueOrDefault(line.Depth, column + (line.Depth - lines[0].Depth) * IndentationPerLevel);
   }
}
