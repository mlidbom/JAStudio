using System;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Storage.Media;
using Xunit;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.Storage.Media;

public class When_working_with_SourceTag
{
   static SourceTag Tag(string value) => SourceTag.Parse(value);

   public class when_parsing : When_working_with_SourceTag
   {
      [XF] public void it_parses_a_single_segment() => Tag("anime").Segments.Count.Must().Be(1);
      [XF] public void it_parses_multiple_segments() => Tag("anime::natsume::s1").Segments.Count.Must().Be(3);
      [XF] public void it_preserves_the_original_string() => Tag("anime::natsume").ToString().Must().Be("anime::natsume");
      [XF] public void it_rejects_empty_string() => Assert.Throws<ArgumentException>(() => Tag(""));
      [XF] public void it_rejects_whitespace() => Assert.Throws<ArgumentException>(() => Tag("  "));
      [XF] public void it_rejects_empty_segments() => Assert.Throws<FormatException>(() => Tag("anime::::natsume"));
      [XF] public void it_rejects_leading_separator() => Assert.Throws<FormatException>(() => Tag("::anime"));
      [XF] public void it_rejects_trailing_separator() => Assert.Throws<FormatException>(() => Tag("anime::"));
   }

   public class when_checking_containment : When_working_with_SourceTag
   {
      [XF] public void child_is_contained_in_parent() => Tag("aa::bb::cc").IsContainedIn(Tag("aa::bb")).Must().BeTrue();
      [XF] public void parent_is_not_contained_in_child() => Tag("aa::bb").IsContainedIn(Tag("aa::bb::cc")).Must().BeFalse();
      [XF] public void tag_is_contained_in_itself() => Tag("aa::bb").IsContainedIn(Tag("aa::bb")).Must().BeTrue();
      [XF] public void single_segment_child() => Tag("anime::natsume").IsContainedIn(Tag("anime")).Must().BeTrue();
      [XF] public void unrelated_tags_are_not_contained() => Tag("forvo").IsContainedIn(Tag("anime")).Must().BeFalse();

      [XF] public void partial_segment_name_does_not_match() => Tag("anime_extra").IsContainedIn(Tag("anime")).Must().BeFalse();
      [XF] public void prefix_substring_without_separator_does_not_match() => Tag("animations").IsContainedIn(Tag("anim")).Must().BeFalse();
   }

   public class when_using_contains : When_working_with_SourceTag
   {
      [XF] public void parent_contains_child() => Tag("aa::bb").Contains(Tag("aa::bb::cc")).Must().BeTrue();
      [XF] public void child_does_not_contain_parent() => Tag("aa::bb::cc").Contains(Tag("aa::bb")).Must().BeFalse();
      [XF] public void tag_contains_itself() => Tag("aa::bb").Contains(Tag("aa::bb")).Must().BeTrue();
   }

   public class when_comparing_equality : When_working_with_SourceTag
   {
      [XF] public void same_value_is_equal() => Tag("anime::natsume").Must().Be(Tag("anime::natsume"));
      [XF] public void different_value_is_not_equal() => Tag("anime::natsume").Equals(Tag("anime::mushishi")).Must().BeFalse();
      [XF] public void same_value_has_same_hash() => Tag("anime").GetHashCode().Must().Be(Tag("anime").GetHashCode());
   }
}
