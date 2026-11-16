using System.IO;
using System.Text.Json;
using JAStudio.Core.Specifications.Fixtures;

namespace JAStudio.Core.Specifications.AICreatedSpecs.Anki.OverHttp;

/// <summary>
/// The Anki add-on's HTTP servers for JAStudio that the specifications call: each serves a copy of the test collection of its own, opened with<br/>
/// Anki's own library, without Anki's window (<see cref="AnkiServingACopyOfTheTestCollection"/>). One Python process runs them all: the script<br/>
/// <c>src/tests/jastudio_tests/anki_http/serving_copies_of_the_test_collection.py</c>, which says what commands it takes.
/// </summary>
static class AnkisHttpServersForSpecifications
{
   static readonly PythonScriptOfTheAddOnsTests TheScriptServingTheCopies = new("jastudio_tests", "anki_http", "serving_copies_of_the_test_collection.py");

   /// <summary>Starts a server on a new copy of the test collection, which serves until it's disposed.</summary>
   public static AnkiServingACopyOfTheTestCollection StartOne() => new(AnswerTo("start"));

   /// <summary>The access file a JAStudio of this user reads, as the add-on names it: Python's <c>the_access_file_of_this_user</c>.</summary>
   public static FileInfo TheAccessFileOfThisUserAsTheAddOnNamesIt() => new(AnswerTo("access-file-of-this-user").GetProperty("accessFile").GetString() ?? "");

   /// <summary>Python's answer to <paramref name="command"/>, one of those <c>serving_copies_of_the_test_collection.py</c> takes.</summary>
   internal static JsonElement AnswerTo(string command) => TheScriptServingTheCopies.AnswerTo(command);
}
