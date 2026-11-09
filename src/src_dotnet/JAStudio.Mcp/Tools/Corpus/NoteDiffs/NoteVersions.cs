using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Storage;
using JAStudio.Mcp.Corpus;
using JAStudio.Mcp.Tools.Corpus.NoteDiffs.ParsedWords;
using JAStudio.Mcp.Tools.Corpus.NoteDiffs.Properties;

namespace JAStudio.Mcp.Tools.Corpus.NoteDiffs;

/// <summary>
/// A note as two versions of the corpus have it, each version's file read into a note as JAStudio reads notes: what a diff compares, so that it<br/>
/// compares notes rather than files. Null in a version that doesn't have the note.
/// </summary>
public class NoteVersions
{
   /// <summary>The path of a sentence's parsed words, as the corpus stores a sentence: <see cref="ParsedWordsChange"/> compares them as words, not as stored values.</summary>
   internal const string ParsedWordsProperty = "parsingResult.parsedWords";

   static readonly IReadOnlySet<string> PropertiesComparedAsMoreThanStoredValues = new HashSet<string> { ParsedWordsProperty };

   readonly NoteSerializer _serializer;

   NoteVersions(JPNote? before, JPNote? after, NoteSerializer serializer)
   {
      Latest = after ?? before ?? throw new ArgumentException("Neither version has the note, so there is no note to compare.");
      Before = before;
      After = after;
      _serializer = serializer;
   }

   public JPNote? Before { get; }
   public JPNote? After { get; }

   /// <summary>The note as the latest of the two versions that has it has it: <see cref="After"/>, unless the note was deleted.</summary>
   public JPNote Latest { get; }

   /// <summary>Reads both versions of the note's file, <paramref name="files"/>, into notes, with <paramref name="serializer"/>, as JAStudio reads notes.</summary>
   /// <remarks>
   /// Only a file in the form this JAStudio stores notes in can be read into a note: one from before a change to that form can't, so neither can<br/>
   /// a diff over such a change. Comparing across the change that made vocab questions objects, 2026-11-08, failed with a <see cref="JsonException"/><br/>
   /// naming only the property it failed at, so the exception is wrapped in one that names the file and the version too.
   /// </remarks>
   public static NoteVersions Read(NoteFileVersions files, NoteSerializer serializer)
   {
      return new NoteVersions(NoteIn(files.Before, "the version compared from"), NoteIn(files.After, "the version compared to"), serializer);

      JPNote? NoteIn(string? file, string version)
      {
         if(file == null) return null;
         try
         {
            return files.NoteFolder switch
            {
               "vocab" => serializer.DeserializeVocab(file),
               "kanji" => serializer.DeserializeKanji(file),
               "sentences" => serializer.DeserializeSentence(file),
               _ => throw new ArgumentOutOfRangeException(nameof(files), files.Path, "A file in a folder that holds no notes.")
            };
         }
         catch(JsonException exception)
         {
            throw new InvalidOperationException($"The file {files.Path} in {version} can't be read into a note, as for a file from before a change to the form JAStudio stores notes in: {exception.Message}", exception);
         }
      }
   }

   /// <summary>How the note differs between the two versions, naming the vocab a sentence's parsed words are matched to by their questions in <paramref name="collection"/>.</summary>
   public NoteDiff Diff(JPCollection collection)
   {
      if(Before == null) return WithNoPropertiesCompared(NoteChangeKind.Added);
      if(After == null) return WithNoPropertiesCompared(NoteChangeKind.Deleted);

      var properties = PropertyChanges.Between(Stored(Before), Stored(After), PropertiesComparedAsMoreThanStoredValues);
      var parsedWords = ParsedWordsChangeBetween(Before, After, collection);

      var isTheSameNote = properties.ValuesChanged.Count == 0 && properties.ListsChanged.Count == 0 && parsedWords == null;
      return new NoteDiff(IdOfTheNote(),
                          Latest.GetQuestion(),
                          isTheSameNote ? NoteChangeKind.ChangedOnlyInItsFile : NoteChangeKind.Changed,
                          properties.ValuesChanged,
                          properties.ListsChanged,
                          parsedWords);

      NoteDiff WithNoPropertiesCompared(NoteChangeKind change) => new(IdOfTheNote(), Latest.GetQuestion(), change, [], [], null);
      string IdOfTheNote() => Latest.GetId().ToString();
   }

   /// <summary>How the parsed words changed between <paramref name="before"/> and <paramref name="after"/> when they're a sentence and the words did. Null otherwise.</summary>
   static ParsedWordsChange? ParsedWordsChangeBetween(JPNote before, JPNote after, JPCollection collection)
   {
      if(before is not SentenceNote sentenceBefore || after is not SentenceNote sentenceAfter) return null;

      var change = ParsedWordsChange.Between(sentenceBefore.GetParsingResult(), sentenceAfter.GetParsingResult(), collection);
      return change.IsNoChange() ? null : change;
   }

   /// <summary>The note as the corpus stores it, written as JAStudio writes notes now, whatever the file it was read from looked like.</summary>
   JsonObject Stored(JPNote note) => JsonNode.Parse(_serializer.Serialize(note))?.AsObject() ?? throw new InvalidOperationException($"The note {note.GetId()} was written as null.");
}
