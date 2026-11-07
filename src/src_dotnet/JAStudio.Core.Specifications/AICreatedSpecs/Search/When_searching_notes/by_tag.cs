using System.Collections.Generic;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Search;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Search.When_searching_notes;

public class by_tag : NoteSearchSpecification
{
   public class for_a_tag_with_child_tags : by_tag
   {
      readonly List<string> _found;
      public for_a_tag_with_child_tags() => _found = QuestionsOfTheNotesFoundBy(NoteProperties.Tags.Include("source::anime::natsume"));

      [XF] public void the_notes_tagged_with_tags_in_its_branch_are_found() => _found.Must().SequenceEqual(InOrdinalOrder(SentenceAboutSchool, SentenceAboutHome));
   }

   public class for_a_tag_with_no_child_tags : by_tag
   {
      readonly List<string> _found;
      public for_a_tag_with_no_child_tags() => _found = QuestionsOfTheNotesFoundBy(NoteProperties.Tags.Include("source::anime::natsume::s3::01"));

      [XF] public void only_the_note_tagged_with_it_is_found() => _found.Must().SequenceEqual([SentenceAboutSchool]);
   }

   public class for_the_start_of_a_tag_name_that_is_not_a_whole_level : by_tag
   {
      readonly List<string> _found;
      public for_the_start_of_a_tag_name_that_is_not_a_whole_level() => _found = QuestionsOfTheNotesFoundBy(NoteProperties.Tags.Include("source::anime::nat"));

      [XF] public void no_note_is_found() => _found.Must().BeEmpty();
   }

   public class for_a_tag_written_in_upper_case : by_tag
   {
      readonly List<string> _found;
      public for_a_tag_written_in_upper_case() => _found = QuestionsOfTheNotesFoundBy(NoteProperties.Tags.Include("SOURCE::WANI"));

      [XF] public void the_case_is_ignored_so_the_note_tagged_with_it_in_lower_case_is_found() => _found.Must().SequenceEqual([VocabToGo]);
   }
}
