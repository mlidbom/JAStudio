using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static System.FormattableString;

namespace JAStudio.Core.Reporting;

/// <summary>
/// The folder a report is written to: <c>report.md</c>, readable at a glance, built up section by section and saved at the end, and a<br/>
/// tab-separated file for each list in full.
/// </summary>
sealed class ReportFolder
{
   /// <summary>The most rows a table in <c>report.md</c> shows: a longer list is in full in its tab-separated file.</summary>
   public const int RowsInTheMarkdown = 100;

   readonly DirectoryInfo _folder;
   readonly StringBuilder _markdown = new();

   /// <summary>Writes into <paramref name="folder"/>, created now if need be.</summary>
   public ReportFolder(DirectoryInfo folder)
   {
      folder.Create();
      _folder = folder;
   }

   /// <summary>Starts <c>report.md</c> with its title, <paramref name="title"/>.</summary>
   public void Title(string title) => _markdown.AppendLine($"# {title}").AppendLine();

   /// <summary>Starts a section, headed <paramref name="heading"/>, with what it found in a few <paramref name="bullets"/>.</summary>
   public void Section(string heading, params string[] bullets)
   {
      _markdown.AppendLine($"## {heading}").AppendLine();
      if(bullets.Length > 0) Bullets(bullets);
   }

   /// <summary>A list of <paramref name="bullets"/>, each a line of text.</summary>
   public void Bullets(params string[] bullets)
   {
      foreach(var bullet in bullets) _markdown.AppendLine($"- {bullet}");
      _markdown.AppendLine();
   }

   /// <summary>A table of <paramref name="rows"/>, under <paramref name="headings"/>, every row shown.</summary>
   public void Table(string[] headings, IEnumerable<string[]> rows)
   {
      _markdown.AppendLine($"| {string.Join(" | ", headings)} |").AppendLine($"|{string.Concat(headings.Select(_ => "---|"))}");
      foreach(var row in rows) _markdown.AppendLine($"| {string.Join(" | ", row.Select(it => it.Replace("|", "\\|", System.StringComparison.Ordinal)))} |");
      _markdown.AppendLine();
   }

   /// <summary>A table of the first <see cref="RowsInTheMarkdown"/> of <paramref name="rows"/>, and all of them in the tab-separated file <paramref name="fileName"/>.</summary>
   public void TableWithTheRestInAFile(string fileName, string[] headings, IReadOnlyList<string[]> rows)
   {
      Table(headings, rows.Take(RowsInTheMarkdown));
      if(rows.Count > RowsInTheMarkdown) _markdown.AppendLine(Invariant($"The first {RowsInTheMarkdown} of {rows.Count:N0}; all are in `{fileName}.tsv`.")).AppendLine();
      TabSeparatedFile(fileName, headings, rows);
   }

   /// <summary>Writes <paramref name="rows"/>, under <paramref name="headings"/>, to the tab-separated file <paramref name="fileName"/>, a tab in a cell made a space.</summary>
   public void TabSeparatedFile(string fileName, string[] headings, IEnumerable<string[]> rows) =>
      File.WriteAllLines(Path.Combine(_folder.FullName, $"{fileName}.tsv"),
                         rows.Select(it => string.Join('\t', it.Select(cell => cell.Replace('\t', ' ')))).Prepend(string.Join('\t', headings)));

   /// <summary>Saves <c>report.md</c>, as written so far.</summary>
   public void SaveTheMarkdown() => File.WriteAllText(Path.Combine(_folder.FullName, "report.md"), _markdown.ToString());
}
