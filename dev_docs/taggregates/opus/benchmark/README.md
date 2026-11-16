# The benchmark of the combined prototypes

`generate_combined.py` writes a C# program that measures the same note-like taggregate in three styles, plus a plain
object graph of the same shape as the baseline:

- **registered:** tevent appliers registered per instance, as in Compze.Teventive 0.6.0-alpha;
- **declared:** tevent routing declared once per class, loaded through `LoadFromHistory` from one creation tevent;
- **snapshot:** the same declared note, loaded through `LoadFromSnapshot`.

For each it measures the bytes a loaded note retains, the bytes a load allocates, and the same after publishing once on
every component and committing. With `--time` it also times loading 80,000 notes in each style.

It's jastudio-rep-opus's `generate_both.py` (in Compze, `dev_docs/taggregate-improvements/opus/benchmark/`) with the
snapshot style added by taggregates-opus.

## Running it

In a folder outside both repositories, whose `Directory.Build.props` would apply to it: copy `generate_combined.py`
there with this project file, `TaggregateMemoryCombined.csproj`. It's kept here as text, so that nothing builds it as
part of JAStudio.

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net11.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>disable</ImplicitUsings>
    <ServerGarbageCollection>false</ServerGarbageCollection>
    <ConcurrentGarbageCollection>false</ConcurrentGarbageCollection>
    <ManagePackageVersionsCentrally>false</ManagePackageVersionsCentrally>
    <CompzeBin>C:\Dev\Compze.worktrees\taggregates-opus-combined\src\Compze.Teventive\bin\Debug\net11.0</CompzeBin>
  </PropertyGroup>
  <ItemGroup>
    <Reference Include="$(CompzeBin)\Compze.*.dll" />
  </ItemGroup>
</Project>
```

Build the Compze branch `taggregates-opus/combined` first (`dotnet build Compze.AllProjects.slnx` in its worktree), then:

```
python generate_combined.py 15 3 Program.cs
dotnet run -c Release
```

The first argument is the number of components, the second the properties per component.
