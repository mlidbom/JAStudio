using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Compze.DependencyInjection.Abstractions;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Specifications.Fixtures;
using Xunit;

// ReSharper disable InconsistentNaming

namespace JAStudio.Web.Specifications;

/// <summary>
/// The CardServer serves a page. The page loads its own script, <c>card.js</c>, by a path versioned by the script's content, so that a web view that<br/>
/// cached an older <c>card.js</c> loads the new one (<see cref="WebRootFiles"/>).
/// </summary>
public class When_a_card_page_is_served : IAsyncLifetime
{
   static readonly Regex VersionedPathOfTheScript = new(@"/js/card\.js\?v=[0-9A-F]{16}");

   readonly CollectionFactory.AppScope _appScope = CollectionFactory.InjectCollectionWithSelectData(DataNeeded.Kanji);
   readonly CardServer _server;
   readonly HttpClient _http = new();
   string _page = "";
   string _script = "";

   public When_a_card_page_is_served() => _server = new CardServer(_appScope.CoreApp.Services.Resolve<IRootResolver>(), new NoteLinkOpenerThatRecordsRequests());

   public async ValueTask InitializeAsync()
   {
      _server.Start();
      _page = await _http.GetStringAsync($"{_server.BaseUrl}/", TestContext.Current.CancellationToken);
      _script = await _http.GetStringAsync($"{_server.BaseUrl}{VersionedPathOfTheScript.Match(_page).Value}", TestContext.Current.CancellationToken);
   }

   public async ValueTask DisposeAsync()
   {
      _http.Dispose();
      await _server.StopAsync();
      _appScope.Dispose();
   }

   [XF] public void the_page_loads_card_js_by_a_path_versioned_by_its_content() => _page.Must().Satisfy(it => VersionedPathOfTheScript.IsMatch(it));
   [XF] public void the_versioned_path_serves_the_script() => _script.Must().Contain("postToWorkspace");
}
