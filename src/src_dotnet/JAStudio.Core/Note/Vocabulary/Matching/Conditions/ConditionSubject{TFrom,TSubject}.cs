using System;
using System.Text.Json.Nodes;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Tokens;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Words;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions;

/// <summary>
/// A subject that a condition can be on, found from the subject of the condition it's in: a word's preceding token, its first token, the text<br/>
/// after it, a token's surface, and so on. Each is defined once, in <see cref="WordConditions"/> or <see cref="TokenConditions"/>.<br/>
/// A condition on it (<see cref="Meets"/>) is a condition on the subject it's found from.
/// </summary>
public sealed class ConditionSubject<TFrom, TSubject> : IConditionSubject<TFrom>, IConditionPath<TFrom, TSubject> where TFrom : class where TSubject : class
{
   //review:matching: ConditionSubject and SubjectCondition are mirror images, easy to mix up. This is what a condition can be on, such as
   //   a word's preceding token, the design document's condition-subject; a SubjectCondition is the condition that it meets another.
   readonly Func<TFrom, TSubject> _foundIn;

   /// <param name="name">The <see cref="Name"/>.</param>
   /// <param name="jsonKey">The <see cref="JsonKey"/>.</param>
   /// <param name="foundIn">Finds this subject of a subject, which every subject has: where there's no token, it's the empty token (<see cref="Tokens.EmptyToken"/>).</param>
   /// <param name="level">The <see cref="Level"/>.</param>
   internal ConditionSubject(string name, string jsonKey, Func<TFrom, TSubject> foundIn, ConditionLevel<TSubject> level)
   {
      Name = name;
      JsonKey = jsonKey;
      _foundIn = foundIn;
      Level = level;
   }

   /// <summary>The subject's name as the editor shows it, such as <c>PrecedingToken</c>.</summary>
   public string Name { get; }

   /// <summary>The key a condition on this subject is stored under in the corpus, such as <c>precedingToken</c>.</summary>
   public string JsonKey { get; }

   /// <summary>The conditions there are on this subject.</summary>
   public ConditionLevel<TSubject> Level { get; }

   /// <summary>A condition that this subject meets <paramref name="condition"/>.</summary>
   public SubjectCondition<TFrom, TSubject> Meets(Condition<TSubject> condition) => new(this, condition);

   Condition<TFrom> IConditionPath<TFrom, TSubject>.Placed(Condition<TSubject> condition) => Meets(condition);

   /// <summary>This subject of <paramref name="from"/>: the empty token (<see cref="Tokens.EmptyToken"/>) as the preceding token of a sentence's first word.</summary>
   internal TSubject FoundIn(TFrom from) => _foundIn(from);

   IConditionLevel IConditionSubject.Level => Level;

   Condition IConditionSubject.Meets(Condition condition) => Meets(condition.AsAConditionOn<TSubject>());

   Condition<TFrom> IConditionSubject<TFrom>.ConditionFrom(JsonObject json, SharedConditions sharedConditions) => Meets(Level.ConditionFrom(json, sharedConditions));

   public override string ToString() => Name;
}
