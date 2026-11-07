using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace JAStudio.UI.Utils;

/// <summary>
/// An <see cref="ObservableCollection{T}"/> whose items can all be replaced at once, such as a search's results by those of the next search,<br/>
/// with a single notification that the whole collection changed.
/// </summary>
/// <remarks>
/// A view showing the collection keeps its state through the replacement, since the collection is still the same object.<br/>
/// A grid sorted by a column, for example, sorts the new items by that column, once, rather than losing its sort to a new collection<br/>
/// or re-sorting for every item added.
/// </remarks>
class ReplaceableObservableCollection<T> : ObservableCollection<T>
{
   /// <summary>Replaces every item with <paramref name="items"/>, and then raises a single <see cref="NotifyCollectionChangedAction.Reset"/>.</summary>
   public void ReplaceAllWith(IEnumerable<T> items)
   {
      CheckReentrancy();
      Items.Clear();
      foreach(var item in items)
         Items.Add(item);

      OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
      OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
      OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
   }
}
