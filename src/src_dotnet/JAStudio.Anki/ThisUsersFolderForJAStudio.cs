using System;
using System.IO;

namespace JAStudio.Anki;

/// <summary>
/// The user's own folder for JAStudio's data, whichever checkout a JAStudio was started from: where the Anki add-on and JAStudio find each other,<br/>
/// by the two access files. On Windows it's <c>%LOCALAPPDATA%\JAStudio</c>. The add-on names the same folder by the same rule for each platform<br/>
/// (Python's <c>the_users_folder_for_jastudio</c>).
/// </summary>
static class ThisUsersFolderForJAStudio
{
   public static DirectoryInfo Folder => new(Path.Combine(TheUsersFolderForLocalData, "JAStudio"));

   /// <summary>The folder's path whether or not the folder exists yet: .NET gives an empty path for one that doesn't, unless told not to check.</summary>
   static string TheUsersFolderForLocalData => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify);
}
