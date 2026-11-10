using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Tagging;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Mcp.Tools.Collection;

namespace JAStudio.Mcp.Tools.Tagging;

/// <summary>A <see cref="TagTreeNode"/> as <see cref="TagTools.GetTheTagTree"/> reports it: its path, the notes in its branch by note type, and its children.</summary>
/// <param name="TagPath">The whole path, such as <c>source::anime</c>.</param>
/// <param name="Vocab">The vocab in the branch, or null when there are none.</param>
/// <param name="Kanji">The kanji in the branch, or null when there are none.</param>
/// <param name="Sentences">The sentences in the branch, or null when there are none.</param>
/// <param name="Children">The nodes one level below, or null when there are none or they're below the levels listed.</param>
/// <param name="PathsBelowNotListed">How many paths are below the node when they're below the levels listed, otherwise null.</param>
public record TagTreeNodeDescription(
   string TagPath,
   NoteCounts? Vocab,
   NoteCounts? Kanji,
   NoteCounts? Sentences,
   IReadOnlyList<TagTreeNodeDescription>? Children,
   int? PathsBelowNotListed)
{
   /// <summary>The descriptions of <paramref name="nodes"/>, listing <paramref name="levelsListed"/> levels below them.</summary>
   public static IReadOnlyList<TagTreeNodeDescription> Of(IEnumerable<TagTreeNode> nodes, int levelsListed) => [..nodes.Select(it => Of(it, levelsListed))];

   /// <summary>The description of <paramref name="node"/>, listing <paramref name="levelsListed"/> levels below it.</summary>
   public static TagTreeNodeDescription Of(TagTreeNode node, int levelsListed)
   {
      var hasChildren = node.Children.Count > 0;
      var listsTheChildren = hasChildren && levelsListed > 0;
      return new TagTreeNodeDescription(node.TagPath,
                                        CountsOf<VocabNote>(),
                                        CountsOf<KanjiNote>(),
                                        CountsOf<SentenceNote>(),
                                        listsTheChildren ? Of(node.Children, levelsListed - 1) : null,
                                        hasChildren && !listsTheChildren ? node.NodesInTheBranch().Count() - 1 : null);

      NoteCounts? CountsOf<TNote>() where TNote : JPNote
      {
         var notesOfTheType = node.NotesInTheBranch.OfType<TNote>().ToList<JPNote>();
         return notesOfTheType.Count > 0 ? NoteCounts.Of(notesOfTheType) : null;
      }
   }
}
