using System.Collections.Generic;
using JAStudio.Core.Note;
using JAStudio.Core.Storage;
using Xunit;

namespace JAStudio.Core.Specifications.Storage;

public class NoteSerializerRoundtripTests : SpecificationUsingACollection
{
   readonly NoteSerializer _serializer;

   public NoteSerializerRoundtripTests() => _serializer = GetService<NoteSerializer>();

   [Fact]
   public void AllKanjiNotes_RoundtripToIdenticalJson()
   {
      var allKanji = NoteServices.Collection.Kanji.All();
      Assert.NotEmpty(allKanji);

      var failures = new List<string>();

      foreach(var kanji in allKanji)
      {
         var json = _serializer.Serialize(kanji);
         var roundtripped = _serializer.DeserializeKanji(json);
         var reJson = _serializer.Serialize(roundtripped);

         if(json != reJson)
         {
            failures.Add($"Kanji '{kanji.GetQuestion()}': JSON differs after roundtrip");
         }
      }

      Assert.True(failures.Count == 0, $"Roundtrip failures:\n{string.Join("\n", failures)}");
   }

   [Fact]
   public void AllVocabNotes_RoundtripToIdenticalJson()
   {
      var allVocab = NoteServices.Collection.Vocab.All();
      Assert.NotEmpty(allVocab);

      var failures = new List<string>();

      foreach(var vocab in allVocab)
      {
         var json = _serializer.Serialize(vocab);
         var roundtripped = _serializer.DeserializeVocab(json);
         var reJson = _serializer.Serialize(roundtripped);

         if(json != reJson)
         {
            failures.Add($"Vocab '{vocab.GetQuestion()}': JSON differs after roundtrip");
         }
      }

      Assert.True(failures.Count == 0, $"Roundtrip failures:\n{string.Join("\n", failures)}");
   }

   [Fact]
   public void AllSentenceNotes_RoundtripToIdenticalJson()
   {
      var allSentences = NoteServices.Collection.Sentences.All();
      Assert.NotEmpty(allSentences);

      var failures = new List<string>();

      foreach(var sentence in allSentences)
      {
         var json = _serializer.Serialize(sentence);
         var roundtripped = _serializer.DeserializeSentence(json);
         var reJson = _serializer.Serialize(roundtripped);

         if(json != reJson)
         {
            failures.Add($"Sentence '{Truncate(sentence.GetQuestion(), 20)}': JSON differs after roundtrip");
         }
      }

      Assert.True(failures.Count == 0, $"Roundtrip failures:\n{string.Join("\n", failures)}");
   }

   [Fact]
   public void KanjiNote_WithRichData_RoundtripsCorrectly()
   {
      var kanji = CreateKanji("試", "test/try", "<primary>ため</primary>", "<primary>し</primary>");
      kanji.UserAnswer.Set("custom answer");
      kanji.UserMnemonic.Set("my mnemonic");
      kanji.SetRadicals("言, 弋, 工");
      kanji.AddUserSimilarMeaning("験");

      var json = _serializer.Serialize(kanji);
      var roundtripped = _serializer.DeserializeKanji(json);

      Assert.Equal(kanji.GetQuestion(), roundtripped.GetQuestion());
      Assert.Equal("custom answer", roundtripped.UserAnswer.Value);
      Assert.Equal("my mnemonic", roundtripped.UserMnemonic.Value);
      Assert.Contains("言", roundtripped.Radicals);
      Assert.Contains("験", roundtripped.UserSimilarMeaning);

      Assert.Equal(json, _serializer.Serialize(roundtripped));
   }

   [Fact]
   public void VocabNote_RoundtripsCorrectly()
   {
      var vocab = CreateVocab("試す", "to test", "ためす");

      var json = _serializer.Serialize(vocab);
      var roundtripped = _serializer.DeserializeVocab(json);

      Assert.Equal("試す", roundtripped.GetQuestion());
      Assert.Contains("ためす", roundtripped.GetReadings());
      Assert.Equal(json, _serializer.Serialize(roundtripped));
   }

   [Fact]
   public void SentenceNote_RoundtripsCorrectly()
   {
      var sentence = CreateTestSentence("テストの文です", "This is a test sentence.");

      var json = _serializer.Serialize(sentence);
      var roundtripped = _serializer.DeserializeSentence(json);

      Assert.Equal("テストの文です", roundtripped.GetQuestion());
      Assert.Equal(json, _serializer.Serialize(roundtripped));
   }

   static string Truncate(string value, int maxLength) =>
      value.Length <= maxLength ? value : value.Substring(0, maxLength);

   [Fact]
   public void AllNotesData_RoundtripsToIdenticalJson()
   {
      var allData = new AllNotesData(
         NoteServices.Collection.Kanji.All(),
         NoteServices.Collection.Vocab.All(),
         NoteServices.Collection.Sentences.All());

      var json = _serializer.Serialize(allData);
      var roundtripped = _serializer.DeserializeAll(json);
      var reJson = _serializer.Serialize(roundtripped);

      Assert.Equal(json, reJson);
   }

   [Fact]
   public void AllNotesData_PreservesNoteCountsAfterRoundtrip()
   {
      var kanji = NoteServices.Collection.Kanji.All();
      var vocab = NoteServices.Collection.Vocab.All();
      var sentences = NoteServices.Collection.Sentences.All();

      var allData = new AllNotesData(kanji, vocab, sentences);
      var json = _serializer.Serialize(allData);
      var roundtripped = _serializer.DeserializeAll(json);

      Assert.Equal(kanji.Count, roundtripped.Kanji.Count);
      Assert.Equal(vocab.Count, roundtripped.Vocab.Count);
      Assert.Equal(sentences.Count, roundtripped.Sentences.Count);
   }

}
