using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using JAStudio.Core.Note.Tagging;
using JAStudio.UI.Views.Workspace.Navigation.TagManagement;

namespace JAStudio.UI.ViewModels.Workspace.Navigation.TagManagement;

/// <summary>
/// A line of the tag tree in the <see cref="TagManagementView"/>: a <see cref="TagTreeNode"/>'s last level and the number of notes in its branch,<br/>
/// with the lines of the paths one level below it, which show when it's expanded.
/// </summary>
partial class TagTreeNodeViewModel : ObservableObject
{
   readonly TagTreeNode _node;
   readonly ISet<string> _expandedPaths;

   /// <param name="node">The path the line shows.</param>
   /// <param name="expandedPaths">The paths whose lines are expanded. The line starts expanded if its path is in it, and keeps it up to date.</param>
   public TagTreeNodeViewModel(TagTreeNode node, ISet<string> expandedPaths)
   {
      _node = node;
      _expandedPaths = expandedPaths;
      IsExpanded = expandedPaths.Contains(node.TagPath);
      Children = [..node.Children.Select(it => new TagTreeNodeViewModel(it, expandedPaths))];
   }

   public string TagPath => _node.TagPath;

   /// <summary>What the line shows for the path: its last level, such as <c>natsume</c>, below the line of its parent.</summary>
   public string LastLevel => _node.LastLevel;

   /// <summary>How many notes have the path, or a tag below it, shown after the <see cref="LastLevel"/>.</summary>
   public int NumberOfNotesInTheBranch => _node.NumberOfNotesInTheBranch;

   public IReadOnlyList<TagTreeNodeViewModel> Children { get; }

   /// <summary>Whether the line shows its <see cref="Children"/>. Bound to the line's expander.</summary>
   [ObservableProperty] public partial bool IsExpanded { get; set; }

   partial void OnIsExpandedChanged(bool value)
   {
      if(value)
         _expandedPaths.Add(TagPath);
      else
         _expandedPaths.Remove(TagPath);
   }

   public IEnumerable<TagTreeNodeViewModel> ThisAndAllBelow() => [this, ..Children.SelectMany(it => it.ThisAndAllBelow())];
}
