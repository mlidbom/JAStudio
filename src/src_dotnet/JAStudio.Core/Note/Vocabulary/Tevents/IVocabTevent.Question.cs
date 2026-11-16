namespace JAStudio.Core.Note.Vocabulary.Tevents;

public partial interface IVocabTevent
{
   /// <summary>
   /// The vocab's question changed (<see cref="VocabNoteQuestion"/>): the text that names the vocab, or the disambiguation that tells it apart<br/>
   /// from other vocab with the same text. Either renames the vocab, so the word-references to it, in other notes, are renamed with it.
   /// </summary>
   public interface Question : IVocabTevent
   {
      /// <summary>The question's text became <see cref="Text"/>.</summary>
      /// <remarks>
      /// A question is always one of the vocab's forms, so when <see cref="Text"/> was one of its entered forms, a <see cref="IVocabTevent.Forms.Removed"/><br/>
      /// follows it.
      /// </remarks>
      public interface TextChanged : Question
      {
         string Text { get; }
      }

      /// <summary>The question's disambiguation became <see cref="Disambiguation"/>, empty when the vocab needs none.</summary>
      public interface DisambiguationChanged : Question
      {
         string Disambiguation { get; }
      }
   }
}
