using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Anki.ThePrimaryJAStudio;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Anki.ThePrimaryJAStudio;

/// <summary>
/// A JAStudio becoming Anki's primary JAStudio, the one the Anki add-on sends what goes from Anki to JAStudio, on the access file in a folder of the<br/>
/// specification's own (<see cref="PrimaryJAStudioAccessFile"/>). A second JAStudio is this process taking it a second time: a hold is the file<br/>
/// held open, which a second opening refuses whoever makes it.
/// </summary>
public class When_a_JAStudio_would_become_Ankis_primary_JAStudio : IDisposable
{
   const int TheCardServersPort = 54321;
   readonly DirectoryInfo _folder = Directory.CreateTempSubdirectory("jastudio-primary-jastudio-of-a-specification-");
   readonly PrimaryJAStudioAccessFile _accessFile;
   readonly List<BeingThePrimaryJAStudio> _eachBeingThePrimaryJAStudio = [];

   public When_a_JAStudio_would_become_Ankis_primary_JAStudio() => _accessFile = new PrimaryJAStudioAccessFile(_folder);

   (int Port, int ProcessId) WhatTheAccessFileHolds()
   {
      using var access = JsonDocument.Parse(File.ReadAllText(_accessFile.File.FullName));
      return (access.RootElement.GetProperty("port").GetInt32(), access.RootElement.GetProperty("processId").GetInt32());
   }

   /// <summary>A JAStudio whose card server listens on <paramref name="cardServersPort"/> becomes the primary JAStudio, unless another is.</summary>
   BeingThePrimaryJAStudio? TakeIt(int cardServersPort)
   {
      var beingIt = _accessFile.TakeUnlessAnotherJAStudioIsIt(cardServersPort);
      if(beingIt != null) _eachBeingThePrimaryJAStudio.Add(beingIt);
      return beingIt;
   }

   /// <summary>Ends each being the primary JAStudio that the specification began, then removes the folder: Windows refuses to delete a file held open.</summary>
   public void Dispose()
   {
      _eachBeingThePrimaryJAStudio.ForEach(it => it.Dispose());
      _folder.Delete(recursive: true);
   }

   public class while_no_JAStudio_is : When_a_JAStudio_would_become_Ankis_primary_JAStudio
   {
      readonly BeingThePrimaryJAStudio? _beingIt;
      public while_no_JAStudio_is() => _beingIt = TakeIt(TheCardServersPort);

      [XF] public void it_becomes_it() => _beingIt.Must().NotBeNull();
      [XF] public void the_access_file_holds_its_card_servers_port_and_its_process_id() => WhatTheAccessFileHolds().Must().Be((TheCardServersPort, Environment.ProcessId));

      public class and_then_stops_being_it : while_no_JAStudio_is
      {
         public and_then_stops_being_it() => _beingIt!.Dispose();

         [XF] public void the_access_file_is_gone() => _accessFile.File.Exists.Must().BeFalse();
         [XF] public void a_JAStudio_that_starts_next_becomes_it() => TakeIt(TheCardServersPort + 1).Must().NotBeNull();
      }

      public class and_another_JAStudio_would_become_it_too : while_no_JAStudio_is
      {
         readonly BeingThePrimaryJAStudio? _theOtherBeingIt;
         public and_another_JAStudio_would_become_it_too() => _theOtherBeingIt = TakeIt(TheCardServersPort + 1);

         [XF] public void the_other_one_does_not() => _theOtherBeingIt.Must().BeNull();
         [XF] public void the_access_file_still_holds_the_first_ones_port() => WhatTheAccessFileHolds().Port.Must().Be(TheCardServersPort);
      }
   }

   public class after_the_primary_JAStudio_ended_without_stopping : When_a_JAStudio_would_become_Ankis_primary_JAStudio
   {
      readonly BeingThePrimaryJAStudio? _beingIt;

      public after_the_primary_JAStudio_ended_without_stopping()
      {
         File.WriteAllText(_accessFile.File.FullName, JsonSerializer.Serialize(new { port = 1, processId = 1 }));
         _beingIt = TakeIt(TheCardServersPort);
      }

      [XF] public void it_becomes_it() => _beingIt.Must().NotBeNull();
      [XF] public void the_access_file_left_behind_is_replaced_by_its_own() => WhatTheAccessFileHolds().Must().Be((TheCardServersPort, Environment.ProcessId));
   }
}
