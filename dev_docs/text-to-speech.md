# Text-to-speech — Design

> **Status: in use (2026-11-08).** Increments 1 and 2 are built, and Magnus uses them in Anki. The facts about the
> tts-providers are verified against the real services. What's left is under [To do](#to-do) and increment 3.

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
- **JAStudio isn't Windows-only,** so the dialog's audio playback mustn't make it so. *(Done: SoundFlow, below.)*
- **Your voices, in priority order:** the TTS dialog offers only the voices chosen, in an order Magnus sets. The first is
  the default. One list serves vocab and sentences.
- **Listening without generating again:** every attempt stays in the dialog to replay, and generating again adds a new
  one. The dialog tells whether a new attempt actually differs from the one before it, and from the note's current
  audio.
- **Nothing changes until one attempt is kept.**
- **The text to speak can be edited** in the dialog, such as to write a word in kana to force its reading. Only what's
  spoken changes, not the note.
- **The default voice speaks as soon as the dialog opens.**

## Terms

- **tts-provider** — a TTS service JAStudio calls itself: Azure Speech or Google Cloud Text-to-Speech
  (`ITextToSpeechProvider`).
- **voice** — one of a tts-provider's voices, such as Azure's `ja-JP-DaichiNeural` (`TextToSpeechVoice`).
- **synthesized speech** — the audio a voice generated for a text, not yet kept (`SynthesizedSpeech`).
- **keeping** a synthesized speech — storing it in the media layer and making it the note's audio.
- **chosen voice** — a voice chosen to be offered in the TTS dialog (`ChosenVoice`). The chosen voices are in priority
  order, and the first is the default.
- **attempt** — one speech in the TTS dialog, numbered in the order generated (`SpeechAttemptViewModel`).
- **normalizing** a speech — bringing it to ReplayGain's reference loudness, as `mp3gain /r /c` does, without
  re-encoding it (`SynthesizedSpeech.NormalizedToTheReplayGainReference`). See [Normalizing](#normalizing).

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
  `X-Microsoft-OutputFormat: audio-24khz-96kbitrate-mono-mp3`, MP3 at 96 kbit/s and 24 kHz. The body of a successful
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
  - **the format is MP3 at 96 kbit/s and 24 kHz, mono,** not Azure's best, 192 kbit/s at 48 kHz. By ear the two are all
    but indistinguishable, and the smaller one is half the size (Magnus, 2026-11-07). Nearly all the TTS audio HyperTTS
    made is in it too: 94% of a sample of 400 of its 32,043 files, which average 34.5 KB;
  - the standard neural voices seem to say a text the same way every time, so comparing formats with them is fair.
    The HD voices don't.
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
    Neural2, both at 24 kHz, against Azure's 96 kbit/s. Google offers no better MP3. Its other formats are
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
  "google": { "apiKey": "…" },
  "chosenVoices": [ { "provider": "Azure", "name": "ja-JP-Daichi:DragonHDOmniLatestNeural", "gender": "Male" } ]
}
```

A tts-provider without settings isn't offered. With no file, none is. The settings types don't print their keys,
so logging one leaks nothing. The chosen voices are written by the dialog for choosing them, which keeps the keys.

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

## Increments

1. **The tts-providers and their settings** (`JAStudio.TextToSpeech`): listing the Japanese voices and speaking a
   text, for Azure and Google, with specs against a fake HTTP handler. Then a first try against the real services,
   with Magnus's go-ahead, since it spends characters on his accounts. *(Built 2026-11-07, and verified against
   both real services.)*
2. **The dialog for one note, end to end.** *(Built 2026-11-07.)*
   - **Opening it** (`TextToSpeechDialog`, `TextToSpeechDialogViewModel`): with the "TTS..." button beside the note's
     TTS audio field in the workspace's editor section, a vocab's "Text to speech" or a sentence's "Audio"
     (`TextToSpeechAudioFieldEditorViewModel`). Or with "Text-to-speech audio..." in a vocab's or a sentence's note
     actions, in its right-click menu, which is also there while reviewing in Anki, where the editor section isn't.
     It opens near the cursor, and the default voice speaks at once.
   - **The dialog:**
     - the text to speak, editable;
     - the note's current audio, its TTS audio as it was when the dialog opened (`CurrentAudioViewModel`);
     - a button per chosen voice, "Every voice", and "Voices..." for choosing them;
     - the attempts, each with ▶, its length, and how it compares with the attempt before it with the same voice and
       text, such as "same as #1" or "differs from #2: 0.25 s longer", and with the current audio, such as
       "differs from current: 0.40 s shorter". Selecting one plays it, and ▶ plays it again.
   - **Keys,** while the keyboard isn't in the text: 1 to 9 play attempts 1 to 9, even the one already selected, 0
     plays the current audio, and Enter keeps the selected attempt. The ▶ buttons don't take the keyboard, so that
     Enter keeps the attempt rather than pressing ▶ again.
   - **Comparing:** on the decoded audio, not on the files, since Azure's standard voices send slightly different bytes
     for the same speech every time (`DecodedAudio`). Two attempts sound the same when they're about as long, and their
     sample-by-sample difference is below 0.2 of the audio's strength. Measured: Azure's standard voice differed from
     itself by 0.025, an HD voice by 1.5, and two voices by 1.2. A new attempt is compared with the one before it with
     the same voice and text, and with the current audio, whatever its voice. HyperTTS's MP3s are 24 kHz mono, as
     JAStudio's are (checked 2026-11-07), so a HyperTTS file can sound the same as a new attempt.
   - **A current audio JAStudio doesn't play:** SoundFlow decodes MP3, WAV and FLAC, but Ogg Vorbis only with a codec
     package of its own, and four imported sentences have `.ogg` audio (2026-11-07). The dialog says it doesn't play
     such audio, and doesn't compare with it (`DecodedAudio.CanDecode`). *Decided 2026-11-08: the corpus holds only
     MP3, and the four are to be converted (`corpus-validity.md`).*
   - **Keeping** (`SpeechKeeping`), the only step that changes anything:
     1. the MP3 is stored in the media layer under a new name naming its voice, such as
        `jastudio-tts-azure-daichi-dragonhdomnilatestneural-<guid>.mp3` (`MediaStorageService.StoreNewAudioFile`);
     2. it's copied into Anki's media folder through Anki's own `col.media.add_file`, for Anki's playback and the
        listening cards (Anki's media sync takes it to mobile). Without Anki this is left out;
     3. the note's TTS audio field references it (`IHasTextToSpeechAudio.TextToSpeechAudio`).
   - **Choosing the voices:** Config > Text-to-speech voices, or "Voices..." in the TTS dialog (`ChosenVoicesDialog`).
     It lists every voice of every tts-provider with a key, the chosen ones first, in order, with ▶ for a sample of
     each, generated once and replayed from memory.
   - **Playing** is SoundFlow's (`SoundFlowAudioPlayer`, `IAudioPlayer`): a cross-platform .NET audio library built on
     miniaudio, a native library of 660 KB on Windows, which it brings for each platform. Tried on 2026-11-07: playback
     starts 9 to 27 ms after the call. JAStudio is built for one platform at a time, so `miniaudio.dll` sits beside
     its assemblies, as SkiaSharp's native library already does inside Anki.
   - **Verified in Anki** by Magnus on 2026-11-07: generating, playing, comparing and keeping.
3. **Later:** one voice for the selected search results, remembered favorite voices, and pronunciation help from a
   vocab's readings where the voice supports it.

## Normalizing

Every speech a voice gives JAStudio is normalized before anything else sees it (`TextToSpeechVoice.SpeakAsync`,
`SynthesizedSpeech.NormalizedToTheReplayGainReference`), so attempts and voice samples sound as loud as each other, and
as the TTS audio HyperTTS made, which Magnus's script (`mp3gainBatchNormalizer`) normalized with `mp3gain -c -r`. It also
lets an attempt sound the same as a HyperTTS file of the same speech: both got the same gain.

The normalizing is `Mp3LoudnessNormalization`'s, a library knowing nothing of JAStudio that does what mp3gain does, byte
for byte: see [mp3-loudness-normalization.md](mp3-loudness-normalization.md).

**Magnus's script skips JAStudio's files** (2026-11-08). Kept files go into Anki's media folder, which the script
normalizes. Tried on 2026-11-08: mp3gain finds a JAStudio file needs no change, but adds its tag, of 174 bytes, so Anki's
copy would differ from the media layer's. So the script leaves out `jastudio-tts-*.mp3`.

## The text spoken

The note's question, cleaned as HyperTTS cleans it: markup removed, and `〜` and `|` deleted
(`IHasTextToSpeechAudio.SpeakableTextOf`). The dialog lets it be edited before speaking. JAStudio knows a vocab's
readings, so a later increment could tell a voice how to pronounce a kanji it gets wrong, through SSML's
`<phoneme>` or `<sub>` where the voice supports them.

**Chirp 3 HD takes no SSML,** so pronunciation help through SSML would work only for the other voices
([Google's pricing page](https://cloud.google.com/text-to-speech/pricing), 2026-11-08).

**Google may answer the same request with the same audio.** Two identical requests to Chirp 3 HD Kore gave
identical audio once (2026-11-07), and different audio an hour earlier, so Google may cache identical requests for a
while. Then generating again gives "same as #1", and a different rendition needs a change to the text, even a small
one.

## To do

- ~~**Playing audio in the dialog without tying JAStudio to Windows**~~ *Done 2026-11-07, with SoundFlow, chosen
  over the note page's web view, which would have needed glue code and a page to be shown, and Anki's own player,
  which isn't there outside Anki.*
- ~~**Normalizing the MP3s before we're done**~~ *Done 2026-11-07, as mp3gain does it. See [Normalizing](#normalizing).*
- **Stop taking in audio from Anki.** HyperTTS isn't used any more (Magnus, 2026-11-08). Nothing in it runs fully
  automatically, so it can stay in Anki until Magnus removes it. Nothing else writes audio in Anki, so the
  Anki-ingestion's last job besides importing, taking in that audio, can go (`corpus-data-model.md`, "Anki's role and
  its consequences", point 2).
- **Activate Google's full account before the free trial ends, around 2027-02-05,** 90 days after 2026-11-07, or Google
  stops the service until it is (see [The accounts](#the-accounts)). Found 2026-11-08: activating keeps the unused
  credit until the trial would have ended, and there's no monthly fee, only use: each kind of voice has a free
  allowance a month, 1 million characters for Chirp 3 HD and for Neural2, then about $30 and $16 per million.
  ([trial](https://docs.cloud.google.com/free/docs/free-cloud-features), [pricing](https://cloud.google.com/text-to-speech/pricing))
- **Found meanwhile: JAStudio's UI doesn't run outside Windows as it is.** Two utilities call Windows' `user32`
  unconditionally: `NativeKeyboardFocus` (`SetFocus`) and `WindowPositioner` (`GetCursorPos`). The web view's
  WebView2 setup (`NoteTabView`) and `JAStudioAnkiAppRoot` check the platform first.

## Open questions

1. ~~**Which audio field a kept speech replaces**~~ *Settled 2026-11-07:* a vocab's `Audio_TTS`, as HyperTTS's TTS
   presets did, and a sentence's `Audio Sentence`, which for an imported sentence holds its recorded clip. The dialog
   shows the audio being replaced.
2. ~~**Recording the voice.**~~ *Settled 2026-11-07:* in the file name, since the media layer works by file name alone,
   with no sidecars (`media-storage-and-import.md`).
3. ~~**Where a kept speech goes.**~~ *Settled 2026-11-07:* a vocab's into `free/TTS/vocab`, with HyperTTS's, and a
   sentence's into `free/TTS/sentences/jastudio` (`TextToSpeechAudioFolders`). HyperTTS's sentence audio is in a
   folder per source, named by import rules rather than by anything a note says, and TTS audio is free, so there's no
   takedown to split it by source for.
4. **The file a kept speech replaces** becomes an orphan, still in the media layer and in Anki's media folder, as when
   HyperTTS regenerated audio. It's left there. Deleting it safely needs a check that no other note references the
   same name, which checking the whole corpus could do for every orphan at once (`corpus-validity.md`).

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
