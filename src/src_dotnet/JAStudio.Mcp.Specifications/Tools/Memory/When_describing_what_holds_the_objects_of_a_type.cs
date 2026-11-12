using System.Collections.Generic;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Mcp.Tools.Memory.Heap;
using Xunit;

// ReSharper disable InconsistentNaming

namespace JAStudio.Mcp.Specifications.Tools.Memory;

[Collection<SpecificationsTakingACensusOfTheHeap>]
public class When_describing_what_holds_the_objects_of_a_type : McpServerSpecification
{
   const int HoldersOfOne = 300;
   const int ObjectsOnlyTheListHolds = 100;
   const int ObjectsTheListAndAHolderOfOneHold = 50;

   /// <summary>Static, so the heap holds them once however many instances of this class xUnit has alive while one calls the tool.</summary>
   static readonly HolderOfOne[] TheHoldersOfOne = [.. Enumerable.Range(0, HoldersOfOne).Select(_ => new HolderOfOne(new HeldObject()))];

   /// <summary>Static, as <see cref="TheHoldersOfOne"/>: its objects are held by its array, of which some <see cref="TheHoldersOfOne"/> hold too.</summary>
   static readonly List<HeldObject> TheList = [.. Enumerable.Range(0, ObjectsOnlyTheListHolds).Select(_ => new HeldObject()),
                                               .. TheHoldersOfOne.Take(ObjectsTheListAndAHolderOfOneHold).Select(it => it.Held)];

   readonly WhatHoldsTheObjectsOfAType _description;

   /// <summary>
   /// Makes the static fields before any instance is made, so before the tool is called: without a static constructor, they may be made as late<br/>
   /// as their first use.
   /// </summary>
   static When_describing_what_holds_the_objects_of_a_type() {}

   public When_describing_what_holds_the_objects_of_a_type() =>
      _description = ResultOf<WhatHoldsTheObjectsOfAType>(CallTool("describe_what_holds_the_objects_of_a_type", new Dictionary<string, object?> { ["type"] = typeof(HeldObject).FullName }));

   [XF] public void it_counts_every_object_of_the_type() => _description.Objects.Must().Be((long)HoldersOfOne + ObjectsOnlyTheListHolds); //todo:compze: Must().Be compares a long with an int as boxed objects, so 1001L and 1001 differ: "Expected 1001 but got 1001". Compare numerically, or refuse to compile; then drop the cast here
   [XF] public void it_lists_the_holder_type_whose_objects_hold_the_most_of_them_first() => _description.HolderTypesHoldingTheMost[0].Type.Must().Be(typeof(HolderOfOne).FullName);
   [XF] public void it_counts_every_object_of_a_holder_type_that_references_one() => TheHolderType<HolderOfOne>().Holders.Must().Be((long)HoldersOfOne); //todo:compze: Must().Be compares a long with an int as boxed objects, so 1001L and 1001 differ: "Expected 1001 but got 1001". Compare numerically, or refuse to compile; then drop the cast here
   [XF] public void it_counts_the_objects_a_holder_type_references() => TheHolderType<HolderOfOne>().ObjectsHeld.Must().Be((long)HoldersOfOne); //todo:compze: Must().Be compares a long with an int as boxed objects, so 1001L and 1001 differ: "Expected 1001 but got 1001". Compare numerically, or refuse to compile; then drop the cast here
   [XF] public void it_gives_a_holder_types_share_of_the_megabytes_of_the_type() => TheHolderType<HolderOfOne>().PercentOfTheMegabytes.Must().Be(75.0);
   [XF] public void a_lists_objects_are_held_by_its_array() => TheHolderType<HeldObject[]>().Holders.Must().Be(1L);
   [XF] public void an_object_referenced_by_objects_of_two_types_counts_with_each() => TheHolderType<HeldObject[]>().ObjectsHeld.Must().Be((long)TheList.Count); //todo:compze: Must().Be compares a long with an int as boxed objects, so 1001L and 1001 differ: "Expected 1001 but got 1001". Compare numerically, or refuse to compile; then drop the cast here
   [XF] public void it_counts_no_object_held_by_a_root_alone_when_every_one_is_referenced_by_an_object() => _description.ObjectsHeldByARootAlone.Must().Be(0L);

   HolderType TheHolderType<THolder>() => _description.HolderTypesHoldingTheMost.Single(it => it.Type == typeof(THolder).FullName);

   class HeldObject;

   record HolderOfOne(HeldObject Held);
}
