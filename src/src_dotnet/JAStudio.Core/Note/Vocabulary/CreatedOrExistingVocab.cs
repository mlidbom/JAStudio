namespace JAStudio.Core.Note.Vocabulary;

/// <summary>
/// What creating a vocab gives back, such as creating a vocab's て-form (<see cref="VocabCloner"/>): the vocab created, or the vocab that had its question<br/>
/// already, since no two vocab share a question, and which of the two it is. A person who asked for a new vocab can then be told why none appeared.
/// </summary>
public sealed class CreatedOrExistingVocab
{
   CreatedOrExistingVocab(VocabNote vocab, bool existedAlready)
   {
      Vocab = vocab;
      ExistedAlready = existedAlready;
   }

   /// <summary>A <paramref name="vocab"/> that was just created.</summary>
   public static CreatedOrExistingVocab Created(VocabNote vocab) => new(vocab, existedAlready: false);

   /// <summary>A <paramref name="vocab"/> that had the question of the vocab to create already, so that none was created.</summary>
   public static CreatedOrExistingVocab Existing(VocabNote vocab) => new(vocab, existedAlready: true);

   public VocabNote Vocab { get; }

   /// <summary>Whether <see cref="Vocab"/> had the question of the vocab to create already, so that none was created.</summary>
   public bool ExistedAlready { get; }
}
