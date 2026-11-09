using System.Collections.Generic;
using Compze.Internals.SystemCE.ReactiveCE;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Configuration;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

/// <summary>Setting the configuration value autoadvance_vocab_starting_seconds, while the configuration's changes are being recorded.</summary>
public class When_setting_a_configuration_value : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   protected readonly JapaneseConfig Config;
   protected readonly List<JapaneseConfig> ConfigurationChanges = [];

   protected When_setting_a_configuration_value()
   {
      Config = GetService<JapaneseConfig>();
      Config.Changed.Subscribe(it => ConfigurationChanges.Add(it));
   }

   public class to_the_value_it_already_has : When_setting_a_configuration_value
   {
      public to_the_value_it_already_has() => Config.AutoadvanceVocabStartingSeconds.Value = Config.AutoadvanceVocabStartingSeconds.Value;

      [XF] public void the_configuration_does_not_change() => ConfigurationChanges.Must().BeEmpty();
   }

   public class to_another_value : When_setting_a_configuration_value
   {
      readonly double _anotherValue;

      public to_another_value()
      {
         _anotherValue = Config.AutoadvanceVocabStartingSeconds.Value + 1;
         Config.AutoadvanceVocabStartingSeconds.Value = _anotherValue;
      }

      [XF] public void the_configuration_changes_once() => ConfigurationChanges.Count.Must().Be(1);
      [XF] public void the_value_is_the_one_set() => Config.AutoadvanceVocabStartingSeconds.Value.Must().Be(_anotherValue);
   }
}
