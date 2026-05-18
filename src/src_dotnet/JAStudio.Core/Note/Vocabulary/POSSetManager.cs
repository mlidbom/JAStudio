using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using Compze.Threading;

namespace JAStudio.Core.Note.Vocabulary;

class POSSetManager
{
   static readonly Dictionary<string, FrozenSet<string>> _posByStr = new();
   static readonly Dictionary<string, string> _stringInterner = new();
   static readonly IMonitor _monitor = IMonitor.New();

   static readonly Dictionary<string, List<string>> _remappings = new()
                                                                  {
                                                                     // Our own harmonizations (for VocabNote usage)
                                                                     ["intransitive verb"] = [POS.Intransitive],
                                                                     ["transitive verb"] = [POS.Transitive],
                                                                     ["godan"] = [POS.GodanVerb],
                                                                     ["ichidan"] = [POS.IchidanVerb],

                                                                     // JMDict POS mappings
                                                                     ["な adjective"] = [POS.NaAdjective],
                                                                     ["noun or participle which takes the aux. verb suru"] = [POS.SuruVerb],
                                                                     ["noun (common) (futsuumeishi)"] = [POS.Noun],
                                                                     ["adjectival nouns or quasi-adjectives (keiyodoshi)"] = [POS.NaAdjective],
                                                                     ["noun, used as a suffix"] = [POS.Noun, POS.Suffix],
                                                                     ["godan verb with 'u' ending"] = [POS.GodanVerb],
                                                                     ["godan verb with 'ru' ending"] = [POS.GodanVerb],
                                                                     ["godan verb with 'mu' ending"] = [POS.GodanVerb],
                                                                     ["godan verb with 'nu' ending"] = [POS.GodanVerb],
                                                                     ["godan verb with 'gu' ending"] = [POS.GodanVerb],
                                                                     ["godan verb with 'ku' ending"] = [POS.GodanVerb],
                                                                     ["godan verb with 'su' ending"] = [POS.GodanVerb],
                                                                     ["godan verb with 'bu' ending"] = [POS.GodanVerb],
                                                                     ["godan verb with 'u' ending (special class)"] = [POS.GodanVerb, POS.SpecialClass],
                                                                     ["godan verb - -aru special class"] = [POS.GodanVerb, POS.SpecialClassAru],
                                                                     ["godan verb with 'tsu' ending"] = [POS.GodanVerb],
                                                                     ["irregular nu verb"] = [POS.NuVerb],
                                                                     ["godan verb - iku/yuku special class"] = [POS.GodanVerb, POS.SpecialClass],
                                                                     ["godan verb with 'ru' ending (irregular verb)"] = [POS.GodanVerb, POS.Irregular],
                                                                     ["ichidan verb"] = [POS.IchidanVerb],
                                                                     ["ichidan verb - zuru verb (alternative form of -jiru verbs)"] = [POS.IchidanVerb, POS.ZuruVerb],
                                                                     ["kuru verb - special class"] = [POS.KuruVerb, POS.SpecialClass],
                                                                     ["yodan verb with 'ru' ending (archaic)"] = [POS.YodanVerb],
                                                                     ["yodan verb with 'ku' ending (archaic)"] = [POS.YodanVerb],
                                                                     ["nidan verb (lower class) with 'ru' ending (archaic)"] = [POS.NidanVerb],
                                                                     ["nidan verb (upper class) with 'ru' ending (archaic)"] = [POS.NidanVerb],
                                                                     ["adjective (keiyoushi)"] = [POS.IAdjective],
                                                                     ["adjective (keiyoushi) - yoi/ii class"] = [POS.IAdjective],
                                                                     ["adverb (fukushi)"] = [POS.Adverb],
                                                                     ["adverb taking the 'to' particle"] = [POS.ToAdverb],
                                                                     [POS.Auxiliary] = [POS.Auxiliary],
                                                                     ["auxiliary adjective"] = [POS.IAdjective, POS.Auxiliary],
                                                                     ["auxiliary verb"] = [POS.Auxiliary],
                                                                     [POS.Conjunction] = [POS.Conjunction],
                                                                     [POS.Copula] = [POS.Copula],
                                                                     ["expressions (phrases, clauses, etc.)"] = [POS.Expression],
                                                                     ["interjection (kandoushi)"] = [POS.Interjection],
                                                                     ["noun, used as a prefix"] = [POS.Prefix, POS.Noun],
                                                                     ["nouns which may take the genitive case particle 'no'"] = [POS.Noun, POS.NoAdjective],
                                                                     [POS.Particle] = [POS.Particle],
                                                                     ["pre-noun adjectival (rentaishi)"] = [POS.PreNounAdjectival],
                                                                     [POS.Prefix] = [POS.Prefix],
                                                                     [POS.Pronoun] = [POS.Pronoun],
                                                                     [POS.Suffix] = [POS.Suffix],
                                                                     ["suru verb - included"] = [POS.SuruVerb],
                                                                     [POS.SuruVerb] = [POS.SuruVerb],
                                                                     ["su verb - precursor to the modern suru"] = [POS.SuVerb],
                                                                     [POS.Counter] = [POS.Counter],
                                                                     [POS.Numeric] = [POS.Numeric],
                                                                     ["noun or verb acting prenominally"] = [POS.Prenominal],
                                                                     ["suru verb - special class"] = [POS.SuruVerb, POS.SpecialClass],
                                                                     ["ichidan verb - kureru special class"] = [POS.IchidanVerb, POS.SpecialClass],
                                                                     ["'taru' adjective"] = [POS.TaruAdjective],
                                                                  };

   static HashSet<string> Harmonize(IEnumerable<string> pos)
   {
      var result = new HashSet<string>();
      foreach(var item in pos)
      {
         if(_remappings.TryGetValue(item.ToLower(), out var mapped))
         {
            result.UnionWith(mapped);
         } else
         {
            result.Add(item);
         }
      }

      return result;
   }

   static string AutoIntern(string str)
   {
      if(!_stringInterner.TryGetValue(str, out var interned))
      {
         _monitor.Locked(() => _stringInterner[str] = str);
         interned = str;
      }

      return interned;
   }

   static FrozenSet<string> InternFrozenSet(HashSet<string> posValuesSet)
   {
      var posKey = string.Join(", ", posValuesSet.OrderBy(s => s));
      if(!_posByStr.TryGetValue(posKey, out var frozenSet))
      {
         var internedList = posValuesSet.Select(AutoIntern).ToFrozenSet();
         _monitor.Locked(() => _posByStr[AutoIntern(posKey)] = internedList);
         frozenSet = internedList;
      }

      return frozenSet;
   }

   public static string InternAndHarmonize(string pos)
   {
      var posValuesList = pos.Split(',')
                             .Select(s => s.Trim().ToLower())
                             .ToList();
      var posValuesSet = Harmonize(posValuesList);
      InternFrozenSet(posValuesSet);
      return string.Join(", ", posValuesSet.OrderBy(s => s));
   }

   public static FrozenSet<string> InternAndHarmonizeFromList(List<string> posList)
   {
      var posValuesSet = Harmonize(posList.Select(p => p.ToLower()));
      return InternFrozenSet(posValuesSet);
   }

   public static FrozenSet<string> Get(string pos) => _posByStr[pos];

   public static bool IsTransitiveVerb(FrozenSet<string> posSet) => posSet.Contains(POS.Transitive);

   public static bool IsIntransitiveVerb(FrozenSet<string> posSet) => posSet.Contains(POS.Intransitive);

   public static bool IsVerb(FrozenSet<string> posSet) => posSet.Overlaps(POS.AllVerbPoses);
}
