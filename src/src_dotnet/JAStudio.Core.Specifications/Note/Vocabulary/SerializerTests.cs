using JAStudio.Core.Note.NoteFields;
using JAStudio.Core.Note.NoteFields.AutoSaveWrappers;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Note.Vocabulary.RelatedVocab;
using Xunit;

namespace JAStudio.Core.Specifications.Note.Vocabulary;

public class RelatedVocabDataSerializerTests : SpecificationStartingWithAnEmptyCollection
{
   readonly IObjectSerializer<RelatedVocabData> _serializer = RelatedVocabData.Serializer();

   [Fact]
   public void EmptyObjectSerializesToEmptyString()
   {
      var emptyData = new RelatedVocabData(
         ergativeTwin: "",
         derivedFrom: new ValueWrapper<string>(""),
         perfectSynonyms: [],
         similar: [],
         antonyms: [],
         confusedWith: [],
         seeAlso: []
      );

      var result = _serializer.Serialize(emptyData);

      Assert.Equal("", result);
   }

   [Fact]
   public void DeserializeEmptyStringReturnsEmptyData()
   {
      var result = _serializer.Deserialize("");

      Assert.Equal("", result.ErgativeTwin);
      Assert.Equal("", result.DerivedFrom.Get());
      Assert.Empty(result.PerfectSynonyms);
      Assert.Empty(result.Synonyms);
      Assert.Empty(result.Antonyms);
      Assert.Empty(result.ConfusedWith);
      Assert.Empty(result.SeeAlso);
   }

   [Fact]
   public void RoundtripWithDataPreservesAllFields()
   {
      var original = new RelatedVocabData(
         ergativeTwin: "開く",
         derivedFrom: new ValueWrapper<string>("開ける"),
         perfectSynonyms: ["完璧", "完全"],
         similar: ["似ている", "同様"],
         antonyms: ["閉じる", "閉める"],
         confusedWith: ["明く"],
         seeAlso: ["開放", "開始"]
      );

      var serialized = _serializer.Serialize(original);
      var deserialized = _serializer.Deserialize(serialized);

      Assert.Equal("開く", deserialized.ErgativeTwin);
      Assert.Equal("開ける", deserialized.DerivedFrom.Get());
      Assert.Equal(["完璧", "完全"], deserialized.PerfectSynonyms);
      Assert.Equal(["似ている", "同様"], deserialized.Synonyms);
      Assert.Equal(["閉じる", "閉める"], deserialized.Antonyms);
      Assert.Equal(["明く"], deserialized.ConfusedWith);
      Assert.Equal(["開放", "開始"], deserialized.SeeAlso);
   }
}
