using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Code;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Texts;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Tokens;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Words;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions;

/// <summary>
/// Something about a subject that is either so or not, such as a token being a godan verb, or a word being a single token. It's a condition in<br/>
/// itself. Each is defined once, in <see cref="WordConditions"/>, <see cref="TokenConditions"/> or <see cref="TextConditions"/>, and is known in<br/>
/// the corpus by its <see cref="JsonName"/>.
/// </summary>
public sealed record Fact<TSubject> : Condition<TSubject> where TSubject : class
{
   //review:matching: "Fact" is a placeholder. Linguistics calls such properties features, as in [+past], but MeCab calls its whole
   //   entry for a token, the part of speech, inflection and readings, the token's feature (node.Feature). So "feature" may suit these,
   //   or only the tokenizer's fields. And "fact" sounds like something true, where this may or may not hold of a subject.
   readonly Func<TSubject, bool> _isTrueOf;

   /// <param name="name">The <see cref="Name"/>.</param>
   /// <param name="jsonName">The <see cref="JsonName"/>.</param>
   /// <param name="isTrueOf">Whether the fact is true of a subject.</param>
   internal Fact(string name, string jsonName, Func<TSubject, bool> isTrueOf)
   {
      Name = name;
      JsonName = jsonName;
      _isTrueOf = isTrueOf;
   }

   /// <summary>The fact's name in code, after its subject's, such as <c>IsAGodanVerb</c> in <c>PrecedingToken.IsAGodanVerb</c>: the name of the field defining it.</summary>
   public string Name { get; }

   /// <summary>The fact's name in the corpus, such as <c>godanVerb</c>.</summary>
   public string JsonName { get; }

   public override bool IsMetBy(TSubject subject) => _isTrueOf(subject);

   internal override IEnumerable<CodeLine> CodeLines(CodeWriting writing) => [writing.LineOf(Name)];

   /// <summary>The key a fact's <see cref="JsonName"/> is stored under in the corpus.</summary>
   internal const string JsonKey = "fact";

   internal override JsonObject ToJson() => new() { [JsonKey] = JsonName };

   /// <summary>Each fact is defined once, so two are the same fact when their names are.</summary>
   public bool Equals(Fact<TSubject>? other) => other is not null && JsonName == other.JsonName;

   public override int GetHashCode() => JsonName.GetHashCode();
}
