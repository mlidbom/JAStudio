using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;

namespace JAStudio.Core.Configuration;

/// <summary>The settings of <see cref="JapaneseConfig"/> that the notes and their pages read, as configured now.</summary>
public class Settings
{
   readonly JapaneseConfig _config;

   internal Settings(JapaneseConfig config) => _config = config;

   public bool HideAllCompounds() => _config.HideAllCompounds.Value;

   public bool ShowBreakdownInEditMode() => _config.ShowSentenceBreakdownInEditMode.Value;

   public bool LogWhenFlushingNotes() => _config.LogWhenFlushingNotes.Value;

   public bool ShowCompoundPartsInSentenceBreakdown() => _config.ShowCompoundPartsInSentenceBreakdown.Value;

   public bool ShowKanjiInSentenceBreakdown() => _config.ShowKanjiInSentenceBreakdown.Value;

   public bool ShowKanjiMnemonicsInSentenceBreakdown() => _config.ShowKanjiMnemonicsInSentenceBreakdown.Value;

   /// <summary>The options that change how a sentence parses for its sentence breakdown, as configured now.</summary>
   public BreakdownParsingOptions BreakdownParsingOptions() =>
      new(_config.HideAllCompounds.Value,
          _config.HideCompositionallyTransparentCompounds.Value,
          _config.AutomaticallyYieldLastTokenInSuruVerbCompoundsToOverlappingCompound.Value,
          _config.AutomaticallyYieldLastTokenInPassiveVerbCompoundsToOverlappingCompound.Value,
          _config.AutomaticallyYieldLastTokenInCausativeVerbCompoundsToOverlappingCompound.Value);
}
