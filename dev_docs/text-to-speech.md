# Text-to-speech — Design

> **Status: draft (2026-11-07).** The direction is agreed with Magnus. Details are settled as each increment needs
> them. Facts about the providers come from their documentation, and are verified against the real services once
> the accounts exist.

## Why

Today the TTS audio comes from HyperTTS, an Anki add-on, through HyperTTS Pro, a vocab.ai subscription that proxies
the TTS services. HyperTTS writes the audio into the Anki note, a save in Anki brings the audio field into the corpus
(`corpus-data-model.md`, "Anki's role and its consequences"), and the file only plays in JAStudio after a media
import (`media-storage-and-import.md`).

JAStudio calling the TTS services itself:
- **gives the very best voices,** the ones that pronounce Japanese most correctly and naturally, including newer
  ones than those tried through HyperTTS, such as Azure's HD voices and Google's Chirp 3 HD voices. Paying for them
  is fine (Magnus, 2026-11-07);
- **puts the audio straight into the media layer,** with no media import before it plays;
- **removes a reason for Anki,** whose role is shrinking to reviewing and importing.

**Cost.** It's almost certainly cheaper than HyperTTS Pro, whose entry plan is $5 a month for 250,000 characters.
Azure's free tier includes 0.5 million neural characters a month. Paid neural voices cost about $15 per million
characters, and HD voices about $22, by third-party sources. Google's Neural2 costs about $16 per million, and
Chirp 3 HD about $30. A vocab question is a few characters, and a sentence a few dozen, so even several tries for
each of a few hundred notes a month comes to cents.

## What HyperTTS does today

Read from its configuration in the `Magnus` profile on 2026-11-07:
- **It runs through HyperTTS Pro,** so there are no Azure or Google accounts of Magnus's own.
- **The enabled automatic rules:** Vocab-Azure-Daichi, which writes `Audio_TTS`; Sentence-Azure-Daichi, which writes
  `Audio Sentence`; and the kanji primary readings, which are going.
- **The text spoken** is the `Q` field, with HTML removed and `〜` and `|` deleted.
- **The manual presets** also use the Azure neural voices Nanami, Mayu, Aoi, Shiori, Keita and Naoki, Google's
  Neural2 voices, and Forvo. Forvo is recordings by native speakers, not TTS, and it writes `Audio_g`.

## Decisions (Magnus, 2026-11-07)

- **Only the very best voices,** meaning those that pronounce Japanese most correctly and naturally most of the
  time, not the best audio encoding. The file format only needs to be good enough. Paying for them is fine.
- **The audio is kept in the format the tts-provider sends.** By ear (Magnus): Google's Neural2 voices sound fine,
  and sound best overall too; Chirp 3 HD's 32 kbit/s is borderline; Azure's is excellent.
- **Microsoft and Google from the start, built for more providers.** Google often gets right what Microsoft doesn't.
- **Forvo is left out of the first iteration.** It's rarely helpful.
- **Generation is manual:** Magnus listens to the results and picks. Nothing is generated when a note is created.
- **The keys live in a settings file in the user's profile,** outside both repositories:
  `%APPDATA%\JAStudio\text-to-speech.json`.
- **JAStudio isn't Windows-only,** so the dialog's audio playback mustn't make it so. See [To do](#to-do).

## Terms

- **tts-provider** — a TTS service JAStudio calls itself: Azure Speech or Google Cloud Text-to-Speech
  (`ITextToSpeechProvider`).
- **voice** — one of a tts-provider's voices, such as Azure's `ja-JP-DaichiNeural` (`TextToSpeechVoice`).
- **synthesized speech** — the audio a voice generated for a text, not yet kept (`SynthesizedSpeech`).
- **keeping** a synthesized speech — storing it in the media layer and making it the note's audio.

## The tts-providers

Both are called over HTTPS with `HttpClient`, with no SDK. Each lists its Japanese voices and speaks a text with
one of them, as MP3. A refused request throws a `TextToSpeechRequestFailedException` that names the tts-provider
and the HTTP status. The code is in `JAStudio.TextToSpeech`. Its specs run against a fake of each web service
(`FakeTextToSpeechService`), which records what was sent.

There are no types mirroring the services' JSON: a request body is SSML or an anonymous object shaped like the
JSON, and a response is read through `JsonElement`, naming only the fields used. Types used only by the serializer
would be reported as never used.

### Azure Speech

- **Speaking:** `POST https://{region}.tts.speech.microsoft.com/cognitiveservices/v1`, with the resource key in
  `Ocp-Apim-Subscription-Key`, SSML as the body (`application/ssml+xml`), and
  `X-Microsoft-OutputFormat: audio-48khz-192kbitrate-mono-mp3`, its highest-quality MP3. The body of a successful
  response is the audio.
- **The voices:** `GET https://{region}.tts.speech.microsoft.com/cognitiveservices/voices/list`. Each voice's
  `ShortName` is what the SSML names, and only the `ja-JP` locale is kept.
- **The best voices:**
  - the standard neural voices, such as `ja-JP-DaichiNeural`;
  - the DragonHD voices `ja-JP-Masaru:DragonHDLatestNeural` and `ja-JP-Nanami:DragonHDLatestNeural`, listed as
    generally available;
  - Dragon HD Omni, an HD version of nearly every released voice, named like `de-DE-Conrad:DragonHDOmniLatestNeural`.
- **Verified against the real service (2026-11-07):**
  - the key in `Ocp-Apim-Subscription-Key` works, on the free tier (F0), for the HD voices too;
  - **Azure lists 9 Japanese voices:** the two DragonHD voices, and the standard Nanami, Keita, Aoi, Daichi, Mayu,
    Naoki and Shiori;
  - **the Dragon HD Omni versions aren't listed, but each of the 7 works,** such as
    `ja-JP-Daichi:DragonHDOmniLatestNeural`. So `AzureSpeechProvider` adds one for each standard voice it lists,
    16 Japanese voices in all;
  - a made-up voice name is refused with 400 Bad Request, so the Omni voices aren't a fallback to some default;
  - the audio is MP3 at 192 kbit/s, 48 kHz, mono.
- **HD voices support fewer SSML elements.** DragonHD supports `<phoneme>` and `<sub>`, but no `<prosody>`.
  Dragon HD Omni has no `<phoneme>` either. That matters for pronunciation fixes later.
- **The region:** HD voices are only in some regions. In Europe those are France Central, Sweden Central and West
  Europe, so the recommendation is **Sweden Central** (`swedencentral`). The key only works with its own region.

### Google Cloud Text-to-Speech

- **Speaking:** `POST https://texttospeech.googleapis.com/v1/text:synthesize` with a JSON body naming the text, the
  voice and `"audioEncoding": "MP3"`. The response holds the audio as base64 in `audioContent`.
- **The voices:** `GET https://texttospeech.googleapis.com/v1/voices?languageCode=ja-JP`. The names look like
  `ja-JP-Neural2-B`, `ja-JP-Wavenet-A` and `ja-JP-Chirp3-HD-<name>`. Chirp 3 HD supports `ja-JP`.
- **The key** goes in the `X-Goog-Api-Key` header.
- **The text is sent as plain text,** not SSML, which Chirp 3 HD voices may not support.
- **Verified against the real service (2026-11-07):**
  - an API key restricted to the Text-to-Speech API works;
  - **Google lists 41 Japanese voices:** 30 Chirp 3 HD voices, named after stars and moons, such as
    `ja-JP-Chirp3-HD-Kore`, 3 Neural2 (B, C and D), 4 WaveNet and 4 Standard;
  - **its MP3 is encoded at a much lower bit rate than Azure's:** 32 kbit/s for Chirp 3 HD and 64 kbit/s for
    Neural2, both at 24 kHz, against Azure's 192 kbit/s at 48 kHz. Google offers no better MP3. Its other formats are
    uncompressed WAV (`LINEAR16`, 24 kHz, about ten times the size) and OGG Opus, about as small as its MP3. Its MP3
    is good enough (Magnus, 2026-11-07), so that's what's kept.
  - **Chirp 3 HD doesn't say the same text the same way twice.** Three requests for `明日は学校に行きます。` with
    `ja-JP-Chirp3-HD-Kore` read 明日 once as あした and twice as あす, and sounded different beyond the format. It's
    a generative model, as Azure's HD voices are: Azure's documentation says they "introduce slight variations in
    each output". So asking again can fix a wrong reading, and listening before keeping matters all the more.

## The settings file

`%APPDATA%\JAStudio\text-to-speech.json` (`TextToSpeechSettings`):

```json
{
  "azure": { "key": "…", "region": "swedencentral" },
  "google": { "apiKey": "…" }
}
```

A tts-provider without settings isn't offered. With no file, none is. The settings types don't print their keys,
so logging one leaks nothing.

## The accounts

- **Azure** (created 2026-11-07, paid by Lidbom Solutions AB, Magnus's company): the Speech resource
  `jastudio-speech-ov1yu0`, in the resource group `jastudio-tts`, in Sweden Central, on the free tier (F0), in the
  subscription "Lidbom Solutions AB". Its key 1 is in the settings file.
  - **If the free 0.5 million characters a month run out,** requests are refused until the next month. Moving to
    pay-as-you-go: `az cognitiveservices account update --name jastudio-speech-ov1yu0 --resource-group jastudio-tts --sku S0`.
  - **To replace the key,** regenerate it in the portal's "Keys and Endpoint", or with
    `az cognitiveservices account keys regenerate`, and put the new one in the settings file.
- **Google** (created 2026-11-07): the project `jastudio-tts-rdxtcd` ("JAStudio-TTS"), in the organization
  `lidbomsolutions.com` of the Workspace account magnus@lidbomsolutions.com. Its billing account, "My Billing
  Account", uses the company's payment details.
  - **It started as a free trial:** $300 of credit for 90 days. When the trial ends, Google stops the services until
    the full account is activated, from a banner in the console.
  - **The API key** "JAStudio-text-to-speech" is restricted to the Text-to-Speech API, and is in the settings file.
    To replace it: create a new one with `gcloud services api-keys create`, put it in the settings file, and delete
    the old one.
  - The organization's other project, "My First Project", was made by the console on the first visit, and isn't
    used.

## Increments (tentative)

1. **The tts-providers and their settings** (`JAStudio.TextToSpeech`): listing the Japanese voices and speaking a
   text, for Azure and Google, with specs against a fake HTTP handler. Then a first try against the real services,
   with Magnus's go-ahead, since it spends characters on his accounts. *(Built 2026-11-07, and verified against
   both real services.)*
2. **The dialog for one note, end to end:**
   - opened from a note's tab or its editor section;
   - pick a voice from the list, grouped by tts-provider;
   - generate it, and it plays;
   - try other voices, each result staying in the list to replay and compare;
   - keep one.

   Keeping it:
   - stores the MP3 in the media layer;
   - copies it into Anki's media folder, through a small Python function, for Anki's playback and the listening
     cards (Anki's media sync takes it to mobile);
   - replaces the note's audio field: vocab `Audio_TTS`, sentence `Audio Sentence`.
3. **Later:** one voice for the selected search results, remembered favorite voices, and pronunciation help from a
   vocab's readings where the voice supports it.

## The text spoken (increment 2)

The note's question, cleaned as HyperTTS cleans it: markup removed, and `〜` and `|` deleted. JAStudio knows a
vocab's readings, so a later increment could tell a voice how to pronounce a kanji it gets wrong, through SSML's
`<phoneme>` or `<sub>` where the voice supports them.

## To do

- **Playing audio in the dialog without tying JAStudio to Windows** (Magnus, 2026-11-07). A Windows-only library
  such as NAudio, first recommended, is out. Options to weigh when increment 2 starts:
  - a cross-platform audio library;
  - the note page's web view, which already plays audio (`AudioPlayButton.razor`);
  - Anki's own player, through a small Python function, which isn't there when JAStudio runs outside Anki.
- **Found meanwhile: JAStudio's UI doesn't run outside Windows as it is.** Two utilities call Windows' `user32`
  unconditionally: `NativeKeyboardFocus` (`SetFocus`) and `WindowPositioner` (`GetCursorPos`). The web view's
  WebView2 setup (`NoteTabView`) and `JAStudioAnkiAppRoot` check the platform first.

## Open questions

1. **Which audio field a kept speech replaces,** when a vocab already has Forvo or imported audio in `Audio_g`:
   probably only `Audio_TTS`, as HyperTTS's TTS presets do.
2. **Recording the voice in the media layer.** `media-storage-and-import.md` designs a `tts` section in the audio
   sidecar, with the engine, the voice and a version, but `AudioAttachment` doesn't have it yet (found 2026-11-07).
   Increment 2 adds it, or decides it isn't needed.
3. **Where the media layer puts a kept speech:** its import rules route files by source tag and field, and TTS
   audio is `Free`.

## Sources

- Azure: [text to speech REST API](https://learn.microsoft.com/en-us/azure/ai-services/speech-service/rest-text-to-speech),
  [HD voices](https://learn.microsoft.com/azure/ai-services/speech-service/high-definition-voices),
  [regions](https://learn.microsoft.com/azure/ai-services/speech-service/regions),
  [free tier](https://learn.microsoft.com/en-us/answers/questions/1654938/text-to-speech-s0-standard-tier-0-5-million-charac).
- Google: [Chirp 3 HD voices](https://docs.cloud.google.com/text-to-speech/docs/chirp3-hd),
  [pricing](https://cloud.google.com/text-to-speech/pricing).
- Prices from third parties: [developersdigest](https://www.developersdigest.tech/models/speech/azure),
  [beri.net](https://www.beri.net/article/elevenlabs-vs-cartesia-vs-azure-tts-latency-pricing-voice-cloning-2026),
  [costbench](https://costbench.com/software/ai-voice-tools/google-cloud-text-to-speech/).
- vocab.ai: [signup page](https://www.vocab.ai/signup), with the entry plan.
