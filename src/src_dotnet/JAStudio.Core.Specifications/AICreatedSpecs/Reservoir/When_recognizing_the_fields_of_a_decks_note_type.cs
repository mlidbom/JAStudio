using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Importing.RawNotes;
using JAStudio.Core.Reservoir.Sources.AnkiPackages;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Reservoir;

/// <summary>
/// The fields of note types seen in the MediaFire folder's decks, recognized as the Japanese, the translation and the media (<see cref="DeckFieldRoles"/>),<br/>
/// and the sentence a note then gives the reservoir.
/// </summary>
public class When_recognizing_the_fields_of_a_decks_note_type : IAIGeneratedSpec
{
   /// <summary>The fields of a Migaku deck's note type: its sentence, with Migaku's furigana, in Expression, and the word it's for in Target.</summary>
   static readonly string[] FieldNamesOfAMigakuDeck = ["Expression", "Meaning", "Audio", "Target"];

   static readonly string[][] NotesOfAMigakuDeck =
   [
      ["ちょっと[;o] &ensp; ごめん[;h] &ensp; 急[いそ,いそぐ;k2]い でる[,でる;k1] から &ensp; じゃあ &ensp; 後[あと;a] で", "Sorry, I am in a hurry, see you later", "[sound:kiss_01.mp3]", "ごめん"],
      ["お前[おまえ,おまえ;h] は[,は;h] 誰[だれ,だれ;k1] だ", "Who are you?", "[sound:kiss_02.mp3]", "誰"]
   ];

   [XF] public void a_migaku_decks_sentence_with_furigana_is_recognized_over_its_target_word() =>
      RolesOf(FieldNamesOfAMigakuDeck, NotesOfAMigakuDeck).ToString().Must().Be("the note type: Japanese Expression (furigana taken away), translation Meaning, audio Audio, image -");

   [XF] public void a_migaku_decks_sentence_keeps_only_its_own_spaces() =>
      FirstSentenceOf(FieldNamesOfAMigakuDeck, NotesOfAMigakuDeck).Must().Be("ちょっと ごめん 急いでるから じゃあ 後で");

   [XF] public void a_sentence_given_only_with_ankis_furigana_loses_them() =>
      FirstSentenceOf(["Sentence", "Meaning"],
                      ["友人帳[ゆうじんちょう]を 寄越[よこ]せ", "Hand over the Book of Friends!"],
                      ["夏目[なつめ]、 早[はや]く 来[こ]い", "Natsume, come quickly"])
        .Must().Be("友人帳を寄越せ");

   [XF] public void a_field_named_sentkanji_is_recognized_as_the_japanese_though_its_name_holds_kanji() =>
      RolesOf(["SentKanji", "SentFurigana", "SentEng", "VocabKanji"],
              ["地球の運動について考える", "地球[ちきゅう]の 運動[うんどう]について 考[かんが]える", "To think about the motion of the Earth", "運動"],
              ["それは異端の思想だ", "それは 異端[いたん]の 思想[しそう]だ", "That is a heretical thought", "異端"])
        .ToString().Must().Be("the note type: Japanese SentKanji, translation SentEng, audio -, image -");

   [XF] public void a_decks_japanese_in_a_field_named_as_a_translation_is_recognized_when_no_other_field_holds_japanese() =>
      RolesOf(["Expression", "Meaning", "Reading"],
              ["", "（フサタ）ハアッ… 建設者！", ""],
              ["", "ここから出られるのか？", ""])
        .ToString().Must().Be("the note type: Japanese Meaning, translation -, audio -, image -");

   [XF] public void a_tag_of_latin_letters_is_no_translation() =>
      RolesOf(["Tag", "SequenceMarker", "Audio", "Expression"],
              ["Cardcaptor_Sakura_46_01", "01_00051_0.03.51.495", "[sound:sakura_01.mp3]", "（藤隆）どうぞ、召し上がれ"],
              ["Cardcaptor_Sakura_46_01", "01_00052_0.03.53.120", "[sound:sakura_02.mp3]", "いただきます！"])
        .ToString().Must().Be("the note type: Japanese Expression, translation -, audio Audio, image -");

   [XF] public void words_are_recognized_as_words_and_give_no_sentences()
   {
      var roles = RolesOf(["Expression", "Meaning"], ["相手", "partner"], ["級", "class"], ["シチュエーション", "situation"]);
      roles.JapaneseFieldHoldsWordsRatherThanSentences.Must().BeTrue();
      roles.SentenceOf(NoteOf(["Expression", "Meaning"], 1, ["相手", "partner"])).Must().BeNull();
   }

   static DeckFieldRoles RolesOf(string[] fieldNames, params string[][] notes) =>
      DeckFieldRoles.RecognizedIn(new AnkiNoteType("the note type", fieldNames), [.. notes.Select((values, index) => NoteOf(fieldNames, index + 1, values))]);

   static string FirstSentenceOf(string[] fieldNames, params string[][] notes) =>
      RolesOf(fieldNames, notes).SentenceOf(NoteOf(fieldNames, 1, notes[0])) is {} sentence ? sentence.Text.Text : "(no sentence)";

   static RawNote NoteOf(string[] fieldNames, long id, string[] values) =>
      new(id, fieldNames.Zip(values).ToDictionary(it => it.First, it => it.Second), []);
}
