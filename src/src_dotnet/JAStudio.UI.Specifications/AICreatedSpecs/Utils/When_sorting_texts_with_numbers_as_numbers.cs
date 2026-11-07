using System.Collections.Generic;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.UI.Utils;

// ReSharper disable InconsistentNaming

namespace JAStudio.UI.Specifications.AICreatedSpecs.Utils;

/// <summary>
/// How the results grid sorts a sentence's external id and source tag (<see cref="TextSortedWithNumbersAsNumbers"/>),<br/>
/// so that sorting by the external id puts an episode's sentences in the order their lines are spoken.
/// </summary>
public class When_sorting_texts_with_numbers_as_numbers : IAIGeneratedSpec
{
   static List<string> Sorted(params string[] texts) => [..texts.Select(it => new TextSortedWithNumbersAsNumbers(it)).Order().Select(it => it.Text)];

   [XF] public void episode_2_comes_before_episode_11() => Sorted("episode_11", "episode_2").Must().SequenceEqual(["episode_2", "episode_11"]);

   [XF] public void subs2srs_ids_sort_by_episode_and_then_by_the_start_time_of_the_line() =>
      Sorted("ny4-nodup_11_0.15.23.540-0.15.25.620", "ny4-nodup_2_0.20.01.000-0.20.03.000", "ny4-nodup_11_0.02.05.120-0.02.07.000")
        .Must().SequenceEqual(["ny4-nodup_2_0.20.01.000-0.20.03.000", "ny4-nodup_11_0.02.05.120-0.02.07.000", "ny4-nodup_11_0.15.23.540-0.15.25.620"]);

   [XF] public void source_tags_sort_season_2_before_season_10() =>
      Sorted("source::anime::natsume::s10::01", "source::anime::natsume::s2::05").Must().SequenceEqual(["source::anime::natsume::s2::05", "source::anime::natsume::s10::01"]);

   [XF] public void an_empty_text_comes_first() => Sorted("episode_1", "").Must().SequenceEqual(["", "episode_1"]);

   [XF] public void texts_that_differ_only_in_case_are_equal() =>
      new TextSortedWithNumbersAsNumbers("Episode_2").CompareTo(new TextSortedWithNumbersAsNumbers("episode_2")).Must().Be(0);
}
