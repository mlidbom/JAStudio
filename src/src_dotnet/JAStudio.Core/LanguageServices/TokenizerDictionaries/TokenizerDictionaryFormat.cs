using System;
using System.Collections.Generic;
using System.IO;
using MeCab;

namespace JAStudio.Core.LanguageServices.TokenizerDictionaries;

/// <summary>
/// How a <see cref="TokenizerDictionary"/> describes a token in the features MeCab gives it (<see cref="MeCabNode.Feature"/>): which of the<br/>
/// comma-separated fields holds the base form, the lemma, the reading and the pronunciation. IPADIC and UniDic both start with the part of<br/>
/// speech in four levels, the inflection type and the inflection form; past those, they differ.
/// </summary>
public sealed class TokenizerDictionaryFormat
{
   readonly int _baseFormField;
   readonly int? _lemmaField;
   readonly Func<int, int> _readingFieldAmong;
   readonly int _pronunciationField;

   TokenizerDictionaryFormat(string name, int baseFormField, int? lemmaField, Func<int, int> readingFieldAmong, int pronunciationField)
   {
      Name = name;
      _baseFormField = baseFormField;
      _lemmaField = lemmaField;
      _readingFieldAmong = readingFieldAmong;
      _pronunciationField = pronunciationField;
   }

   /// <summary>
   /// IPADIC's: part of speech, inflection type, inflection form, base form, reading, pronunciation, in nine fields. An unknown word has the<br/>
   /// first seven, its base form <c>*</c>. IPADIC has no lemma.
   /// </summary>
   public static TokenizerDictionaryFormat Ipadic { get; } = new("IPADIC", baseFormField: 6, lemmaField: null, readingFieldAmong: _ => 7, pronunciationField: 8);

   /// <summary>
   /// UniDic's: 26 fields in the releases up to 2.x, 29 since 3.0, the first 13 the same in all. The lemma (<c>lemma</c>) is the eighth, the<br/>
   /// pronunciation (<c>pron</c>) the tenth, and the base form as spelled in the text (<c>orthBase</c>) the eleventh. The reading as written<br/>
   /// (<c>kana</c>) is the 21st of 29 fields, and the 18th of 26. An unknown word has only the first six.
   /// </summary>
   /// <remarks>
   /// The lemma (語彙素) is UniDic's word for the token, uniting its spellings: ボク and ぼく are both <c>僕</c>. Some lemmas carry a<br/>
   /// sub-classification after a hyphen, such as <c>僕-代名詞</c>; a proper noun's lemma is its reading in katakana.
   /// </remarks>
   public static TokenizerDictionaryFormat UniDic { get; } =
      new("UniDic", baseFormField: 10, lemmaField: 7, readingFieldAmong: fieldCount => fieldCount >= 29 ? 20 : 17, pronunciationField: 9);

   /// <summary>The format's name, such as <c>IPADIC</c>.</summary>
   public string Name { get; }

   /// <summary>
   /// The format of the dictionary in <paramref name="folder"/>, as its configuration file, <c>dicrc</c>, tells: IPADIC's names itself, and<br/>
   /// UniDic's defines MeCab's output format <c>unidic</c>.
   /// </summary>
   public static TokenizerDictionaryFormat Of(DirectoryInfo folder)
   {
      var configuration = File.ReadAllText(Path.Combine(folder.FullName, "dicrc"));
      if(configuration.Contains("Configuration file of IPADIC", StringComparison.Ordinal)) return Ipadic;
      if(configuration.Contains("node-format-unidic", StringComparison.Ordinal)) return UniDic;
      throw new ArgumentException($"The tokenizer dictionary in {folder.FullName} is neither IPADIC nor UniDic, by its dicrc.");
   }

   /// <summary>The token MeCab gives as <paramref name="node"/>, which starts at <paramref name="start"/> in the text, described as this format describes it.</summary>
   internal MeCabToken TokenOf(MeCabNode node, int start)
   {
      var fields = MeCabFeatureFields.Of(node.Feature);
      return new MeCabToken(start,
                            node.Surface,
                            IsUnknownToTheDictionary: node.Stat == MeCabNodeStat.Unk,
                            PartOfSpeech: string.Join(',', FieldOrNothing(0), FieldOrNothing(1), FieldOrNothing(2), FieldOrNothing(3)),
                            InflectionType: FieldOrNothing(4),
                            InflectionForm: FieldOrNothing(5),
                            BaseForm: FieldOrNothing(_baseFormField),
                            Lemma: _lemmaField is {} lemmaField ? FieldOrNothing(lemmaField) : null,
                            Reading: FieldOrNothing(_readingFieldAmong(fields.Count)),
                            Pronunciation: FieldOrNothing(_pronunciationField));

      // MeCab's "nothing" for a field the features don't have, as an unknown word's don't.
      string FieldOrNothing(int index) => index < fields.Count ? fields[index] : MeCabToken.Nothing;
   }

   public override string ToString() => Name;
}
