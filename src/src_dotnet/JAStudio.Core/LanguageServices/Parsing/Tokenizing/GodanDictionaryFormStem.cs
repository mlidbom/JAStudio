namespace JAStudio.Core.LanguageServices.Parsing.Tokenizing;

// Base class for split tokens
public abstract class SplitTokenBase : AnalysisToken
{
   MeCabToken Source { get; }
   readonly string _surface;
   readonly string _baseForm;

   protected SplitTokenBase(MeCabToken source, string surface, string baseForm)
   {
      Source = source;
      _surface = surface;
      _baseForm = baseForm;
   }

   public override MeCabToken SourceToken => Source;
   public override string Surface => _surface;
   public override string BaseForm => _baseForm;

   public override string ToString() => $"{GetType().Name}('{Surface}', '{BaseForm}')";
}

// Base class for dictionary form tokens
abstract class DictionaryFormsTokenBase : SplitTokenBase
{
   protected DictionaryFormsTokenBase(MeCabToken source, string surface, string baseForm)
      : base(source, surface, baseForm) {}

   // TODO: this feels odd, but without it it seems that things go haywire...
   public override bool IsInflectableWord => true;
}

// Dictionary form stem
class DictionaryFormStem : DictionaryFormsTokenBase
{
   public DictionaryFormStem(MeCabToken source, string surface, string baseForm)
      : base(source, surface, baseForm) {}

   public override bool IsDictionaryVerbFormStem => true;
}

// Dictionary form inflection
class DictionaryFormInflection : DictionaryFormsTokenBase
{
   public DictionaryFormInflection(MeCabToken source, string surface, string baseForm)
      : base(source, surface, baseForm) {}

   public override bool IsDictionaryVerbInflection => true;
}

// Godan dictionary form stem
class GodanDictionaryFormStem : DictionaryFormStem
{
   public GodanDictionaryFormStem(MeCabToken source, string surface, string baseForm)
      : base(source, surface, baseForm) {}
}

// Godan potential dictionary form stem
class GodanPotentialDictionaryFormStem : GodanDictionaryFormStem
{
   public GodanPotentialDictionaryFormStem(MeCabToken source, string surface, string baseForm)
      : base(source, surface, baseForm) {}

   public override bool IsGodanPotentialStem => true;

   // TODO: this leaves us with these tokens claiming both to be an ichidan (via the base class) and a godan... We can't tell which, but still..
   public override bool IsGodanVerb => true;
}

// Godan dictionary form inflection
class GodanDictionaryFormInflection : DictionaryFormInflection
{
   public GodanDictionaryFormInflection(MeCabToken source, string surface, string baseForm)
      : base(source, surface, baseForm) {}
}

// Godan potential inflection dictionary form stem
class GodanPotentialInflectionDictionaryFormStem : DictionaryFormStem
{
   public GodanPotentialInflectionDictionaryFormStem(MeCabToken source, string surface, string baseForm)
      : base(source, surface, baseForm) {}

   public override bool IsGodanPotentialInflection => true;
}

// Godan potential inflection dictionary form inflection
class GodanPotentialInflectionDictionaryFormInflection : DictionaryFormInflection
{
   public GodanPotentialInflectionDictionaryFormInflection(MeCabToken source)
      : base(source, "る", "る") {}
}

// Ichidan dictionary form stem
class IchidanDictionaryFormStem : DictionaryFormStem
{
   public IchidanDictionaryFormStem(MeCabToken source, string surface, string baseForm)
      : base(source, surface, baseForm) {}
}

// Ichidan dictionary form inflection
class IchidanDictionaryFormInflection : DictionaryFormInflection
{
   public IchidanDictionaryFormInflection(MeCabToken source)
      : base(source, "る", "る") {}
}

// Kuru verb dictionary form stem
class KuruVerbDictionaryFormStem : DictionaryFormStem
{
   public KuruVerbDictionaryFormStem(MeCabToken source, string surface, string baseForm)
      : base(source, surface, baseForm) {}
}

// Kuru verb dictionary form inflection
class KuruVerbDictionaryFormInflection : DictionaryFormInflection
{
   public KuruVerbDictionaryFormInflection(MeCabToken source)
      : base(source, "る", "る") {}
}

// Suru verb dictionary form stem
class SuruVerbDictionaryFormStem : DictionaryFormStem
{
   public SuruVerbDictionaryFormStem(MeCabToken source, string surface, string baseForm)
      : base(source, surface, baseForm) {}
}

// Suru verb dictionary form inflection
class SuruVerbDictionaryFormInflection : DictionaryFormInflection
{
   public SuruVerbDictionaryFormInflection(MeCabToken source)
      : base(source, "る", "る") {}
}
