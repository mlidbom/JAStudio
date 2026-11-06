using JAStudio.Anki;
using JAStudio.Core;

namespace JAStudio.UI;

/// <summary>
/// Extension methods to access Anki-specific services through the shared service collection.
/// QueryBuilder lives in JAStudio.Anki (it builds Anki-format search queries),
/// so Core cannot reference it. UI creates it lazily here.
/// </summary>
static class AnkiServiceExtensions
{
   static QueryBuilder? _queryBuilder;

   public static QueryBuilder QueryBuilder(this TemporaryServiceCollection services)
   {
      return _queryBuilder ??= new QueryBuilder(services.ExternalNoteIdMap);
   }
}
