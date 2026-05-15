using System.Collections.Generic;
using Compze.Utilities.Testing.Must;
using Compze.Utilities.Testing.XUnit.BDD;
using JAStudio.Core.Storage.Media;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.Storage.Media;

public class When_configuring_media_import_routing
{
   static SourceTag Tag(string value) => SourceTag.Parse(value);

   public class with_multiple_sentence_rules : When_configuring_media_import_routing
   {
      readonly List<ImportRule> _rules =
      [
         new ImportRule(Tag("anime::natsume"), nameof(SentenceMediaField.Audio),      "commercial-001", CopyrightStatus.Commercial),
         new ImportRule(Tag("anime::natsume"), nameof(SentenceMediaField.Screenshot), "commercial-001", CopyrightStatus.Commercial),
         new ImportRule(Tag("anime"),          nameof(SentenceMediaField.Audio),      "commercial-002", CopyrightStatus.Commercial),
         new ImportRule(Tag("anime"),          nameof(SentenceMediaField.Screenshot), "commercial-002", CopyrightStatus.Commercial)
      ];

      [XF] public void it_resolves_the_longest_matching_prefix() => _rules.TryResolve(Tag("anime::natsume::s1::01"), nameof(SentenceMediaField.Audio))!.TargetDirectory.Must().Be("commercial-001");
      [XF] public void it_falls_back_to_shorter_prefix() => _rules.TryResolve(Tag("anime::mushishi::s1::05"), nameof(SentenceMediaField.Audio))!.TargetDirectory.Must().Be("commercial-002");
   }

   public class with_no_matching_rule : When_configuring_media_import_routing
   {
      readonly List<ImportRule> _rules =
      [
         new ImportRule(Tag("anime"), nameof(SentenceMediaField.Audio), "commercial-001", CopyrightStatus.Commercial)
      ];

      [XF] public void it_returns_null() => _rules.TryResolve(Tag("forvo"), nameof(SentenceMediaField.Audio)).Must().BeNull();
   }

   public class with_vocab_rules_having_different_copyright_per_field : When_configuring_media_import_routing
   {
      readonly List<ImportRule> _rules;

      public with_vocab_rules_having_different_copyright_per_field() =>
         _rules =
         [
            new ImportRule(Tag("wani"), nameof(VocabMediaField.AudioFirst), "commercial/wani", CopyrightStatus.Commercial),
            new ImportRule(Tag("wani"), nameof(VocabMediaField.AudioTts),   "free/tts",        CopyrightStatus.Free),
            new ImportRule(Tag("wani"), nameof(VocabMediaField.UserImage),  "free/user",       CopyrightStatus.Free)
         ];

      [XF] public void audio_first_is_commercial() => _rules.TryResolve(Tag("wani::level05"), nameof(VocabMediaField.AudioFirst))!.Copyright.Must().Be(CopyrightStatus.Commercial);
      [XF] public void audio_tts_is_free() => _rules.TryResolve(Tag("wani::level05"), nameof(VocabMediaField.AudioTts))!.Copyright.Must().Be(CopyrightStatus.Free);
      [XF] public void audio_first_goes_to_commercial_dir() => _rules.TryResolve(Tag("wani::level05"), nameof(VocabMediaField.AudioFirst))!.TargetDirectory.Must().Be("commercial/wani");
      [XF] public void audio_tts_goes_to_free_dir() => _rules.TryResolve(Tag("wani::level05"), nameof(VocabMediaField.AudioTts))!.TargetDirectory.Must().Be("free/tts");
      [XF] public void user_image_is_free() => _rules.TryResolve(Tag("wani::level05"), nameof(VocabMediaField.UserImage))!.Copyright.Must().Be(CopyrightStatus.Free);
      [XF] public void unconfigured_field_returns_null() => _rules.TryResolve(Tag("wani::level05"), nameof(VocabMediaField.AudioSecond)).Must().BeNull();
   }
}
