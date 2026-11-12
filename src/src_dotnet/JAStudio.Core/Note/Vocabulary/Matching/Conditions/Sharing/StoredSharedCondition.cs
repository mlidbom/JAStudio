using System;
using System.Text.Json.Nodes;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing;

/// <summary>
/// A shared-condition as its file in the corpus keeps it (<see cref="SharedConditionsFolder"/>), before <see cref="SharedConditions"/> makes it: the name of<br/>
/// its subject in code, such as <c>Word</c>, its name, and its definition as the corpus stores conditions.
/// </summary>
/// <remarks>
/// The file holds them as a JSON object: <c>{"subject": "Word", "name": "FollowsAPastTenseStem", "definition": {"anyOf": [...]}}</c>.
/// </remarks>
/// <param name="FileName">The name of the file it's in, such as <c>Word.FollowsAPastTenseStem.json</c>.</param>
/// <param name="Subject">Its subject's name in code (<see cref="IConditionLevel.SubjectNameInCode"/>).</param>
/// <param name="Name">Its name.</param>
/// <param name="Definition">Its definition, as the corpus stores conditions.</param>
sealed record StoredSharedCondition(string FileName, string Subject, string Name, JsonObject Definition)
{
   const string SubjectKey = "subject";
   const string NameKey = "name";
   const string DefinitionKey = "definition";

   /// <summary><paramref name="sharedCondition"/> as its file keeps it.</summary>
   internal static StoredSharedCondition Of(ISharedCondition sharedCondition) =>
      new(SharedConditionsFolder.FileNameOf(sharedCondition), sharedCondition.Level.SubjectNameInCode, sharedCondition.Name, sharedCondition.Definition.ToJson());

   /// <summary>The shared-condition that the file named <paramref name="fileName"/> keeps as <paramref name="json"/>, as <see cref="ToJson"/> writes it.</summary>
   /// <exception cref="FormatException">When <paramref name="json"/> isn't a shared-condition's.</exception>
   internal static StoredSharedCondition From(string fileName, JsonNode? json)
   {
      var stored = json as JsonObject ?? throw new FormatException($"A shared-condition's file holds a JSON object: {fileName}");
      return new StoredSharedCondition(fileName, TextUnder(SubjectKey), TextUnder(NameKey), stored[DefinitionKey] as JsonObject ?? throw Missing(DefinitionKey));

      string TextUnder(string key) => stored[key]?.GetValue<string>() ?? throw Missing(key);

      FormatException Missing(string key) => new($"A shared-condition's file without its {key}: {fileName}");
   }

   /// <summary>The JSON the file keeps.</summary>
   internal JsonObject ToJson() => new() { [SubjectKey] = Subject, [NameKey] = Name, [DefinitionKey] = Definition.DeepClone() };

   /// <summary>
   /// The shared-condition the file keeps, new, without its definition yet: <see cref="SharedConditions.Load"/> gives each its definition once every one<br/>
   /// is made, so that a definition can use any of them.
   /// </summary>
   /// <exception cref="FormatException">
   /// When its subject is none a shared-condition can be on, its name can't name one there (<see cref="IConditionLevel.WhyItCannotNameASharedCondition"/>), or<br/>
   /// the file is named for another shared-condition.
   /// </exception>
   internal ISharedCondition NewSharedCondition()
   {
      var level = SharedConditions.LevelOfTheSubjectNamed(Subject) ?? throw new FormatException($"A shared-condition on {Subject}, which no condition is on, in {FileName}.");
      if(level.WhyItCannotNameASharedCondition(Name) is {} whyNot) throw new FormatException($"{whyNot} In {FileName}.");
      var sharedCondition = level.NewSharedCondition(Name);
      return FileName == SharedConditionsFolder.FileNameOf(sharedCondition)
                ? sharedCondition
                : throw new FormatException($"The shared-condition {sharedCondition.FullName} is in {FileName}, not in {SharedConditionsFolder.FileNameOf(sharedCondition)}.");
   }
}
