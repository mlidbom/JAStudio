using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Texts;

/// <summary>
/// A condition that a text compares in one way with one of the <see cref="Texts"/>: <see cref="TextIsOneOf"/>, <see cref="TextStartsWithOneOf"/>,<br/>
/// <see cref="TextEndsWithOneOf"/> or <see cref="TextContainsOneOf"/>. Every comparison is ordinal: exact, character by character.
/// </summary>
public abstract record TextComparison : Condition<string>
{
   protected TextComparison(TextSet texts) => Texts = texts;

   /// <summary>The texts a text is compared with.</summary>
   public TextSet Texts { get; }

   public override bool IsMetBy(string subject) => Texts.Texts.Any(it => Compares(subject, it));

   /// <summary>The comparison in code, such as <c>EndsWithOneOf("ん", "い")</c>: its name, then its texts as string literals.</summary>
   internal override IEnumerable<Code.CodeLine> CodeLines(Code.CodeWriting writing) => [writing.LineOf($"{Name}({Texts.AsStringLiterals})")];

   /// <summary>Whether <paramref name="subject"/> compares with <paramref name="text"/> as this condition requires.</summary>
   protected abstract bool Compares(string subject, string text);

   /// <summary>The comparison's name in code, before the texts, such as <c>EndsWithOneOf</c>.</summary>
   protected abstract string Name { get; }

   internal override IEnumerable<TextSet> ComparedTextSets() => [Texts];

   /// <summary>The comparison as the corpus stores it: its texts under <paramref name="jsonKey"/>, such as <c>endsWithOneOf</c>.</summary>
   protected JsonObject ToJsonUnder(string jsonKey) => new() { [jsonKey] = Texts.ToJson() };
}
