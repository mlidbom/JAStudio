using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Compze.Contracts;
using Compze.Threading;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Texts;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Tokens;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Words;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing;

/// <summary>
/// The corpus's shared-conditions (<see cref="SharedCondition{TSubject}"/>), each kept in a file of its own in the corpus checkout<br/>
/// (<see cref="SharedConditionsFolder"/>). The collection loads them as it loads its notes, before them, since a vocab's match-condition uses them by name.
/// </summary>
/// <remarks>
/// They're read on the threads that load the notes and parse sentences, and defined on the UI thread, so they're kept under a monitor.
/// </remarks>
public sealed class SharedConditions
{
   /// <summary>The conditions on each kind of subject a shared-condition can be on, in the order they're listed: a word's first, then a token's, then a text's.</summary>
   public static IReadOnlyList<IConditionLevel> SubjectsTheyCanBeOn { get; } = [WordConditions.Level, TokenConditions.Level, TextConditions.Level];

   /// <summary>
   /// The conditions on the subject that code names <paramref name="subjectNameInCode"/> (<see cref="IConditionLevel.SubjectNameInCode"/>), such as <c>Token</c>, of<br/>
   /// the <see cref="SubjectsTheyCanBeOn"/>: none when a shared-condition can be on no subject so named.
   /// </summary>
   public static IConditionLevel? LevelOfTheSubjectNamed(string subjectNameInCode)
   {
      //review:matching: SubjectsTheyCanBeOn lists levels, the conditions on each kind of subject, as this returns one: "LevelsTheyCanBeOn" would
      //   say so, but a shared-condition is on a subject, as its file's "subject" and the MCP tool's parameter say. Kept.
      return SubjectsTheyCanBeOn.SingleOrDefault(it => it.SubjectNameInCode == subjectNameInCode);
   }

   readonly SharedConditionsFolder _folder;
   readonly IMonitor _monitor = IMonitor.New();
   Dictionary<(IConditionLevel Level, string Name), ISharedCondition> _byLevelAndName = [];

   internal SharedConditions(IEnvironmentPaths paths) => _folder = new SharedConditionsFolder(paths);

   /// <summary>Every shared-condition: those on a word first, then a token's, then a text's, each kind by name.</summary>
   public IReadOnlyList<ISharedCondition> All =>
      _monitor.Locked(() => _byLevelAndName.Values.OrderBy(it => ListingOrderOf(it.Level)).ThenBy(it => it.Name, StringComparer.Ordinal).ToList());

   /// <summary>The shared-conditions on <paramref name="level"/>'s subject, by name: those the match-condition editor offers where a condition on it goes.</summary>
   public IReadOnlyList<ISharedCondition> On(IConditionLevel level) => [.. All.Where(it => it.Level == level)];

   /// <summary>The shared-condition on <paramref name="level"/>'s subject named <paramref name="name"/>, as a match-condition read from the corpus uses it.</summary>
   /// <exception cref="FormatException">When there's none.</exception>
   internal SharedCondition<TSubject> Named<TSubject>(ConditionLevel<TSubject> level, string name) where TSubject : class =>
      _monitor.Locked(() => _byLevelAndName.TryGetValue((level, name), out var found)
                               ? (SharedCondition<TSubject>)found
                               : throw new FormatException($"There's no shared-condition {level.SubjectNameInCode}.{name}."));

   /// <summary>Defines a shared-condition on <paramref name="level"/>'s subject, named <paramref name="name"/>, that stands for <paramref name="definition"/>, and saves it in the corpus.</summary>
   /// <exception cref="Compze.Contracts.Exceptions.ArgumentAssertionFailedException">
   /// When <paramref name="name"/> can't name one there (<see cref="IConditionLevel.WhyItCannotNameASharedCondition"/>), or names one already.
   /// </exception>
   public SharedCondition<TSubject> Define<TSubject>(ConditionLevel<TSubject> level, string name, Condition<TSubject> definition) where TSubject : class
   {
      var sharedCondition = new SharedCondition<TSubject>(name, level);
      sharedCondition.Define(definition);
      AddAndSave(sharedCondition);
      return sharedCondition;
   }

   /// <summary>
   /// Defines a shared-condition on <paramref name="level"/>'s subject, named <paramref name="name"/>, that stands for the condition <paramref name="definition"/><br/>
   /// holds, as the corpus stores conditions, and saves it in the corpus.
   /// </summary>
   /// <exception cref="Compze.Contracts.Exceptions.ArgumentAssertionFailedException">When <paramref name="name"/> can't name one there, or names one already.</exception>
   /// <exception cref="FormatException">When <paramref name="definition"/> isn't a condition on the subject, or uses a shared-condition there's none of.</exception>
   public ISharedCondition DefineFrom(IConditionLevel level, string name, JsonObject definition)
   {
      var sharedCondition = level.NewSharedCondition(name);
      sharedCondition.DefineFrom(definition, this);
      AddAndSave(sharedCondition);
      return sharedCondition;
   }

   /// <summary>
   /// Loads the shared-conditions from the corpus checkout anew, in place of those held, as the collection loads its notes. Each is first made by name,<br/>
   /// then defined, so that a definition can use any of them.
   /// </summary>
   /// <exception cref="FormatException">
   /// When a file isn't a shared-condition's, a definition uses a shared-condition there's none of, or a shared-condition uses itself, directly or<br/>
   /// through others.
   /// </exception>
   internal void Load()
   {
      List<(ISharedCondition SharedCondition, JsonObject Definition)> made = [.. _folder.ReadEveryFile().Select(it => (it.NewSharedCondition(), it.Definition))];
      _monitor.Locked(() => _byLevelAndName = made.ToDictionary(it => KeyOf(it.SharedCondition), it => it.SharedCondition));

      made.ForEach(it => it.SharedCondition.DefineFrom(it.Definition, this));
      if(made.Select(it => it.SharedCondition).FirstOrDefault(it => it.UsesItself) is {} usingItself)
         throw new FormatException($"The shared-condition {usingItself.FullName} uses itself, directly or through others: {usingItself.Definition}");
   }

   /// <summary>
   /// Adds <paramref name="sharedCondition"/> to those held and saves it in its file. Its name must be one it can have on its subject<br/>
   /// (<see cref="IConditionLevel.WhyItCannotNameASharedCondition"/>), and none another shared-condition there has.
   /// </summary>
   void AddAndSave(ISharedCondition sharedCondition)
   {
      //review:matching: AddAndSave was Add, which hid that it saves the shared-condition in the corpus, as Define and DefineFrom say they do.
      var whyItCannotBeNamedSo = sharedCondition.Level.WhyItCannotNameASharedCondition(sharedCondition.Name);
      Contract.Argument.Assert(whyItCannotBeNamedSo is null, () => $"{whyItCannotBeNamedSo}");
      _monitor.Locked(() =>
      {
         Contract.Argument.Assert(!_byLevelAndName.ContainsKey(KeyOf(sharedCondition)), () => $"There's a shared-condition {sharedCondition.FullName} already.");
         _byLevelAndName.Add(KeyOf(sharedCondition), sharedCondition);
         _folder.Save(sharedCondition);
      });
   }

   static (IConditionLevel Level, string Name) KeyOf(ISharedCondition sharedCondition) => (sharedCondition.Level, sharedCondition.Name);

   /// <summary>Where the shared-conditions on <paramref name="level"/>'s subject are listed: those on a word first (<see cref="SubjectsTheyCanBeOn"/>).</summary>
   static int ListingOrderOf(IConditionLevel level) => SubjectsTheyCanBeOn.ToList().IndexOf(level);
}
