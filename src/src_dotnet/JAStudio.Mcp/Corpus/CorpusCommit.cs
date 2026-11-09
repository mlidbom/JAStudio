namespace JAStudio.Mcp.Corpus;

/// <summary>A commit of the corpus repository, by any name git knows it by: its hash, or a branch's name.</summary>
public record CorpusCommit(string Name)
{
   public override string ToString() => Name;
}
