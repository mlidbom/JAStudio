using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Configuration;

namespace JAStudio.Core.UI.Web.Sentence;

/// <summary>
/// One of the sentence view toggles (<see cref="JapaneseConfig.SentenceViewToggles"/>) that's on, as the sentence breakdown's title shows it: a short<br/>
/// abbreviation, such as <c>SK</c> for showing the kanji, with the toggle's title on hover, so that how the breakdown is shown is seen at a glance.
/// </summary>
public sealed class SentenceViewToggleViewModel
{
   static readonly Dictionary<string, string> Abbreviations = new()
   {
      { "show_sentence_breakdown_in_edit_mode", "EM" },
      { "show_kanji_in_sentence_breakdown", "SK" },
      { "show_compound_parts_in_sentence_breakdown", "SCP" },
      { "show_kanji_mnemonics_in_sentence_breakdown", "SKM" },
      { "automatically_yield_last_token_in_suru_verb_compounds_to_overlapping_compound", "YSV" },
      { "automatically_yield_last_token_in_passive_verb_compounds_to_overlapping_compound", "YPV" },
      { "automatically_yield_last_token_in_causative_verb_compounds_to_overlapping_compound", "YCV" },
      { "hide_compositionally_transparent_compounds", "HCTC" },
      { "hide_all_compounds", "HAC" }
   };

   static readonly HashSet<string> TogglesChangingParsing =
   [
      "automatically_yield_last_token_in_suru_verb_compounds_to_overlapping_compound",
      "automatically_yield_last_token_in_passive_verb_compounds_to_overlapping_compound",
      "automatically_yield_last_token_in_causative_verb_compounds_to_overlapping_compound"
   ];

   static readonly HashSet<string> TogglesWorthNoticing = ["show_sentence_breakdown_in_edit_mode", "hide_all_compounds"];

   readonly ConfigurationValue<bool> _toggle;

   SentenceViewToggleViewModel(ConfigurationValue<bool> toggle) => _toggle = toggle;

   /// <summary>The toggles that are on, in the order the configuration lists them.</summary>
   public static List<SentenceViewToggleViewModel> TheOnesOnIn(JapaneseConfig config) =>
      [.. config.SentenceViewToggles.Where(it => it.Value).Select(it => new SentenceViewToggleViewModel(it))];

   /// <summary>The toggle's name in the configuration, such as <c>show_kanji_in_sentence_breakdown</c>, which the cards' styles colour it by.</summary>
   public string Name => _toggle.Name;

   /// <summary>The toggle's title, which says what it does and its shortcut.</summary>
   public string Title => _toggle.Title;

   public string Abbreviation => Abbreviations.TryGetValue(Name, out var abbreviation) ? abbreviation : $"MISSING_ABBREVIATION:{Name}";

   /// <summary>Whether the toggle changes how sentences are parsed, such as yielding a suru verb's last token to a compound overlapping it, rather than only what's shown.</summary>
   public bool ChangesParsing => TogglesChangingParsing.Contains(Name);

   /// <summary>Whether the toggle, on, shows the breakdown in a way that's easy to forget is on, such as its edit mode, so that it should stand out.</summary>
   public bool IsWorthNoticing => TogglesWorthNoticing.Contains(Name);
}
