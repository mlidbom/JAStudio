using System;
using System.Collections.Generic;

namespace JAStudio.Core.LanguageServices.JamdictEx;

/// <summary>
/// Forms, such as every word form of the dictionary, kept in ordinal order to tell whether any of them starts with a text: whether a text could<br/>
/// still grow into one of them.
/// </summary>
/// <remarks>
/// In ordinal order the forms starting with a text come right where the text itself would be, so a binary search finds whether any does. It<br/>
/// holds the forms it's given, not copies, so it costs a reference per form, where a set of every start of every form would cost many times<br/>
/// the forms themselves.
/// </remarks>
sealed class FormsInOrder
{
   readonly string[] _forms;

   public FormsInOrder(IEnumerable<string> forms)
   {
      _forms = [.. forms];
      Array.Sort(_forms, StringComparer.Ordinal);
   }

   /// <summary>Whether any of the forms starts with <paramref name="text"/>, or is it.</summary>
   public bool AnyStartsWith(string text)
   {
      var index = Array.BinarySearch(_forms, text, StringComparer.Ordinal);
      if(index >= 0) return true;

      var indexOfTheFirstFormAfterTheText = ~index;
      return indexOfTheFirstFormAfterTheText < _forms.Length && _forms[indexOfTheFirstFormAfterTheText].StartsWith(text, StringComparison.Ordinal);
   }
}
