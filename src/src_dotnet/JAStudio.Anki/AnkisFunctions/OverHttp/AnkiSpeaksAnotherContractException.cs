using System;

namespace JAStudio.Anki.AnkisFunctions.OverHttp;

/// <summary>
/// The Anki add-on and this JAStudio speak different versions of the contract between them: the functions JAStudio calls in Anki, their arguments<br/>
/// and their answers. They refuse each other until one of them is updated, since a call could otherwise do something else than JAStudio means.
/// </summary>
public sealed class AnkiSpeaksAnotherContractException(int versionOfTheAddOn, int versionOfJAStudio)
   : Exception($"The Anki add-on speaks version {versionOfTheAddOn} of its contract with JAStudio, and this JAStudio speaks version {versionOfJAStudio}. Update the older of the two.");
