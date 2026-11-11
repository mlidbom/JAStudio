using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JAStudio.Core.Reservoir.Analysis;
using JAStudio.Core.Reservoir.Sources;
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

   readonly SqliteConnection _connection;

   ReservoirFile(SqliteConnection connection) => _connection = connection;

   /// <summary>The reservoir in <paramref name="file"/>, created empty when there's no such file.</summary>
   public static ReservoirFile OpenOrCreate(FileInfo file)
   {
      var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = file.FullName, Pooling = false, DefaultTimeout = SecondsToWaitForAnotherWriter }.ToString());
      connection.Open();
      Execute(connection, Schema);
      return new ReservoirFile(connection);
   }

   /// <summary>Whether the reservoir has the sentences of <paramref name="source"/>'s work: a work is added once, and found by its collection and name.</summary>
   public bool Has(ReservoirSource source) =>
      Scalar("select count(*) from source where collection = $collection and work = $work", ("$collection", source.Collection), ("$work", source.Work)) > 0;

   /// <summary>Adds <paramref name="work"/>'s sentences, those the reservoir has already counting as found once more.</summary>
   public WorkAddedToTheReservoir Add(SourceWork work)
   {
      using var transaction = _connection.BeginTransaction(deferred: false);
      var lastSentenceBefore = Scalar("select coalesce(max(id), 0) from sentence");
      var sourceId = Scalar("insert into source(collection, work, kind, location, sentences, new_sentences) values($collection, $work, $kind, $location, 0, 0) returning id",
                            ("$collection", work.Source.Collection), ("$work", work.Source.Work), ("$kind", work.Source.Kind.ToString()), ("$location", work.Source.Location));

      using var add = _connection.CreateCommand();
      add.CommandText = AddSentence;
      var parameters = new[] { "$text", "$source", "$position", "$translation", "$audioFile", "$imageFile" };
      foreach(var name in parameters) add.Parameters.Add(new SqliteParameter { ParameterName = name });
      add.Prepare();

      var sentences = 0L;
      foreach(var sentence in work.Sentences)
      {
         SetValues(sentence.Text.Text, sourceId, sentence.Position, sentence.Translation, sentence.AudioFile, sentence.ImageFile);
         add.ExecuteNonQuery();
         sentences++;
      }

      var newSentences = Scalar("select coalesce(max(id), 0) from sentence") - lastSentenceBefore;
      ExecuteWith("update source set sentences = $sentences, new_sentences = $newSentences where id = $id", ("$sentences", sentences), ("$newSentences", newSentences), ("$id", sourceId));
      transaction.Commit();
      return new WorkAddedToTheReservoir(work.Source, sentences, newSentences);

      void SetValues(params object?[] values)
      {
         for(var index = 0; index < values.Length; index++) add.Parameters[index].Value = values[index] ?? DBNull.Value;
      }
   }

   /// <summary>How many distinct sentences the reservoir holds.</summary>
   public long SentenceCount => Scalar("select count(*) from sentence");

   /// <summary>Up to <paramref name="count"/> sentences with no analysis yet, in the order they were added, from the first added after <paramref name="afterSentenceId"/>.</summary>
   public IReadOnlyList<(long Id, string Text)> UnanalyzedSentences(long afterSentenceId, int count)
   {
      using var command = _connection.CreateCommand();
      command.CommandText = """
                            select sentence.id, sentence.text from sentence left join sentence_analysis on sentence_analysis.sentence = sentence.id
                            where sentence_analysis.sentence is null and sentence.id > $after order by sentence.id limit $count
                            """;
      command.Parameters.AddWithValue("$after", afterSentenceId);
      command.Parameters.AddWithValue("$count", count);
      using var reader = command.ExecuteReader();
      var sentences = new List<(long, string)>();
      while(reader.Read()) sentences.Add((reader.GetInt64(0), reader.GetString(1)));
      return sentences;
   }

   /// <summary>Saves <paramref name="analyses"/>, each of the sentence with its id, and every number <paramref name="numbers"/> has given.</summary>
   public void SaveAnalyses(IReadOnlyList<(long SentenceId, ReservoirSentenceAnalysis Analysis)> analyses, ReservoirAnalysisNumbers numbers)
   {
      using var transaction = _connection.BeginTransaction(deferred: false);
      using var save = _connection.CreateCommand();
      save.CommandText = "insert or replace into sentence_analysis(sentence, analysis) values($sentence, $analysis)";
      var sentence = save.Parameters.Add(new SqliteParameter { ParameterName = "$sentence" });
      var analysis = save.Parameters.Add(new SqliteParameter { ParameterName = "$analysis" });
      save.Prepare();
      foreach(var (sentenceId, sentenceAnalysis) in analyses)
      {
         sentence.Value = sentenceId;
         analysis.Value = MemoryPackSerializer.Serialize(sentenceAnalysis);
         save.ExecuteNonQuery();
      }

      foreach(var vocab in numbers.Vocab)
         ExecuteWith("insert or ignore into vocab(number, note_id, question) values($number, $noteId, $question)", ("$number", vocab.Number), ("$noteId", vocab.NoteId), ("$question", vocab.Question));
      foreach(var (reason, number) in numbers.Reasons)
         ExecuteWith("insert or ignore into failure_reason(number, text) values($number, $text)", ("$number", number), ("$text", reason));
      transaction.Commit();
   }

   /// <summary>The numbers the reservoir's analyses have given so far, to go on numbering from.</summary>
   public ReservoirAnalysisNumbers AnalysisNumbers()
   {
      var vocab = Rows("select number, note_id, question from vocab", reader => new NumberedVocab(reader.GetInt32(0), reader.GetString(1), reader.GetString(2)));
      var reasons = Rows("select text, number from failure_reason", reader => KeyValuePair.Create(reader.GetString(0), reader.GetInt32(1)));
      return new ReservoirAnalysisNumbers(vocab, reasons);
   }

   /// <summary>Every analyzed sentence, with its analysis, read as they're enumerated.</summary>
   public IEnumerable<AnalyzedReservoirSentence> AnalyzedSentences()
   {
      using var command = _connection.CreateCommand();
      command.CommandText = """
                            select sentence.id, sentence.text, sentence.source, sentence.occurrences, sentence.audio_file is not null, sentence_analysis.analysis
                            from sentence join sentence_analysis on sentence_analysis.sentence = sentence.id
                            """;
      using var reader = command.ExecuteReader();
      while(reader.Read())
         yield return new AnalyzedReservoirSentence(reader.GetInt64(0), reader.GetString(1), reader.GetInt64(2), reader.GetInt32(3), reader.GetBoolean(4),
                                                    AnalysisIn(reader, 5, sentenceId: reader.GetInt64(0)));
   }

   /// <summary>The id and text of every sentence, read as they're enumerated.</summary>
   public IEnumerable<(long Id, string Text)> EverySentenceText()
   {
      using var command = _connection.CreateCommand();
      command.CommandText = "select id, text from sentence order by id";
      using var reader = command.ExecuteReader();
      while(reader.Read()) yield return (reader.GetInt64(0), reader.GetString(1));
   }

   /// <summary>The analyses of those of the sentences with the ids <paramref name="ids"/> that have one, by the sentence's id.</summary>
   public IReadOnlyDictionary<long, ReservoirSentenceAnalysis> AnalysesOf(IEnumerable<long> ids) =>
      ids.Distinct()
         .Chunk(500)
         .SelectMany(batch => Rows($"select sentence, analysis from sentence_analysis where sentence in ({string.Join(",", batch)})",
                                   reader => KeyValuePair.Create(reader.GetInt64(0), AnalysisIn(reader, 1, sentenceId: reader.GetInt64(0)))))
         .ToDictionary();

   /// <summary>The sentences with the ids <paramref name="ids"/>, by id.</summary>
   public IReadOnlyDictionary<long, StoredReservoirSentence> SentencesWithIds(IEnumerable<long> ids)
   {
      var sentences = new Dictionary<long, StoredReservoirSentence>();
      foreach(var batch in ids.Distinct().Chunk(500))
         foreach(var sentence in Rows($"select id, text, source, position, translation, audio_file, image_file, occurrences from sentence where id in ({string.Join(",", batch)})",
                                      reader => new StoredReservoirSentence(reader.GetInt64(0), reader.GetString(1), reader.GetInt64(2), reader.GetString(3),
                                                                            NullOr(reader, 4), NullOr(reader, 5), NullOr(reader, 6), reader.GetInt32(7))))
            sentences[sentence.Id] = sentence;
      return sentences;

      static string? NullOr(SqliteDataReader reader, int column) => reader.IsDBNull(column) ? null : reader.GetString(column);
   }

   /// <summary>For each collection, how many works, sentences and new sentences the reservoir took from it: those no earlier work had.</summary>
   public IReadOnlyList<(string Collection, long Works, long Sentences, long NewSentences)> CollectionSummaries() =>
      Rows("select collection, count(*), sum(sentences), sum(new_sentences) from source group by collection order by sum(new_sentences) desc",
           reader => (reader.GetString(0), reader.GetInt64(1), reader.GetInt64(2), reader.GetInt64(3)));

   /// <summary>Every source the reservoir's sentences came from, by id.</summary>
   public IReadOnlyDictionary<long, ReservoirSource> Sources() =>
      Rows("select id, collection, work, kind, location from source",
           reader => KeyValuePair.Create(reader.GetInt64(0), new ReservoirSource(reader.GetString(1), reader.GetString(2), Enum.Parse<ReservoirSourceKind>(reader.GetString(3)), reader.GetString(4))))
        .ToDictionary();

   /// <summary>The analysis kept in the column <paramref name=column/> that <paramref name=reader/> is at, of the sentence <paramref name=sentenceId/>.</summary>
   static ReservoirSentenceAnalysis AnalysisIn(SqliteDataReader reader, int column, long sentenceId) =>
      MemoryPackSerializer.Deserialize<ReservoirSentenceAnalysis>((byte[])reader.GetValue(column)) ?? throw new InvalidDataException($"Sentence {sentenceId}'s analysis is empty.");

   List<T> Rows<T>(string select, Func<SqliteDataReader, T> row)
   {
      using var command = _connection.CreateCommand();
      command.CommandText = select;
      using var reader = command.ExecuteReader();
      var rows = new List<T>();
      while(reader.Read()) rows.Add(row(reader));
      return rows;
   }

   public void Dispose() => _connection.Dispose();

   long Scalar(string select, params (string name, object value)[] parameters)
   {
      using var command = _connection.CreateCommand();
      command.CommandText = select;
      foreach(var (name, value) in parameters) command.Parameters.AddWithValue(name, value);
      return Convert.ToInt64(command.ExecuteScalar(), System.Globalization.CultureInfo.InvariantCulture);
   }

   void ExecuteWith(string statement, params (string name, object value)[] parameters)
   {
      using var command = _connection.CreateCommand();
      command.CommandText = statement;
      foreach(var (name, value) in parameters) command.Parameters.AddWithValue(name, value);
      command.ExecuteNonQuery();
   }

   static void Execute(SqliteConnection connection, string statements)
   {
      using var command = connection.CreateCommand();
      command.CommandText = statements;
      command.ExecuteNonQuery();
   }
}
