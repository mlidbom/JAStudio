// A reviewer's checks of the taggregate design, run against the combined prototype with static slots (PROTOTYPE defined) or Compze.Teventive 0.6.0-alpha.
using Compze.Abstractions;
using Compze.Teventive;
using Compze.Teventive.Taggregates.BaseClasses;
using Compze.Teventive.Taggregates.Tevents;

Console.WriteLine("== A commit callback that throws, as a failed save of a note's file would ==");
{
   var note = Note.Create("N", null);
   ((ITaggregate)note).Commit(_ => {});
   note.Edit("edited");
   try
   {
      ((ITaggregate)note).Commit(_ => throw new IOException("the disk said no"));
   }
   catch(IOException) {}

   var handedToTheNextCommit = -1;
   ((ITaggregate)note).Commit(tevents => handedToTheNextCommit = tevents.Count);
   Console.WriteLine($"   The edit is applied (text: {note.Text}), and the next Commit was handed {handedToTheNextCommit} tevent(s).");
}

Console.WriteLine("== Without Taggregates: plain tevents that no taggregate publishes, on one dispatcher, subscribed to by a meaning they share ==");
{
   var collection = IMutableTeventDispatcher<IVocabChange>.New(TeventDispatcherConfig.IgnoreAllUnhandled);
   var told = new List<string>();
   collection.Register().For<IVocabChange.ChangesWhatTheVocabMatches>(it => told.Add(it.GetType().Name));
   collection.Dispatch(new VocabChange.FormsChanged());
   collection.Dispatch(new VocabChange.QuestionChanged());
   collection.Dispatch(new VocabChange.MnemonicChanged());
   var formsChanged = new VocabChange.FormsChanged();
   const int dispatches = 100_000;
   var allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
   for(var index = 0; index < dispatches; index++) collection.Dispatch(formsChanged);
   var allocatedForEach = (GC.GetAllocatedBytesForCurrentThread() - allocatedBefore) / dispatches;
   Console.WriteLine($"   Dispatched FormsChanged, QuestionChanged, MnemonicChanged. The subscriber to what changes what the vocab matches was told: {string.Join(", ", told.Take(3))}");
   Console.WriteLine($"   Each dispatch allocates {allocatedForEach} B, and a note keeps nothing for it.");
}

#if PROTOTYPE
Console.WriteLine("== Two notes on one collection dispatcher: a subscriber that reacts to a change of note A by changing note B ==");
{
   var collection = IMutableTeventDispatcher<INoteTevent>.New(TeventDispatcherConfig.IgnoreAllUnhandled);
   var a = Note.Create("A", collection);
   var b = Note.Create("B", collection);
   var names = new Dictionary<TaggregateId, string> { [a.Id] = "A", [b.Id] = "B" };
   var seenByTheSecondSubscriber = new List<string>();
   collection.Register().For<INoteTevent.Edited>(it => { if(it.TaggregateId == a.Id) b.Mark(); });
   collection.Register().For<INoteTevent>(it => seenByTheSecondSubscriber.Add($"{names[it.TaggregateId]}.{it.GetType().Name}"));
   a.Edit("edited");
   Console.WriteLine($"   Published in the order A.Edited, B.Marked. The second subscriber was told: {string.Join(", ", seenByTheSecondSubscriber)}");
}

Console.WriteLine("== A second thread publishes on a note while the first thread is still inside the note's hook, saving and dispatching ==");
{
   var note = Note.Create("N", null);
   using var theHookIsRunning = new ManualResetEventSlim();
   using var letTheHookReturn = new ManualResetEventSlim();
   note.WhenAPublishCascadeEnds(() =>
   {
      theHookIsRunning.Set();
      letTheHookReturn.Wait();
   });
   var firstThread = Task.Run(() => note.Edit("from the first thread"));
   theHookIsRunning.Wait();
   note.WhenAPublishCascadeEnds(() => {});
   try
   {
      note.Edit("from the second thread");
      Console.WriteLine("   The second thread's publish went through.");
   }
   catch(Exception exception)
   {
      Console.WriteLine($"   The second thread's publish threw {exception.GetType().Name}: {exception.Message}");
   }

   letTheHookReturn.Set();
   firstThread.Wait();
}
#endif

interface IVocabChange : Compze.Tessaging.TessageTypes.ITevent
{
   interface ChangesWhatTheVocabMatches : IVocabChange;
   interface FormsChanged : ChangesWhatTheVocabMatches;
   interface QuestionChanged : ChangesWhatTheVocabMatches;
   interface MnemonicChanged : IVocabChange;
}

static class VocabChange
{
   internal class FormsChanged : IVocabChange.FormsChanged;
   internal class QuestionChanged : IVocabChange.QuestionChanged;
   internal class MnemonicChanged : IVocabChange.MnemonicChanged;
}

interface INoteTevent : ITaggregateTevent
{
   interface Created : INoteTevent, ITaggregateCreatedTevent;

   interface Edited : INoteTevent
   {
      string Text { get; }
   }

   interface Marked : INoteTevent;
}

interface INoteTevent<out T> : ITaggregateTevent<T> where T : INoteTevent;

class NoteTevent<T>(T tevent) : TaggregateTevent<T>(tevent), INoteTevent<T> where T : INoteTevent;

abstract class NoteTevent : TaggregateTevent, INoteTevent
{
   protected NoteTevent() {}
   NoteTevent(TaggregateId taggregateId) : base(taggregateId) {}

   internal class Created(TaggregateId taggregateId) : NoteTevent(taggregateId), INoteTevent.Created;

   internal class Edited(string text) : NoteTevent, INoteTevent.Edited
   {
      public string Text { get; } = text;
   }

   internal class Marked : NoteTevent, INoteTevent.Marked;
}

class Note : Taggregate<Note, INoteTevent, NoteTevent, INoteTevent<INoteTevent>, NoteTevent<NoteTevent>>
{
   readonly IMutableTeventDispatcher<INoteTevent>? _collection;
   Action? _whenAPublishCascadeEnds;

   Note(string name, IMutableTeventDispatcher<INoteTevent>? collection)
   {
      Name = name;
      _collection = collection;
      RegisterTeventAppliers()
        .For<INoteTevent.Created>(_ => {})
        .For<INoteTevent.Edited>(it => Text = it.Text)
        .For<INoteTevent.Marked>(_ => {});
   }

   public string Name { get; }
   public string Text { get; private set; } = "";

   public static Note Create(string name, IMutableTeventDispatcher<INoteTevent>? collection)
   {
      var note = new Note(name, collection);
      note.Publish(new NoteTevent.Created(new TaggregateId()));
      return note;
   }

   public void Edit(string text) => Publish(new NoteTevent.Edited(text));
   public void Mark() => Publish(new NoteTevent.Marked());
   public void WhenAPublishCascadeEnds(Action action) => _whenAPublishCascadeEnds = action;

#if PROTOTYPE
   /// As the documents describe JPNote's override: commit, then dispatch each tevent on the collection's one dispatcher.
   protected override void OnPublishCascadeEnded(IReadOnlyList<ITaggregateTevent<INoteTevent>> teventsOfTheCascade)
   {
      _whenAPublishCascadeEnds?.Invoke();
      if(_collection is null) return;
      ((ITaggregate)this).Commit(_ => {});
      foreach(var tevent in teventsOfTheCascade) _collection.Dispatch(tevent);
   }
#endif
}
