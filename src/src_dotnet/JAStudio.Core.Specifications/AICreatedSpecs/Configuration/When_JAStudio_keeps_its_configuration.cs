using System.Collections.Generic;
using System.IO;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Configuration;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Configuration;

/// <summary>
/// JAStudio keeps its configuration in a file of its own in the add-on folder's user files (<see cref="ConfigurationFile"/>). Until 2026-11-13 Anki kept<br/>
/// it, in the add-on folder's <c>meta.json</c>, and the first time JAStudio starts from that.
/// </summary>
public class When_JAStudio_keeps_its_configuration : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   protected JapaneseConfig Configuration => GetService<JapaneseConfig>();

   protected Dictionary<string, object> TheConfigurationSaved() => GetService<IConfigDictSource>().Load();

   protected void AnkiKept(string configurationJson) =>
      File.WriteAllText(Path.Combine(GetService<IEnvironmentPaths>().AddonRootDir, "meta.json"), $$"""{"config": {{configurationJson}}, "mod": 0}""");

   public class in_an_add_on_folder_Anki_kept_no_configuration_in : When_JAStudio_keeps_its_configuration
   {
      [XF] public void every_value_is_its_default() => Configuration.ReanalysisThreads.Value.Must().Be(4L);
   }

   public class in_an_add_on_folder_Anki_kept_a_configuration_in : When_JAStudio_keeps_its_configuration
   {
      public in_an_add_on_folder_Anki_kept_a_configuration_in() =>
         AnkiKept("""{"reanalysis_threads": 16, "autoadvance_vocab_hiragana_seconds": 0.7, "hide_all_compounds": true}""");

      [XF] public void it_has_the_whole_numbers_Anki_kept() => Configuration.ReanalysisThreads.Value.Must().Be(16L);
      [XF] public void it_has_the_other_numbers_Anki_kept() => Configuration.AutoadvanceVocabHiraganaSeconds.Value.Must().Be(0.7);
      [XF] public void it_has_the_booleans_Anki_kept() => Configuration.HideAllCompounds.Value.Must().BeTrue();

      public class and_Anki_s_configuration_changes_after_JAStudio_loaded_it : in_an_add_on_folder_Anki_kept_a_configuration_in
      {
         public and_Anki_s_configuration_changes_after_JAStudio_loaded_it()
         {
            _ = Configuration;
            AnkiKept("""{"reanalysis_threads": 4}""");
         }

         [XF] public void JAStudio_keeps_its_own() => TheConfigurationSaved()["reanalysis_threads"].Must().Be(16L);
      }
   }

   public class after_values_are_changed : When_JAStudio_keeps_its_configuration
   {
      public after_values_are_changed()
      {
         Configuration.ReanalysisThreads.Value = 8;
         Configuration.AutoadvanceVocabKanjiSeconds.Value = 2.5;
         Configuration.HideAllCompounds.Value = true;
      }

      [XF] public void the_whole_numbers_are_saved_as_whole_numbers() => TheConfigurationSaved()["reanalysis_threads"].Must().Be(8L);
      [XF] public void the_other_numbers_are_saved_as_such() => TheConfigurationSaved()["autoadvance_vocab_kanji_seconds"].Must().Be(2.5);
      [XF] public void the_booleans_are_saved() => TheConfigurationSaved()["hide_all_compounds"].Must().Be(true);
   }
}
