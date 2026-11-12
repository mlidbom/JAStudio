using System.Collections.Generic;
using System.Text.Json.Nodes;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Code;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Texts;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions;

/// <summary>
/// A condition that a <see cref="ConditionSubject{TFrom,TSubject}"/> meets a <see cref="Condition"/>, such as "the preceding token is a godan verb".<br/>
/// Made by <see cref="ConditionSubject{TFrom,TSubject}.Meets"/>.
/// </summary>
/// <remarks>
/// Every subject is found, so the condition is tested on it as written: where there's no token, such as before a sentence's first word, the subject<br/>
/// is the empty token (<see cref="Tokens.EmptyToken"/>), which has no part of speech and no facts. So "the preceding token isn't a godan verb" and<br/>
/// "it's not so that the preceding token is a godan verb" mean the same, and are both met at a sentence's start.
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

   public override bool IsMetBy(TFrom subject) => IsEmpty || Condition.IsMetBy(Subject.FoundIn(subject));

   /// <summary>The <see cref="Condition"/>'s code, with the subject's name and a dot in front of it, such as <c>PrecedingToken.IsAGodanVerb</c>.</summary>
   internal override IEnumerable<CodeLine> CodeLines(CodeWriting writing) => Condition.CodeLines(writing.ForTheConditionHeldWith(CodePiece.Of($"{Subject.Name}.")));

   internal override JsonObject ToJson() => new() { [Subject.JsonKey] = Condition.ToJson() };

   internal override IEnumerable<TextSet> ComparedTextSets() => Condition.ComparedTextSets();

   protected override Conditions.Condition WithChildReplaced(int index, Conditions.Condition replacement) => Subject.Meets(replacement.AsAConditionOn<TSubject>());

   /// <summary>Whether <paramref name="other"/> is a condition on the same subject, which it must meet a condition meaning the same as this one's.</summary>
   protected override bool MeansTheSameAsAConditionNotShared(Conditions.Condition other) =>
      other is SubjectCondition<TFrom, TSubject> onASubject && onASubject.Subject == Subject && Condition.MeansTheSameAs(onASubject.Condition);
}
