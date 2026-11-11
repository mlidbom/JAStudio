using System.Collections.Generic;
using System.Text.Json.Nodes;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Texts;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions;

/// <summary>
/// A condition that a <see cref="ConditionSubject{TFrom,TSubject}"/> meets a <see cref="Condition"/>, such as "the preceding token is a godan verb".<br/>
/// Made by <see cref="ConditionSubject{TFrom,TSubject}.Meets"/>.
/// </summary>
/// <remarks>
/// A subject can be absent, as the sentence's first token has no preceding token. Then the condition isn't met: "the preceding token: not a<br/>
/// godan verb" isn't met at the sentence's start, while "not: the preceding token: a godan verb" is.
/// </remarks>
public sealed record SubjectCondition<TFrom, TSubject> : Condition<TFrom>, ISubjectCondition where TFrom : class where TSubject : class
{
   internal SubjectCondition(ConditionSubject<TFrom, TSubject> subject, Condition<TSubject> condition)
   {
      Subject = subject;
      Condition = condition;
   }

   /// <summary>The subject the <see cref="Condition"/> is on.</summary>
   public ConditionSubject<TFrom, TSubject> Subject { get; }

   /// <summary>The condition the <see cref="Subject"/> must meet.</summary>
   public Condition<TSubject> Condition { get; }

   IConditionSubject ISubjectCondition.Subject => Subject;

   Conditions.Condition ISubjectCondition.Condition => Condition;

   public override IReadOnlyList<Conditions.Condition> Children => [Condition];

   public override bool IsEmpty => Condition.IsEmpty;

   public override bool IsMetBy(TFrom subject) => IsEmpty || SubjectIsFoundAndMeetsTheCondition(subject);

   /// <summary>The <see cref="Condition"/>'s own lines, with the subject's name in front of the first, such as "The preceding token: is a godan verb".</summary>
   public override IEnumerable<string> DescriptionLines() => Condition.DescriptionLinesPrefixedWith($"{Subject.Name}: ");

   internal override JsonObject ToJson() => new() { [Subject.JsonKey] = Condition.ToJson() };

   internal override IEnumerable<TextSet> ComparedTextSets() => Condition.ComparedTextSets();

   protected override Conditions.Condition WithChildReplaced(int index, Conditions.Condition replacement) => Subject.Meets(replacement.AsAConditionOn<TSubject>());

   /// <summary>Whether the <see cref="Subject"/> is found in <paramref name="from"/> and meets the <see cref="Condition"/>: an absent subject meets no condition.</summary>
   bool SubjectIsFoundAndMeetsTheCondition(TFrom from) => Subject.FoundIn(from) is {} found && Condition.IsMetBy(found);
}
