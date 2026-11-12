using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Compze.Contracts;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Code;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Texts;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing;

/// <summary>
/// A condition with a name, defined once in the corpus and used by name in the match-condition of any vocab, as a fact is, such as<br/>
/// <c>FollowsAPastTenseStem</c>: a subject meets it when it meets its <see cref="Definition"/>. It's on one kind of subject, and offered wherever a<br/>
/// condition on that subject goes: one on a token under any token, as <c>PrecedingToken.IsAdverbLike</c>. The corpus's are <see cref="SharedConditions"/>.
/// </summary>
/// <remarks>
/// Every match-condition that uses it holds this same object, so a word is tested against its definition as it is now. Two are equal when their<br/>
/// names are, since a name is defined once on each kind of subject. Its definition may use other shared-conditions, but never itself, directly or<br/>
/// through others.
/// </remarks>
public sealed record SharedCondition<TSubject> : Condition<TSubject>, ISharedCondition where TSubject : class
{
   Condition<TSubject>? _definition;

   /// <summary>A shared-condition without its definition yet, which <see cref="SharedConditions"/> gives it before anything uses it.</summary>
   internal SharedCondition(string name, ConditionLevel<TSubject> level)
   {
      Name = name;
      Level = level;
   }

   public string Name { get; }

   public string FullName => $"{Level.SubjectNameInCode}.{Name}";

   /// <summary>The conditions on its subject, among which it's offered.</summary>
   public ConditionLevel<TSubject> Level { get; }

   IConditionLevel ISharedCondition.Level => Level;

   /// <summary>The condition it stands for: a subject meets the shared-condition when it meets this.</summary>
   public Condition<TSubject> Definition => _definition ?? throw new InvalidOperationException($"The shared-condition {FullName} is used before it's defined.");

   Condition ISharedCondition.Definition => Definition;

   Condition ISharedCondition.AsACondition => this;

   /// <summary>Gives it <paramref name="definition"/>.</summary>
   internal void Define(Condition<TSubject> definition) => _definition = definition;

   void ISharedCondition.DefineFrom(JsonObject json, SharedConditions sharedConditions) => Define(Level.ConditionFrom(json, sharedConditions));

   void ISharedCondition.RewriteItsDefinitionAs(Condition definition)
   {
      Contract.Argument.Assert(definition.MeansTheSameAs(Definition), () => $"{FullName}'s definition rewritten as {definition}, which doesn't mean the same as {Definition}.");
      Define(definition.AsAConditionOn<TSubject>());
   }

   bool ISharedCondition.UsesItself
   {
      get
      {
         return UsedIn(Definition, lookedInto: []);

         bool UsedIn(Condition condition, HashSet<ISharedCondition> lookedInto) =>
            condition.SharedConditionsUsed().Any(used => used.Equals(this) || lookedInto.Add(used) && UsedIn(used.Definition, lookedInto));
      }
   }

   public override bool IsEmpty => Definition.IsEmpty;

   public override bool IsMetBy(TSubject subject) => Definition.IsMetBy(subject);

   /// <summary>Its name, in the color of a shared-condition's (<see cref="CodePieceKind.SharedConditionName"/>), as a fact's is written: <c>FollowsAPastTenseStem</c>.</summary>
   internal override IEnumerable<CodeLine> CodeLines(CodeWriting writing) => [writing.LineOfASharedCondition(Name)];

   /// <summary>The key the name of a shared-condition that a match-condition uses is stored under in the corpus.</summary>
   internal const string JsonKey = "sharedCondition";

   internal override JsonObject ToJson() => new() { [JsonKey] = Name };

   public override IEnumerable<ISharedCondition> SharedConditionsUsed() => [this];

   /// <summary>Its definition's: a shared-condition means what its definition does (<see cref="Condition.MeansTheSameAs"/>).</summary>
   protected override bool MeansTheSameAsAConditionNotShared(Condition other) => Definition.MeansTheSameAs(other);

   /// <summary>Its definition's, since it means what its definition does.</summary>
   internal override int HashCodeOfItsMeaning() => Definition.HashCodeOfItsMeaning();

   /// <summary>Its definition's, so that a matching rule whose condition is a shared-condition's definition sees its texts in the shared-condition too.</summary>
   internal override IEnumerable<TextSet> ComparedTextSets() => Definition.ComparedTextSets();

   /// <summary>Each name is defined once on each kind of subject, so two are the same shared-condition when their names are.</summary>
   public bool Equals(SharedCondition<TSubject>? other) => other is not null && Name == other.Name;

   public override int GetHashCode() => Name.GetHashCode();
}
