# Japanese Language Studio for Anki
Turns anki into a full featured tool for studying japanese in anki. A short sample, anything but complete, list of features:
* Automatic breakdown and explanation of sentences
* Playing the audio for each word/phrase in the sentence
* Listing and navigating to each word/phrase in a sentence
* Listing and navigating to all the kanji in a sentence
* Breaking down compound words and phrases into their component parts and tracking that across sentences etc 
* Listing all words with a kanji
* Listing all kanji in a word and navigating to them
* Tracking words relationships such as synonyms, antonyms,

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/mlidbom)

## Development Setup

### Windows

**Prerequisites:**
- .NET 10.0 SDK
- Python 3.13
- Git with long paths enabled

**One-time setup:**
```powershell
.\setup-dev.ps1
```

This script will:
1. Enable Git long paths support (required for Compze submodule)
2. Initialize git submodules
3. Create Python virtual environment
4. Install Python dependencies
5. Build the .NET solution

**Quick commands:**
```powershell
dotnet build JAStudio.slnx -c Debug   # Fast .NET build
dotnet test JAStudio.slnx             # .NET tests
venv\Scripts\python.exe -m pytest     # Python tests
.\full-build.ps1                      # Full validation (build + stubs + type check)
```

### Linux / CI

```bash
./setup-dev.sh                                        # One-time setup
JASTUDIO_VENV_PATH="$(pwd)/venv" dotnet test JAStudio.slnx --filter "FullyQualifiedName!~BulkLoaderTests"
source venv/bin/activate && pytest
```

