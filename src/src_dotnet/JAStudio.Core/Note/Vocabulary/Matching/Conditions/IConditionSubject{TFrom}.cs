using System.Text.Json.Nodes;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions;

/// <summary>
/// A <see cref="ConditionSubject{TFrom,TSubject}"/> as the <see cref="ConditionLevel{TSubject}"/> of <typeparamref name="TFrom"/> knows it: by its name in the<br/>
/// corpus, whatever its own subject is.
/// </summary>
interface IConditionSubject<TFrom> where TFrom : class
{
   /// <summary>The key a condition on this subject is stored under in the corpus, such as <c>precedingToken</c>.</summary>
   string JsonKey { get; }

   /// <summary>The condition that this subject meets the condition <paramref name="json"/> holds, as the corpus stores it.</summary>
   Condition<TFrom> ConditionFrom(JsonObject json);
}
