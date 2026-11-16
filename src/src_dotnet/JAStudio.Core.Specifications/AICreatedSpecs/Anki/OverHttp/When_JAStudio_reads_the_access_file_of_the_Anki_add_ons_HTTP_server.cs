using System;
using System.IO;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Anki.AnkisFunctions.OverHttp;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Anki.OverHttp;

/// <summary>
/// JAStudio learns how to reach the HTTP server the Anki add-on runs for it from the access file (<see cref="AnkisHttpServerAccessFile"/>), which<br/>
/// the add-on writes when it starts serving and removes when it stops. Each specification has the add-on's own serving, Python's<br/>
/// <c>ServingJAStudio</c>, of a copy of the test collection (<see cref="AnkiServingACopyOfTheTestCollection"/>), whose 253 vocab the Anki search<br/>
/// <c>note:_Vocab</c> finds.
/// </summary>
[NCrunch.Framework.RequiresCapability("PYTHON_VENV")]
public class When_JAStudio_reads_the_access_file_of_the_Anki_add_ons_HTTP_server : IDisposable, IAIGeneratedSpec
{
   readonly AnkiServingACopyOfTheTestCollection _anki = AnkisHttpServersForSpecifications.StartOne();

   public virtual void Dispose() => _anki.Dispose();

   public class while_the_add_on_serves : When_JAStudio_reads_the_access_file_of_the_Anki_add_ons_HTTP_server
   {
      readonly AnkiOverHttp _ankiReachedWithTheAccessTheFileGives;

      public while_the_add_on_serves() => _ankiReachedWithTheAccessTheFileGives = new AnkiOverHttp(_anki.AccessFile);

      public override void Dispose()
      {
         _ankiReachedWithTheAccessTheFileGives.Dispose();
         base.Dispose();
      }

      [XF] public void the_server_answers_a_call_made_with_the_access_the_file_gives() => _ankiReachedWithTheAccessTheFileGives.Notes.Find("note:_Vocab").Count.Must().Be(253);
   }

   public class once_the_add_on_has_stopped_serving : When_JAStudio_reads_the_access_file_of_the_Anki_add_ons_HTTP_server
   {
      public once_the_add_on_has_stopped_serving() => _anki.StopsServing();

      [XF] public void the_file_gives_no_access() => _anki.AccessFile.AccessOrNullWhileNoAnkiServes().Must().BeNull();
   }

   public class from_a_folder_in_which_no_add_on_has_written_one : When_JAStudio_reads_the_access_file_of_the_Anki_add_ons_HTTP_server
   {
      readonly AnkisHttpServerAccessFile _neverWritten;

      public from_a_folder_in_which_no_add_on_has_written_one() =>
         _neverWritten = new AnkisHttpServerAccessFile(new FileInfo(Path.Combine(_anki.Folder.FullName, "anki-http-server-access.json")));

      [XF] public void the_file_gives_no_access() => _neverWritten.AccessOrNullWhileNoAnkiServes().Must().BeNull();
   }

   public class of_this_user : When_JAStudio_reads_the_access_file_of_the_Anki_add_ons_HTTP_server
   {
      [XF] public void it_is_the_file_the_add_on_names_as_this_users() =>
         AnkisHttpServerAccessFile.OfThisUser.File.FullName.Must().Be(AnkisHttpServersForSpecifications.TheAccessFileOfThisUserAsTheAddOnNamesIt().FullName);
   }
}
