"""The HTTP server through which JAStudio, as a program of its own, calls Anki: see dev_docs/moving-jastudio-out-of-anki.md.

It serves on this computer's loopback address, on a port the system picks, with Python's standard library alone. Every request has to carry the
server's token, which only the same user can read, since the functions can read and delete in Anki's collection.

    GET  /contract                              the version of what is served here, which JAStudio compares with its own
    GET  /collection                            the path of the collection Anki has open
    POST /call/<function>?collection=<path>     calls a function, with a JSON object of its arguments by name as the body

A call names the collection JAStudio expects, by its path, and is refused when Anki has another open: JAStudio's ids are those of one collection.
A call, and /collection, are refused while JAStudio may not use Anki's collection (AnkiForJAStudio.why_the_collection_cannot_be_used_now).

Answers are JSON objects: {"result": ...} with status 200; {"refused": why, ...} with 409, in words for a person; {"error": ...} otherwise.

It answers one request a connection, and says so in every answer (Connection: close). A client that isn't told keeps the connection for its next
request, and finds it closed: .NET's HttpClient then failed about one call in ten with "The response ended prematurely".
"""
from __future__ import annotations

import hmac
import inspect
import json
import os
import secrets
import threading
import traceback
from dataclasses import dataclass
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from typing import TYPE_CHECKING, cast, override
from urllib.parse import parse_qs, urlsplit

from jaspythonutils.sysutils.typed import non_optional

from jastudio.anki_http.functions_on_the_collection import FunctionsOnTheCollection, JsonText

if TYPE_CHECKING:
    from collections.abc import Callable

    from jastudio.anki_http.anki_for_jastudio import AnkiForJAStudio
    from jastudio.anki_http.functions_on_the_collection import JsonValue

CONTRACT_VERSION = 1
"""The version of what the server serves: the functions, their arguments and their answers. It changes whenever one of them does."""


@dataclass(frozen=True)
class Answer:
    status: int
    body: bytes

    @staticmethod
    def with_the_result(result: JsonValue) -> Answer:
        if isinstance(result, JsonText):
            return Answer(200, b'{"result": ' + result.encode("utf-8") + b"}")
        return Answer._of(200, {"result": result})

    @staticmethod
    def refused(why: str, **more: JsonValue) -> Answer: return Answer._of(409, {"refused": why, **more})

    @staticmethod
    def error(status: int, error: str) -> Answer: return Answer._of(status, {"error": error})

    @staticmethod
    def _of(status: int, body: dict[str, JsonValue]) -> Answer: return Answer(status, json.dumps(body, ensure_ascii=False).encode("utf-8"))


class HttpServerForJAStudio:
    def __init__(self, anki: AnkiForJAStudio) -> None:
        self._anki: AnkiForJAStudio = anki
        self._functions: dict[str, Callable[..., JsonValue]] = FunctionsOnTheCollection(anki).by_name
        self._server: ThreadingHTTPServer | None = None
        self.token: str = secrets.token_urlsafe(32)
        """What every request has to carry, as "Authorization: Bearer <token>": a new one each time Anki starts."""

    def start(self) -> None:
        """Starts serving, on a thread of its own, with a thread for each request."""
        server = ThreadingHTTPServer(("127.0.0.1", 0), self._request_handler())
        server.daemon_threads = True
        self._server = server
        threading.Thread(target=server.serve_forever, name="HttpServerForJAStudio", daemon=True).start()

    @property
    def port(self) -> int: return non_optional(self._server).server_address[1]

    def stop(self) -> None:
        server = non_optional(self._server)
        server.shutdown()
        server.server_close()
        self._server = None

    def answer_to(self, method: str, path_and_query: str, authorization: str | None, body: bytes) -> Answer:
        """The answer to a request, whatever the request is: every failure to answer it is an answer too."""
        if not self._carries_the_token(authorization):
            return Answer.error(401, "The request doesn't carry this server's token.")

        url = urlsplit(path_and_query)
        if method == "GET" and url.path == "/contract":
            return Answer.with_the_result({"version": CONTRACT_VERSION})

        why_not_now = self._anki.why_the_collection_cannot_be_used_now()
        if why_not_now is not None:
            return Answer.refused(why_not_now)

        if method == "GET" and url.path == "/collection":
            return Answer.with_the_result({"path": self._anki.collection().path})
        if method == "POST" and url.path.startswith("/call/"):
            return self._answer_to_the_call_of(url.path.removeprefix("/call/"), parse_qs(url.query).get("collection", [None])[0], body)
        return Answer.error(404, f"Nothing is served at {method} {url.path}.")

    def _answer_to_the_call_of(self, function_name: str, collection_expected: str | None, body: bytes) -> Answer:
        function = self._functions.get(function_name)
        if function is None:
            return Answer.error(404, f"There is no function named {function_name}. The functions are: {', '.join(sorted(self._functions))}.")

        collection_open = self._anki.collection().path
        if collection_expected is None:
            return Answer.error(400, "The call doesn't name the collection it expects, as ?collection=<its path>.")
        if not _are_the_same_file(collection_expected, collection_open):
            return Answer.refused("Anki has another collection open than the one JAStudio works with.", collectionOpen=collection_open)

        try:
            arguments = cast("object", json.loads(body or b"{}"))
        except ValueError as error:
            return Answer.error(400, f"The body isn't JSON: {error}")
        why_they_do_not_fit = _why_the_arguments_do_not_fit(function, arguments)
        if why_they_do_not_fit is not None:
            return Answer.error(400, f"The arguments don't fit {function_name}{inspect.signature(function)}: {why_they_do_not_fit}")

        try:
            return Answer.with_the_result(function(**cast("dict[str, object]", arguments)))
        except Exception:  # the boundary where a failed call is reported to its caller, with what failed
            return Answer.error(500, traceback.format_exc())

    def _carries_the_token(self, authorization: str | None) -> bool:
        return authorization is not None and hmac.compare_digest(authorization.encode("utf-8"), f"Bearer {self.token}".encode())

    def _request_handler(self) -> type[BaseHTTPRequestHandler]:
        server_for_jastudio = self

        class RequestHandler(BaseHTTPRequestHandler):
            def do_GET(self) -> None: self._answer(b"")

            def do_POST(self) -> None: self._answer(self.rfile.read(int(self.headers.get("Content-Length", "0"))))

            def _answer(self, body: bytes) -> None:
                answer = server_for_jastudio.answer_to(self.command, self.path, self.headers.get("Authorization"), body)
                self.send_response(answer.status)
                self.send_header("Content-Type", "application/json; charset=utf-8")
                self.send_header("Content-Length", str(len(answer.body)))
                self.send_header("Connection", "close")
                self.end_headers()
                self.wfile.write(answer.body)

            @override
            def log_message(self, format: str, *args: object) -> None:
                """Logs nothing: http.server would write a line to stderr for every request."""

        return RequestHandler


def _are_the_same_file(path: str, other_path: str) -> bool:
    return os.path.normcase(os.path.abspath(path)) == os.path.normcase(os.path.abspath(other_path))


def _why_the_arguments_do_not_fit(function: Callable[..., JsonValue], arguments: object) -> str | None:
    """Why the arguments, as the request's JSON gave them, can't be passed to the function by name, or None when they can."""
    if not isinstance(arguments, dict):
        return "they aren't a JSON object."
    try:
        inspect.signature(function).bind(**cast("dict[str, object]", arguments))
    except TypeError as error:
        return str(error)
    return None
