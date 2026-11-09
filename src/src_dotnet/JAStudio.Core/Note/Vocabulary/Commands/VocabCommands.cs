using System;
using System.Collections.Generic;
using JAStudio.Core.Note.Commands;

namespace JAStudio.Core.Note.Vocabulary.Commands;

/// <summary>
/// Every command of a vocab's menus (<see cref="INoteCommand{TNote}"/>): those that change the vocab, such as accepting its meaning, and those that<br/>
/// create a vocab from it, its variations, such as its する-verb or its て-form, and its combinations with a text prefixed or suffixed onto it.
/// </summary>
/// <remarks>
/// The vocab's menus are built from these commands, and JAStudio's MCP server runs <see cref="All"/> of them, so the two do the same things.<br/>
/// Each menu item a person runs on a vocab is one of these, other than those that open notes, copy text, open a dialog or reparse sentences, and<br/>
/// those that edit a field (<see cref="EditableFields.VocabEditableFields"/>).
/// </remarks>
public static class VocabCommands
{
   public static readonly NoteChangingCommand<VocabNote> AcceptMeaning = new("acceptMeaning", "Accept meaning", it => it.AcceptMeaning());
   public static readonly NoteChangingCommand<VocabNote> GenerateAnswer = new("generateAnswer", "Generate answer", it => it.GenerateAndSetAnswer());
   public static readonly NoteChangingCommand<VocabNote> RepopulatePartsOfSpeech = new("repopulatePartsOfSpeech", "Repopulate parts of speech", it => it.PartsOfSpeech.SetAutomaticallyFromDictionary());

   /// <summary>Replaces the vocab's compound parts with the words its question splits into (<see cref="VocabNoteCompoundParts.AutoGenerate"/>).</summary>
   public static readonly NoteChangingCommand<VocabNote> AutogenerateCompoundParts = new("autogenerateCompoundParts", "Autogenerate compounds", it => it.CompoundParts.AutoGenerate());

   /// <summary>Adds the synonym, and the synonym's synonyms, to the vocab's synonyms, and the vocab's synonyms to the synonym's.</summary>
   public static readonly NoteChangingCommandTakingAText<VocabNote> AddSynonymsTransitivelyOneLevel =
      new("addSynonymsTransitivelyOneLevel", "Synonyms transitively one level", "the synonym", (it, synonym) => it.RelatedNotes.Synonyms.AddTransitivelyOneLevel(WordReference.FromDisambiguationName(synonym)));

   /// <summary>A copy of the vocab, with one of its forms as its question.</summary>
   public static readonly VocabCreatingCommandTakingAText CloneToForm = new("cloneToForm", "Clone to form", "the form to clone the vocab to", (it, form) => it.CloneToForm(form));

   public static readonly VocabCreatingCommand CreateSuruVerb = new("createSuruVerb", "する-verb", it => it.CreateSuruVerb());
   public static readonly VocabCreatingCommand CreateShimasuVerb = new("createShimasuVerb", "します-verb", it => it.CreateShimasuVerb());
   public static readonly VocabCreatingCommand CreateNaAdjective = new("createNaAdjective", "な-adjective", it => it.CreateNaAdjective());
   public static readonly VocabCreatingCommand CreateNoAdjective = new("createNoAdjective", "の-adjective", it => it.CreateNoAdjective());
   public static readonly VocabCreatingCommand CreateNiAdverb = new("createNiAdverb", "に-adverb", it => it.CreateNiAdverb());
   public static readonly VocabCreatingCommand CreateToAdverb = new("createToAdverb", "と-adverb", it => it.CreateToAdverb());

   public static readonly VocabCreatingCommand CreateMasuForm = new("createMasuForm", "ます-form", it => it.CreateMasuForm());
   public static readonly VocabCreatingCommand CreateTeForm = new("createTeForm", "て-form", it => it.CreateTeForm());
   public static readonly VocabCreatingCommand CreateTaForm = new("createTaForm", "た-form", it => it.CreateTaForm());
   public static readonly VocabCreatingCommand CreateNaiForm = new("createNaiForm", "ない-form", it => it.CreateNaiForm());
   public static readonly VocabCreatingCommand CreateEStem = new("createEStem", "え-stem/godan-imperative", it => it.SuffixToEStem(""));
   public static readonly VocabCreatingCommand CreateBaForm = new("createBaForm", "ば-form", it => it.CreateBaForm());
   public static readonly VocabCreatingCommand CreateReceptiveForm = new("createReceptiveForm", "{receptive/passive}-form", it => it.CreateReceptiveForm());
   public static readonly VocabCreatingCommand CreateCausativeForm = new("createCausativeForm", "causative", it => it.CreateCausativeForm());
   public static readonly VocabCreatingCommand CreateImperative = new("createImperative", "imperative", it => it.CreateImperative());
   public static readonly VocabCreatingCommand CreatePotentialGodan = new("createPotentialGodan", "Potential-godan", it => it.CreatePotentialGodan());

   public static readonly VocabCreatingCommand CreateKuForm = new("createKuForm", "く-form-of-い-adjective", it => it.CreateKuForm());
   public static readonly VocabCreatingCommand CreateSaForm = new("createSaForm", "さ-form-of-い-adjective", it => it.CreateSaForm());
   public static readonly VocabCreatingCommand CreateTePrefixed = new("createTePrefixed", "て-prefixed", it => it.CreateTePrefixedWord());
   public static readonly VocabCreatingCommand CreateOPrefixed = new("createOPrefixed", "お-prefixed", it => it.CreateOPrefixedWord());
   public static readonly VocabCreatingCommand CreateNSuffixed = new("createNSuffixed", "ん-suffixed", it => it.CreateNSuffixedWord());
   public static readonly VocabCreatingCommand CreateKaSuffixed = new("createKaSuffixed", "か-suffixed", it => it.CreateKaSuffixedWord());

   public static readonly VocabCreatingCommandTakingAText PrefixOntoDictionaryForm = PrefixOnto("prefixOntoDictionaryForm", "Dictionary form", "the vocab", (it, prefix) => it.PrefixToDictionaryForm(prefix));
   public static readonly VocabCreatingCommandTakingAText PrefixOntoChopping1 = PrefixOntoChopping(1);
   public static readonly VocabCreatingCommandTakingAText PrefixOntoChopping2 = PrefixOntoChopping(2);
   public static readonly VocabCreatingCommandTakingAText PrefixOntoChopping3 = PrefixOntoChopping(3);

   public static readonly VocabCreatingCommandTakingAText SuffixOntoDictionaryForm = SuffixOnto("suffixOntoDictionaryForm", "dictionary-form", "the vocab", (it, suffix) => it.CreateSuffixVersion(suffix));
   public static readonly VocabCreatingCommandTakingAText SuffixOntoIStem = SuffixOnto("suffixOntoIStem", "い-stem", "the vocab's い-stem", (it, suffix) => it.SuffixToIStem(suffix));
   public static readonly VocabCreatingCommandTakingAText SuffixOntoTeStem = SuffixOnto("suffixOntoTeStem", "て-stem", "the vocab's て-stem", (it, suffix) => it.SuffixToTeStem(suffix));
   public static readonly VocabCreatingCommandTakingAText SuffixOntoEStem = SuffixOnto("suffixOntoEStem", "え-stem", "the vocab's え-stem", (it, suffix) => it.SuffixToEStem(suffix));
   public static readonly VocabCreatingCommandTakingAText SuffixOntoAStem = SuffixOnto("suffixOntoAStem", "あ-stem", "the vocab's あ-stem", (it, suffix) => it.SuffixToAStem(suffix));
   public static readonly VocabCreatingCommandTakingAText SuffixOntoChopping1 = SuffixOntoChopping(1);
   public static readonly VocabCreatingCommandTakingAText SuffixOntoChopping2 = SuffixOntoChopping(2);
   public static readonly VocabCreatingCommandTakingAText SuffixOntoChopping3 = SuffixOntoChopping(3);
   public static readonly VocabCreatingCommandTakingAText SuffixOntoChopping4 = SuffixOntoChopping(4);

   /// <summary>Every command above, in the same order.</summary>
   public static IReadOnlyList<INoteCommand<VocabNote>> All { get; } =
   [
      AcceptMeaning, GenerateAnswer, RepopulatePartsOfSpeech, AutogenerateCompoundParts, AddSynonymsTransitivelyOneLevel,
      CloneToForm,
      CreateSuruVerb, CreateShimasuVerb, CreateNaAdjective, CreateNoAdjective, CreateNiAdverb, CreateToAdverb,
      CreateMasuForm, CreateTeForm, CreateTaForm, CreateNaiForm, CreateEStem, CreateBaForm, CreateReceptiveForm, CreateCausativeForm, CreateImperative, CreatePotentialGodan,
      CreateKuForm, CreateSaForm, CreateTePrefixed, CreateOPrefixed, CreateNSuffixed, CreateKaSuffixed,
      PrefixOntoDictionaryForm, PrefixOntoChopping1, PrefixOntoChopping2, PrefixOntoChopping3,
      SuffixOntoDictionaryForm, SuffixOntoIStem, SuffixOntoTeStem, SuffixOntoEStem, SuffixOntoAStem, SuffixOntoChopping1, SuffixOntoChopping2, SuffixOntoChopping3, SuffixOntoChopping4
   ];

   /// <summary>A command that creates a vocab with a text prefixed onto <paramref name="onto"/>, such as "the vocab", as an expression.</summary>
   static VocabCreatingCommandTakingAText PrefixOnto(string name, string label, string onto, Func<VocabCloner, string, VocabNote> create) =>
      new(name, label, $"the text to prefix onto {onto}", create);

   /// <summary>A command that creates a vocab with a text prefixed onto the vocab after chopping <paramref name="characters"/> characters off its start.</summary>
   static VocabCreatingCommandTakingAText PrefixOntoChopping(int characters) =>
      PrefixOnto($"prefixOntoChopping{characters}", $"chop-{characters}", $"the vocab, after chopping {NumberOfCharacters(characters)} off its start", (it, prefix) => it.PrefixToChopped(prefix, characters));

   /// <summary>A command that creates a vocab with a text suffixed onto <paramref name="onto"/>, such as "the vocab's い-stem", as an expression.</summary>
   static VocabCreatingCommandTakingAText SuffixOnto(string name, string label, string onto, Func<VocabCloner, string, VocabNote> create) =>
      new(name, label, $"the text to suffix onto {onto}", create);

   /// <summary>A command that creates a vocab with a text suffixed onto the vocab after chopping <paramref name="characters"/> characters off its end.</summary>
   static VocabCreatingCommandTakingAText SuffixOntoChopping(int characters) =>
      SuffixOnto($"suffixOntoChopping{characters}", $"chop-{characters}", $"the vocab, after chopping {NumberOfCharacters(characters)} off its end", (it, suffix) => it.SuffixToChopped(suffix, characters));

   static string NumberOfCharacters(int count) => count == 1 ? "1 character" : $"{count} characters";
}
