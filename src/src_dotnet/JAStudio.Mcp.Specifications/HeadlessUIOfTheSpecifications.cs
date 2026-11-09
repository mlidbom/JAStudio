using System;
using JAStudio.Mcp.UI;

namespace JAStudio.Mcp.Specifications;

/// <summary>
/// The headless UI the specifications of a JAStudio with a headless UI share, started the first time one asks for it, since a process starts<br/>
/// one at most. It renders windows as the headless JAStudio's does, but its web pages it only records (<see cref="WebPageImages"/>), and an action<br/>
/// waits for no work in the background, since the specifications' windows do none.
/// </summary>
static class HeadlessUIOfTheSpecifications
{
   public static readonly WebPageImagesThatRecordThePagesAsked WebPageImages = new();

   static readonly Lazy<HeadlessUI> TheHeadlessUI = new(() => HeadlessUI.Start(WebPageImages, howLongBackgroundWorkIsWaitedFor: TimeSpan.Zero));

   public static HeadlessUI Started => TheHeadlessUI.Value;
}
