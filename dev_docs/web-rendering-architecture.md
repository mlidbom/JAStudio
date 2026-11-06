# Web Rendering Architecture: From String Interpolation to Blazor

## Status: Card Rendering Done for Vocab, Kanji and Sentences

**Remaining:**
- Card templates for mobile review still need to be designed and installed in Anki. The stubs in the repo aren't
  enough for that. See
  [Card Templates Installed in Anki](#card-templates-installed-in-anki-status-2026-11-05).
- The corpus browser (Phase 2) has not been started. The only Blazor pages are the three `/card/...` pages.

## Completed Work

### Infrastructure (Done)
- **CardServer** (`JAStudio.Web/CardServer.cs`): In-process Kestrel/Blazor Server on OS-assigned port
- **AppendingPrerenderer** (`JAStudio.Core/UI/Web/AppendingPrerenderer.cs`): Generic iframe renderer
  - Takes `Func<TNote, string cardTemplateName, string side, string>` — builds iframe URL
  - On question: returns front iframe, schedules background prerender of back iframe
  - On answer: appends prerendered back iframe to mustache HTML
  - Knows nothing about card types — fully generic
- **Python shim** (`dotnet_rendering_content_renderer_anki_shim.py`): Passes `card.template()["name"]` as `cardTemplateName` to .NET `Render()` method

### Vocab Cards (Done)
- `VocabCard.razor` — page at `/card/vocab/{Side}?NoteId=X&CardType=Y`
  - Reading front: shows question text
  - Listening front: renders nothing (empty body — Anki plays audio before hook fires)
  - Back (both types): full answer with all 20+ sub-components
  - Uses `card@CardType` for container class (dynamic `cardReading`/`cardListening`)
- `VocabNoteRenderer.cs` — single `RenderIframe` method, builds URL with `{side}`, `NoteId`, `CardType`
- 24 Blazor components in `Components/Vocab/` (VocabMeaning, VocabKanjiList, VocabSentencesList, etc.)
- Mustache templates simplified to audio-only stubs in the repo. These stubs are not yet installed in Anki; see
  [Card Templates Installed in Anki](#card-templates-installed-in-anki-status-2026-11-05).
  - `vocab.front.read.mustache`: `{{Q}}`
  - `vocab.front.listening.mustache`: `{{Audio_b}}{{Audio_g}}...`
  - `vocab.back.reading.mustache`: audio tags only
  - `vocab.back.listening.mustache`: `{{FrontSide}}`
  - `vocab.back.mustache`: **deleted** (was the shared back template with `##BLAZOR_IFRAME##`)

### Kanji Cards (Done)
- `KanjiCard.razor` — page at `/card/kanji/{Side}?NoteId=X&CardType=Y`
- `KanjiNoteRenderer.cs` — same pattern as vocab
- 6 Blazor components in `Components/Kanji/`

### Key Pattern: How Blazor Components Work
Components receive the note as a `[Parameter]`, access domain data directly, render themselves:
```razor
@if(KanjiListViewModel != null)
{
    <div id="kanji_list" class="page_section">
        @foreach(var kanji in KanjiListViewModel.KanjiList)
        {
            <div class="kanji_item @(string.Join(" ", kanji.Kanji.GetMetaTags()))">
                <span class="kanji_kanji clipboard">@kanji.Question()</span>
                ...
            </div>
        }
    </div>
}

@code {
    [Parameter, EditorRequired] public VocabNote Note { get; set; } = null!;
    [Parameter] public KanjiListViewModel? KanjiListViewModel { get; set; }
}
```

### Key Discovery: Anki Audio Behavior
Anki processes `{{Audio_*}}` tags and queues audio playback **before** the `card_will_show` hook fires. The hook only controls the visual HTML. So the iframe can fully replace the HTML without affecting audio playback. This means the front side always returns an iframe — even for listening cards where the iframe renders an empty page.

### Sentence Cards (Done — 2026-02-19, `2c62f00f5` "Migrate sentence display.")
- `SentenceCard.razor` is the page at `/card/sentence/{Side}?NoteId=X&CardType=Y&DisplayContext=Z`:
  - **Reading front:** shows the question.
  - **Listening front:** renders nothing; Anki plays the audio.
  - **Back:** the screenshot, question, answers, comments and the sentence breakdown.
- `SentenceNoteRenderer.cs` now returns an `AppendingPrerenderer`, the same pattern as vocab and kanji.
- The Blazor components live in `Components/Sentence/`:
  - `SentenceAnswer`, `SentenceComments`, `SentenceExpression`, `SentenceInvalidForDisplay`
  - `SentenceBreakdown`, `SentenceBreakdownEntry`, `SentenceBreakdownCompoundPart`, `SentenceBreakdownKanji`
  - `SentenceTokenTable`, `SentenceTokens`, `SentenceViewSettings`
- The old string-building renderers are deleted: `UdSentenceBreakdownRenderer`, `SentenceRenderer`,
  `QuestionRenderer`, `PreRenderingContentRenderer`.
- Screenshots and audio come from JAStudio's own media storage, not Anki's media folder. `CardServer` serves
  them through the `/media?name={filename}` endpoint, which looks the file up in `MediaFileIndex`.
- `SentenceExpression.razor` renders the `<wbr>` word-break hints.
- **The sentence reading is deliberately not shown** (Magnus, 2026-11-05). The old back template rendered it with
  Anki's `{{furigana::Reading}}`, and the port dropped it. The sentence audio covers pronunciation, and a reading
  that guesses is worse than useless.

## Card Templates Installed in Anki (status 2026-11-05)

**What Anki's templates still do, given the iframe rendering:**
- **Question side:** Anki's rendered HTML is thrown away and replaced by the iframe. The front template only
  decides whether the card exists (`{{Q}}` on reading cards, the audio fields on listening cards) and queues
  front-side audio.
- **Answer side:** the iframe is a fixed, full-window layer appended on top of Anki's rendered back HTML. Anki
  plays the back template's audio fields. The rest of that template renders invisibly underneath the iframe.

**What is actually installed:**
- The stub templates in `JAStudio.Web/Styles/notes/` (`{{Q}}` and audio fields only) are **not installed** in
  either the `Magnus` or the `Anki-Development` profile.
- Both profiles still have the old full templates. Their back templates reference 10 to 15 fields each, plus
  `{{Tags}}`.
- The invisible body therefore still loads images such as `{{Screenshot}}` and runs its `<script>`.
- `src/web/notes/` holds an older copy of the full templates. Nothing in the code or the build reads it; only
  `.gitignore` and the `.idea` project files mention `src/web/`.

**Next step (changed 2026-11-06):** basic review must work on mobile, where the add-on doesn't run and the card
is exactly what the template renders. So the stubs are not the target any more: they would show a mobile card
with no answer. The next step is to design templates that render the study-note fields well on a phone, while
desktop keeps replacing or covering them with the iframe, and to install those templates. That is also the
prerequisite for removing fields from those note types. See `corpus-data-model.md`, "The minimal Anki note type".

## Superseded Architecture Notes

(The rest of the original doc below is the original vision doc. Much of it is now implemented. Kept for reference.)

```
Kestrel web server (in-process via pythonnet)
  ├── /card/{noteType}/{side}?nid={id}   → Razor-rendered full HTML page
  ├── /media/audio/{path}                → audio files from our own storage
  ├── /media/images/{path}               → images from our own storage
  ├── /css/cards.css                     → stylesheets
  ├── /js/card.js                        → client-side JavaScript
  └── /browse/...                        → corpus browser (standalone UI)
```

### Key Decisions

#### 1. Complete HTML replacement via hook

The Anki hook receives the fully-rendered HTML *after* Anki has processed `{{Audio}}` and registered it for native playback. We replace the **entire** HTML string with our own content. Anki's audio auto-play continues to work because it was processed before the hook fired.

The Anki template becomes trivial:

```html
{{Audio}}
placeholder
```

Set once per note type. Never changed again.

#### 2. iframe for full document ownership

The hook returns an `<iframe>` pointing at our localhost server:

```python
def on_card_render(output, context):
    nid = context.note().id
    port = jastudio_server.port
    output.answer_text = (
        f'<iframe src="http://localhost:{port}/card/sentence/back?nid={nid}" '
        f'style="width:100%;height:100%;border:none"></iframe>'
    )
```

The iframe gives us:

- **Full `<html>` ownership** — our own `<head>`, stylesheets, scripts
- **Complete CSS isolation** — no bleed from Anki's styles
- **No script conflicts** — independent JavaScript context
- **Full Blazor support** — normal page lifecycle, no adapter needed

#### 3. Kestrel in-process via pythonnet

We already load .NET into Anki's Python process via pythonnet. Kestrel is just another .NET library. It starts in the same process on addon initialization, binding to an OS-assigned port.

```csharp
public class JaStudioServer
{
    WebApplication? _app;

    public int Port { get; private set; }

    public void Start()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddRazorPages();
        // Register existing domain services

        _app = builder.Build();
        _app.MapGet("/card/{noteType}/{side}", CardEndpoint.Render);
        _app.MapGet("/media/{**path}", MediaEndpoint.Serve);
        _app.MapStaticAssets();

        _app.RunAsync("http://localhost:0"); // OS picks a free port
        Port = new Uri(_app.Urls.First()).Port;
    }
}
```

#### 4. Razor templates replace string-building renderers

Each card type gets a single `.cshtml` file. Shared fragments become Razor partials.

**Before** (17 renderer classes, ~1500 lines of string interpolation):

```csharp
html.AppendLine($$$"""
    <li class="sentenceVocabEntry depth1 word_priority_very_high {{{match.MetaTagsString}}}">
        <div class="sentenceVocabEntryDiv">
            {{{BuildInvalidForDisplaySpan(match)}}}
            <audio src="{{{match.AudioPath}}}"></audio><a class="play-button"></a>
            <span class="vocabQuestion clipboard">{{{match.ParsedForm}}}</span>
            {{{vocabFormHtml}}}
            ...
""");
```

**After** (Razor `.cshtml` files with shared partials):

```html
@* SentenceBack.cshtml — the full page *@
@model SentenceCardModel

<!DOCTYPE html>
<html>
<head>
    <link href="http://localhost:@Model.Port/css/cards.css" rel="stylesheet" />
</head>
<body>
    <div id="container1">
        <div id="container2" class="@Model.Tags card@Model.CardNumber">
            <div class="topSection">
                <div class="image">@Html.Raw(Model.Screenshot)</div>
                <div id="answerSection">
                    @if (Model.HasUserQuestion)
                    {
                        <div class="expression user clipboard">@Model.UserQuestion</div>
                    }
                    <div class="expression clipboard @(Model.HasUserQuestion ? "overridden" : "")">
                        @Model.SourceQuestion
                    </div>
                    <div class="reading">@Html.Raw(Model.FuriganaReading)</div>

                    @* ... answers ... *@
                </div>
            </div>

            @await IncludeAsync("_SentenceBreakdown", Model.Breakdown)
        </div>
    </div>
    <script src="http://localhost:@Model.Port/js/card.js"></script>
</body>
</html>
```

```html
@* _KanjiList.cshtml — shared partial, used by sentence and vocab cards *@
@model KanjiListModel

@if (Model.KanjiList.Count > 0)
{
    <div class="vocab_kanji_list">
        @foreach (var kanji in Model.KanjiList)
        {
            <div class="kanji_item @string.Join(" ", kanji.MetaTags)">
                <div class="kanji_main">
                    <span class="kanji_kanji clipboard">@kanji.Question</span>
                    <span class="kanji_answer">@kanji.Answer</span>
                    <span class="kanji_readings">@kanji.Readings</span>
                </div>
                @if (kanji.ShowMnemonic)
                {
                    <div class="kanji_mnemonic">@kanji.Mnemonic</div>
                }
            </div>
        }
    </div>
}
```

The C# renderer classes disappear entirely. There is no intermediary layer that builds models and passes them to templates. The Blazor/Razor components inject domain services directly and render themselves — it's a normal Blazor application:

```razor
@* SentenceBreakdown.razor *@
@inject VocabCollection Vocab
@inject Settings Settings

<div class="breakdown page_section">
    <div class="page_section_title">Sentence breakdown # <ViewSettings Config="Config" /></div>
    <ul class="sentenceVocabList userExtra depth1">
        @foreach (var match in _analysis.DisplayedMatches)
        {
            <VocabEntry Match="match" />
        }
    </ul>
</div>

@code {
    [Parameter] public SentenceNote Note { get; set; }
    SentenceViewModel _analysis;

    protected override void OnParametersSet()
    {
        _analysis = new SentenceViewModel(Note, Settings, Vocab);
    }
}
```

#### 5. Media independence

Audio and images are served by Kestrel from our own storage — not the Anki media folder. This is critical for the corpus browser (50K+ vocab, hundreds of thousands of sentences — the vast majority not in any user's Anki collection).

```html
<audio src="http://localhost:PORT/media/audio/vocab_12345.mp3"></audio>
<img src="http://localhost:PORT/media/images/screenshot_67890.jpg" />
```

Audio playback is JS-driven (click handlers), independent of Anki's `[sound:]` system.

#### 6. Prerendering strategy

The expensive work is domain analysis — tokenizing Japanese text, dictionary lookups, compound detection, kanji resolution. Turning the completed analysis into HTML via Blazor components is trivially fast (sub-millisecond).

So prerendering means caching the **domain object**, not pre-rendered HTML. When Anki shows the question, we compute the analysis and cache it. When the answer iframe loads, the Blazor page grabs the cached analysis and renders instantly.

```csharp
// On question display (triggered via pythonnet)
_analysisCache[noteId] = new SentenceViewModel(note, settings, vocab);

// Blazor page — grabs the cached analysis, renders instantly
@inject AnalysisCache Cache

@code {
    [Parameter] public long NoteId { get; set; }

    protected override void OnParametersSet()
    {
        _analysis = Cache.GetOrCompute(NoteId);
    }
}
```

No need to hook into Blazor's rendering pipeline or hold pre-rendered HTML strings. The current `PreRenderingContentRenderer<TNote>` with its parallel task dictionary is completely eliminated.

Note: Anki replaces the page entirely when revealing the answer (question HTML is discarded), so any client-side state from the question view is lost. Prerendering must happen server-side at the domain level.

## Phased Implementation

### Phase 1: Kestrel + Razor (static SSR)

**Goal**: Replace string interpolation with Razor templates. Serve media independently.

1. Add ASP.NET / Kestrel packages to the solution
2. Create `JaStudioServer` — minimal Kestrel startup, OS-assigned port
3. Start the server on addon initialization (Python side)
4. Create Razor templates for one card type (sentence) + shared partials
5. Modify the Python hook to return an iframe pointing at the server
6. Serve CSS/JS from the server instead of Anki media folder
7. Serve audio/images from our own storage
8. Migrate remaining card types (vocab, kanji)
9. Remove old string-building renderer classes
10. Remove mustache template files (replaced by Razor)

**Outcome**: All 17 renderer classes eliminated. Static Anki templates. Media independence. Corpus browser foundation.

### Phase 2: Corpus Browser

**Goal**: Standalone web UI for browsing the full corpus outside Anki.

1. Add routes: `/browse/vocab`, `/browse/sentences`, `/browse/kanji`
2. Reuse the same Razor partial templates from card rendering
3. Add search/filter/pagination
4. Navigate to `localhost:PORT/browse/...` in any browser

**Outcome**: Full-featured search/browse/preview for 50K+ items, sharing components with Anki card rendering.

### Phase 3: Blazor Interactive Islands

**Goal**: Make specific card components interactive (live dictionary lookup, inline editing, etc.).

Since we own the full `<html>` inside the iframe, Blazor Server works without any page lifecycle compromises.

1. Add Blazor Server packages
2. Convert specific components to interactive Blazor components:
   - Click-to-expand vocabulary entries
   - Live dictionary lookup on word click
   - Inline editing of user fields
   - Interactive conjugation tables
   - Pitch accent visualization on hover
3. Static SSR for most of the card, Blazor circuits only for interactive islands

**Outcome**: Each Anki card becomes a live application with direct in-process access to the full domain model (dictionary, tokenizer, analysis engine, 50K vocab database).

## Performance Considerations

### iframe load time

The iframe loads a page from localhost. Expected latency:

| Step | Time |
|---|---|
| HTTP round-trip to localhost | < 1ms |
| Razor rendering (if not pre-rendered) | ~5-50ms (depending on card complexity) |
| Browser parse + paint | ~5-10ms |
| **Total** | **~10-60ms** |

Pre-rendered pages (question-time prerendering) will be at the low end. Even worst-case, well under 100ms — imperceptible during card transitions.

### Audio playback

Click-to-play audio served from localhost has negligible latency:

| Step | Time |
|---|---|
| HTTP GET 100KB audio from localhost | ~2-5ms |
| Browser audio decode | ~5-10ms |
| **Total cold play** | **~8-15ms** |

Human perception threshold for audio delay: ~20-30ms. Comfortably below.

### Blazor Server (Phase 3)

SignalR over localhost: < 1ms round-trip. Interactive updates are effectively instant.
Each card creates a new SignalR circuit (~few KB server memory). Circuits are disposed when the card changes.

## What This Replaces

| Current | New |
|---|---|
| 17 renderer classes with string interpolation | Gone — Blazor components inject domain services and render themselves |
| `PreRenderingContentRenderer<TNote>` tag dictionary | Gone — standard Blazor page lifecycle |
| Mustache templates (`.mustache` files) | Static 2-line Anki templates (set once) |
| `##TAG##` fragment replacement pipeline | Gone — it's just a Blazor application |
| `__magnus_js.js` in Anki media folder | JS served by Kestrel |
| CSS in Anki media folder | CSS served by Kestrel |
| Audio/images must be in Anki media folder | Media served from our own storage |
| No corpus browsing outside Anki | Standalone browser UI on same server |
| Static HTML cards | A full Blazor application served via localhost |
