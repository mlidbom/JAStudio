using System;
using System.Collections.Generic;
using System.Linq;

namespace JAStudio.Core.Note.Search;

/// <summary>
/// Where a condition is in a search's tree of conditions: the index of the child to go down to at each level, starting at the root.<br/>
/// The search editor finds conditions by their position, because two equal conditions can be in different places in the same search.
/// </summary>
public sealed record ConditionPosition
{
   ConditionPosition(IReadOnlyList<int> indexesFromTheRoot) => IndexesFromTheRoot = indexesFromTheRoot;

   /// <summary>The position of the search's root.</summary>
   public static ConditionPosition Root { get; } = new([]);

   /// <summary>The index of the child to go down to at each level, starting at the root. Empty for the root itself.</summary>
   public IReadOnlyList<int> IndexesFromTheRoot { get; }

   public bool IsRoot => IndexesFromTheRoot.Count == 0;

   /// <summary>The position of the child at <paramref name="index"/> of the condition at this position.</summary>
   public ConditionPosition Child(int index) => new([..IndexesFromTheRoot, index]);

   /// <summary>The position of the condition this position's condition is a child of.</summary>
   public ConditionPosition Parent => IsRoot ? throw new InvalidOperationException("The root has no parent.") : new ConditionPosition([..IndexesFromTheRoot.SkipLast(1)]);

   /// <summary>This position's index among the children of its <see cref="Parent"/>.</summary>
   public int IndexInParent => IsRoot ? throw new InvalidOperationException("The root has no parent.") : IndexesFromTheRoot[^1];

   /// <summary>The index of the child of the root to go down to first.</summary>
   internal int FirstIndex => IndexesFromTheRoot[0];

   /// <summary>This position as seen from the child of the root it goes down to first.</summary>
   internal ConditionPosition BelowTheFirstChild => new([..IndexesFromTheRoot.Skip(1)]);

   public bool Equals(ConditionPosition? other) => other is not null && IndexesFromTheRoot.SequenceEqual(other.IndexesFromTheRoot);

   public override int GetHashCode() => IndexesFromTheRoot.Aggregate(0, HashCode.Combine);

   public override string ToString() => IsRoot ? "the root" : string.Join(".", IndexesFromTheRoot);
}
