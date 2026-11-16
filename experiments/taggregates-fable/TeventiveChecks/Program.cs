// Checks, against Compze.Teventive 0.6.0-alpha from nuget.org, of what JAStudio's notes would rely on in Compze.Teventive, of the two flaws that
// reading the code found, and of what a shared tomponent costs for each instance. The model is shaped as JAStudio's notes are: a base class with
// tevent appliers of its own under two subclasses, components, the same class of shared tomponent twice in a component, and a generic shared tomponent.
using System.Diagnostics;
using Compze.Abstractions;
using Compze.Tessaging.TessageTypes;
using Compze.Teventive;
using Compze.Teventive.Taggregates.BaseClasses;
using Compze.Teventive.Taggregates.BaseClasses.Shared;
using Compze.Teventive.Taggregates.Tevents;

const string theObserverThatPublishesEachTime = "the-observer-that-publishes-each-time";
if(args.Contains(theObserverThatPublishesEachTime))
{
   // Run in a process of its own, since it ends in a stack overflow, which nothing can catch.
   var endless = Vocab.Create("endless");
   ((ITaggregate)endless).TeventStream.Subscribe(new Observer(it =>
   {
      if(((INoteTevent.TagAdded)it.Tevent).Tag == "first") endless.AddTag("second");
   }));
   endless.AddTag("first");
   Console.WriteLine("The publish returned.");
   return;
}

Console.WriteLine("== What JAStudio's notes would rely on ==");

// One dispatcher for the whole collection, which every note's published tevents go to in their wrappers, as JAStudio would have.
var collectionWide = IMutableTeventDispatcher<INoteTevent>.New(TeventDispatcherConfig.IgnoreAllUnhandled);
List<string> changesToWhatAVocabMatches = [], relatedWordsChanged = [], antonymsAdded = [], tagsAddedToAVocab = [], tagsAddedToASentence = [], wordSetTeventsByTheirOwnType = [], audioAdded = [];
collectionWide.Register()
              .For<IVocabTevent.ChangeToWhatItMatches>(it => changesToWhatAVocabMatches.Add(it.GetType().Name))
              .For<IVocabTevent.RelatedWordsChanged>(it => relatedWordsChanged.Add(it is ISynonymsTevent<IWordSetTevent.Added> ? "synonyms" : it is IAntonymsTevent<IWordSetTevent.Added> ? "antonyms" : it.GetType().Name))
              .ForWrapped<IPublisherTevent<IAntonymsTevent<IWordSetTevent.Added>>>(it => antonymsAdded.Add(it.Tevent.Tevent.Word))
              .ForWrapped<IVocabTevent<INoteTevent.TagAdded>>(it => tagsAddedToAVocab.Add(it.Tevent.Tag))
              .ForWrapped<ISentenceTevent<INoteTevent.TagAdded>>(it => tagsAddedToASentence.Add(it.Tevent.Tag))
              .ForGenericTevent<IWordSetTevent.Added>(it => wordSetTeventsByTheirOwnType.Add(it.Word))
              .ForWrapped<IPublisherTevent<IAudioTevent<IMediaAdded<AudioRef>>>>(it => audioAdded.Add(it.Tevent.Tevent.Reference.FileName));

void Follow(Note note) => ((ITaggregate)note).TeventStream.Subscribe(new Observer(it => collectionWide.Dispatch((IPublisherTevent<INoteTevent>)it)));

var vocab = Vocab.Create("taberu", Follow);
vocab.Question.Rename("kuu");
vocab.Forms.Add("tabe");
vocab.AddTag("verb");
vocab.RelatedVocab.Synonyms.Add("kurau");
vocab.RelatedVocab.Antonyms.Add("haku");
vocab.Audio.Add(new AudioRef("taberu.mp3"));
var sentence = Sentence.Create(Follow);
sentence.AddTag("spoken");

Check("One tevent that two components apply: a rename, published by the question, is applied by the forms too",
      vocab.Question.Text == "kuu" && vocab.Forms.TimesItsCachesWereDropped == 3 && vocab.Forms.Entered.SequenceEqual(["tabe"]),
      $"question {vocab.Question.Text}, the forms' caches dropped {vocab.Forms.TimesItsCachesWereDropped} times");
Check("A subscription to a meaning that several tevents share gets each of them",
      changesToWhatAVocabMatches.SequenceEqual(["Created", "Renamed", "EnteredFormAdded"]), string.Join(", ", changesToWhatAVocabMatches));
Check("A base class's tevent applier runs in a subclass, and its tevent is told apart by the subclass that published it",
      vocab.Tags.SequenceEqual(["verb"]) && tagsAddedToAVocab.SequenceEqual(["verb"]) && tagsAddedToASentence.SequenceEqual(["spoken"]),
      $"vocab: {string.Join(", ", tagsAddedToAVocab)}; sentence: {string.Join(", ", tagsAddedToASentence)}");
Check("Two shared tomponents of one class, in a component, are told apart by their slots, and a subclass of the shared tomponent works",
      vocab.RelatedVocab.Synonyms.Words.SetEquals(["kurau"]) && vocab.RelatedVocab.Antonyms.Words.SetEquals(["haku"]) && antonymsAdded.SequenceEqual(["haku"]),
      $"synonyms {string.Join(", ", vocab.RelatedVocab.Synonyms.Words)}; antonyms {string.Join(", ", vocab.RelatedVocab.Antonyms.Words)}");
Check("A subscription to an interface of the owner's that a slot's adopting wrapper tevent implements gets the tevents adopted through that slot",
      relatedWordsChanged.SequenceEqual(["synonyms", "antonyms"]), string.Join(", ", relatedWordsChanged));
Check("A subscription to a shared tomponent's own tevent type gets nothing of what is adopted",
      wordSetTeventsByTheirOwnType.Count == 0, $"{wordSetTeventsByTheirOwnType.Count} received");
Check("A generic shared tomponent, with generic tevents, in a slot of the root", vocab.Audio.All.Count == 1 && audioAdded.SequenceEqual(["taberu.mp3"]), string.Join(", ", audioAdded));

var plain = IMutableTeventDispatcher<ITevent>.New(TeventDispatcherConfig.IgnoreAllUnhandled);
var mediaAddedOfAnyKind = 0;
plain.Register().For<IMediaAdded<MediaRef>>(_ => mediaAddedOfAnyKind++);
plain.Dispatch(new MediaAdded<AudioRef>(new AudioRef("covariant.mp3")));
Check("A generic tevent interface that isn't a wrapper is matched by covariance", mediaAddedOfAnyKind == 1, $"{mediaAddedOfAnyKind} received");

Console.WriteLine();
Console.WriteLine("== Loading from a compacted-history, with what Compze has today ==");
var loaded = Vocab.Loaded(new TaggregateId(), "nomu", synonyms: ["suu", "inryou"], antonyms: ["haku"]);
Check("A vocab loads from a creation tevent and a tevent adopted for each shared tomponent's word",
      loaded.Question.Text == "nomu" && loaded.RelatedVocab.Synonyms.Words.SetEquals(["suu", "inryou"]) && loaded.RelatedVocab.Antonyms.Words.SetEquals(["haku"]) && loaded.Version == 4,
      $"version {loaded.Version}");
var uncommittedAfterLoading = -1;
((ITaggregate)loaded).Commit(it => uncommittedAfterLoading = it.Count);
loaded.AddTag("drink");
Check("Loading leaves nothing uncommitted, and the loaded vocab publishes", uncommittedAfterLoading == 0 && loaded.Version == 5 && loaded.Tags.SequenceEqual(["drink"]), $"{uncommittedAfterLoading} uncommitted, version {loaded.Version}");
var unstamped = Vocab.LoadedWithoutStampingTheVersion(new TaggregateId(), "miru");
var refusal = Thrown(() => unstamped.AddTag("see"));
Check("Without the version stamped through the member for internal use, the loaded vocab refuses its next tevent", refusal is not null, refusal?.Message ?? "it published");

Console.WriteLine();
Console.WriteLine("== The two places that don't stand a publish from inside them ==");
var observed = Vocab.Create("observed");
List<string> notified = [];
var hasPublished = false;
((ITaggregate)observed).TeventStream.Subscribe(new Observer(it =>
{
   var tag = ((INoteTevent.TagAdded)it.Tevent).Tag;
   notified.Add(tag);
   if(tag != "first" || hasPublished) return;
   hasPublished = true;
   observed.AddTag("second");
}));
var thrownByThePublish = Thrown(() => observed.AddTag("first"));
Console.WriteLine($"   An observer that publishes once, on the taggregate it observes: notified of [{string.Join(", ", notified)}], tags now [{string.Join(", ", observed.Tags)}],");
Console.WriteLine($"   and the publish threw {thrownByThePublish?.GetType().Name ?? "nothing"}: {thrownByThePublish?.Message}");

using(var child = Process.Start(new ProcessStartInfo(Environment.ProcessPath!, [Environment.GetCommandLineArgs()[0], theObserverThatPublishesEachTime]) { RedirectStandardError = true, RedirectStandardOutput = true })!)
{
   var standardError = child.StandardError.ReadToEndAsync();
   var standardOutput = child.StandardOutput.ReadToEndAsync();
   child.WaitForExit();
   const char lineFeed = (char)10;
   var firstLineOfTheError = standardError.Result.Split(lineFeed).FirstOrDefault(it => it.Trim() != "")?.Trim() ?? "";
   Console.WriteLine($"   An observer that publishes each time it is notified of the tevent it reacts to, in a process of its own: exit code {child.ExitCode}, it said \"{firstLineOfTheError}{standardOutput.Result.Trim()}\"");
}

var committed = Vocab.Create("committed");
((ITaggregate)committed).Commit(_ => {});
committed.AddTag("before");
int handedToTheCallback = -1, uncommittedAfterwards = -1;
var thrownByTheCommit = Thrown(() => ((ITaggregate)committed).Commit(it =>
{
   handedToTheCallback = it.Count;
   committed.AddTag("during the commit");
}));
((ITaggregate)committed).Commit(it => uncommittedAfterwards = it.Count);
Console.WriteLine($"   A tevent published from inside Commit's callback: the callback was handed {handedToTheCallback}, the tags are [{string.Join(", ", committed.Tags)}],");
Console.WriteLine($"   the next Commit was handed {uncommittedAfterwards}, and the commit threw {thrownByTheCommit?.GetType().Name ?? "nothing"}.");

Console.WriteLine();
Console.WriteLine("== What a shared tomponent costs for each instance (80,000 taggregates) ==");
const int count = 80_000;
Holder.SharedTomponentCount = 0;
var none = Measure("No shared tomponent, loaded from its creation tevent", () => Holder.Loaded(withATeventForEachSharedTomponent: false));
Holder.SharedTomponentCount = 4;
var four = Measure("4 shared tomponents, loaded from its creation tevent only", () => Holder.Loaded(withATeventForEachSharedTomponent: false));
var fourLoaded = Measure("4 shared tomponents, loaded from its creation tevent and one tevent adopted for each", () => Holder.Loaded(withATeventForEachSharedTomponent: true));
Console.WriteLine($"   A shared tomponent that has seen no tevent keeps {(four.Kept - none.Kept) / 4:N0} B, and one that has seen its first {(fourLoaded.Kept - none.Kept) / 4:N0} B.");
Console.WriteLine($"   Loading one adopted tevent allocates {(fourLoaded.Allocated - four.Allocated) / 4:N0} B and takes {(fourLoaded.Microseconds - four.Microseconds) / 4:N2} microseconds.");

static void Check(string what, bool holds, string detail) => Console.WriteLine($"   {(holds ? "HOLDS" : "FAILS")}: {what} [{detail}]");

static Exception? Thrown(Action action)
{
   try
   {
      action();
      return null;
   }
   catch(Exception exception)
   {
      return exception;
   }
}

static (long Kept, long Allocated, double Microseconds) Measure<T>(string what, Func<T> create) where T : class
{
   create(); // warm up: static constructors, reflection caches, the JIT
   var holder = new T[count];
   var keptBefore = GC.GetTotalMemory(forceFullCollection: true);
   var allocatedBefore = GC.GetTotalAllocatedBytes(precise: true);
   var started = Stopwatch.GetTimestamp();
   for(var index = 0; index < holder.Length; index++) holder[index] = create();
   var elapsed = Stopwatch.GetElapsedTime(started);
   var allocated = (GC.GetTotalAllocatedBytes(precise: true) - allocatedBefore) / count;
   var kept = (GC.GetTotalMemory(forceFullCollection: true) - keptBefore) / count;
   Console.WriteLine($"   {what}: {kept:N0} B kept, {allocated:N0} B allocated, {elapsed.TotalMicroseconds / count:N2} microseconds, for each");
   GC.KeepAlive(holder);
   return (kept, allocated, elapsed.TotalMicroseconds / count);
}

sealed class Observer(Action<ITaggregateTevent<ITaggregateTevent>> onNext) : IObserver<ITaggregateTevent<ITaggregateTevent>>
{
   public void OnCompleted() {}
   public void OnError(Exception error) {}
   public void OnNext(ITaggregateTevent<ITaggregateTevent> value) => onNext(value);
}

// ---- The notes' base class: tevents and tevent appliers of its own, under the vocab and the sentence ----

interface INoteTevent : ITaggregateTevent
{
   interface TagAdded : INoteTevent { string Tag { get; } }
}

interface INoteTevent<out T> : ITaggregateTevent<T> where T : INoteTevent;

class NoteTevent<T>(T tevent) : TaggregateTevent<T>(tevent), INoteTevent<T> where T : INoteTevent;

abstract class NoteTevent : TaggregateTevent, INoteTevent
{
   protected NoteTevent() {}
   protected NoteTevent(TaggregateId id) : base(id) {}

   internal class TagAdded(string tag) : NoteTevent, INoteTevent.TagAdded
   {
      public string Tag { get; } = tag;
   }
}

abstract class Note : Taggregate<Note, INoteTevent, NoteTevent, INoteTevent<INoteTevent>, NoteTevent<NoteTevent>>
{
   readonly List<string> _tags = [];
   public IReadOnlyList<string> Tags => _tags;

   protected Note() => RegisterTeventAppliers().For<INoteTevent.TagAdded>(it => _tags.Add(it.Tag));

   public void AddTag(string tag) => Publish(new NoteTevent.TagAdded(tag));
}

// ---- The sentence: a second subclass, so that a base class's tevent can be told apart by who published it ----

interface ISentenceTevent : INoteTevent
{
   interface Created : ISentenceTevent, ITaggregateCreatedTevent;
}

interface ISentenceTevent<out T> : INoteTevent<T> where T : INoteTevent;

class SentenceTevent<T>(T tevent) : NoteTevent<T>(tevent), ISentenceTevent<T> where T : INoteTevent;

abstract class SentenceTevent : NoteTevent, ISentenceTevent
{
   protected SentenceTevent(TaggregateId id) : base(id) {}

   internal class Created(TaggregateId id) : SentenceTevent(id), ISentenceTevent.Created;
}

class Sentence : Note
{
   protected override Type WrapperTeventImplementation => typeof(SentenceTevent<SentenceTevent>);

   Sentence() => RegisterTeventAppliers().For<ISentenceTevent.Created>(_ => {});

   public static Sentence Create(Action<Note> beforeItPublishes)
   {
      var sentence = new Sentence();
      beforeItPublishes(sentence);
      sentence.Publish(new SentenceTevent.Created(new TaggregateId()));
      return sentence;
   }
}

// ---- The vocab ----

interface IVocabTevent : INoteTevent
{
   /// The creation tevent is also the question's text being set, which the forms take as a change to the matched forms.
   interface Created : ITaggregateCreatedTevent, Question.TextSet;

   /// A meaning several tevents share: after it, sentences may parse differently.
   interface ChangeToWhatItMatches : IVocabTevent;

   /// A meaning the adopting wrapper tevents of the word sets' slots share.
   interface RelatedWordsChanged : IVocabTevent;

   interface Question : IVocabTevent
   {
      /// The question is one of the vocab's forms, so setting its text changes the matched forms: one tevent that two components apply.
      interface TextSet : Question, Forms.MatchedFormsChanged { string Text { get; } }

      interface Renamed : TextSet;
   }

   interface Forms : IVocabTevent
   {
      interface MatchedFormsChanged : Forms, ChangeToWhatItMatches;
      interface EnteredFormAdded : MatchedFormsChanged { string Form { get; } }
   }

   interface RelatedVocab : IVocabTevent;
}

interface IVocabTevent<out T> : INoteTevent<T> where T : INoteTevent;

class VocabTevent<T>(T tevent) : NoteTevent<T>(tevent), IVocabTevent<T> where T : INoteTevent;

abstract class VocabTevent : NoteTevent, IVocabTevent
{
   protected VocabTevent() {}
   protected VocabTevent(TaggregateId id) : base(id) {}

   internal class Created(TaggregateId id, string text) : VocabTevent(id), IVocabTevent.Created
   {
      public string Text { get; } = text;
   }

   internal abstract class Question : VocabTevent, IVocabTevent.Question
   {
      internal class Renamed(string text) : Question, IVocabTevent.Question.Renamed
      {
         public string Text { get; } = text;
      }
   }

   internal abstract class Forms : VocabTevent, IVocabTevent.Forms
   {
      internal class EnteredFormAdded(string form) : Forms, IVocabTevent.Forms.EnteredFormAdded
      {
         public string Form { get; } = form;
      }
   }

   internal abstract class RelatedVocab : VocabTevent, IVocabTevent.RelatedVocab;
}

/// The adopting wrapper tevent of the synonyms' slot. It is also a RelatedWordsChanged of the vocab's.
interface ISynonymsTevent<out T> : IVocabTevent.RelatedVocab, IVocabTevent.RelatedWordsChanged, IPublisherTevent<T> where T : IWordSetTevent;

class SynonymsTevent<T>(T tevent) : VocabTevent.RelatedVocab, ISynonymsTevent<T> where T : IWordSetTevent
{
   public T Tevent { get; } = tevent;
}

interface IAntonymsTevent<out T> : IVocabTevent.RelatedVocab, IVocabTevent.RelatedWordsChanged, IPublisherTevent<T> where T : IWordSetTevent;

class AntonymsTevent<T>(T tevent) : VocabTevent.RelatedVocab, IAntonymsTevent<T> where T : IWordSetTevent
{
   public T Tevent { get; } = tevent;
}

interface IAudioTevent<out T> : IVocabTevent, IPublisherTevent<T> where T : IMediaListTevent<AudioRef>;

class AudioTevent<T>(T tevent) : VocabTevent, IAudioTevent<T> where T : IMediaListTevent<AudioRef>
{
   public T Tevent { get; } = tevent;
}

class Vocab : Note
{
   protected override Type WrapperTeventImplementation => typeof(VocabTevent<VocabTevent>);

   public QuestionPart Question { get; }
   public FormsPart Forms { get; }
   public RelatedVocabPart RelatedVocab { get; }
   public MediaList<AudioRef> Audio { get; }

   Vocab()
   {
      Question = new QuestionPart(this);
      Forms = new FormsPart(this);
      RelatedVocab = new RelatedVocabPart(this);
      Audio = new MediaList<AudioRef>(new SharedTomponentSlot<INoteTevent, NoteTevent, IMediaListTevent<AudioRef>, IAudioTevent<IMediaListTevent<AudioRef>>>(this, typeof(AudioTevent<IMediaListTevent<AudioRef>>)));
   }

   public static Vocab Create(string question, Action<Note>? beforeItPublishes = null)
   {
      var vocab = new Vocab();
      beforeItPublishes?.Invoke(vocab);
      vocab.Publish(new VocabTevent.Created(new TaggregateId(), question));
      return vocab;
   }

   /// Loads a vocab from its stored state as a compacted-history: its creation tevent, and a tevent adopted for each word of each word set. The
   /// adopting wrapper tevents are constructed here by hand, and the versions set through a member marked for Compze's internal use.
   public static Vocab Loaded(TaggregateId id, string question, string[] synonyms, string[] antonyms) => Loaded(id, question, synonyms, antonyms, stampTheVersions: true);

   public static Vocab LoadedWithoutStampingTheVersion(TaggregateId id, string question) => Loaded(id, question, [], [], stampTheVersions: false);

   static Vocab Loaded(TaggregateId id, string question, string[] synonyms, string[] antonyms, bool stampTheVersions)
   {
      NoteTevent[] compactedHistory =
      [
         new VocabTevent.Created(id, question),
         ..synonyms.Select(NoteTevent (it) => new SynonymsTevent<WordSetTevent.Added>(new WordSetTevent.Added(it))),
         ..antonyms.Select(NoteTevent (it) => new AntonymsTevent<WordSetTevent.Added>(new WordSetTevent.Added(it)))
      ];

      var vocab = new Vocab();
      var version = 0;
      ((ITaggregate)vocab).LoadFromHistory(compactedHistory.Select(tevent =>
      {
         var stamped = (IMutableTaggregateTevent)tevent;
         stamped.SetTaggregateIdInternal(id);
         if(stampTheVersions) stamped.SetTaggregateVersionInternal(++version);
         return TaggregateIdentifyingTevent.WrapIn(typeof(VocabTevent<VocabTevent>), tevent);
      }).ToList());
      return vocab;
   }
}

class QuestionPart : Note.Component<QuestionPart, IVocabTevent.Question, VocabTevent.Question>
{
   public string Text { get; private set; } = "";

   public QuestionPart(Vocab vocab) : base(vocab) => RegisterTeventAppliers().For<IVocabTevent.Question.TextSet>(it => Text = it.Text);

   public void Rename(string text) => Publish(new VocabTevent.Question.Renamed(text));
}

class FormsPart : Note.Component<FormsPart, IVocabTevent.Forms, VocabTevent.Forms>
{
   readonly List<string> _entered = [];
   public IReadOnlyList<string> Entered => _entered;
   public int TimesItsCachesWereDropped { get; private set; }

   public FormsPart(Vocab vocab) : base(vocab) =>
      RegisterTeventAppliers().For<IVocabTevent.Forms.MatchedFormsChanged>(_ => TimesItsCachesWereDropped++)
                              .For<IVocabTevent.Forms.EnteredFormAdded>(it => _entered.Add(it.Form));

   public void Add(string form) => Publish(new VocabTevent.Forms.EnteredFormAdded(form));
}

/// A component that holds two shared tomponents of one class, as a vocab's related vocab holds its word sets.
class RelatedVocabPart : Note.Component<RelatedVocabPart, IVocabTevent.RelatedVocab, VocabTevent.RelatedVocab>
{
   public WordSet Synonyms { get; }
   public MutualWordSet Antonyms { get; }

   public RelatedVocabPart(Vocab vocab) : base(vocab)
   {
      Synonyms = new WordSet(new SharedTomponentSlot<IVocabTevent.RelatedVocab, VocabTevent.RelatedVocab, IWordSetTevent, ISynonymsTevent<IWordSetTevent>>(this, typeof(SynonymsTevent<IWordSetTevent>)));
      Antonyms = new MutualWordSet(new SharedTomponentSlot<IVocabTevent.RelatedVocab, VocabTevent.RelatedVocab, IWordSetTevent, IAntonymsTevent<IWordSetTevent>>(this, typeof(AntonymsTevent<IWordSetTevent>)));
   }
}

// ---- The shared tomponents ----

interface IWordSetTevent : ITevent
{
   interface Added : IWordSetTevent { string Word { get; } }
}

class WordSetTevent : IWordSetTevent
{
   internal class Added(string word) : WordSetTevent, IWordSetTevent.Added
   {
      public string Word { get; } = word;
   }
}

class WordSet : SharedTomponent<IWordSetTevent>
{
   readonly HashSet<string> _words = [];
   public IReadOnlySet<string> Words => _words;

   public WordSet(ISharedTomponentSlot<IWordSetTevent> slot) : base(slot) => RegisterTeventAppliers().For<IWordSetTevent.Added>(it => _words.Add(it.Word));

   public void Add(string word) => Publish(new WordSetTevent.Added(word));
}

class MutualWordSet(ISharedTomponentSlot<IWordSetTevent> slot) : WordSet(slot);

record MediaRef(string FileName);

record AudioRef(string FileName) : MediaRef(FileName);

interface IMediaListTevent<out TReference> : ITevent where TReference : MediaRef;

interface IMediaAdded<out TReference> : IMediaListTevent<TReference> where TReference : MediaRef
{
   TReference Reference { get; }
}

class MediaAdded<TReference>(TReference reference) : IMediaAdded<TReference> where TReference : MediaRef
{
   public TReference Reference { get; } = reference;
}

class MediaList<TReference> : SharedTomponent<IMediaListTevent<TReference>> where TReference : MediaRef
{
   readonly List<TReference> _all = [];
   public IReadOnlyList<TReference> All => _all;

   public MediaList(ISharedTomponentSlot<IMediaListTevent<TReference>> slot) : base(slot) => RegisterTeventAppliers().For<IMediaAdded<TReference>>(it => _all.Add(it.Reference));

   public void Add(TReference reference) => Publish(new MediaAdded<TReference>(reference));
}

// ---- For measuring what a shared tomponent costs: a taggregate with none or four, each holding an integer ----

interface ICounterTevent : ITevent
{
   interface Set : ICounterTevent { int Value { get; } }
}

class CounterSet(int value) : ICounterTevent.Set
{
   public int Value { get; } = value;
}

class Counter : SharedTomponent<ICounterTevent>
{
   public int Value { get; private set; }

   public Counter(ISharedTomponentSlot<ICounterTevent> slot) : base(slot) => RegisterTeventAppliers().For<ICounterTevent.Set>(it => Value = it.Value);
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

class Holder : Taggregate<Holder, IHolderTevent, HolderTevent, IHolderTevent<IHolderTevent>, HolderTevent<HolderTevent>>
{
   /// How many shared tomponents a holder is given, none or four.
   internal static int SharedTomponentCount;

   public Counter? First { get; }
   public Counter? Second { get; }
   public Counter? Third { get; }
   public Counter? Fourth { get; }

   Holder()
   {
      RegisterTeventAppliers().For<IHolderTevent.Created>(_ => {});
      if(SharedTomponentCount == 0) return;

      First = new Counter(new SharedTomponentSlot<IHolderTevent, HolderTevent, ICounterTevent, IFirstTevent<ICounterTevent>>(this, typeof(FirstTevent<ICounterTevent>)));
      Second = new Counter(new SharedTomponentSlot<IHolderTevent, HolderTevent, ICounterTevent, ISecondTevent<ICounterTevent>>(this, typeof(SecondTevent<ICounterTevent>)));
      Third = new Counter(new SharedTomponentSlot<IHolderTevent, HolderTevent, ICounterTevent, IThirdTevent<ICounterTevent>>(this, typeof(ThirdTevent<ICounterTevent>)));
      Fourth = new Counter(new SharedTomponentSlot<IHolderTevent, HolderTevent, ICounterTevent, IFourthTevent<ICounterTevent>>(this, typeof(FourthTevent<ICounterTevent>)));
   }

   internal static Holder Loaded(bool withATeventForEachSharedTomponent)
   {
      var id = new TaggregateId();
      HolderTevent[] compactedHistory = withATeventForEachSharedTomponent
                                           ? [new HolderTevent.Created(id), new FirstTevent<CounterSet>(new CounterSet(1)), new SecondTevent<CounterSet>(new CounterSet(2)), new ThirdTevent<CounterSet>(new CounterSet(3)), new FourthTevent<CounterSet>(new CounterSet(4))]
                                           : [new HolderTevent.Created(id)];

      var holder = new Holder();
      var history = new List<ITaggregateTevent<ITaggregateTevent>>(compactedHistory.Length);
      for(var index = 0; index < compactedHistory.Length; index++)
      {
         var stamped = (IMutableTaggregateTevent)compactedHistory[index];
         stamped.SetTaggregateIdInternal(id);
         stamped.SetTaggregateVersionInternal(index + 1);
         history.Add(TaggregateIdentifyingTevent.WrapIn(typeof(HolderTevent<HolderTevent>), compactedHistory[index]));
      }

      ((ITaggregate)holder).LoadFromHistory(history);
      return holder;
   }
}
