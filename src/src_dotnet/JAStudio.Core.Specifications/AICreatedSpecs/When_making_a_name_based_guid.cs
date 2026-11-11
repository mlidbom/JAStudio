using System;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.SysUtils;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

/// <summary>Name-based GUIDs, RFC 4122's version 5 (<see cref="NameBasedGuid"/>), checked against the GUID Python's uuid.uuid5 gives python.org in the DNS namespace.</summary>
public class When_making_a_name_based_guid : IAIGeneratedSpec
{
   static readonly Guid DnsNamespace = new("6ba7b810-9dad-11d1-80b4-00c04fd430c8");

   [XF] public void it_is_the_one_rfc_4122_gives() => NameBasedGuid.Of(DnsNamespace, "python.org").Must().Be(new Guid("886313e1-3b8a-5372-9b90-0c9aee199e5d"));

   [XF] public void the_same_name_gives_the_same_guid() => NameBasedGuid.Of(DnsNamespace, "a name").Must().Be(NameBasedGuid.Of(DnsNamespace, "a name"));

   [XF] public void another_name_gives_another_guid() => NameBasedGuid.Of(DnsNamespace, "a name").Must().NotBe(NameBasedGuid.Of(DnsNamespace, "another name"));
}
