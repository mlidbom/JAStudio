using System;
using System.Collections.Generic;
using System.IO;
using MeCab;

namespace JAStudio.Core.LanguageServices.TokenizerDictionaries;

/// <summary>
/// A dictionary the tokenizer, MeCab, reads with: the words it knows, with their parts of speech, inflections and base forms, and the costs by<br/>
/// which it picks the likeliest way to cut a text into words. Not JMdict, the dictionary JAStudio looks words up in for their meanings.
/// </summary>
/// <remarks>
/// JAStudio's tokenizer reads with IPADIC, which MeCab.DotNet's package comes with (<see cref="TheBundledIpadic"/>): IPADIC hasn't changed since<br/>
/// 2007. UniDic, from Japan's national language institute (NINJAL), is the one maintained today; switching to it is under way on the branch<br/>
/// <c>unidic</c> (<c>dev_docs/todo/switching-to-unidic.md</c>).
/// </remarks>
public sealed class TokenizerDictionary
{
   readonly MeCabTaggerPool _taggers;

   TokenizerDictionary(string name, DirectoryInfo folder)
   {
      Name = name;
      Folder = folder;
      Format = TokenizerDictionaryFormat.Of(folder);
      _taggers = new MeCabTaggerPool(folder);
   }

   /// <summary>IPADIC, as MeCab.DotNet's package copies it into <c>dic</c> next to this assembly.</summary>
   public static TokenizerDictionary TheBundledIpadic { get; } =
      new("bundled IPADIC", new DirectoryInfo(Path.Combine(Path.GetDirectoryName(typeof(TokenizerDictionary).Assembly.Location)
                                                           ?? throw new InvalidOperationException("Cannot determine the assembly's folder"),
                                                           "dic")));

   /// <summary>The tokenizer dictionary in <paramref name="folder"/>, named by the folder's name, such as <c>unidic-cwj-202512</c>.</summary>
   public static TokenizerDictionary In(DirectoryInfo folder) => new(folder.Name, folder);

   /// <summary>What the dictionary is called in a report, such as <c>bundled IPADIC</c> or <c>unidic-cwj-202512</c>.</summary>
   public string Name { get; }

   /// <summary>The folder of the dictionary's files, which MeCab maps into memory.</summary>
   public DirectoryInfo Folder { get; }

   /// <summary>How the dictionary describes a token in the features MeCab gives it: IPADIC's way or UniDic's.</summary>
   public TokenizerDictionaryFormat Format { get; }

   /// <summary>The tokens MeCab cuts <paramref name="text"/> into, reading with this dictionary.</summary>
   public IReadOnlyList<MeCabToken> Tokenize(string text) => LendATaggerTo(tagger => MeCabToken.EachOf(text, tagger.ParseToNodes(text), Format));

   /// <summary>
   /// Lends <paramref name="parse"/> a tagger that reads with this dictionary, which no other parse uses until <paramref name="parse"/> returns<br/>
   /// (<see cref="MeCabTaggerPool"/>).
   /// </summary>
   internal TResult LendATaggerTo<TResult>(Func<MeCabTagger, TResult> parse) => _taggers.LendATaggerTo(parse);

   public override string ToString() => $"{Name} ({Folder.FullName})";
}
