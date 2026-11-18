using System;
using System.Linq;
using Compze.Tessaging.TessageBus;
using JAStudio.Core.Configuration;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Tevents;
using JAStudio.Core.Studying.StudyNotes;

namespace JAStudio.Core.UI.Web;

/// <summary>
/// The changes after which a page that shows a note no longer shows things as they are, so that it has to be shown anew. Such a page is the<br/>
/// page of one of the note's cards, which Anki shows, or the note's own page (<see cref="CardServerUrl"/>).
/// </summary>
/// <remarks>
/// The changes are: a unit of work changed or deleted the note, its study-note changed, such as its tags in Anki, the collection was loaded<br/>
/// again, which replaces every note, and a setting changed, such as how sentence breakdowns are shown.<br/>
/// A change to another note that the page shows something of, such as a vocab in a sentence's breakdown, isn't among them: it shows the next time<br/>
/// the page is shown.
/// </remarks>
public sealed class ChangesThatOutdateNotePages(IDynamicTeventObservers dynamicObservers)
{
   readonly IDynamicTeventObservers _dynamicObservers = dynamicObservers;

   /// <summary>
   /// Calls <paramref name="thePageIsOutdated"/> after each change that outdates a page showing the note with the id <paramref name="shownNote"/>,<br/>
   /// once it has committed, on a thread of the tevents' observation, until what this returns is disposed. A call may still start just after the<br/>
   /// disposing returns (<see cref="IDynamicTeventObservers.ObserveTevents"/>).
   /// </summary>
   public IDisposable Observe(NoteId shownNote, Action thePageIsOutdated) =>
      _dynamicObservers.ObserveTevents(observe => observe
                                            .ForTeventsOfEachUnitOfWork<INoteTevent>(changes =>
                                             {
                                                if(changes.Any(it => it.NoteId.Equals(shownNote))) thePageIsOutdated();
                                             })
                                            .ForTevent<IStudyNoteChanged>(it =>
                                             {
                                                if(it.NoteId.Equals(shownNote)) thePageIsOutdated();
                                             })
                                            .ForTevent<ICollectionReloaded>(_ => thePageIsOutdated())
                                            .ForTevent<IConfigurationChanged>(_ => thePageIsOutdated()));
}
