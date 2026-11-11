using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Compze.Contracts;
using JAStudio.Core.Reservoir.Analysis;
using JAStudio.Core.Reservoir.Analysis.Numbering;
using JAStudio.Core.Reservoir.Sources;
using JAStudio.Core.SysUtils.Data.Sqlite;
using MemoryPack;
using Microsoft.Data.Sqlite;

namespace JAStudio.Core.Reservoir;

/// <summary>
/// The reservoir, kept in a SQLite file of its own: every sentence imported, not loaded or parsed in normal use, ready to be taken into the<br/>
/// working set when it's needed (<c>dev_docs/todo/expanding-the-sentence-corpus.md</c>). Each distinct sentence is kept once, with the<br/>
/// <see cref="ReservoirSource"/> it was first found in, where in it, and how often it was found in all.
/// </summary>
/// <remarks>
/// A sentence found again keeps the source it was found in first, unless that source had no audio for it and this one has: then it takes this<br/>
/// one's, since a sentence with audio is the one worth taking into the working set.
/// </remarks>
public sealed class ReservoirFile : IDisposable
{
   const string Schema = """
                         pragma journal_mode = wal;
                         pragma synchronous = off;
                         create table if not exists source(
                            id integer primary key,
                            collection text not null,
                            work text not null,
                            kind text not null,
                            location text not null,
                            sentences integer not null,
                            new_sentences integer not null,
                            unique(collection, work));
                         create table if not exists sentence(
                            id integer primary key,
                            text text not null unique,
                            source integer not null references source(id),
                            position text not null,
                            translation text,
                            audio_file text,
                            image_file text,
                            occurrences integer not null default 1);
                         create table if not exists sentence_analysis(
                            sentence integer primary key references sentence(id),
                            analysis blob not null);
                         create table if not exists vocab(
                            number integer primary key,
                            note_id text not null unique,
                            question text not null);
                         create table if not exists failure_reason(
                            number integer primary key,
                            text text not null unique);
                         """;

   /// <summary>
   /// Adds a sentence, or, when the reservoir has its text already, counts it found once more, and keeps it from this source instead when<br/>
   /// this source has its audio and the one it's kept from hasn't.
   /// </summary>
   const string AddSentence = """
                              insert into sentence(text, source, position, translation, audio_file, image_file) values($text, $source, $position, $translation, $audioFile, $imageFile)
                              on conflict(text) do update set
                                 occurrences = occurrences + 1,
                                 source = iif(audio_file is null and excluded.audio_file is not null, excluded.source, source),
                                 position = iif(audio_file is null and excluded.audio_file is not null, excluded.position, position),
                                 translation = iif(audio_file is null and excluded.audio_file is not null, excluded.translation, translation),
                                 image_file = iif(audio_file is null and excluded.audio_file is not null, excluded.image_file, image_file),
                                 audio_file = iif(audio_file is null and excluded.audio_file is not null, excluded.audio_file, audio_file)
                              """;

   /// <summary>
   /// How long a write waits for another process writing the same reservoir, such as a filling while an analyzing runs. Each write takes the<br/>
   /// lock when it starts (<c>BEGIN IMMEDIATE</c>), so a writer waits instead of failing when the other commits first.
   /// </summary>
   const int SecondsToWaitForAnotherWriter = 600;

   /// <summary>
   /// The format of the analyses the reservoir keeps (<see cref="ReservoirSentenceAnalysis"/>), which the file holds as its <c>user_version</c>.<br/>
   /// It goes up by one whenever the format changes: the analyses of an older format are then forgotten and made anew, never read as if they were<br/>
   /// of this one. Analyses of a newer format, made by newer code, are neither read nor written to.
   /// </summary>
   /// <remarks>0, the first, has no <see cref="ReservoirWord.IsValid"/>, so an invalid match the sentence breakdown displays looked valid.</remarks>
   const long AnalysisFormat = 1;

   /// <summary>The most ids a query lists in its <c>in (…)</c>: more are read in several queries.</summary>
   const int IdsPerQuery = 500;

   readonly SqliteConnection _connection;

   ReservoirFile(SqliteConnection connection) => _connection = connection;

   /// <summary>The reservoir in <paramref name="file"/>, created empty when there's no such file.</summary>
   public static ReservoirFile OpenOrCreate(FileInfo file)
   {
      var isNew = !File.Exists(file.FullName);
      var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = file.FullName, Pooling = false, DefaultTimeout = SecondsToWaitForAnotherWriter }.ToString());
      connection.Open();
      connection.Execute(Schema);
      var reservoir = new ReservoirFile(connection);
      if(isNew) reservoir.MarkTheAnalysesAsOfTheCurrentFormat();
      return reservoir;
   }

   /// <summary>Whether the reservoir has the sentences of <paramref name="source"/>'s work: a work is added once, and found by its collection and name.</summary>
   public bool Has(ReservoirSource source) =>
      _connection.ScalarAsLong("select count(*) from source where collection = $collection and work = $work", ("$collection", source.Collection), ("$work", source.Work)) > 0;

   /// <summary>Adds <paramref name="work"/>'s sentences, those the reservoir has already counting as found once more.</summary>
   public WorkAddedToTheReservoir Add(SourceWork work)
   {
      using var transaction = _connection.BeginTransaction(deferred: false);
      var lastSentenceIdBefore = LastSentenceId();
      var sourceId = AddTheSource(work.Source);
      var sentences = AddTheSentences(work.Sentences, sourceId);
      var newSentences = LastSentenceId() - lastSentenceIdBefore;
      _connection.Execute("update source set sentences = $sentences, new_sentences = $newSentences where id = $id", ("$sentences", sentences), ("$newSentences", newSentences), ("$id", sourceId));
      transaction.Commit();
      return new WorkAddedToTheReservoir(work.Source, sentences, newSentences);
   }

   /// <summary>How many distinct sentences the reservoir holds.</summary>
   public long SentenceCount => _connection.ScalarAsLong("select count(*) from sentence");

   /// <summary>Up to <paramref name="count"/> sentences with no analysis yet, in the order they were added, from the first added after <paramref name="afterSentenceId"/>.</summary>
   public IReadOnlyList<ReservoirSentence> UnanalyzedSentences(long afterSentenceId, int count) =>
      _connection.Rows("""
                       select sentence.id, sentence.text from sentence left join sentence_analysis on sentence_analysis.sentence = sentence.id
                       where sentence_analysis.sentence is null and sentence.id > $after order by sentence.id limit $count
                       """,
                       ReservoirSentenceIn,
                       ("$after", afterSentenceId),
                       ("$count", count));

   /// <summary>Saves <paramref name="analyses"/>, each of the sentence with its id, and every number <paramref name="numbers"/> has given.</summary>
   public void SaveAnalyses(IReadOnlyList<(long SentenceId, ReservoirSentenceAnalysis Analysis)> analyses, ReservoirAnalysisNumbers numbers)
   {
      AssertTheAnalysesAreOfTheCurrentFormat();
      using var transaction = _connection.BeginTransaction(deferred: false);
      using(var save = _connection.Prepared("insert or replace into sentence_analysis(sentence, analysis) values($sentence, $analysis)", "$sentence", "$analysis"))
      {
         foreach(var (sentenceId, analysis) in analyses) save.RunWith(sentenceId, MemoryPackSerializer.Serialize(analysis));
      }

      SaveTheNumbers(numbers);
      transaction.Commit();
   }

   /// <summary>The numbers the reservoir's analyses have given so far, to go on numbering from.</summary>
   public ReservoirAnalysisNumbers AnalysisNumbers() =>
      new(_connection.Rows("select number, note_id, question from vocab", reader => new NumberedVocab(reader.GetInt32(0), reader.GetString(1), reader.GetString(2))),
          _connection.Rows("select number, text from failure_reason", reader => new NumberedFailureReason(reader.GetInt32(0), reader.GetString(1))));

   /// <summary>
   /// Whether the analyses the reservoir keeps are of an older format than <see cref="AnalysisFormat"/>, which can't be read: they're to be<br/>
   /// forgotten and made anew (<see cref="ForgetTheAnalysesOfAnOlderFormat"/>).
   /// </summary>
   public bool KeepsAnalysesOfAnOlderFormat => FormatOfTheAnalysesKept < AnalysisFormat;

   /// <summary>Forgets every analysis the reservoir keeps, which are of an older format, so that analyzing makes them anew in <see cref="AnalysisFormat"/>.</summary>
   public void ForgetTheAnalysesOfAnOlderFormat()
   {
      Contract.State.Assert(KeepsAnalysesOfAnOlderFormat);
      _connection.Execute("drop table sentence_analysis");
      _connection.Execute(Schema);
      MarkTheAnalysesAsOfTheCurrentFormat();
   }

   /// <summary>Every analyzed sentence, with its analysis, read as they're enumerated.</summary>
   public IEnumerable<AnalyzedReservoirSentence> AnalyzedSentences()
   {
      AssertTheAnalysesAreOfTheCurrentFormat();
      return _connection.RowsReadAsEnumerated("""
                                              select sentence.id, sentence.text, sentence.source, sentence.occurrences, sentence.audio_file is not null, sentence_analysis.analysis
                                              from sentence join sentence_analysis on sentence_analysis.sentence = sentence.id
                                              """,
                                              reader => new AnalyzedReservoirSentence(reader.GetInt64(0), reader.GetString(1), reader.GetInt64(2), reader.GetInt32(3), reader.GetBoolean(4),
                                                                                      AnalysisIn(reader, 5, sentenceId: reader.GetInt64(0))));
   }

   /// <summary>Every sentence, in the order they were added, read as they're enumerated.</summary>
   public IEnumerable<ReservoirSentence> EverySentence() => _connection.RowsReadAsEnumerated("select id, text from sentence order by id", ReservoirSentenceIn);

   /// <summary>The analyses of those of the sentences with the ids <paramref name="ids"/> that have one, by the sentence's id.</summary>
   public IReadOnlyDictionary<long, ReservoirSentenceAnalysis> AnalysesOf(IEnumerable<long> ids)
   {
      AssertTheAnalysesAreOfTheCurrentFormat();
      return ids.Distinct()
                .Chunk(IdsPerQuery)
                .SelectMany(batch => _connection.Rows($"select sentence, analysis from sentence_analysis where sentence in ({string.Join(",", batch)})",
                                                      reader => KeyValuePair.Create(reader.GetInt64(0), AnalysisIn(reader, 1, sentenceId: reader.GetInt64(0)))))
                .ToDictionary();
   }

   /// <summary>The sentences with the ids <paramref name="ids"/>, by id.</summary>
   public IReadOnlyDictionary<long, StoredReservoirSentence> SentencesWithIds(IEnumerable<long> ids) =>
      ids.Distinct()
         .Chunk(IdsPerQuery)
         .SelectMany(batch => _connection.Rows($"select id, text, source, position, translation, audio_file, image_file, occurrences from sentence where id in ({string.Join(",", batch)})",
                                               StoredSentenceIn))
         .ToDictionary(it => it.Id);

   /// <summary>For each collection, how many works, sentences and new sentences the reservoir took from it: those no earlier work had.</summary>
   public IReadOnlyList<(string Collection, long Works, long Sentences, long NewSentences)> CollectionSummaries() =>
      _connection.Rows("select collection, count(*), sum(sentences), sum(new_sentences) from source group by collection order by sum(new_sentences) desc",
                       reader => (reader.GetString(0), reader.GetInt64(1), reader.GetInt64(2), reader.GetInt64(3)));

   /// <summary>Every source the reservoir's sentences came from, by id.</summary>
   public IReadOnlyDictionary<long, ReservoirSource> Sources() =>
      _connection.Rows("select id, collection, work, kind, location from source",
                       reader => KeyValuePair.Create(reader.GetInt64(0), new ReservoirSource(reader.GetString(1), reader.GetString(2), Enum.Parse<ReservoirSourceKind>(reader.GetString(3)), reader.GetString(4))))
                 .ToDictionary();

   public void Dispose() => _connection.Dispose();

   /// <summary>Adds <paramref name="source"/>, its sentences not counted yet, and gives its id.</summary>
   long AddTheSource(ReservoirSource source) =>
      _connection.ScalarAsLong("insert into source(collection, work, kind, location, sentences, new_sentences) values($collection, $work, $kind, $location, 0, 0) returning id",
                               ("$collection", source.Collection), ("$work", source.Work), ("$kind", source.Kind.ToString()), ("$location", source.Location));

   /// <summary>Adds <paramref name="sentences"/>, from the source with the id <paramref name="sourceId"/>, and says how many there were.</summary>
   long AddTheSentences(IEnumerable<SourceSentence> sentences, long sourceId)
   {
      using var add = _connection.Prepared(AddSentence, "$text", "$source", "$position", "$translation", "$audioFile", "$imageFile");
      var added = 0L;
      foreach(var sentence in sentences)
      {
         add.RunWith(sentence.Text.Text, sourceId, sentence.Position, sentence.Translation, sentence.AudioFile, sentence.ImageFile);
         added++;
      }

      return added;
   }

   /// <summary>The id of the sentence added last, or 0 when there's none: a new sentence's id is one more.</summary>
   long LastSentenceId() => _connection.ScalarAsLong("select coalesce(max(id), 0) from sentence");

   /// <summary>Records in the file, as its <c>user_version</c>, that the analyses it keeps are of the current format, <see cref="AnalysisFormat"/>.</summary>
   void MarkTheAnalysesAsOfTheCurrentFormat() => _connection.Execute($"pragma user_version = {AnalysisFormat}");

   /// <summary>The format of the analyses the reservoir keeps, as the file records it: <see cref="AnalysisFormat"/>, an older one, or a newer one, made by newer code.</summary>
   long FormatOfTheAnalysesKept => _connection.ScalarAsLong("pragma user_version");

   /// <summary>
   /// Asserts that the analyses kept are of <see cref="AnalysisFormat"/>, the only one this code reads and writes: analyses of an older format are<br/>
   /// to be made anew first, and those of a newer one, made by newer code, as when a checkout of an older commit opens the reservoir, are left alone.
   /// </summary>
   void AssertTheAnalysesAreOfTheCurrentFormat() =>
      Contract.State.Assert(FormatOfTheAnalysesKept == AnalysisFormat,
                            () => KeepsAnalysesOfAnOlderFormat
                                     ? "The reservoir's analyses are of an older format, which can't be read: analyze it anew first."
                                     : $"The reservoir's analyses are of format {FormatOfTheAnalysesKept}, made by newer code than this, which reads and writes format {AnalysisFormat} only.");

   /// <summary>Saves every number <paramref name="numbers"/> has given, those saved already left as they are.</summary>
   void SaveTheNumbers(ReservoirAnalysisNumbers numbers)
   {
      foreach(var vocab in numbers.Vocab)
         _connection.Execute("insert or ignore into vocab(number, note_id, question) values($number, $noteId, $question)", ("$number", vocab.Number), ("$noteId", vocab.NoteId), ("$question", vocab.Question));
      foreach(var reason in numbers.Reasons)
         _connection.Execute("insert or ignore into failure_reason(number, text) values($number, $text)", ("$number", reason.Number), ("$text", reason.Text));
   }

   static ReservoirSentence ReservoirSentenceIn(SqliteDataReader reader) => new(reader.GetInt64(0), reader.GetString(1));

   static StoredReservoirSentence StoredSentenceIn(SqliteDataReader reader) =>
      new(reader.GetInt64(0), reader.GetString(1), reader.GetInt64(2), reader.GetString(3), TextOrNullIn(reader, 4), TextOrNullIn(reader, 5), TextOrNullIn(reader, 6), reader.GetInt32(7));

   static string? TextOrNullIn(SqliteDataReader reader, int column) => reader.IsDBNull(column) ? null : reader.GetString(column);

   /// <summary>The analysis kept in the column <paramref name="column"/> that <paramref name="reader"/> is at, of the sentence <paramref name="sentenceId"/>.</summary>
   static ReservoirSentenceAnalysis AnalysisIn(SqliteDataReader reader, int column, long sentenceId) =>
      MemoryPackSerializer.Deserialize<ReservoirSentenceAnalysis>((byte[])reader.GetValue(column)) ?? throw new InvalidDataException($"Sentence {sentenceId}'s analysis is empty.");
}
