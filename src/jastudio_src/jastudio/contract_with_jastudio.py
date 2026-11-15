"""The contract between the Anki add-on and JAStudio, both ways over HTTP: the functions JAStudio calls in Anki, on the add-on's server
(jastudio.anki_http), and the requests the add-on makes of JAStudio, on JAStudio's card server (jastudio.primary_jastudio).

Each side compares the other's version with its own before it relies on it, so that an add-on and a JAStudio built from different checkouts
refuse each other, saying so, where they'd otherwise misunderstand each other. JAStudio's is AnkiAddOnContract.Version.
"""
from __future__ import annotations

CONTRACT_VERSION = 4
"""The version of the contract this add-on speaks. It changes whenever a function, a request, an argument or an answer does, together with
JAStudio's AnkiAddOnContract.Version. Version 4 added the requests from Anki to JAStudio (2026-11-15)."""
