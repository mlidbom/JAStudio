using System;
using System.Collections.Generic;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Mcp.Tools.Memory.Heap;

// ReSharper disable InconsistentNaming

namespace JAStudio.Mcp.Specifications.Tools.Memory;

/// <remarks>
/// bug: flaky. It failed three times in full runs of the specifications, on 2026-11-17 and twice on 2026-11-18, each time in another of its cases,<br/>
///    and passed when run again. The third time the message was kept: it counted 95 copies of the repeated string where the heap holds 101, all<br/>
///    of them held by a static field, so none was collected. Likely cause, not verified: <c>HeapCensus</c> snapshots the live process while<br/>
///    other specifications run in it, and a garbage collection that one of them set off may be moving objects as the snapshot is taken. Holding<br/>
///    off collections while it's taken, such as with <c>GC.TryStartNoGCRegion</c>, would test that.
/// </remarks>
public class When_describing_what_the_heap_holds : McpServerSpecification
{
   const int ObjectsCounted = 500;
   const int CopiesOfTheString = 100;
   static readonly string TheRepeatedString = "a string the heap holds a hundred copies of " + new string('x', 300);

   /// <summary>Static, so the heap holds them once however many instances of this class xUnit has alive while one calls the tool.</summary>
   static readonly List<ObjectCountedOnTheHeap> TheObjectsCounted = [.. Enumerable.Range(0, ObjectsCounted).Select(_ => new ObjectCountedOnTheHeap())];

   /// <summary>Static, as <see cref="TheObjectsCounted"/>.</summary>
   static readonly List<string> TheCopiesOfTheString = [.. Enumerable.Range(0, CopiesOfTheString).Select(_ => new string(TheRepeatedString.AsSpan()))];

   readonly WhatTheHeapHolds _description;

   /// <summary>
   /// Makes the static fields before any instance is made, so before the tool is called: without a static constructor, they may be made as late<br/>
   /// as their first use.
   /// </summary>
   static When_describing_what_the_heap_holds() {}

   public When_describing_what_the_heap_holds() =>
      _description = ResultOf<WhatTheHeapHolds>(CallTool("describe_what_the_heap_holds", new Dictionary<string, object?> { ["typesNamed"] = nameof(ObjectCountedOnTheHeap), ["maxRepeatedStringsListed"] = 1000 }));

   [XF] public void it_lists_only_the_types_whose_name_contains_the_text_given() => _description.TypesTakingTheMost.Select(it => it.Type).Must().Satisfy(types => types.All(type => type.Contains(nameof(ObjectCountedOnTheHeap))));
   //bug: failed in one of three runs of the whole suite on 2026-11-16, and passed alone and in the next run of the whole suite. The quiet test
   //   output didn't say what it counted.
   /// <remarks>
   /// bug: flaky: it failed once in a full run of JAStudio.Mcp.Specifications (2026-11-17), and passed alone and in the next full run. It counts<br/>
   ///    objects on the heap the whole test process shares, so specifications running at the same time are the likely cause. The message wasn't kept.
   /// </remarks>
   [XF] public void it_counts_every_object_of_the_type() => _description.TypesTakingTheMost.Single(it => it.Type.EndsWith("+" + nameof(ObjectCountedOnTheHeap), StringComparison.Ordinal)).Objects.Must().Be((long)ObjectsCounted); //todo:compze: Must().Be compares a long with an int as boxed objects, so 1001L and 1001 differ: "Expected 1001 but got 1001". Compare numerically, or refuse to compile; then drop the cast here
   [XF] public void it_counts_the_whole_heap_whatever_types_it_lists() => _description.Objects.Must().BeGreaterThan(ObjectsCounted + CopiesOfTheString);
   [XF] public void it_counts_every_string_of_a_repeated_value_the_copies_and_the_original() => TheRepeatedStringAsDescribed().Copies.Must().Be(CopiesOfTheString + 1L); //todo:compze: Must().Be compares a long with an int as boxed objects, so 1001L and 1001 differ: "Expected 1001 but got 1001". Compare numerically, or refuse to compile; then drop the cast here
   [XF] public void it_shows_the_first_100_characters_of_a_longer_value() => TheRepeatedStringAsDescribed().Value.Must().Be(TheRepeatedString[..100] + "…");

   RepeatedString TheRepeatedStringAsDescribed() => _description.Strings.RepeatedStringsTakingTheMost.Single(it => it.Value.StartsWith("a string the heap holds", StringComparison.Ordinal));

   class ObjectCountedOnTheHeap;
}
