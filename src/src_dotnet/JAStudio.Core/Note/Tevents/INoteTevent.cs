using Compze.Teventive.Taggregates.Tevents;

namespace JAStudio.Core.Note.Tevents;

public interface INoteTevent : ITaggregateTevent;

public interface INoteTevent<out TTevent> : ITaggregateTevent<TTevent> where TTevent : INoteTevent;
