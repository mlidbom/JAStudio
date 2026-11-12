using System.Collections.Generic;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions;

/// <summary>
/// A <see cref="ConditionLevel{TSubject}"/> as the match-condition editor offers it, whatever its subject: the conditions there are on a word, a token or<br/>
/// a text, which the editor's add menu offers where a condition on that subject goes.
/// </summary>
public interface IConditionLevel
{
   /// <summary>What the conditions are on, such as "a token".</summary>
   string SubjectName { get; }

   /// <summary>What the conditions are on, as code names it in front of a shared-condition defined on it: <c>Token</c> in <c>Token.IsAdverbLike</c>.</summary>
   string SubjectNameInCode { get; }

   /// <summary>The facts about the subject, such as a token being a godan verb: conditions in themselves, which take no value.</summary>
   IReadOnlyList<Condition> Facts { get; }

   /// <summary>The subjects found from this one, such as a word's preceding token, each with the conditions on it.</summary>
   IReadOnlyList<IConditionSubject> Subjects { get; }

   /// <summary>The kinds of condition made from texts typed into the editor, such as <c>EndsWithOneOf</c>.</summary>
   IReadOnlyList<ConditionTestWithTexts> TestsWithTexts { get; }

   /// <summary>A new, empty "all of" group of conditions on this level's subject.</summary>
   Condition NewAllOfGroup();

   /// <summary>A new, empty "any of" group of conditions on this level's subject.</summary>
   Condition NewAnyOfGroup();

   /// <summary>A new shared-condition named <paramref name="name"/> on this level's subject, without its definition yet: <see cref="SharedConditions"/> defines it.</summary>
   internal ISharedCondition NewSharedCondition(string name);

   /// <summary>
   /// Why <paramref name="name"/> can't name a shared-condition on this level's subject, or null when it can: it must be a name in code, a capital<br/>
   /// letter then letters and digits, such as <c>FollowsAPastTenseStem</c>, and none of the other conditions' here, since the code shows them alike.
   /// </summary>
   internal string? WhyItCannotNameASharedCondition(string name);
}
