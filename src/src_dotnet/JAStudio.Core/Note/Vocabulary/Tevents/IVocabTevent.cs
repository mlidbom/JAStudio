using Compze.Teventive.Taggregates.Tevents;
using JAStudio.Core.Note.Tevents;

namespace JAStudio.Core.Note.Vocabulary.Tevents;

/// <summary>
/// Something that happened to a vocab (<see cref="VocabNote"/>): the root of a vocab's tevents. Each part of a vocab that changes has a group of<br/>
/// its own below it, which is itself a tevent: subscribing to the group, such as <see cref="Forms"/>, is subscribing to every change to that<br/>
/// part, and subscribing to one of its tevents, such as <see cref="Forms.Added"/>, to that change only.
/// </summary>
public partial interface IVocabTevent : INoteTevent
{
   /// <summary>
   /// A new vocab was created, with <see cref="IVocabTevent.Question.TextChanged.Text"/> as its question. Never published for a vocab loaded from the corpus:<br/>
   /// a vocab is loaded from its stored data, and publishes nothing.
   /// </summary>
   /// <remarks>It is also a <see cref="IVocabTevent.Question.TextChanged"/>, so that what follows a vocab's question follows a new vocab's too.</remarks>
   public interface Created : IVocabTevent, ITaggregateCreatedTevent, Question.TextChanged;
}

/// <summary>
/// A tevent as a vocab published it: see <see cref="INoteTevent{TTevent}"/>. Any <see cref="INoteTevent"/>, since a vocab also publishes the tevents<br/>
/// every note has.
/// </summary>
public interface IVocabTevent<out TTevent> : INoteTevent<TTevent> where TTevent : INoteTevent;
