#!/usr/bin/env python3
"""Adds a Content-Security-Policy to the published client's staticwebapp.config.json.

The published index.html contains an inline <script type="importmap"> whose content changes with every build, so a
fixed policy would have to allow all inline scripts ('unsafe-inline'), which defeats most of the protection. Instead
this script hashes each inline script that is actually in the published page and allows exactly those.

Usage: apply-csp.py <published wwwroot folder> <API base URL>
"""
import base64
import hashlib
import json
import re
import sys
from pathlib import Path
from urllib.parse import urlparse

INLINE_SCRIPT = re.compile(r"<script(?![^>]*\bsrc=)[^>]*>(.*?)</script>", re.DOTALL | re.IGNORECASE)


def inline_script_hashes(html: str) -> list[str]:
    hashes = []
    for body in INLINE_SCRIPT.findall(html):
        if not body.strip():
            continue
        digest = hashlib.sha256(body.encode("utf-8")).digest()
        hashes.append("'sha256-" + base64.b64encode(digest).decode("ascii") + "'")
    return hashes


def build_policy(api_origin: str, script_hashes: list[str]) -> str:
    scripts = " ".join(["'self'", "'wasm-unsafe-eval'", *script_hashes])
    return "; ".join(
        [
            "default-src 'self'",
            f"script-src {scripts}",
            # Radzen and Blazor set inline styles; styles cannot run code, so inline styles are allowed.
            "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com",
            "font-src 'self' https://fonts.gstatic.com data:",
            "img-src 'self' data:",
            f"connect-src 'self' {api_origin}",
            "manifest-src 'self'",
            # The label scanner's OCR engine runs in a web worker served from our own origin.
            "worker-src 'self'",
            "object-src 'none'",
            "base-uri 'self'",
            "form-action 'self'",
            "frame-ancestors 'none'",
        ]
    )


def main() -> int:
    if len(sys.argv) != 3:
        print(__doc__)
        return 2

    root = Path(sys.argv[1])
    parsed = urlparse(sys.argv[2])
    api_origin = f"{parsed.scheme}://{parsed.netloc}"

    index = root / "index.html"
    config_path = root / "staticwebapp.config.json"

    hashes = inline_script_hashes(index.read_text(encoding="utf-8"))
    policy = build_policy(api_origin, hashes)

    config = json.loads(config_path.read_text(encoding="utf-8"))
    config.setdefault("globalHeaders", {})["Content-Security-Policy"] = policy
    config_path.write_text(json.dumps(config, indent=2) + "\n", encoding="utf-8")

    print(f"Content-Security-Policy set ({len(hashes)} inline script hash(es), API origin {api_origin})")
    return 0


if __name__ == "__main__":
    sys.exit(main())