using System.Collections.Generic;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Mcp.Discovery;
using ModelContextProtocol;
using static Compze.Must.MustActions;

// ReSharper disable InconsistentNaming

namespace JAStudio.Mcp.Specifications.Gateway;

/// <summary>
/// Claude's MCP gateway finds the running JAStudios, connects to them by name, and offers their tools under the connection's name, following each<br/>
/// JAStudio connected to as it stops and runs again. Here the JAStudio is the standalone JAStudio on the stand-in source checkout <c>checkout</c>,<br/>
/// which is in no git repository, so its name is <c>standalone-checkout</c>.
/// </summary>
public class When_a_JAStudio_runs : GatewaySpecification
{
   const string ItsName = "standalone-checkout";

   [XF] public void it_is_listed_by_the_name_of_its_host_and_its_source_checkout() =>
      ReportOfTheRunningJAStudios().Must().Contain($"- {ItsName}: the standalone JAStudio on the corpus checkout {TheJAStudio.CorpusCheckout.FullName}");

   [XF] public void none_of_its_tools_are_offered() => ToolsOfferedUnder(ItsName).Must().BeEmpty();

   [XF] public void connecting_to_a_name_no_running_JAStudio_has_fails() => Invoking(() => ConnectTo("anki-nowhere")).Must().Throw<McpException>();

   public class and_another_JAStudio_with_the_same_host_runs_on_another_corpus_checkout_in_the_same_source_checkout : When_a_JAStudio_runs
   {
      public and_another_JAStudio_with_the_same_host_runs_on_another_corpus_checkout_in_the_same_source_checkout() =>
         RunningJAStudios.Announce(RunningJAStudio.InThisProcess(JAStudioHost.Standalone, ServerUrl, AddOnFolder.CreateSubdirectory("another_corpus"), AddOnFolder));

      [XF] public void it_is_listed_by_the_same_name_with_2_after_it() => ReportOfTheRunningJAStudios().Must().Contain($"- {ItsName}-2: the standalone JAStudio");
   }

   public class and_the_gateway_connects_to_it_by_its_name : When_a_JAStudio_runs
   {
      public and_the_gateway_connects_to_it_by_its_name() => ConnectTo(ItsName);

      [XF] public void every_one_of_its_tools_is_offered_under_the_connections_name() => ToolsOfferedUnder(ItsName).Must().SequenceEqual(TheJAStudiosTools());

      [XF] public void a_call_of_one_of_its_tools_through_the_gateway_is_answered_by_the_JAStudio() =>
         TextOf(CallThroughTheGateway($"{ItsName}__describe_collection")).Must().Be(TextOf(CallTool("describe_collection", new Dictionary<string, object?>())));

      [XF] public void it_is_listed_as_connected() => ReportOfTheRunningJAStudios().Must().Contain($"- {ItsName}, connected: the standalone JAStudio");

      [XF] public void connecting_to_it_again_answers_that_it_is_connected_already() => ConnectTo(ItsName).Must().Contain("Connected to it already");

      public class and_the_JAStudio_stops : and_the_gateway_connects_to_it_by_its_name
      {
         public and_the_JAStudio_stops()
         {
            ItsAnnouncement.Dispose();
            WaitUntilTheGatewayHasFollowed(() => ToolsOfferedUnder(ItsName).Count == 0);
         }

         [XF] public void its_tools_are_withdrawn() => ToolsOfferedUnder(ItsName).Must().BeEmpty();

         [XF] public void it_is_listed_as_connected_to_but_not_running() =>
            ReportOfTheRunningJAStudios().Must().Satisfy(it => it.Contains("Connected to, but not running now:") && it.Contains($"- {ItsName}: the standalone JAStudio"));

         public class and_runs_again : and_the_JAStudio_stops
         {
            public and_runs_again()
            {
               ItsAnnouncement = RunningJAStudios.Announce(TheJAStudio);
               WaitUntilTheGatewayHasFollowed(() => ToolsOfferedUnder(ItsName).Count > 0);
            }

            [XF] public void every_one_of_its_tools_is_offered_again() => ToolsOfferedUnder(ItsName).Must().SequenceEqual(TheJAStudiosTools());
         }
      }

      public class and_disconnects_from_it : and_the_gateway_connects_to_it_by_its_name
      {
         public and_disconnects_from_it() => DisconnectFrom(ItsName);

         [XF] public void its_tools_are_withdrawn() => ToolsOfferedUnder(ItsName).Must().BeEmpty();

         [XF] public void it_is_listed_as_not_connected() => ReportOfTheRunningJAStudios().Must().Contain($"- {ItsName}: the standalone JAStudio");
      }
   }

   public class and_the_gateway_connects_with_no_name_given : When_a_JAStudio_runs
   {
      public and_the_gateway_connects_with_no_name_given() => ConnectTo(null);

      [XF] public void every_tool_of_the_JAStudio_on_the_corpus_checkout_of_the_gateways_source_checkout_is_offered() =>
         ToolsOfferedUnder(ItsName).Must().SequenceEqual(TheJAStudiosTools());
   }
}
