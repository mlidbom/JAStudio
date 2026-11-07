using JAStudio.Core.Note.NoteFields;
using JAStudio.Core.Note.Tagging;

namespace JAStudio.Core.Note.Vocabulary;

public class VocabNoteRegister
{
   public VocabNoteRegister(VocabNote vocab) => Vocab = vocab;

   VocabNote Vocab { get; }

   public TagFlagField Polite => new(Vocab, Tags.Vocab.Register.Polite);
   public TagFlagField Formal => new(Vocab, Tags.Vocab.Register.Formal);
   public TagFlagField Informal => new(Vocab, Tags.Vocab.Register.Informal);
   public TagFlagField Archaic => new(Vocab, Tags.Vocab.Register.Archaic);
   public TagFlagField Sensitive => new(Vocab, Tags.Vocab.Register.Sensitive);
   public TagFlagField Vulgar => new(Vocab, Tags.Vocab.Register.Vulgar);
   public TagFlagField Humble => new(Vocab, Tags.Vocab.Register.Humble);
   public TagFlagField Literary => new(Vocab, Tags.Vocab.Register.Literary);
   public TagFlagField Honorific => new(Vocab, Tags.Vocab.Register.Honorific);
   public TagFlagField RoughMasculine => new(Vocab, Tags.Vocab.Register.RoughMasculine);
   public TagFlagField SoftFeminine => new(Vocab, Tags.Vocab.Register.SoftFeminine);
   public TagFlagField Slang => new(Vocab, Tags.Vocab.Register.Slang);
   public TagFlagField Derogatory => new(Vocab, Tags.Vocab.Register.Derogatory);
   public TagFlagField Childish => new(Vocab, Tags.Vocab.Register.Childish);
}
