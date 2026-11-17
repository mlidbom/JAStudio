using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Tagging;
using JAStudio.Core.Note.Vocabulary;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Storage;

/// <summary>
/// The corpus stores a note's tags, and a vocab's related vocab, in ordinal order, which is the same on every machine. In the current culture's<br/>
/// order, as they once were, a machine with another culture would have written every note it saved in another order. The tags and the words here<br/>
/// are ones whose ordinal order differs from the order of the usual cultures: capitals before lower case, and hiragana before katakana.
/// </summary>
public class When_a_vocab_with_tags_and_related_vocab_is_stored : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   readonly IReadOnlyList<string?> _storedTags;
   readonly IReadOnlyList<string?> _storedConfusedWith;

   public When_a_vocab_with_tags_and_related_vocab_is_stored()
   {
      var vocab = CreateVocab("見る", "to see", "みる");
      vocab.Edit(it =>
      {
         it.Tags.Set(Tag.FromName("apple"));
         it.Tags.Set(Tag.FromName("Zebra"));
         it.RelatedNotes.ConfusedWith.Add(new WordReference("アル"));
         it.RelatedNotes.ConfusedWith.Add(new WordReference("いる"));
      });

      using var stored = JsonDocument.Parse(TheStoredFileOf(vocab));
      _storedTags = [.. stored.RootElement.GetProperty("tags").EnumerateArray().Select(it => it.GetString())];
      _storedConfusedWith = [.. stored.RootElement.GetProperty("relatedVocab").GetProperty("confusedWith").EnumerateArray().Select(it => it.GetProperty("text").GetString())];
   }

   [XF] public void its_tags_are_in_ordinal_order_capitals_before_lower_case() => _storedTags.Must().SequenceEqual(["Zebra", "apple"]);
   [XF] public void its_confused_with_words_are_in_ordinal_order_hiragana_before_katakana() => _storedConfusedWith.Must().SequenceEqual(["いる", "アル"]);
}
