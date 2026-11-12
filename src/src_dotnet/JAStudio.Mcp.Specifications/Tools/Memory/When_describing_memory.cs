using System.Collections.Generic;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Mcp.Tools.Memory;

// ReSharper disable InconsistentNaming

namespace JAStudio.Mcp.Specifications.Tools.Memory;

public class When_describing_memory : McpServerSpecification
{
   protected MemoryDescription DescribeMemory(bool alsoAfterCollectingGarbage) =>
      ResultOf<MemoryDescription>(CallTool("describe_memory", new Dictionary<string, object?> { ["alsoAfterCollectingGarbage"] = alsoAfterCollectingGarbage }));

   public class without_collecting_garbage : When_describing_memory
   {
      readonly MemoryDescription _description;
      public without_collecting_garbage() => _description = DescribeMemory(alsoAfterCollectingGarbage: false);

      [XF] public void it_reports_the_garbage_collectors_mode_of_this_process() => _description.GarbageCollectorMode.Must().Be(TheGarbageCollector.Mode);
      [XF] public void it_reports_the_garbage_collectors_settings() => _description.GarbageCollectorConfiguration.Must().NotBeEmpty();
      [XF] public void it_reports_the_processs_working_set() => _description.MemoryUse.WorkingSetMegabytes.Must().BeGreaterThan(0);
      [XF] public void it_reports_no_memory_use_after_collecting_garbage() => _description.MemoryUseAfterCollectingGarbage.Must().BeNull();
   }

   public class also_after_collecting_garbage : When_describing_memory
   {
      readonly MemoryDescription _description;
      public also_after_collecting_garbage() => _description = DescribeMemory(alsoAfterCollectingGarbage: true);

      [XF] public void it_reports_the_memory_use_after_collecting_garbage() => _description.MemoryUseAfterCollectingGarbage.Must().NotBeNull();
   }
}
