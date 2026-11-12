using System.Collections.Generic;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions;

/// <summary>
/// A <see cref="ConditionLevel{TSubject}"/> as the match-condition editor offers it, whatever its subject: the conditions there are on a word, a token or<br/>
/// a text, which the editor's add menu offers where a condition on that subject goes.
/// </summary>
public interface IConditionLevel
{
   /// <summary>What the conditions are on, such as "a token".</summary>
   string SubjectName { get; }

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
}
