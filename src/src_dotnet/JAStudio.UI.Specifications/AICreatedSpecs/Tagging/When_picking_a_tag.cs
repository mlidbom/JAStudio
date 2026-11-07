using System.Collections.Generic;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.UI.ViewModels.Workspace.Tagging;

// ReSharper disable InconsistentNaming

namespace JAStudio.UI.Specifications.AICreatedSpecs.Tagging;

/// <summary>
/// The tag picker lists the offered tag paths that contain every typed part, and picks the first.<br/>
/// For adding a tag, it also lists the typed text itself, last, as a new tag, when that can be a tag no note has yet.
/// </summary>
public class When_picking_a_tag : IAIGeneratedSpec
{
   static readonly IReadOnlyList<string> OfferedTagPaths = ["source::anime", "source::anime::natsume::s3::05"];

   static TagPickerViewModel PickerThatOffersANewTag(string typed) => new(OfferedTagPaths, typed, offersTheTypedTextAsANewTag: true);

   static List<string> ChoicesListed(TagPickerViewModel picker) => [..picker.Choices.Select(it => it.ToString())];

   public class that_offers_the_typed_text_as_a_new_tag : When_picking_a_tag
   {
      [XF] public void a_name_no_offered_path_has_is_listed_last_as_a_new_tag() =>
         ChoicesListed(PickerThatOffersANewTag("nat")).Must().SequenceEqual(["source::anime::natsume::s3::05", "nat   (a new tag)"]);

      [XF] public void the_first_matching_offered_path_is_picked_rather_than_the_new_tag() =>
         PickerThatOffersANewTag("nat").Picked?.TagPath.Must().Be("source::anime::natsume::s3::05");

      [XF] public void the_new_tag_is_picked_when_no_offered_path_matches() => PickerThatOffersANewTag("todo::check").Picked?.IsNew.Must().BeTrue();

      [XF] public void typed_parts_separated_by_spaces_are_no_new_tag() => ChoicesListed(PickerThatOffersANewTag("nat s3")).Must().SequenceEqual(["source::anime::natsume::s3::05"]);

      [XF] public void an_offered_path_typed_in_another_case_is_no_new_tag() =>
         ChoicesListed(PickerThatOffersANewTag("Source::Anime")).Must().SequenceEqual(["source::anime", "source::anime::natsume::s3::05"]);

      [XF] public void a_name_with_an_empty_level_is_no_new_tag() => PickerThatOffersANewTag("todo::::check").Choices.Must().BeEmpty();

      [XF] public void a_jastudio_defined_tag_is_no_new_tag() => PickerThatOffersANewTag("_uk").Choices.Must().BeEmpty();
   }

   public class that_does_not_offer_the_typed_text_as_a_new_tag : When_picking_a_tag
   {
      [XF] public void only_matching_offered_paths_are_listed() =>
         ChoicesListed(new TagPickerViewModel(OfferedTagPaths, "nat", offersTheTypedTextAsANewTag: false)).Must().SequenceEqual(["source::anime::natsume::s3::05"]);

      [XF] public void an_offered_path_typed_when_it_opens_is_picked() =>
         new TagPickerViewModel(OfferedTagPaths, "source::anime", offersTheTypedTextAsANewTag: false).Picked?.TagPath.Must().Be("source::anime");
   }
}
