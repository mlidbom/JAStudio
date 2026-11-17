using Compze.Contracts;
using Compze.Tessaging.TessageBus;
using Compze.Teventive;
using Compze.Teventive.Taggregates.BaseClasses;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Tevents;
using JAStudio.Core.Note.Vocabulary.Tevents;
using JAStudio.Core.Storage.Dto;

namespace JAStudio.Core.Note.Vocabulary;

/// <summary>
/// A <see cref="VocabNote"/>'s question: its <see cref="Text"/>, the word itself, and, when another vocab has the same text,<br/>
/// a <see cref="Disambiguation"/> that tells them apart, such as <c>masu</c> for the <c>入る</c> that follows a masu stem.
/// </summary>
/// <remarks>
/// Other notes refer to a vocab by a <see cref="WordReference"/> that names it (<see cref="Reference"/>), such as a synonym or a compound part.
/// </remarks>
/// <remarks>
/// A component of its vocab: a new vocab's created tevent gives it its <see cref="Text"/>, and renaming the vocab publishes<br/>
/// <see cref="IVocabTevent.Question.Renamed"/>, with both, which its appliers apply.
/// </remarks>
public class VocabNoteQuestion : TeventiveComponent<VocabNote, INoteTevent, NoteTevent, VocabNoteQuestion, IVocabTevent.Question, VocabTevent.Question>, ITeventRouted
{
   /// <remarks>A new <see cref="Text"/>, applied, also discards what the vocab's forms derived from it (<see cref="VocabNoteForms.OnQuestionChanged"/>).</remarks>
   static readonly TeventRouting<VocabNoteQuestion> Appliers = TeventRouting<VocabNoteQuestion>.DeclareAppliers(it => it
     .For<IVocabTevent.Question.PropertyUpdated.Text>(static (question, tevent) => question.Text = tevent.Text)
     .For<IVocabTevent.Question.PropertyUpdated.Disambiguation>(static (question, tevent) => question.Disambiguation = tevent.Disambiguation)
     .For<IVocabTevent.Question.PropertyUpdated.Text>(static (question, _) => question._vocab.Forms.OnQuestionChanged()));

   TeventRouting ITeventRouted.TeventAppliers => Appliers;

   /// <summary>
   /// Declares, through <paramref name="handle"/>, the participation handler by which a vocab renamed keeps the names other notes refer to it by:<br/>
   /// every word-reference that named it by its old name is renamed to its new one, in the unit of work that renamed it<br/>
   /// (<see cref="JPCollection.RenameTheWordReferences"/>). A vocab without a question had no name to be referred to by.
   /// </summary>
   internal static void RenameTheWordReferencesThrough(TessageBusHandlerRegistrar handle) => handle
     .ForTevent(static (IVocabTevent.Question.Renamed it, JPCollection collection) =>
      {
         if(it.From.Text != string.Empty) collection.RenameTheWordReferences(it.From, new WordReference(it.Text, it.Disambiguation));
      });

   /// <summary>Separates the <see cref="Text"/> from the <see cref="Disambiguation"/> in a <see cref="DisambiguationName"/>, so neither may contain it.</summary>
   public const string DisambiguationMarker = ":";

   readonly VocabNote _vocab;

   /// <param name="vocab">The vocab it's the question of.</param>
   /// <param name="data">The vocab as the corpus stores it, or null for a new vocab.</param>
   internal VocabNoteQuestion(VocabNote vocab, VocabDto? data) : base(vocab)
   {
      _vocab = vocab;
      Text = data?.Question.Text ?? string.Empty;
      Disambiguation = data?.Question.Disambiguation ?? string.Empty;
   }

   /// <summary>The question itself: the vocab's main form, as <c>入る</c>. Empty only on an empty vocab, which is no vocab yet (<see cref="VocabNote(NoteServices)"/>).</summary>
   public string Text { get; private set; }

   /// <summary>What tells this vocab apart from others with the same <see cref="Text"/>, as <c>masu</c> in <c>入る:masu</c>. Empty for most vocab.</summary>
   public string Disambiguation { get; private set; }

   /// <summary>Whether the vocab has its question: false only on an empty vocab, which is no vocab yet (<see cref="VocabNote(NoteServices)"/>).</summary>
   public bool HasText => Text != string.Empty;

   /// <summary>Whether the vocab has a <see cref="Disambiguation"/>.</summary>
   public bool IsDisambiguated => Disambiguation != string.Empty;

   /// <summary>
   /// The name the vocab is shown and typed by where it has to be told apart from others: the <see cref="Text"/>, followed by the <see cref="DisambiguationMarker"/><br/>
   /// and the <see cref="Disambiguation"/> if the vocab has one, as <c>入る:masu</c>.
   /// </summary>
   public string DisambiguationName => IsDisambiguated ? Text + DisambiguationMarker + Disambiguation : Text;

   /// <summary>The <see cref="WordReference"/> that names this vocab exactly: its <see cref="Text"/> and its <see cref="Disambiguation"/>. Other notes refer to it by it.</summary>
   public WordReference Reference => new(Text, Disambiguation);

   /// <summary>Whether <paramref name="reference"/> names this vocab exactly, its <see cref="Disambiguation"/> included.</summary>
   public bool IsNamedBy(WordReference reference) => reference.Text == Text && reference.Disambiguation == Disambiguation;

   public string WithoutNoiseCharacters => Text.Replace(Mine.VocabPrefixSuffixMarker, "");

   public VocabStems Stems() => new(_vocab);

   /// <summary>
   /// Sets the <see cref="Text"/>, keeping the <see cref="Disambiguation"/>: renames the vocab (<see cref="RenameTo"/>). The question is always one of the<br/>
   /// vocab's forms, so the old one stops being one, and the new one is dropped from the entered forms, where it would be a copy.
   /// </summary>
   public void SetText(string text)
   {
      AssertCanBeTheText(text);
      RenameTo(new WordReference(text, Disambiguation));
   }

   /// <summary>Sets the <see cref="Disambiguation"/>, empty for none: renames the vocab (<see cref="RenameTo"/>).</summary>
   public void SetDisambiguation(string disambiguation)
   {
      AssertHasNoDisambiguationMarker(disambiguation);
      AssertNoOtherVocabHas(new WordReference(Text, disambiguation));
      RenameTo(new WordReference(Text, disambiguation));
   }

   /// <summary>Fails, saying why, when <paramref name="text"/> can't be the vocab's <see cref="Text"/>, with its <see cref="Disambiguation"/>.</summary>
   internal void AssertCanBeTheText(string text)
   {
      Contract.Argument.Assert(text != string.Empty, () => "A vocab's question can't be empty.");
      AssertHasNoDisambiguationMarker(text);
      AssertNoOtherVocabHas(new WordReference(text, Disambiguation));
   }

   /// <summary>Fails, saying why, when a vocab other than this one has <paramref name="question"/> as its question: no two vocab may share one.</summary>
   void AssertNoOtherVocabHas(WordReference question) =>
      Contract.Argument.Assert(!_vocab.Services.Collection.Vocab.AnotherVocabHasTheQuestion(_vocab, question),
                               () => $"Another vocab is named {question.DisambiguationName}, and no two vocab may share a question. A disambiguation tells two vocab with the same text apart.");

   /// <summary>
   /// Renames the vocab <paramref name="newName"/>, unless that is its name, publishing <see cref="IVocabTevent.Question.Renamed"/>, so that the<br/>
   /// word-references that named it by its old name are renamed too (<see cref="RenameTheWordReferencesThrough"/>). Its entered forms then drop its<br/>
   /// new question (<see cref="VocabNoteForms.DropTheQuestionFromTheEnteredForms"/>).
   /// </summary>
   void RenameTo(WordReference newName)
   {
      var oldName = Reference;
      if(_vocab.PublishIfChanged(oldName, newName, it => new VocabTevent.Question.Renamed(oldName, it.Text, it.Disambiguation)))
         _vocab.Forms.DropTheQuestionFromTheEnteredForms();
   }

   internal VocabQuestionDto ToDto() => new() { Text = Text, Disambiguation = Disambiguation };

   static void AssertHasNoDisambiguationMarker(string text) =>
      Contract.Argument.Assert(!text.Contains(DisambiguationMarker),
                               () => $"'{text}' contains '{DisambiguationMarker}', which separates a vocab's question from its disambiguation when other notes refer to it."
                                   + " The question and the disambiguation are set separately.");

   public override string ToString() => Text;
}
