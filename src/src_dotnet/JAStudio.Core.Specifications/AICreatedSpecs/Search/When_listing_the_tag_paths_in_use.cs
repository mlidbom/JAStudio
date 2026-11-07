using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Search.TagProperties;
using JAStudio.Core.Specifications.AICreatedSpecs.Search.When_searching_notes;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Search;

/// <summary>The tag paths the tag condition's editor offers, and how typing parts of a path filters them.</summary>
public class When_listing_the_tag_paths_in_use : NoteSearchSpecification
{
   readonly TagPathsInUse _tagPaths;

   public When_listing_the_tag_paths_in_use() => _tagPaths = GetService<JPCollection>().TagPathsInUse();

   [XF] public void a_tag_a_note_has_is_listed() => _tagPaths.All.Contains("source::anime::natsume::s3::01").Must().BeTrue();
   [XF] public void a_parent_of_a_tag_is_listed_even_though_no_note_has_it_alone() => _tagPaths.All.Contains("source::anime::natsume").Must().BeTrue();
   [XF] public void a_tag_comes_before_the_tags_in_its_branch() =>
      (_tagPaths.All.ToList().IndexOf("source::anime") < _tagPaths.All.ToList().IndexOf("source::anime::natsume")).Must().BeTrue();

   public class when_matching_typed_parts : When_listing_the_tag_paths_in_use
   {
      [XF] public void the_paths_containing_every_part_are_found() => _tagPaths.Matching("nat s3 02").Must().SequenceEqual(["source::anime::natsume::s3::02"]);
      [XF] public void the_case_of_the_parts_is_ignored() => _tagPaths.Matching("WANI").Must().SequenceEqual(["source::wani"]);
      [XF] public void with_nothing_typed_every_path_is_found() => _tagPaths.Matching("  ").Must().SequenceEqual(_tagPaths.All);
   }
}
