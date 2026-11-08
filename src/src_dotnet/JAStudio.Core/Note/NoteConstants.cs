using System;
using System.Collections.Generic;
// ReSharper disable MemberCanBeInternal
// ReSharper disable MemberCanBePrivate.Global

namespace JAStudio.Core.Note;

public static class CardTypes
{
   public const string Reading = "Reading";
   public const string Listening = "Listening";
}

public static class NoteTypes
{
   public const string Kanji = "_Kanji";
   public const string Vocab = "_Vocab";
   public const string Sentence = "_japanese_sentence";

   public static readonly HashSet<string> All = [Kanji, Vocab, Sentence];

   public static readonly IReadOnlyList<string> AllList = [Kanji, Vocab, Sentence];

   public static Func<Guid, NoteId> IdFactoryFromName(string noteTypeName) => IDFactoriesByName[noteTypeName];

   //create a dictionary from type to string for our supported type her

   static readonly Dictionary<string, Func<Guid, NoteId>> IDFactoriesByName = new()
                                                                              {
                                                                                 [Kanji] = g => new KanjiId(g),
                                                                                 [Vocab] = g => new VocabId(g),
                                                                                 [Sentence] = g => new SentenceId(g),
                                                                              };
}

/// <summary>Keywords of Anki's search language, such as the <c>note:</c> of <c>note:"Immersion Kit Sentence"</c>.</summary>
public static class Builtin
{
   /// <summary>Searches by Anki note id, as in <c>nid:1234,5678</c>.</summary>
   public const string NoteId = "nid";

   public const string Tag = "tag";
   public const string Note = "note";
   public const string Deck = "deck";
   public const string Card = "card";
}

public static class Mine
{
   public const string AppName = "JA-Studio";

   // ReSharper disable once UnusedMember.Global - Used from Python (dotnet_rendering_content_renderer_anki_shim.py, common.py)
   public static readonly string AppStillLoadingMessage = $"{AppName} still loading, the view will refresh when done...";
   public const string VocabPrefixSuffixMarker = "〜";
}
