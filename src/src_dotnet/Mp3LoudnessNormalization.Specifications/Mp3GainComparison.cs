using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xunit;

namespace Mp3LoudnessNormalization.Specifications;

/// <summary>
/// MP3 files from JAStudio's media layer, <c>src/jas_database/media</c>, normalized by <see cref="Mp3File"/> and by mp3gain (<c>/r /c /s s</c>),<br/>
/// and compared byte for byte.<br/>
/// It needs mp3gain.exe, which the <c>MP3GAIN</c> environment variable names, and the media layer, and skips the specification calling it without them.<br/>
/// mp3gain works on copies, in a folder of its own under the temporary folder.
/// </summary>
sealed class Mp3GainComparison
{
   Mp3GainComparison(IReadOnlyList<ComparedFile> files) => Files = files;

   /// <summary>Every file compared.</summary>
   public IReadOnlyList<ComparedFile> Files { get; }

   /// <summary>The files <see cref="Mp3File"/> normalized differently from mp3gain.</summary>
   public IEnumerable<ComparedFile> FilesNormalizedDifferently => Files.Where(it => !it.CameOutTheSame);

   /// <summary>Compares up to <paramref name="filesPerFolder"/> files, spread evenly, from each folder of each repository of the media layer.</summary>
   public static Mp3GainComparison OfTheMediaLayer(int filesPerFolder)
   {
      var mp3Gain = Environment.GetEnvironmentVariable("MP3GAIN");
      if(mp3Gain is null || !File.Exists(mp3Gain)) Assert.Skip("Set the MP3GAIN environment variable to mp3gain.exe's path to compare Mp3LoudnessNormalization with mp3gain.");
      var media = Path.Combine(RepositoryRoot(), "src", "jas_database", "media");
      if(!Directory.Exists(media)) Assert.Skip($"The media layer isn't at {media}.");

      var work = Path.Combine(Path.GetTempPath(), "jastudio-mp3gain-comparison");
      if(Directory.Exists(work)) Directory.Delete(work, recursive: true);
      Directory.CreateDirectory(work);
      var originals = FilesFrom(media, filesPerFolder);
      var copies = Enumerable.Range(0, originals.Count).Select(index => Path.Combine(work, $"f{index:D6}.mp3")).ToArray();
      for(var index = 0; index < originals.Count; index++) File.Copy(originals[index], copies[index]);
      foreach(var batch in copies.Chunk(200)) Run(mp3Gain, work, ["/r", "/c", "/s", "s", "/q", ..batch.Select(Path.GetFileName).OfType<string>()]);

      return new Mp3GainComparison([..originals.Select((original, index) => ComparedFile.Of(Path.GetRelativePath(media, original), File.ReadAllBytes(original), File.ReadAllBytes(copies[index])))]);
   }

   static List<string> FilesFrom(string media, int filesPerFolder) =>
   [
      ..Directory.GetDirectories(media)
                 .Where(repository => !Path.GetFileName(repository).StartsWith('.'))
                 .SelectMany(Directory.GetDirectories)
                 .Order(StringComparer.Ordinal)
                 .SelectMany(folder =>
                  {
                     var all = Directory.GetFiles(folder, "*.mp3", SearchOption.AllDirectories).Order(StringComparer.Ordinal).ToArray();
                     var stride = Math.Max(1, all.Length / filesPerFolder);
                     return all.Where((_, index) => index % stride == 0).Take(filesPerFolder);
                  })
   ];

   static string RepositoryRoot()
   {
      for(var folder = new DirectoryInfo(AppContext.BaseDirectory); folder is not null; folder = folder.Parent)
         if(File.Exists(Path.Combine(folder.FullName, "JAStudio.slnx"))) return folder.FullName;
      throw new InvalidOperationException($"No folder above {AppContext.BaseDirectory} holds JAStudio.slnx.");
   }

   static void Run(string program, string folder, IEnumerable<string> arguments)
   {
      var start = new ProcessStartInfo(program) { WorkingDirectory = folder, RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
      foreach(var argument in arguments) start.ArgumentList.Add(argument);
      using var process = Process.Start(start) ?? throw new InvalidOperationException($"{program} didn't start.");
      var errors = process.StandardError.ReadToEndAsync();
      process.StandardOutput.ReadToEnd();
      process.WaitForExit();
      if(process.ExitCode != 0) throw new InvalidOperationException($"{program} failed with {process.ExitCode}: {errors.Result}");
   }

   /// <summary>One file of the media layer, as <see cref="Mp3File"/> and mp3gain normalized it.</summary>
   public sealed class ComparedFile
   {
      ComparedFile(string pathInTheMediaLayer, bool cameOutTheSame, bool startsWithAFrameWhoseAudioDataIsMissing)
      {
         PathInTheMediaLayer = pathInTheMediaLayer;
         CameOutTheSame = cameOutTheSame;
         StartsWithAFrameWhoseAudioDataIsMissing = startsWithAFrameWhoseAudioDataIsMissing;
      }

      public static ComparedFile Of(string pathInTheMediaLayer, byte[] original, byte[] normalizedByMp3Gain) =>
         new(pathInTheMediaLayer,
             new Mp3File(original).NormalizedToTheReplayGainReference().Bytes().AsSpan().SequenceEqual(normalizedByMp3Gain),
             Mp3StartInspection.StartsWithAFrameWhoseAudioDataIsMissing(original));

      public string PathInTheMediaLayer { get; }

      /// <summary>Whether <see cref="Mp3File"/> normalized it byte for byte as mp3gain did.</summary>
      public bool CameOutTheSame { get; }

      /// <summary>Whether its first audio frame's data is in a frame before it, which isn't there: the known exception. See <see cref="Mp3StartInspection"/>.</summary>
      public bool StartsWithAFrameWhoseAudioDataIsMissing { get; }

      public override string ToString() => PathInTheMediaLayer;
   }
}
