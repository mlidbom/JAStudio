# The UI specifications see the UI's internals

**The rule it breaks:** the specification conventions say never to make anything public just so tests can reach it,
and that a test that can't reach something through the container is a design signal: fix the design, not the
visibility.

**The exception:** `JAStudio.UI` grants `InternalsVisibleTo` to `JAStudio.UI.Specifications` (in `JAStudio.UI.csproj`).

**Why:** Magnus approved it on 2026-11-06.

- The rule is aimed at libraries, which should be specified through the API they publish. JAStudio is an application.
- In JAStudio, `public` already means something: reachable from Python, whose type stubs in `typings/` cover every public
  type. Making the workspace's view models public for the specifications would widen what the Python layer can reach,
  the opposite of keeping it thin.

**What stays the same:** the specifications are still black-box. They start the application's real services with an
empty collection (`SpecificationStartingWithAnEmptyCollection`), and open notes through the `WorkspaceViewModel` the way
the app does (`SpecificationUsingAWorkspace`), rather than building view models by hand.
