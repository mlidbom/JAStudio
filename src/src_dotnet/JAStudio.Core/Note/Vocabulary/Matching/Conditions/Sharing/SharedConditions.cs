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

   /// <summary>
   /// The shared-conditions on <paramref name="level"/>'s subject, by name, that <paramref name="sharedCondition"/>'s definition can use: those the tree<br/>
   /// editor offers in it. Not <paramref name="sharedCondition"/> itself, nor one defined through it, which would make it use itself.
   /// </summary>
   public IReadOnlyList<ISharedCondition> UsableInTheDefinitionOf(ISharedCondition sharedCondition, IConditionLevel level) =>
      [.. On(level).Where(it => !it.AsACondition.DependsOnTheDefinitionOf(sharedCondition))];

   /// <summary>
   /// The first shared-condition, as they're listed (<see cref="All"/>), whose definition means the same as <paramref name="condition"/><br/>
   /// (<see cref="Condition.MeansTheSameAs"/>), or none: the one to use in its place, rather than define another.
   /// </summary>
   public ISharedCondition? WhoseDefinitionMeansTheSameAs(Condition condition) =>
      //review:matching: was a lookup of SharedConditionTools' own, listing what find_the_conditions_vocab_share finds. Which shared-condition a condition
      //   already has a name in is a question about the shared-conditions, so it's theirs.
      All.FirstOrDefault(it => it.Definition.MeansTheSameAs(condition));

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
   /// Why <paramref name="name"/> can't name a new shared-condition on <paramref name="level"/>'s subject, or null when it can: it must be a name in code, none<br/>
   /// of the other conditions' there (<see cref="IConditionLevel.WhyItCannotNameASharedCondition"/>), and none another shared-condition there has.
   /// </summary>
   public string? WhyItCannotNameANewOne(IConditionLevel level, string name) =>
      level.WhyItCannotNameASharedCondition(name)
      ?? (On(level).Any(it => it.Name == name) ? $"There's a shared-condition {level.SubjectNameInCode}.{name} already." : null);

   /// <summary>
   /// Makes a shared-condition named <paramref name="name"/> of <paramref name="condition"/>, on its level's subject, as a line of the tree editor is made one,<br/>
   /// saves it in its file, and uses it wherever its definition is (<see cref="UseWhereverItsDefinitionIs"/>): in the match-condition of each of<br/>
   /// <paramref name="vocab"/>, and in the definitions of the other shared-conditions. None of them then means anything else.
   /// </summary>
   /// <exception cref="Compze.Contracts.Exceptions.ArgumentAssertionFailedException">When <paramref name="name"/> can't name it (<see cref="WhyItCannotNameANewOne"/>).</exception>
   public ISharedCondition MakeOf(ConditionWithItsLevel condition, string name, IEnumerable<VocabNote> vocab)
   {
      var sharedCondition = condition.Level.NewSharedCondition(name);
      sharedCondition.Redefine(condition.Condition);
      AddAndSave(sharedCondition);
      UseWhereverItsDefinitionIs(sharedCondition, vocab);
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
   /// Gives <paramref name="sharedCondition"/>, one of these, <paramref name="definition"/>, a condition on its subject, in place of its definition, and<br/>
   /// saves it in its file. What it means changes, and with it what every match-condition that depends on its definition means<br/>
   /// (<see cref="Condition.DependsOnTheDefinitionOf"/>): the sentences of those vocab need reparsing.
   /// </summary>
   /// <exception cref="Compze.Contracts.Exceptions.ArgumentAssertionFailedException">
   /// When <paramref name="definition"/> depends on <paramref name="sharedCondition"/>'s definition, which would make it use itself.
   /// </exception>
   public void Redefine(ISharedCondition sharedCondition, Condition definition)
   {
      Contract.Argument.Assert(All.Contains(sharedCondition), () => $"{sharedCondition.FullName} isn't one of the corpus's shared-conditions.");
      Contract.Argument.Assert(!definition.DependsOnTheDefinitionOf(sharedCondition), () => $"{sharedCondition.FullName} defined as {definition} would use itself.");
      sharedCondition.Redefine(definition);
      _monitor.Locked(() => _folder.Save(sharedCondition));
   }

   /// <summary>
   /// Gives <paramref name="sharedCondition"/>, one of these, the definition <paramref name="definition"/> holds, as the corpus stores conditions, in place<br/>
   /// of its definition, and saves it in its file, as <see cref="Redefine"/> does.
   /// </summary>
   /// <exception cref="FormatException">When <paramref name="definition"/> isn't a condition on the subject, or uses a shared-condition there's none of.</exception>
   /// <exception cref="Compze.Contracts.Exceptions.ArgumentAssertionFailedException">When the definition would make it use itself.</exception>
   public void RedefineFrom(ISharedCondition sharedCondition, JsonObject definition) => Redefine(sharedCondition, sharedCondition.Level.ConditionFrom(definition, this));

   /// <summary>
   /// Uses <paramref name="sharedCondition"/>, one of these, by name wherever its definition is (<see cref="Condition.WithSharedConditionUsed"/>): in the<br/>
   /// match-condition of each of <paramref name="vocab"/>, saving each vocab that changes, and in the definitions of the other shared-conditions,<br/>
   /// saving each that changes in its file.
   /// </summary>
   /// <remarks>
   /// Each changes only to mean the same, so the vocab match the same words, and weigh the same, as before. Its own definition isn't changed, which would<br/>
   /// make it use itself.
   /// </remarks>
   public void UseWhereverItsDefinitionIs(ISharedCondition sharedCondition, IEnumerable<VocabNote> vocab)
   {
      Contract.Argument.Assert(All.Contains(sharedCondition), () => $"{sharedCondition.FullName} isn't one of the corpus's shared-conditions.");
      All.Where(it => !it.Equals(sharedCondition)).ToList().ForEach(UseItInTheDefinitionOf);
      vocab.ToList().ForEach(it => it.MatchingConfiguration.UseWhereverItsDefinitionIs(sharedCondition));
      return;

      void UseItInTheDefinitionOf(ISharedCondition other)
      {
         var withItUsed = other.Definition.WithSharedConditionUsed(sharedCondition);
         if(withItUsed.Equals(other.Definition)) return;
         other.RewriteItsDefinitionAs(withItUsed);
         Contract.State.Assert(!other.UsesItself, () => $"Using {sharedCondition.FullName} in {other.FullName}'s definition made {other.FullName} use itself: {other.Definition}");
         _monitor.Locked(() => _folder.Save(other));
      }
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
