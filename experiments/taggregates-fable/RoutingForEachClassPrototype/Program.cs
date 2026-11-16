// A prototype of tevent routing decided once for each teventive class, to measure what the proposal would cost for each instance. It is not
// Compze's code and not its design in detail: the smallest thing that has the proposal's shape. Each class declares its tevent appliers once, in a
// static declaration whose lambdas take the instance; a component and a shared tomponent are entries in it, got from the instance by a lambda; a slot
// is a static of its owner's class; an instance keeps a reference to its class's declaration. The tevents are Compze's own types, and the models are
// the ones TaggregateCost and TeventiveChecks measure with Compze.Teventive 0.6.0-alpha, so the numbers compare.
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq.Expressions;
using Compze.Abstractions;
using Compze.Tessaging.TessageTypes;
using Compze.Teventive;
using Compze.Teventive.Taggregates.BaseClasses;
using Compze.Teventive.Taggregates.Tevents;

const int count = 80_000;

Console.WriteLine("== The model of TaggregateCost: a root with four text values, and four components of two values each ==");
Measure("A root with 4 tevent appliers, loaded from one creation tevent", () => RoutedVocabWithoutParts.Loaded(NewLoaded()));
Measure("The same, created by publishing", () => RoutedVocabWithoutParts.Created(NewLoaded()));
Measure("With 4 components of 2 tevent appliers each, loaded", () => RoutedVocab.Loaded(NewLoaded()));
Measure("The same, created by publishing", () => RoutedVocab.Created(NewLoaded()));

var vocabs = Enumerable.Range(0, count).Select(_ => RoutedVocab.Loaded(NewLoaded())).ToArray();
Require(vocabs[0] is { Question: "question", Answer: "answer", Mnemonic: "mnemonic", Version: 1 } && vocabs[0].Forms.Values.SequenceEqual(["form"]) && vocabs[0].Synonyms.Values.SequenceEqual(["synonym"]),
        "a loaded vocab has the state of its creation tevent");
var before = Snapshot();
foreach(var vocab in vocabs) vocab.ChangeAnswer("another answer");
Report("The first change published on each loaded instance, its class told of it", before);
before = Snapshot();
foreach(var vocab in vocabs) vocab.ChangeAnswer("a third answer");
Report("A second change of the same type", before);
Require(vocabs[0] is { Answer: "a third answer", Version: 3 } && RoutedVocab.TimesToldThatItPublished == 2 * count, "each change was applied, and the class was told of each");
var committed = 0;
vocabs[0].Commit(it => committed = it.Count);
Require(committed == 2, "the two changes were uncommitted until Commit");
GC.KeepAlive(vocabs);

Console.WriteLine();
Console.WriteLine("== The model of TeventiveChecks: a root with one tevent applier, and none or four shared tomponents ==");
var none = Measure("No shared tomponent, loaded from its creation tevent", RoutedHolderWithoutSharedTomponents.Loaded);
var four = Measure("4 shared tomponents, loaded from its creation tevent only", () => RoutedHolder.Loaded(withATeventForEachSharedTomponent: false));
var fourLoaded = Measure("4 shared tomponents, loaded from its creation tevent and one tevent adopted for each", () => RoutedHolder.Loaded(withATeventForEachSharedTomponent: true));
Console.WriteLine($"   A shared tomponent that has seen no tevent keeps {(four.Kept - none.Kept) / 4:N0} B, and one that has seen its first {(fourLoaded.Kept - none.Kept) / 4:N0} B.");
Console.WriteLine($"   Loading one adopted tevent allocates {(fourLoaded.Allocated - four.Allocated) / 4:N0} B and takes {(fourLoaded.Microseconds - four.Microseconds) / 4:N2} microseconds.");

var holder = RoutedHolder.Loaded(withATeventForEachSharedTomponent: true);
Require(holder is { First.Value: 1, Second.Value: 2, Third.Value: 3, Fourth.Value: 4, Version: 5 }, "each shared tomponent was loaded through its own slot");
holder.Third.Set(30);
Require(holder is { Second.Value: 2, Third.Value: 30, Fourth.Value: 4, Version: 6 }, "a shared tomponent publishes through its slot, and only it applies the tevent");
Console.WriteLine("   The loaded and published state is as expected.");

static VocabTevent.Loaded NewLoaded() => new(new TaggregateId(), "question", "answer", "explanation", "mnemonic", ["form"], ["noun"], ["part"], ["synonym"]);

static void Require(bool holds, string what)
{
   if(!holds) throw new InvalidOperationException($"The prototype is wrong: it does not hold that {what}.");
}

static (long Kept, long Allocated, double Microseconds) Measure<T>(string what, Func<T> create) where T : class
{
   create(); // warm up: static constructors, reflection caches, the JIT
   var instances = new T[count];
   var snapshot = Snapshot();
   for(var index = 0; index < instances.Length; index++) instances[index] = create();
   var result = Report(what, snapshot);
   GC.KeepAlive(instances);
   return result;
}

static (long Kept, long Allocated, long Ticks) Snapshot() => (GC.GetTotalMemory(forceFullCollection: true), GC.GetTotalAllocatedBytes(precise: true), Stopwatch.GetTimestamp());

static (long Kept, long Allocated, double Microseconds) Report(string what, (long Kept, long Allocated, long Ticks) before)
{
   var elapsed = Stopwatch.GetElapsedTime(before.Ticks);
   var allocated = (GC.GetTotalAllocatedBytes(precise: true) - before.Allocated) / count;
   var kept = (GC.GetTotalMemory(forceFullCollection: true) - before.Kept) / count;
   Console.WriteLine($"   {what}: {kept:N0} B kept, {allocated:N0} B allocated, {elapsed.TotalMicroseconds / count:N2} microseconds, for each");
   return (kept, allocated, elapsed.TotalMicroseconds / count);
}

// ==== The prototype ====

/// One teventive class's declaration of its tevent appliers, which every instance of the class shares.
abstract class TeventAppliers
{
   /// Applies <paramref name="tevent"/> to <paramref name="teventive"/>, an instance of the class this is the declaration of.
   internal abstract void Apply(object teventive, ITevent tevent);

   /// Starts the declaration of <typeparamref name="TTeventive"/>'s tevent appliers.
   public static TeventAppliers<TTeventive> Of<TTeventive>() where TTeventive : class => new([]);
}

/// The declaration of <typeparamref name="TTeventive"/>'s tevent appliers: entries, in the order declared. The entries that match a tevent's type are
/// resolved the first time the class meets that type, and kept in a dictionary that is never changed once published, so it is read without a lock.
sealed class TeventAppliers<TTeventive> : TeventAppliers where TTeventive : class
{
   /// Each entry gives, for a tevent's type, what applies such a tevent, or null when the entry doesn't match the type.
   readonly List<Func<Type, Action<object, ITevent>?>> _entries;
   Dictionary<Type, Action<object, ITevent>[]> _appliersByResolvedTeventType = [];

   internal TeventAppliers(IEnumerable<Func<Type, Action<object, ITevent>?>> entriesOfTheBaseClass) => _entries = [..entriesOfTheBaseClass];

   /// A tevent applier for every tevent assignable to <typeparamref name="TTevent"/>.
   public TeventAppliers<TTeventive> For<TTevent>(Action<TTeventive, TTevent> applier) where TTevent : ITevent =>
      With(teventType => typeof(TTevent).IsAssignableFrom(teventType) ? (teventive, tevent) => applier((TTeventive)teventive, (TTevent)tevent) : null);

   /// A component, got from the instance by <paramref name="component"/>, which is handed every tevent assignable to <typeparamref name="TComponentTevent"/>.
   public TeventAppliers<TTeventive> Component<TComponent, TComponentTevent>(Func<TTeventive, TComponent> component) where TComponent : IAppliesTevents =>
      With(teventType => typeof(TComponentTevent).IsAssignableFrom(teventType) ? (teventive, tevent) => component((TTeventive)teventive).Apply(tevent) : null);

   /// The shared tomponent in <paramref name="slot"/>, which is handed the tevent inside every tevent that the slot's adopting wrapper tevent type is assignable from.
   public TeventAppliers<TTeventive> SharedTomponent<TOwnerTeventImplementation, TTomponent, TTomponentTevent, TAdoptingWrapperTevent>(
      Slot<TTeventive, TOwnerTeventImplementation, TTomponent, TTomponentTevent, TAdoptingWrapperTevent> slot)
      where TOwnerTeventImplementation : TaggregateTevent
      where TTomponent : RoutedSharedTomponent<TTomponentTevent>
      where TTomponentTevent : class, ITevent
      where TAdoptingWrapperTevent : class, IPublisherTevent<TTomponentTevent> =>
      With(teventType => typeof(TAdoptingWrapperTevent).IsAssignableFrom(teventType)
                            ? (teventive, tevent) => slot.TomponentOf((TTeventive)teventive).Apply(((IPublisherTevent<TTomponentTevent>)tevent).Tevent)
                            : null);

   /// The declaration of a subclass, which starts from this one: the base class's entries come first.
   public TeventAppliers<TSubclass> ThenFor<TSubclass>() where TSubclass : class, TTeventive => new(_entries);

   TeventAppliers<TTeventive> With(Func<Type, Action<object, ITevent>?> entry)
   {
      _entries.Add(entry);
      return this;
   }

   internal override void Apply(object teventive, ITevent tevent)
   {
      var teventType = tevent.GetType();
      if(!Volatile.Read(ref _appliersByResolvedTeventType).TryGetValue(teventType, out var appliers)) appliers = Resolve(teventType);
      for(var index = 0; index < appliers.Length; index++) appliers[index](teventive, tevent);
   }

   Action<object, ITevent>[] Resolve(Type teventType)
   {
      lock(_entries)
      {
         if(_appliersByResolvedTeventType.TryGetValue(teventType, out var resolvedMeanwhile)) return resolvedMeanwhile;

         Action<object, ITevent>[] appliers = [.._entries.Select(entry => entry(teventType)).OfType<Action<object, ITevent>>()];
         if(appliers.Length == 0) throw new InvalidOperationException($"No tevent applier of {typeof(TTeventive).Name} matches a {teventType.Name}.");

         Volatile.Write(ref _appliersByResolvedTeventType, new Dictionary<Type, Action<object, ITevent>[]>(_appliersByResolvedTeventType) { [teventType] = appliers });
         return appliers;
      }
   }
}

interface IAppliesTevents
{
   void Apply(ITevent tevent);
}

interface IPublishesForItsParts<in TTeventImplementation>
{
   void Publish(TTeventImplementation tevent);
}

/// A taggregate whose tevent appliers are its class's: it keeps the declaration, its id and version, and the list of uncommitted tevents once it has any.
abstract class RoutedTaggregate<TTeventImplementation>(TeventAppliers appliers) : IPublishesForItsParts<TTeventImplementation>
   where TTeventImplementation : TaggregateTevent
{
   readonly TeventAppliers _appliers = appliers;
   TaggregateId? _id;
   List<ITaggregateTevent<ITaggregateTevent>>? _uncommittedTevents;
   int _reentrancyLevel;
   bool _isApplyingTevents;

   protected abstract Type WrapperTeventImplementation { get; }

   public TaggregateId Id => _id ?? throw new InvalidOperationException("The taggregate has no id before its creation tevent.");
   public int Version { get; private set; }

   protected void Publish(TTeventImplementation tevent)
   {
      if(_isApplyingTevents) throw new InvalidOperationException("A tevent applier can't publish.");
      if(Version == 0 && tevent is not ITaggregateCreatedTevent) throw new InvalidOperationException("The first tevent a taggregate publishes must be a creation tevent.");

      Stamp(tevent);
      _reentrancyLevel++;
      try
      {
         Apply(tevent);
         (_uncommittedTevents ??= []).Add(TaggregateIdentifyingTevent.WrapIn(WrapperTeventImplementation, tevent));
      }
      finally
      {
         _reentrancyLevel--;
      }

      if(_reentrancyLevel == 0) OnTeventsPublished();
   }

   /// Rebuilds the taggregate's state from a compacted-history that its own code has constructed. Nothing is published, wrapped or left uncommitted.
   protected void LoadFromCompactedHistory(params ReadOnlySpan<TTeventImplementation> compactedHistory)
   {
      foreach(var tevent in compactedHistory)
      {
         Stamp(tevent);
         Apply(tevent);
      }
   }

   /// Called when a call to Publish has completed. The prototype has no tevent handlers, so that is after each tevent.
   protected virtual void OnTeventsPublished() {}

   /// Hands over the uncommitted tevents, each in its wrapper. The list is taken out first, so a tevent published from inside the callback is uncommitted for the next commit.
   public void Commit(Action<IReadOnlyList<ITaggregateTevent<ITaggregateTevent>>> commit)
   {
      var committed = _uncommittedTevents;
      _uncommittedTevents = null;
      commit(committed ?? []);
   }

   void Stamp(TTeventImplementation tevent)
   {
      var stamped = (IMutableTaggregateTevent)tevent;
      stamped.SetTaggregateVersionInternal(Version + 1);
      if(Version > 0) stamped.SetTaggregateIdInternal(Id);
   }

   void Apply(TTeventImplementation tevent)
   {
      _isApplyingTevents = true;
      try
      {
         if(Version == 0) _id = tevent.TaggregateId;
         Version = tevent.TaggregateVersion;
         _appliers.Apply(this, tevent);
      }
      finally
      {
         _isApplyingTevents = false;
      }
   }

   void IPublishesForItsParts<TTeventImplementation>.Publish(TTeventImplementation tevent) => Publish(tevent);
}

/// A component: it keeps its class's declaration and its parent, which it publishes through.
abstract class RoutedComponent<TTeventImplementation>(TeventAppliers appliers, IPublishesForItsParts<TTeventImplementation> parent) : IAppliesTevents
{
   readonly TeventAppliers _appliers = appliers;
   readonly IPublishesForItsParts<TTeventImplementation> _parent = parent;

   void IAppliesTevents.Apply(ITevent tevent) => _appliers.Apply(this, tevent);

   protected void Publish(TTeventImplementation tevent) => _parent.Publish(tevent);
}

interface ISlot<in TTomponentTevent>
{
   void Publish(object owner, TTomponentTevent tevent);
}

/// A slot and the owner it is in: what an owner hands a shared tomponent when it constructs it. A struct, so that handing it over allocates nothing.
readonly struct SlotIn<TTomponentTevent>(object owner, ISlot<TTomponentTevent> slot)
{
   internal object Owner { get; } = owner;
   internal ISlot<TTomponentTevent> Slot { get; } = slot;
}

/// A shared tomponent: it keeps its class's declaration, its owner and its slot.
abstract class RoutedSharedTomponent<TTomponentTevent>(TeventAppliers appliers, SlotIn<TTomponentTevent> slotIn) where TTomponentTevent : class, ITevent
{
   readonly TeventAppliers _appliers = appliers;
   readonly object _owner = slotIn.Owner;
   readonly ISlot<TTomponentTevent> _slot = slotIn.Slot;

   internal void Apply(TTomponentTevent tevent) => _appliers.Apply(this, tevent);

   protected void Publish(TTomponentTevent tevent) => _slot.Publish(_owner, tevent);
}

/// A slot of the owner class <typeparamref name="TOwner"/>, declared once as a static: the adopting wrapper tevent that tells its shared tomponent's tevents apart, and how
/// to get the shared tomponent from an owner. It is the one place that wraps a tevent in the adopting wrapper tevent, to publish it and to load it.
sealed class Slot<TOwner, TOwnerTeventImplementation, TTomponent, TTomponentTevent, TAdoptingWrapperTevent>(Type adoptingWrapperTeventImplementation, Func<TOwner, TTomponent> tomponentOf)
   : ISlot<TTomponentTevent>
   where TOwner : class
   where TOwnerTeventImplementation : TaggregateTevent
   where TTomponent : RoutedSharedTomponent<TTomponentTevent>
   where TTomponentTevent : class, ITevent
   where TAdoptingWrapperTevent : class, IPublisherTevent<TTomponentTevent>
{
   readonly Type _adoptingWrapperTeventDefinition = adoptingWrapperTeventImplementation.GetGenericTypeDefinition();
   readonly ConcurrentDictionary<Type, Func<TTomponentTevent, TOwnerTeventImplementation>> _adoptionByTeventType = new();

   public SlotIn<TTomponentTevent> In(TOwner owner) => new(owner, this);

   internal TTomponent TomponentOf(TOwner owner) => tomponentOf(owner);

   /// <paramref name="tevent"/> in the slot's adopting wrapper tevent, closed over the tevent's own type.
   public TOwnerTeventImplementation Adopt(TTomponentTevent tevent) => _adoptionByTeventType.GetOrAdd(tevent.GetType(), CompileTheAdoptionOf)(tevent);

   void ISlot<TTomponentTevent>.Publish(object owner, TTomponentTevent tevent) => ((IPublishesForItsParts<TOwnerTeventImplementation>)owner).Publish(Adopt(tevent));

   Func<TTomponentTevent, TOwnerTeventImplementation> CompileTheAdoptionOf(Type teventType)
   {
      var constructor = _adoptingWrapperTeventDefinition.MakeGenericType(teventType).GetConstructors().Single();
      var tevent = Expression.Parameter(typeof(TTomponentTevent));
      var adopted = Expression.Convert(Expression.New(constructor, Expression.Convert(tevent, teventType)), typeof(TOwnerTeventImplementation));
      return Expression.Lambda<Func<TTomponentTevent, TOwnerTeventImplementation>>(adopted, tevent).Compile();
   }
}

// ==== The model of TaggregateCost, on the prototype ====

/// A base class with a tevent applier of its own, so that a subclass's declaration starts from its base class's.
abstract class RoutedVocabBase(TeventAppliers appliers) : RoutedTaggregate<VocabTevent>(appliers)
{
   protected static readonly TeventAppliers<RoutedVocabBase> BaseAppliers =
      TeventAppliers.Of<RoutedVocabBase>()
                    .For<IVocabTevent.PropertyUpdated.Question>(static (vocab, tevent) => vocab.Question = tevent.Question);

   public string Question { get; private set; } = "";

   protected override Type WrapperTeventImplementation => typeof(VocabTevent<VocabTevent>);
}

sealed class RoutedVocabWithoutParts : RoutedVocabBase
{
   static readonly TeventAppliers<RoutedVocabWithoutParts> Appliers =
      BaseAppliers.ThenFor<RoutedVocabWithoutParts>()
                  .For<IVocabTevent.PropertyUpdated.Answer>(static (vocab, tevent) => vocab.Answer = tevent.Answer)
                  .For<IVocabTevent.PropertyUpdated.Explanation>(static (vocab, tevent) => vocab.Explanation = tevent.Explanation)
                  .For<IVocabTevent.PropertyUpdated.Mnemonic>(static (vocab, tevent) => vocab.Mnemonic = tevent.Mnemonic);

   public string Answer { get; private set; } = "";
   public string Explanation { get; private set; } = "";
   public string Mnemonic { get; private set; } = "";

   RoutedVocabWithoutParts() : base(Appliers) {}

   internal static RoutedVocabWithoutParts Loaded(VocabTevent.Loaded state)
   {
      var vocab = new RoutedVocabWithoutParts();
      vocab.LoadFromCompactedHistory(state);
      return vocab;
   }

   internal static RoutedVocabWithoutParts Created(VocabTevent.Loaded state)
   {
      var vocab = new RoutedVocabWithoutParts();
      vocab.Publish(state);
      return vocab;
   }
}

sealed class RoutedVocab : RoutedVocabBase
{
   static readonly TeventAppliers<RoutedVocab> Appliers =
      BaseAppliers.ThenFor<RoutedVocab>()
                  .For<IVocabTevent.PropertyUpdated.Answer>(static (vocab, tevent) => vocab.Answer = tevent.Answer)
                  .For<IVocabTevent.PropertyUpdated.Explanation>(static (vocab, tevent) => vocab.Explanation = tevent.Explanation)
                  .For<IVocabTevent.PropertyUpdated.Mnemonic>(static (vocab, tevent) => vocab.Mnemonic = tevent.Mnemonic)
                  .Component<RoutedFormsPart, IVocabTevent.Forms>(static vocab => vocab.Forms)
                  .Component<RoutedPartsOfSpeechPart, IVocabTevent.PartsOfSpeech>(static vocab => vocab.PartsOfSpeech)
                  .Component<RoutedCompoundPartsPart, IVocabTevent.CompoundParts>(static vocab => vocab.CompoundParts)
                  .Component<RoutedSynonymsPart, IVocabTevent.Synonyms>(static vocab => vocab.Synonyms);

   internal static int TimesToldThatItPublished;

   public string Answer { get; private set; } = "";
   public string Explanation { get; private set; } = "";
   public string Mnemonic { get; private set; } = "";
   public RoutedFormsPart Forms { get; }
   public RoutedPartsOfSpeechPart PartsOfSpeech { get; }
   public RoutedCompoundPartsPart CompoundParts { get; }
   public RoutedSynonymsPart Synonyms { get; }

   RoutedVocab() : base(Appliers)
   {
      Forms = new RoutedFormsPart(this);
      PartsOfSpeech = new RoutedPartsOfSpeechPart(this);
      CompoundParts = new RoutedCompoundPartsPart(this);
      Synonyms = new RoutedSynonymsPart(this);
   }

   protected override void OnTeventsPublished() => TimesToldThatItPublished++;

   internal static RoutedVocab Loaded(VocabTevent.Loaded state)
   {
      var vocab = new RoutedVocab();
      vocab.LoadFromCompactedHistory(state);
      return vocab;
   }

   internal static RoutedVocab Created(VocabTevent.Loaded state)
   {
      var vocab = new RoutedVocab();
      vocab.Publish(state);
      TimesToldThatItPublished = 0;
      return vocab;
   }

   internal void ChangeAnswer(string answer) => Publish(new VocabTevent.AnswerChanged(answer));
}

sealed class RoutedFormsPart(IPublishesForItsParts<VocabTevent.Forms> vocab) : RoutedComponent<VocabTevent.Forms>(Appliers, vocab)
{
   static readonly TeventAppliers<RoutedFormsPart> Appliers =
      TeventAppliers.Of<RoutedFormsPart>()
                    .For<IVocabTevent.Forms.PropertyUpdated.Values>(static (part, tevent) => part.Values = tevent.FormValues)
                    .For<IVocabTevent.Forms.Locked>(static (part, _) => part.IsLocked = true);

   public string[] Values { get; private set; } = [];
   public bool IsLocked { get; private set; }
}

sealed class RoutedPartsOfSpeechPart(IPublishesForItsParts<VocabTevent.PartsOfSpeech> vocab) : RoutedComponent<VocabTevent.PartsOfSpeech>(Appliers, vocab)
{
   static readonly TeventAppliers<RoutedPartsOfSpeechPart> Appliers =
      TeventAppliers.Of<RoutedPartsOfSpeechPart>()
                    .For<IVocabTevent.PartsOfSpeech.PropertyUpdated.Values>(static (part, tevent) => part.Values = tevent.PartsOfSpeechValues)
                    .For<IVocabTevent.PartsOfSpeech.Locked>(static (part, _) => part.IsLocked = true);

   public string[] Values { get; private set; } = [];
   public bool IsLocked { get; private set; }
}

sealed class RoutedCompoundPartsPart(IPublishesForItsParts<VocabTevent.CompoundParts> vocab) : RoutedComponent<VocabTevent.CompoundParts>(Appliers, vocab)
{
   static readonly TeventAppliers<RoutedCompoundPartsPart> Appliers =
      TeventAppliers.Of<RoutedCompoundPartsPart>()
                    .For<IVocabTevent.CompoundParts.PropertyUpdated.Values>(static (part, tevent) => part.Values = tevent.CompoundPartsValues)
                    .For<IVocabTevent.CompoundParts.Locked>(static (part, _) => part.IsLocked = true);

   public string[] Values { get; private set; } = [];
   public bool IsLocked { get; private set; }
}

sealed class RoutedSynonymsPart(IPublishesForItsParts<VocabTevent.Synonyms> vocab) : RoutedComponent<VocabTevent.Synonyms>(Appliers, vocab)
{
   static readonly TeventAppliers<RoutedSynonymsPart> Appliers =
      TeventAppliers.Of<RoutedSynonymsPart>()
                    .For<IVocabTevent.Synonyms.PropertyUpdated.Values>(static (part, tevent) => part.Values = tevent.SynonymsValues)
                    .For<IVocabTevent.Synonyms.Locked>(static (part, _) => part.IsLocked = true);

   public string[] Values { get; private set; } = [];
   public bool IsLocked { get; private set; }
}

// ==== The model of TeventiveChecks' measurement, on the prototype ====

sealed class RoutedCounter(SlotIn<ICounterTevent> slotIn) : RoutedSharedTomponent<ICounterTevent>(Appliers, slotIn)
{
   static readonly TeventAppliers<RoutedCounter> Appliers =
      TeventAppliers.Of<RoutedCounter>().For<ICounterTevent.Set>(static (counter, tevent) => counter.Value = tevent.Value);

   public int Value { get; private set; }

   public void Set(int value) => Publish(new CounterSet(value));
}

sealed class RoutedHolderWithoutSharedTomponents : RoutedTaggregate<HolderTevent>
{
   static readonly TeventAppliers<RoutedHolderWithoutSharedTomponents> Appliers =
      TeventAppliers.Of<RoutedHolderWithoutSharedTomponents>().For<IHolderTevent.Created>(static (_, _) => {});

   RoutedHolderWithoutSharedTomponents() : base(Appliers) {}

   protected override Type WrapperTeventImplementation => typeof(HolderTevent<HolderTevent>);

   internal static RoutedHolderWithoutSharedTomponents Loaded()
   {
      var holder = new RoutedHolderWithoutSharedTomponents();
      holder.LoadFromCompactedHistory(new HolderTevent.Created(new TaggregateId()));
      return holder;
   }
}

sealed class RoutedHolder : RoutedTaggregate<HolderTevent>
{
   // The slots are declared above the declaration that enters them: static fields are initialized in the order written.
   static readonly Slot<RoutedHolder, HolderTevent, RoutedCounter, ICounterTevent, IFirstTevent<ICounterTevent>> FirstSlot = new(typeof(FirstTevent<>), static holder => holder.First);
   static readonly Slot<RoutedHolder, HolderTevent, RoutedCounter, ICounterTevent, ISecondTevent<ICounterTevent>> SecondSlot = new(typeof(SecondTevent<>), static holder => holder.Second);
   static readonly Slot<RoutedHolder, HolderTevent, RoutedCounter, ICounterTevent, IThirdTevent<ICounterTevent>> ThirdSlot = new(typeof(ThirdTevent<>), static holder => holder.Third);
   static readonly Slot<RoutedHolder, HolderTevent, RoutedCounter, ICounterTevent, IFourthTevent<ICounterTevent>> FourthSlot = new(typeof(FourthTevent<>), static holder => holder.Fourth);

   static readonly TeventAppliers<RoutedHolder> Appliers =
      TeventAppliers.Of<RoutedHolder>()
                    .For<IHolderTevent.Created>(static (_, _) => {})
                    .SharedTomponent(FirstSlot)
                    .SharedTomponent(SecondSlot)
                    .SharedTomponent(ThirdSlot)
                    .SharedTomponent(FourthSlot);

   public RoutedCounter First { get; }
   public RoutedCounter Second { get; }
   public RoutedCounter Third { get; }
   public RoutedCounter Fourth { get; }

   RoutedHolder() : base(Appliers)
   {
      First = new RoutedCounter(FirstSlot.In(this));
      Second = new RoutedCounter(SecondSlot.In(this));
      Third = new RoutedCounter(ThirdSlot.In(this));
      Fourth = new RoutedCounter(FourthSlot.In(this));
   }

   protected override Type WrapperTeventImplementation => typeof(HolderTevent<HolderTevent>);

   /// Loads a holder from a compacted-history: its creation tevent, and when asked, a tevent that each slot adopts for its shared tomponent.
   internal static RoutedHolder Loaded(bool withATeventForEachSharedTomponent)
   {
      var holder = new RoutedHolder();
      var created = new HolderTevent.Created(new TaggregateId());
      if(withATeventForEachSharedTomponent)
         holder.LoadFromCompactedHistory(created, FirstSlot.Adopt(new CounterSet(1)), SecondSlot.Adopt(new CounterSet(2)), ThirdSlot.Adopt(new CounterSet(3)), FourthSlot.Adopt(new CounterSet(4)));
      else
         holder.LoadFromCompactedHistory(created);
      return holder;
   }
}

// ==== The tevents, as in TaggregateCost ====

interface IVocabTevent<out T> : ITaggregateTevent<T> where T : IVocabTevent;

interface IVocabTevent : ITaggregateTevent
{
   /// The stored state of a vocab, as one tevent: it is every property update at once.
   interface Loaded : ITaggregateCreatedTevent, PropertyUpdated.Question, PropertyUpdated.Answer, PropertyUpdated.Explanation, PropertyUpdated.Mnemonic,
                      Forms.PropertyUpdated.Values, PartsOfSpeech.PropertyUpdated.Values, CompoundParts.PropertyUpdated.Values, Synonyms.PropertyUpdated.Values;

   interface AnswerChanged : PropertyUpdated.Answer;

   interface PropertyUpdated : IVocabTevent
   {
      interface Question : PropertyUpdated { string Question { get; } }
      interface Answer : PropertyUpdated { string Answer { get; } }
      interface Explanation : PropertyUpdated { string Explanation { get; } }
      interface Mnemonic : PropertyUpdated { string Mnemonic { get; } }
   }

   interface Forms : IVocabTevent
   {
      interface Locked : Forms;
      interface PropertyUpdated : Forms { interface Values : PropertyUpdated { string[] FormValues { get; } } }
   }

   interface PartsOfSpeech : IVocabTevent
   {
      interface Locked : PartsOfSpeech;
      interface PropertyUpdated : PartsOfSpeech { interface Values : PropertyUpdated { string[] PartsOfSpeechValues { get; } } }
   }

   interface CompoundParts : IVocabTevent
   {
      interface Locked : CompoundParts;
      interface PropertyUpdated : CompoundParts { interface Values : PropertyUpdated { string[] CompoundPartsValues { get; } } }
   }

   interface Synonyms : IVocabTevent
   {
      interface Locked : Synonyms;
      interface PropertyUpdated : Synonyms { interface Values : PropertyUpdated { string[] SynonymsValues { get; } } }
   }
}

class VocabTevent<T>(T tevent) : TaggregateTevent<T>(tevent), IVocabTevent<T> where T : IVocabTevent;

abstract class VocabTevent : TaggregateTevent, IVocabTevent
{
   protected VocabTevent() {}
   protected VocabTevent(TaggregateId id) : base(id) {}

   internal class Loaded(TaggregateId id, string question, string answer, string explanation, string mnemonic, string[] forms, string[] partsOfSpeech, string[] compoundParts, string[] synonyms)
      : VocabTevent(id), IVocabTevent.Loaded
   {
      public string Question { get; } = question;
      public string Answer { get; } = answer;
      public string Explanation { get; } = explanation;
      public string Mnemonic { get; } = mnemonic;
      public string[] FormValues { get; } = forms;
      public string[] PartsOfSpeechValues { get; } = partsOfSpeech;
      public string[] CompoundPartsValues { get; } = compoundParts;
      public string[] SynonymsValues { get; } = synonyms;
   }

   internal class AnswerChanged(string answer) : VocabTevent, IVocabTevent.AnswerChanged
   {
      public string Answer { get; } = answer;
   }

   internal abstract class Forms : VocabTevent, IVocabTevent.Forms { internal class Locked : Forms, IVocabTevent.Forms.Locked; }
   internal abstract class PartsOfSpeech : VocabTevent, IVocabTevent.PartsOfSpeech { internal class Locked : PartsOfSpeech, IVocabTevent.PartsOfSpeech.Locked; }
   internal abstract class CompoundParts : VocabTevent, IVocabTevent.CompoundParts { internal class Locked : CompoundParts, IVocabTevent.CompoundParts.Locked; }
   internal abstract class Synonyms : VocabTevent, IVocabTevent.Synonyms { internal class Locked : Synonyms, IVocabTevent.Synonyms.Locked; }
}

// ==== The tevents, as in TeventiveChecks' measurement ====

interface ICounterTevent : ITevent
{
   interface Set : ICounterTevent { int Value { get; } }
}

class CounterSet(int value) : ICounterTevent.Set
{
   public int Value { get; } = value;
}

interface IHolderTevent : ITaggregateTevent
{
   interface Created : IHolderTevent, ITaggregateCreatedTevent;
}

interface IHolderTevent<out T> : ITaggregateTevent<T> where T : IHolderTevent;

class HolderTevent<T>(T tevent) : TaggregateTevent<T>(tevent), IHolderTevent<T> where T : IHolderTevent;

class HolderTevent : TaggregateTevent, IHolderTevent
{
   protected HolderTevent() {}
   HolderTevent(TaggregateId id) : base(id) {}

   internal class Created(TaggregateId id) : HolderTevent(id), IHolderTevent.Created;
}

interface IFirstTevent<out T> : IHolderTevent, IPublisherTevent<T> where T : ICounterTevent;
class FirstTevent<T>(T tevent) : HolderTevent, IFirstTevent<T> where T : ICounterTevent { public T Tevent { get; } = tevent; }
interface ISecondTevent<out T> : IHolderTevent, IPublisherTevent<T> where T : ICounterTevent;
class SecondTevent<T>(T tevent) : HolderTevent, ISecondTevent<T> where T : ICounterTevent { public T Tevent { get; } = tevent; }
interface IThirdTevent<out T> : IHolderTevent, IPublisherTevent<T> where T : ICounterTevent;
class ThirdTevent<T>(T tevent) : HolderTevent, IThirdTevent<T> where T : ICounterTevent { public T Tevent { get; } = tevent; }
interface IFourthTevent<out T> : IHolderTevent, IPublisherTevent<T> where T : ICounterTevent;
class FourthTevent<T>(T tevent) : HolderTevent, IFourthTevent<T> where T : ICounterTevent { public T Tevent { get; } = tevent; }
