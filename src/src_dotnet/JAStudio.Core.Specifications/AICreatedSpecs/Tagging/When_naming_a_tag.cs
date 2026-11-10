using System;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Tagging;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Tagging;

/// <summary>Tags ignore case, as in Anki, and a name typed by a person can only be a tag's name if Anki would keep it as one tag.</summary>
public class When_naming_a_tag : IAIGeneratedSpec
{
   public class that_differs_from_an_existing_tag_only_in_case : When_naming_a_tag
   {
      readonly Tag _existing = Tag.FromName("source::anime::when-naming-a-tag");
      readonly Tag _named = Tag.FromName("Source::Anime::When-Naming-A-Tag");

      [XF] public void it_is_the_existing_tag() => ReferenceEquals(_named, _existing).Must().BeTrue();
      [XF] public void it_keeps_the_existing_spelling() => _named.Name.Must().Be("source::anime::when-naming-a-tag");
   }

   public class typed_by_a_person : When_naming_a_tag
   {
      static string WhyItCannot(string typedName) =>
         Tag.WhyItCannotBeTheNameOfATag(typedName) ?? throw new InvalidOperationException($"{typedName} can be the name of a tag.");

      [XF] public void a_path_with_levels_can_be_a_tags_name() => Tag.WhyItCannotBeTheNameOfATag("source::anime::natsume").Must().BeNull();
      [XF] public void an_empty_name_cannot() => WhyItCannot("").Must().Be("A tag needs a name.");
      [XF] public void a_name_with_a_space_cannot() => WhyItCannot("source::the anime").Must().Be("A tag can't contain spaces.");
      [XF] public void a_name_with_an_empty_level_cannot() => WhyItCannot("a::::b").Must().Be("A tag can't have an empty level, as a::::b has.");
      [XF] public void a_name_ending_in_the_level_separator_cannot() => WhyItCannot("source::").Must().Be("A tag can't have an empty level, as source:: has.");
   }

   public class defined_by_jastudio : When_naming_a_tag
   {
      [XF] public void a_tag_defined_in_a_nested_class_is() => Tags.IsDefinedByJAStudio(Tags.Vocab.Matching.Uses.PrefixIsNot.Name).Must().BeTrue();
      [XF] public void case_is_ignored() => Tags.IsDefinedByJAStudio("-::SENTENCE::USES::HIDDEN-MATCHES").Must().BeTrue();
      [XF] public void a_tag_a_person_applies_is_not() => Tags.IsDefinedByJAStudio("source::anime::natsume").Must().BeFalse();
      [XF] public void a_source_tag_JAStudio_sets_when_it_creates_a_note_is() => Tags.IsDefinedByJAStudio(Tags.Source.Jamdict.Name).Must().BeTrue();
   }

   public class maintained_by_jastudio : When_naming_a_tag
   {
      [XF] public void a_generated_tag_is() => Tags.IsMaintainedByJAStudio(Tags.Kanji.InAnyVocabForm.Name).Must().BeTrue();
      [XF] public void a_source_tag_JAStudio_sets_when_it_creates_a_note_is_not() => Tags.IsMaintainedByJAStudio(Tags.Source.Jastudio.Name).Must().BeFalse();
      [XF] public void a_tag_a_person_applies_is_not() => Tags.IsMaintainedByJAStudio("source::anime::natsume").Must().BeFalse();
   }

   public class as_a_tag_a_person_manages : When_naming_a_tag
   {
      [XF] public void a_source_tag_JAStudio_sets_can_be() => Tags.WhyItCannotBeATagAPersonManages(Tags.Source.Jamdict.Name).Must().BeNull();
      [XF] public void a_tag_JAStudio_maintains_cannot_be() =>
         Tags.WhyItCannotBeATagAPersonManages(Tags.Kanji.InAnyVocabForm.Name)
             .Must().NotBeNull().Be($"{Tags.Kanji.InAnyVocabForm.Name} is maintained by JAStudio, which generates it.");
      [XF] public void a_name_with_a_space_cannot_be() => Tags.WhyItCannotBeATagAPersonManages("source::the anime").Must().NotBeNull().Be("A tag can't contain spaces.");
   }
}
