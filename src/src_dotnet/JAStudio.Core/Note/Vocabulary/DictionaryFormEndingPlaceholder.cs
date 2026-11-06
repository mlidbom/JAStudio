using System.Collections.Generic;

namespace JAStudio.Core.Note.Vocabulary;

/// <summary>
/// The <c>う</c> at the start of a form of a vocab that requires the dictionary form start, standing in for every verb's dictionary form ending.<br/>
/// It lets such a vocab be entered once, as <c>う限り</c>, instead of once per ending: <c>く限り</c>, <c>ぐ限り</c>, <c>す限り</c>, ….
/// </summary>
/// <remarks>
/// A vocab that requires the dictionary form start only matches starting at the token holding a verb's dictionary form ending,<br/>
/// so the first character of any form it can match is always one of those endings.<br/>
/// A form starting with <c>う</c> therefore means "after any verb", while a form starting with another ending,<br/>
/// like <c>るところ</c>, still means only the verbs with that ending.
/// </remarks>
static class DictionaryFormEndingPlaceholder
{
   const char Placeholder = 'う';

   /// <summary>
   /// The dictionary form endings, other than the <see cref="Placeholder"/> itself, that a form starting with the placeholder also matches.<br/>
   /// <c>る</c> covers ichidan verbs, <c>する</c> and <c>来る</c> as well as the godan verbs ending in <c>る</c>.
   /// </summary>
   static readonly char[] TheOtherDictionaryFormEndings = ['く', 'ぐ', 'す', 'つ', 'ぬ', 'ぶ', 'む', 'る'];

   /// <summary>
   /// <paramref name="form"/> itself, followed, if it starts with the <see cref="Placeholder"/>,<br/>
   /// by the same form starting with each of <see cref="TheOtherDictionaryFormEndings"/> instead.
   /// </summary>
   public static IEnumerable<string> FormAndItsExpansions(string form)
   {
      yield return form;
      if(!form.StartsWith(Placeholder)) yield break;

      var formAfterThePlaceholder = form[1..];
      foreach(var ending in TheOtherDictionaryFormEndings)
      {
         yield return ending + formAfterThePlaceholder;
      }
   }
}
