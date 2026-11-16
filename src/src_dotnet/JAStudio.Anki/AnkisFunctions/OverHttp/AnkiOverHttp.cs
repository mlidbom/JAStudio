using System;
using System.IO;
using System.Text.Json;
using Compze.Threading;

namespace JAStudio.Anki.AnkisFunctions.OverHttp;

/// <summary>
/// The functions JAStudio calls in Anki, as a JAStudio that is a program of its own reaches them: over HTTP, in the server the Anki add-on runs<br/>
/// for it (Python's <c>HttpServerForJAStudio</c>, in <c>jastudio.anki_http</c>). Each group of them is one of its properties.
/// </summary>
/// <remarks>
/// Anki is a program beside JAStudio, which starts, ends and changes profile on its own. So each call looks for the Anki that serves now, in the<br/>
/// access file (<see cref="AnkisHttpServerAccessFile"/>), and JAStudio works with one of Anki's collections at a time<br/>
/// (<see cref="CollectionJAStudioWorksWith"/>), which every call names.
/// </remarks>
/// <remarks>
/// A call fails with <see cref="NoAnkiServesJAStudioException"/> while no Anki serves, with <see cref="AnkiRefusedTheCallException"/> while Anki<br/>
/// can't let JAStudio use its collection, or has another one open, and with <see cref="CallToAnkiFailedException"/> when the function fails in<br/>
/// Anki. <see cref="WhyItCannotBeCalledNow"/> tells beforehand.
/// </remarks>
public sealed partial class AnkiOverHttp : IDisposable
{
   readonly AnkisHttpServerAccessFile _accessFile;
   readonly IMonitor _monitor = IMonitor.New();
   AnkisHttpServer? _theServerLastFound;
   FileInfo? _collectionJAStudioWorksWith;

   /// <param name="accessFile">Where the Anki add-on says how to reach its HTTP server, for as long as it serves.</param>
   public AnkiOverHttp(AnkisHttpServerAccessFile accessFile)
   {
      _accessFile = accessFile;
      Notes = new ItsNotes(this);
      Tags = new ItsTags(this);
      Media = new ItsMedia(this);
      Database = new ItsDatabase(this);
      Windows = new ItsWindows(this);
   }

   public IAnkisNotes Notes { get; }
   public IAnkisTags Tags { get; }
   public IAnkisMedia Media { get; }
   public IAnkisDatabase Database { get; }
   public IAnkisWindows Windows { get; }

   /// <summary>
   /// The file of the Anki collection JAStudio works with, whose notes' ids it holds: the one Anki had open at JAStudio's first call, or when<br/>
   /// JAStudio last took the one Anki has open (<see cref="TakeTheCollectionAnkiHasOpen"/>). Null until the first call.
   /// </summary>
   /// <remarks>An Anki profile has a collection of its own, so when another profile opens in Anki, Anki has another collection open than this one, and refuses JAStudio's calls.</remarks>
   public FileInfo? CollectionJAStudioWorksWith => _monitor.Locked(() => _collectionJAStudioWorksWith);

   /// <summary>
   /// Why a call would fail right now, in words for a person, or null when Anki would answer it: no Anki serves JAStudio, Anki can't let JAStudio<br/>
   /// use its collection, Anki has another collection open than the one JAStudio works with (<see cref="TheOtherCollectionAnkiHasOpen"/>), or the<br/>
   /// add-on speaks another version of the contract. It asks Anki each time.
   /// </summary>
   public string? WhyItCannotBeCalledNow => WhatHindersACallNow()?.Why;

   /// <summary>
   /// The collection Anki has open, when it isn't the one JAStudio works with: Anki then refuses JAStudio's calls until JAStudio takes that one<br/>
   /// instead (<see cref="TakeTheCollectionAnkiHasOpen"/>). Null when Anki has the collection JAStudio works with open, before JAStudio works<br/>
   /// with one, and while Anki can't say which it has open. It asks Anki each time.
   /// </summary>
   public FileInfo? TheOtherCollectionAnkiHasOpen => WhatHindersACallNow()?.TheOtherCollectionAnkiHasOpen;

   /// <summary>
   /// Has JAStudio work with the collection Anki has open from now on, whichever it worked with before. The study-notes JAStudio holds are then<br/>
   /// another collection's, until it reads them again.
   /// </summary>
   public void TakeTheCollectionAnkiHasOpen()
   {
      var collectionAnkiHasOpen = TheServerThatServesNow().TheCollectionAnkiHasOpen();
      _monitor.Locked(() => _collectionJAStudioWorksWith = collectionAnkiHasOpen);
   }

   public void Dispose() => _monitor.Locked(() => _theServerLastFound?.Dispose());

   /// <summary>What would make a call fail right now, found by asking Anki which collection it has open, or null when Anki would answer a call.</summary>
   HindranceToACall? WhatHindersACallNow()
   {
      try
      {
         var collectionAnkiHasOpen = TheServerThatServesNow().TheCollectionAnkiHasOpen();
         return CollectionJAStudioWorksWith is {} collection && !AreTheSameFile(collection, collectionAnkiHasOpen)
                   ? new HindranceToACall($"Anki has another collection open than the one JAStudio works with: {collectionAnkiHasOpen.FullName}", collectionAnkiHasOpen)
                   : null;
      }
      catch(NoAnkiServesJAStudioException noAnkiServes)
      {
         return new HindranceToACall(noAnkiServes.Why);
      }
      catch(AnkiRefusedTheCallException refused)
      {
         return new HindranceToACall(refused.Why);
      }
      catch(AnkiSpeaksAnotherContractException anotherContract)
      {
         return new HindranceToACall(anotherContract.Message);
      }
   }

   /// <summary>Why a call would fail, in words for a person, and the collection Anki has open when that is why.</summary>
   sealed record HindranceToACall(string Why, FileInfo? TheOtherCollectionAnkiHasOpen = null);

   /// <summary>Calls the add-on's function <paramref name="function"/> with <paramref name="arguments"/>, by the names the function has them by, and returns its answer.</summary>
   TResult Call<TResult>(string function, params (string Name, object Value)[] arguments) =>
      ResultOfTheCall(function, arguments, result => result.Deserialize<TResult>() ?? throw new InvalidOperationException($"{function} answered with null."));

   /// <summary>Calls the add-on's function <paramref name="function"/>, which answers with nothing, with <paramref name="arguments"/>, by the names the function has them by.</summary>
   void Call(string function, params (string Name, object Value)[] arguments) => ResultOfTheCall(function, arguments, _ => 0);

   /// <summary>
   /// Calls the add-on's function <paramref name="function"/>, whose answer is JSON it didn't build itself, such as what a SELECT built, and returns<br/>
   /// that JSON as the text it is.
   /// </summary>
   string CallForItsJsonText(string function, params (string Name, object Value)[] arguments) => ResultOfTheCall(function, arguments, result => result.GetRawText());

   TResult ResultOfTheCall<TResult>(string function, (string Name, object Value)[] arguments, Func<JsonElement, TResult> read)
   {
      var server = TheServerThatServesNow();
      return server.ResultOfTheCall(function, TheCollectionJAStudioWorksWithOrTheOneOpenIn(server), arguments, read);
   }

   /// <summary>
   /// The server of the Anki that serves JAStudio now, as the access file names it. The port and the token are new each time an Anki starts<br/>
   /// serving, so the file is read for each call.
   /// </summary>
   AnkisHttpServer TheServerThatServesNow()
   {
      var access = _accessFile.AccessOrNullWhileNoAnkiServes() ?? throw new NoAnkiServesJAStudioException("Anki isn't running, or has no profile open.");
      return _monitor.Locked(() =>
      {
         if(_theServerLastFound?.Access == access) return _theServerLastFound;

         _theServerLastFound?.Dispose();
         return _theServerLastFound = new AnkisHttpServer(access);
      });
   }

   /// <summary>The collection JAStudio works with. At JAStudio's first call it has none yet, and takes the one Anki has open in <paramref name="server"/>.</summary>
   FileInfo TheCollectionJAStudioWorksWithOrTheOneOpenIn(AnkisHttpServer server)
   {
      if(CollectionJAStudioWorksWith is {} collection) return collection;

      var collectionAnkiHasOpen = server.TheCollectionAnkiHasOpen();
      return _monitor.Locked(() => _collectionJAStudioWorksWith ??= collectionAnkiHasOpen);
   }

   static bool AreTheSameFile(FileInfo file, FileInfo otherFile) =>
      string.Equals(Path.GetFullPath(file.FullName), Path.GetFullPath(otherFile.FullName), OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
}
