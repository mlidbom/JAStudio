namespace JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;

/// <summary>
/// The options that change how a sentence parses for its sentence breakdown, the words JAStudio shows for it, and only there: a sentence's stored<br/>
/// parsing result is the same under any of them. Each is an option of JAStudio's configuration of the same name. A <see cref="TextAnalysis"/> is<br/>
/// made under one set of them (<see cref="TextAnalysis.BreakdownParsingOptions"/>).
/// </summary>
/// <param name="HideAllCompounds">Hides a compound whose parts all have valid matches of their own, with exceptions, showing the parts instead.</param>
/// <param name="HideCompositionallyTransparentCompounds">Hides a compound whose meaning is that of its parts, showing the parts instead.</param>
/// <param name="AutomaticallyYieldLastTokenInSuruVerbCompoundsToOverlappingCompound">
/// A suru verb compound yields its last part to a compound that overlaps it from there.
/// </param>
/// <param name="AutomaticallyYieldLastTokenInPassiveVerbCompoundsToOverlappingCompound">
/// A passive verb compound yields its last part to a compound that overlaps it from there.
/// </param>
/// <param name="AutomaticallyYieldLastTokenInCausativeVerbCompoundsToOverlappingCompound">
/// A causative verb compound yields its last part to a compound that overlaps it from there.
/// </param>
public sealed record BreakdownParsingOptions(
   bool HideAllCompounds,
   bool HideCompositionallyTransparentCompounds,
   bool AutomaticallyYieldLastTokenInSuruVerbCompoundsToOverlappingCompound,
   bool AutomaticallyYieldLastTokenInPassiveVerbCompoundsToOverlappingCompound,
   bool AutomaticallyYieldLastTokenInCausativeVerbCompoundsToOverlappingCompound);
