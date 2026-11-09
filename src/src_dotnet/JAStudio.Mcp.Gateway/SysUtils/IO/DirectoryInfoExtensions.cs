using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace JAStudio.Mcp.Gateway.SysUtils.IO;

/// <summary>What the gateway does with whole folders: copying the build output it runs the headless JAStudio from, and deleting it.</summary>
static class DirectoryInfoExtensions
{
   /// <summary>How many times <see cref="DeleteOnceItsFilesAreReleasedAsync"/> tries to delete a folder whose files are still locked, before it gives up.</summary>
   const int DeleteAttemptsAtMost = 25;

   /// <summary>How long <see cref="DeleteOnceItsFilesAreReleasedAsync"/> waits between its attempts to delete a folder whose files are still locked.</summary>
   static readonly TimeSpan TimeBetweenDeleteAttempts = TimeSpan.FromMilliseconds(200);

   extension(DirectoryInfo @this)
   {
      /// <summary>Copies every file in this folder and its subfolders to the same place in <paramref name="destination"/>, creating the folders there as needed.</summary>
      internal void CopyEverythingTo(DirectoryInfo destination)
      {
         foreach(var folder in @this.EnumerateDirectories("*", SearchOption.AllDirectories).Prepend(@this))
         {
            var folderCopy = Directory.CreateDirectory(Path.Combine(destination.FullName, Path.GetRelativePath(@this.FullName, folder.FullName)));
            foreach(var file in folder.EnumerateFiles()) file.CopyTo(Path.Combine(folderCopy.FullName, file.Name));
         }
      }

      /// <summary>
      /// Deletes this folder and everything in it, if it exists, waiting up to a few seconds for the files in it to be released by a process<br/>
      /// that has just exited.
      /// </summary>
      /// <remarks>
      /// On Windows, the files a process has loaded stay locked for a moment after it has exited: deleting the headless JAStudio's runtime folder<br/>
      /// right after stopping it failed with "Access to the path 'Compze.Abstractions.dll' is denied" (2026-11-09), and succeeded a moment later.
      /// </remarks>
      internal async Task DeleteOnceItsFilesAreReleasedAsync()
      {
         for(var attempt = 1;; attempt++)
         {
            @this.Refresh();
            if(!@this.Exists) return;

            try
            {
               @this.Delete(recursive: true);
               return;
            }
            catch(Exception exception) when(exception is IOException or UnauthorizedAccessException && attempt < DeleteAttemptsAtMost)
            {
               await Task.Delay(TimeBetweenDeleteAttempts);
            }
         }
      }
   }
}
