using System.Text.Json.Serialization;
using JAStudio.Core.Note.Vocabulary.Matching;

namespace JAStudio.Core.Storage.Dto;

/// <summary>What a set <see cref="RequireForbidFlagField"/> says, as the corpus stores it in <see cref="VocabMatchingDto.RequiredOrForbidden"/>.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<RequiredOrForbiddenDto>))]
public enum RequiredOrForbiddenDto
{
   [JsonStringEnumMemberName("required")] Required,
   [JsonStringEnumMemberName("forbidden")] Forbidden
}
