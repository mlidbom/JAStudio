using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Configuration;
using JAStudio.Core.LanguageServices;

namespace JAStudio.Core.Note;

public class KanjiNoteMnemonicMaker
{
   readonly JapaneseConfig _config;
   internal KanjiNoteMnemonicMaker(JapaneseConfig config) => _config = config;

   public string CreateDefaultMnemonic(KanjiNote kanjiNote)
   {
      var readingsMappings = _config.ReadingsMappingsDict;

      string CreateReadingsTag(string kanaReading)
      {
         var reading = KanaUtils.Romanize(kanaReading);
         var readingLength = reading.Length;

         if(readingsMappings.TryGetValue(reading, out var mappedRead))
         {
            var readTagCount = System.Text.RegularExpressions.Regex.Matches(mappedRead, "<read>").Count;
            return readTagCount <= 1
                      ? mappedRead
                      : $"<compound-reading>{mappedRead}</compound-reading>";
         }

         string TryCombineFragmentaryMatchesIntoOneReading()
         {
            // Build segments with mapped readings by start index
            var segmentsWithMappedReadingsByStartIndex = new List<List<string>>();
            for(var currentPosition = 0; currentPosition < readingLength; currentPosition++)
            {
               var candidates = new List<string>();
               for(var substringLength = currentPosition + 1; substringLength <= readingLength; substringLength++)
               {
                  var candidate = reading.Substring(currentPosition, substringLength - currentPosition);
                  if(readingsMappings.ContainsKey(candidate))
                  {
                     candidates.Add(candidate);
                  }
               }

               segmentsWithMappedReadingsByStartIndex.Add(candidates);
            }

            // Remove dead end paths
            void RemoveDeadEndPaths()
            {
               bool ReachedEndOfReading(int pathIndex, string match) => pathIndex + match.Length == readingLength;

               bool IsDeadEndPath(int pathIndex, string match) =>
                  pathIndex + match.Length < readingLength &&
                  !segmentsWithMappedReadingsByStartIndex[pathIndex + match.Length].Any();

               var matchesRemoved = true;
               while(matchesRemoved)
               {
                  matchesRemoved = false;
                  for(var pathIndex = 0; pathIndex < readingLength; pathIndex++)
                  {
                     var toRemove = segmentsWithMappedReadingsByStartIndex[pathIndex]
                                   .Where(match => !ReachedEndOfReading(pathIndex, match) && IsDeadEndPath(pathIndex, match))
                                   .ToList();

                     foreach(var match in toRemove)
                     {
                        matchesRemoved = true;
                        segmentsWithMappedReadingsByStartIndex[pathIndex].Remove(match);
                     }
                  }
               }
            }

            // Find shortest path, preferring long starting segments
            List<string> FindShortestPathPreferLongStartingSegments()
            {
               var shortestPathsToPosition = new Dictionary<int, List<string>> { { 0, [] } };

               // Sort candidates longest first so that the longest starting candidate will be preferred
               foreach(var candidates in segmentsWithMappedReadingsByStartIndex)
               {
                  candidates.Sort((a, b) => b.Length.CompareTo(a.Length));
               }

               for(var currentPosition = 0; currentPosition < readingLength; currentPosition++)
               {
                  if(!shortestPathsToPosition.ContainsKey(currentPosition))
                     continue;

                  foreach(var currentSegment in segmentsWithMappedReadingsByStartIndex[currentPosition])
                  {
                     var positionAfterSegment = currentPosition + currentSegment.Length;

                     if(!shortestPathsToPosition.ContainsKey(positionAfterSegment) ||
                        shortestPathsToPosition[currentPosition].Count < shortestPathsToPosition[positionAfterSegment].Count)
                     {
                        shortestPathsToPosition[positionAfterSegment] =
                           shortestPathsToPosition[currentPosition].Concat([currentSegment]).ToList();
                     }
                  }
               }

               return shortestPathsToPosition.TryGetValue(readingLength, out var path) ? path : [];
            }

            RemoveDeadEndPaths();
            var shortestPath = FindShortestPathPreferLongStartingSegments();

            if(!shortestPath.Any())
               return "";

            var combinedReading = string.Join("-", shortestPath.Select(fragment => readingsMappings[fragment]));
            return $"<compound-reading>{combinedReading}</compound-reading>";
         }

         var combined = TryCombineFragmentaryMatchesIntoOneReading();
         if(!string.IsNullOrEmpty(combined))
            return combined;

         return $"<read>{char.ToUpper(reading[0]) + reading.Substring(1)}</read>";
      }

      var radicalNames = kanjiNote.GetRadicalsNotes()
                                  .Select(rad => rad.PrimaryRadicalMeaning)
                                  .ToList();

      var radicalParts = string.Join(" ", radicalNames.Select(name => $"<rad>{name}</rad>"));
      var meaningPart = $"<kan>{kanjiNote.PrimaryMeaning}</kan>";
      var readingsParts = string.Join(" ", kanjiNote.Readings.PrimaryKana().Select(CreateReadingsTag));

      var mnemonic = $"{radicalParts} {meaningPart} {readingsParts} ...";
      return mnemonic.Trim();
   }
}
