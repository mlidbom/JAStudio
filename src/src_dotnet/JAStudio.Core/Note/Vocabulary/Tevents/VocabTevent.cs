using JAStudio.Core.Note.Tevents;

namespace JAStudio.Core.Note.Vocabulary.Tevents;

/// <summary>The base class of the classes of the <see cref="IVocabTevent"/>s: each interface has a class of the same name here.</summary>
public abstract partial class VocabTevent : NoteTevent, IVocabTevent
{
   protected VocabTevent() {}

   VocabTevent(VocabId vocabId) : base(vocabId) {}

   public class Created(VocabId vocabId, string question) : VocabTevent(vocabId), IVocabTevent.Created
   {
      public string Text { get; } = question;
   }
}

/// <summary>The class of the wrapper <see cref="IVocabTevent{TTevent}"/>.</summary>
public class VocabTevent<TTevent>(TTevent tevent) : NoteTevent<TTevent>(tevent), IVocabTevent<TTevent> where TTevent : IVocabTevent;
