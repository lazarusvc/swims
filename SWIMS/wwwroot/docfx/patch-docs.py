#!/usr/bin/env python3
"""
Post-DocFX build patcher.
Removes the PDF download button from dev/api/global sections — only user-guide ships a PDF.
Run automatically by the GenerateDocFX MSBuild target after every docfx build.
"""
import json, pathlib, sys

docs_root = pathlib.Path(sys.argv[1]) if len(sys.argv) > 1 else pathlib.Path(__file__).parent.parent / "docs"

STRIP_PDF_FROM = [
    "toc.json",
    "dev/toc.json",
    "api/toc.json",
]

for rel in STRIP_PDF_FROM:
    p = docs_root / rel
    if not p.exists():
        print(f"  skip  {rel} (not found)")
        continue
    data = json.loads(p.read_text(encoding="utf-8"))
    if "pdf" in data:
        del data["pdf"]
        p.write_text(json.dumps(data, separators=(",", ":")), encoding="utf-8")
        print(f"  patch {rel}: removed pdf key")
    else:
        print(f"  ok    {rel}: no pdf key present")
