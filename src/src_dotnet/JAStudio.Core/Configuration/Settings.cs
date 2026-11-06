using Compze.Internals.SystemCE.ReactiveCE;

namespace JAStudio.Core.Configuration;

public class Settings
{
   readonly JapaneseConfig _config;
   bool _initialized;

   bool _hideTransparentCompounds;
   bool _showBreakdownInEditMode;
   bool _hideAllCompounds;
   bool _logWhenFlushingNotes;
   bool _showCompoundPartsInSentenceBreakdown;
   bool _showKanjiInSentenceBreakdown;
   bool _showKanjiMnemonicsInSentenceBreakdown;
   bool _automaticallyYieldLastTokenInSuruVerbCompoundsToOverlappingCompound;
   bool _automaticallyYieldLastTokenInPassiveVerbCompoundsToOverlappingCompound;
   bool _automaticallyYieldLastTokenInCausativeVerbCompoundsToOverlappingCompound;

   internal Settings(JapaneseConfig config) => _config = config;

   void EnsureInitialized()
   {
      if(_initialized) return;
      _initialized = true;
      Refresh();
      _config.Changed.Subscribe(_ => Refresh());
   }

   void Refresh()
   {
      _hideTransparentCompounds = _config.HideCompositionallyTransparentCompounds.Value;
      _showBreakdownInEditMode = _config.ShowSentenceBreakdownInEditMode.Value;
      _hideAllCompounds = _config.HideAllCompounds.Value;
      _logWhenFlushingNotes = _config.LogWhenFlushingNotes.Value;
      _showCompoundPartsInSentenceBreakdown = _config.ShowCompoundPartsInSentenceBreakdown.Value;
      _showKanjiInSentenceBreakdown = _config.ShowKanjiInSentenceBreakdown.Value;
      _showKanjiMnemonicsInSentenceBreakdown = _config.ShowKanjiMnemonicsInSentenceBreakdown.Value;
      _automaticallyYieldLastTokenInSuruVerbCompoundsToOverlappingCompound = _config.AutomaticallyYieldLastTokenInSuruVerbCompoundsToOverlappingCompound.Value;
      _automaticallyYieldLastTokenInPassiveVerbCompoundsToOverlappingCompound = _config.AutomaticallyYieldLastTokenInPassiveVerbCompoundsToOverlappingCompound.Value;
      _automaticallyYieldLastTokenInCausativeVerbCompoundsToOverlappingCompound = _config.AutomaticallyYieldLastTokenInCausativeVerbCompoundsToOverlappingCompound.Value;
   }

   public bool HideTransparentCompounds()
   {
      EnsureInitialized();
      return _hideTransparentCompounds;
   }

   public bool HideAllCompounds()
   {
      EnsureInitialized();
      return _hideAllCompounds;
   }

   public bool ShowBreakdownInEditMode()
   {
      EnsureInitialized();
      return _showBreakdownInEditMode;
   }

   public bool LogWhenFlushingNotes()
   {
      EnsureInitialized();
      return _logWhenFlushingNotes;
   }

   public bool ShowCompoundPartsInSentenceBreakdown()
   {
      EnsureInitialized();
      return _showCompoundPartsInSentenceBreakdown;
   }

   public bool ShowKanjiInSentenceBreakdown()
   {
      EnsureInitialized();
      return _showKanjiInSentenceBreakdown;
   }

   public bool ShowKanjiMnemonicsInSentenceBreakdown()
   {
      EnsureInitialized();
      return _showKanjiMnemonicsInSentenceBreakdown;
   }

   public bool AutomaticallyYieldLastTokenInSuruVerbCompoundsToOverlappingCompound()
   {
      EnsureInitialized();
      return _automaticallyYieldLastTokenInSuruVerbCompoundsToOverlappingCompound;
   }

   public bool AutomaticallyYieldLastTokenInPassiveVerbCompoundsToOverlappingCompound()
   {
      EnsureInitialized();
      return _automaticallyYieldLastTokenInPassiveVerbCompoundsToOverlappingCompound;
   }

   public bool AutomaticallyYieldLastTokenInCausativeVerbCompoundsToOverlappingCompound()
   {
      EnsureInitialized();
      return _automaticallyYieldLastTokenInCausativeVerbCompoundsToOverlappingCompound;
   }
}
