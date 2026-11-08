using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Importing.RawNotes;

namespace JAStudio.Core.Importing.Checking;

/// <summary>
/// What an import check found about one checked field of a <see cref="RawNote"/>: its question, its external id, or a media field such as its audio.<br/>
/// The report shows the <see cref="Mark"/>, with the <see cref="Value"/> and the <see cref="Finding"/> on hover, and lists <see cref="WhatIsWrong"/> in a column of its own.
/// </summary>
public class FieldCheck(CheckMark mark, string value, string finding, IReadOnlyList<string> whatIsWrong)
{
   public CheckMark Mark { get; } = mark;

   /// <summary>The field's value: the question, the external id, or the names of the files the field references.</summary>
   public string Value { get; } = value;

   /// <summary>What the check found, in full, such as "The file … is in the media layer, used by the sentence …".</summary>
   public string Finding { get; } = finding;

   /// <summary>What's wrong, a short line for each thing, such as "Audio used by the sentence …". Empty unless the mark is orange or red.</summary>
   public IReadOnlyList<string> WhatIsWrong { get; } = whatIsWrong;

   public static FieldCheck Fine(string value, string finding) => new(CheckMark.Fine, value, finding, []);
   public static FieldCheck Neutral(string value, string finding) => new(CheckMark.Neutral, value, finding, []);
   public static FieldCheck AlreadyInJAStudio(string value, string finding, string whatIsWrong) => new(CheckMark.AlreadyInJAStudio, value, finding, [whatIsWrong]);
   public static FieldCheck Problem(string value, string finding, string whatIsWrong) => new(CheckMark.Problem, value, finding, [whatIsWrong]);

   /// <summary>A red mark whose reason is the raw note's as a whole, such as an imported note that's gone from JAStudio, said once for the note rather than for each field.</summary>
   public static FieldCheck ProblemOfTheNoteAsAWhole(string value, string finding) => new(CheckMark.Problem, value, finding, []);

   public static FieldCheck NotMapped() => Neutral("", "The import profile maps no field to this.");

   /// <summary>The check of a media field from the checks of its files, one per file: the worst mark, with every file's finding and what's wrong.</summary>
   public static FieldCheck OfAMediaField(IReadOnlyList<FieldCheck> fileChecks) =>
      fileChecks.Count == 0
         ? Neutral("", "No file.")
         : new FieldCheck(fileChecks.Max(it => it.Mark),
                          string.Join(", ", fileChecks.Select(it => it.Value)),
                          string.Join(Environment.NewLine, fileChecks.Select(it => it.Finding)),
                          [.. fileChecks.SelectMany(it => it.WhatIsWrong)]);

   public override string ToString() => $"{Mark}: {Finding}";
}
