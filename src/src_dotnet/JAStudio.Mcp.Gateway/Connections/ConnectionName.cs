using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using JAStudio.Mcp.Discovery;

namespace JAStudio.Mcp.Gateway.Connections;

/// <summary>
/// The name the gateway connects to a JAStudio as, which Claude names it by, and which its tools are offered under: <c>anki-main__search_notes</c>.<br/>
/// It says what hosts the JAStudio and the branch of the source checkout it was started on, such as <c>anki-main</c> or <c>standalone-unidic</c>,<br/>
/// and so what code it runs.
/// </summary>
sealed partial record ConnectionName
{
   ConnectionName(string value) => Value = value;

   public string Value { get; }

   /// <summary>
   /// The name for <paramref name="jastudio"/>: what hosts it, then the branch checked out in the source checkout it was started on, such as<br/>
   /// <c>anki-main</c>. A source checkout with no branch checked out, or an add-on folder in no source checkout, such as a copy, gives the name<br/>
   /// of its folder instead of the branch's.
   /// </summary>
   public static async Task<ConnectionName> ForAsync(RunningJAStudio jastudio, CancellationToken cancellationToken) =>
      new(AsPartOfAToolsName($"{jastudio.Host}-{await WhatTheCodeIsCalledAsync(jastudio.AddOnFolder, cancellationToken)}"));

   /// <summary>Parses <paramref name="name"/>, a name Claude gave, such as <c>anki-main</c>.</summary>
   public static ConnectionName Parse(string name) =>
      AsPartOfAToolsName(name) == name ? new ConnectionName(name) : throw new ArgumentException($"'{name}' is no connection's name: one is such as anki-main.", nameof(name));

   /// <summary>This name with <paramref name="number"/> added, such as <c>headless-main-2</c>: a second JAStudio would be named the same as another.</summary>
   public ConnectionName Numbered(int number) => new($"{Value}-{number}");

   /// <summary>The name the gateway offers the tool <paramref name="toolName"/> of the JAStudio connected as this under, such as <c>anki-main__search_notes</c>.</summary>
   public string OfItsTool(string toolName) => $"{Value}__{toolName}";

   public override string ToString() => Value;

   /// <summary>
   /// The branch checked out in the source checkout whose <c>src</c> folder <paramref name="addOnFolder"/> is, or the source checkout's folder's name<br/>
   /// when no branch is checked out there, or when the folder <paramref name="addOnFolder"/> is in is no git checkout, such as a copy, that folder's name.
   /// </summary>
   static async Task<string> WhatTheCodeIsCalledAsync(DirectoryInfo addOnFolder, CancellationToken cancellationToken)
   {
      var sourceCheckout = addOnFolder.Parent ?? addOnFolder;
      if(!IsAGitCheckout(sourceCheckout)) return sourceCheckout.Name;

      var branch = (await ChildProcesses.RunToCompletionAsync(new CommandLine("git", ["branch", "--show-current"]), sourceCheckout, cancellationToken)).Trim();
      return branch.Length > 0 ? branch : sourceCheckout.Name;
   }

   /// <summary>Whether <paramref name="folder"/> is a git checkout: it has <c>.git</c> in it, a folder in a main checkout, and a file in a linked worktree.</summary>
   static bool IsAGitCheckout(DirectoryInfo folder) => Path.Exists(Path.Combine(folder.FullName, ".git"));

   /// <summary><paramref name="name"/> in lower case, with every run of characters a tool's name can't have, such as <c>/</c> in a branch's name, as one <c>-</c>.</summary>
   static string AsPartOfAToolsName(string name) => CharactersAToolsNameCannotHave().Replace(name.ToLowerInvariant(), "-");

   [GeneratedRegex("[^a-z0-9_-]+")]
   private static partial Regex CharactersAToolsNameCannotHave();
}
