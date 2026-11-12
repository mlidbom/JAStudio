using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JAStudio.UI.Menus.UIAgnosticMenuStructure;

namespace JAStudio.UI.Utils;

/// <summary>
/// Picks the keys of a menu whose items come from a catalog rather than being written out, such as the tree editor's add menu, which<br/>
/// offers the conditions there are on a subject. An item gets the first letter of a word in its name that no item before it took, skipping words<br/>
/// such as "the" and "is", else the first such letter in those words, else anywhere in its name, else no key.
/// </summary>
static class AutomaticMnemonics
{
   static readonly HashSet<string> WordsThatSayNothing = ["the", "a", "an", "is", "of", "its", "by", "has", "in"];

   /// <summary><paramref name="menu"/>'s items, in their order, each with the key picked for it from its name (<see cref="SpecMenuItem.WithAcceleratorKey"/>), or as it is when it gets none.</summary>
   public static List<SpecMenuItem> WithKeysPicked(IReadOnlyList<SpecMenuItem> menu)
   {
      var taken = new HashSet<char>();
      return [..menu.Select(WithItsKeyPicked)];

      SpecMenuItem WithItsKeyPicked(SpecMenuItem item)
      {
         if(KeysToPickFrom(item.Name).Where(it => !taken.Contains(it)).Cast<char?>().FirstOrDefault() is not {} key) return item;

         taken.Add(key);
         return item.WithAcceleratorKey(key);
      }
   }

   /// <summary>The keys an item named <paramref name="itemName"/> may get, the best first.</summary>
   static IEnumerable<char> KeysToPickFrom(string itemName) =>
      FirstLettersOfTheWordsThatSaySomething(itemName).Concat(LettersOfTheWordsThatSaySomething(itemName)).Concat(LettersOf(itemName));

   static IEnumerable<char> FirstLettersOfTheWordsThatSaySomething(string itemName) => WordsThatSaySomething(itemName).Select(it => it[0]).Where(IsAKey);

   static IEnumerable<char> LettersOfTheWordsThatSaySomething(string itemName) => WordsThatSaySomething(itemName).SelectMany(it => it).Where(IsAKey);

   /// <summary>
   /// The words of <paramref name="itemName"/> that say something, lowercased: those that spaces separate, and in a name from code, those its dots and<br/>
   /// capitals separate, so that <c>PrecedingToken</c> is "preceding" and "token", and <c>IsAGodanVerb</c> says "godan" and "verb".
   /// </summary>
   static IEnumerable<string> WordsThatSaySomething(string itemName) =>
      WordBoundaryInCode.Replace(itemName, " ").ToLowerInvariant().Split([' ', '.']).Where(it => it.Length > 0 && !WordsThatSayNothing.Contains(it));

   /// <summary>Where a new word starts in a name from code: before a capital that follows a lowercase letter, or that starts a word of more than one letter.</summary>
   static readonly Regex WordBoundaryInCode = new("(?<=[a-z])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])", RegexOptions.CultureInvariant);

   static IEnumerable<char> LettersOf(string itemName) => itemName.ToLowerInvariant().Where(IsAKey);

   static bool IsAKey(char character) => character is >= 'a' and <= 'z';
}
