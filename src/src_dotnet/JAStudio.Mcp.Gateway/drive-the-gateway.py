"""Drives JAStudio's MCP gateway over stdio, as Claude Code does, through the steps given, and prints each answer.

A changed gateway reaches Claude Code only when Magnus reconnects it, with /mcp or a new session, so a change to it is tried with this before that:
it starts the gateway through start-the-gateway.ps1, from the source checkout this script is in, exactly as Claude Code does. Each step is a tool
call, `<tool>` or `<tool>=<arguments as JSON>`, or `tools`, which lists the gateway's tools. For example:

    venv\\Scripts\\python.exe src\\src_dotnet\\JAStudio.Mcp.Gateway\\drive-the-gateway.py list_the_running_jastudios start_headless_jastudio tools
        headless-main__describe_collection 'disconnect_from_jastudio={"name":"headless-main"}' stop_headless_jastudio

The design of the gateway is in dev_docs/jastudio-mcp.md.
"""

from __future__ import annotations

import io
import json
import queue
import subprocess
import sys
import threading
from pathlib import Path
from typing import IO, cast, final

GATEWAY_FOLDER = Path(__file__).resolve().parent
SOURCE_CHECKOUT = GATEWAY_FOLDER.parents[2]
HOW_LONG_AN_ANSWER_IS_WAITED_FOR_IN_SECONDS = 300

type JsonObject = dict[str, object]


def parsed(json_text: str) -> object:
    return cast(object, json.loads(json_text))


def as_json_object(value: object) -> JsonObject:
    if not isinstance(value, dict):
        raise TypeError(f"Expected a JSON object, got: {value}")
    return cast(JsonObject, value)


def as_json_array(value: object) -> list[object]:
    if not isinstance(value, list):
        raise TypeError(f"Expected a JSON array, got: {value}")
    return cast(list[object], value)


@final
class Gateway:
    """The gateway, started as Claude Code starts it, and the conversation with it."""

    def __init__(self) -> None:
        self._process: subprocess.Popen[str] = subprocess.Popen(
            ["pwsh", "-NoProfile", "-NonInteractive", "-File", str(GATEWAY_FOLDER / "start-the-gateway.ps1")],
            cwd=SOURCE_CHECKOUT, stdin=subprocess.PIPE, stdout=subprocess.PIPE, stderr=subprocess.DEVNULL, text=True, encoding="utf-8")
        self._answers: queue.Queue[JsonObject] = queue.Queue()
        self.notifications: list[str] = []
        self._last_request_id: int = 0
        threading.Thread(target=self._read_what_it_writes, args=(self._stream(self._process.stdout),), daemon=True).start()

    @staticmethod
    def _stream(stream: IO[str] | None) -> IO[str]:
        if stream is None:
            raise RuntimeError("The gateway was started without the stream redirected.")
        return stream

    def _read_what_it_writes(self, output: IO[str]) -> None:
        for line in output:
            message = as_json_object(parsed(line))
            if "id" in message:
                self._answers.put(message)
            else:
                self.notifications.append(str(message.get("method")))

    def _send(self, message: JsonObject) -> None:
        stdin = self._stream(self._process.stdin)
        _ = stdin.write(json.dumps({"jsonrpc": "2.0", **message}) + "\n")
        stdin.flush()

    def ask(self, method: str, params: JsonObject) -> JsonObject:
        self._last_request_id += 1
        self._send({"id": self._last_request_id, "method": method, "params": params})
        answer = self._answers.get(timeout=HOW_LONG_AN_ANSWER_IS_WAITED_FOR_IN_SECONDS)
        if answer.get("id") != self._last_request_id:
            raise RuntimeError(f"The gateway answered request {answer.get('id')}, not {self._last_request_id}: {answer}")
        return answer

    def start_the_conversation(self) -> None:
        _ = self.ask("initialize", {"protocolVersion": "2025-06-18", "capabilities": {}, "clientInfo": {"name": "drive-the-gateway", "version": "1"}})
        self._send({"method": "notifications/initialized", "params": {}})

    def tool_names(self) -> list[str]:
        tools = as_json_array(as_json_object(self.ask("tools/list", {})["result"])["tools"])
        return [str(as_json_object(tool)["name"]) for tool in tools]

    def call(self, tool: str, arguments: JsonObject) -> str:
        answer = self.ask("tools/call", {"name": tool, "arguments": arguments})
        result = as_json_object(answer.get("result", answer))
        text = "\n".join(str(as_json_object(content).get("text", "")) for content in as_json_array(result.get("content", [])))
        return f"{text}\n(an error)" if result.get("isError") else text

    def stop(self) -> None:
        self._stream(self._process.stdin).close()
        _ = self._process.wait(timeout=60)


def run(steps: list[str]) -> None:
    if isinstance(sys.stdout, io.TextIOWrapper):
        sys.stdout.reconfigure(encoding="utf-8")  # The answers hold Japanese, which a Windows console's own encoding can't.
    gateway = Gateway()
    gateway.start_the_conversation()
    print(f"The gateway's tools: {gateway.tool_names()}\n")
    for step in steps:
        tool, _, arguments = step.partition("=")
        if tool == "tools":
            names = gateway.tool_names()
            print(f"--- tools: {len(names)}: {names}\n")
        else:
            print(f"--- {step}\n{gateway.call(tool, as_json_object(parsed(arguments)) if arguments else {})}\n")
    print(f"Notifications from the gateway: {gateway.notifications}")
    gateway.stop()


if __name__ == "__main__":
    run(sys.argv[1:])
