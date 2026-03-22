#!/usr/bin/env python3
"""Generate lib/index.json from front matter in lib/ markdown files."""

import json
import os
import re
import datetime
import glob


def get_field(fm, key):
    match = re.search(rf'^{key}:\s*(.+)$', fm, re.MULTILINE)
    return match.group(1).strip() if match else ""


def get_list_field(fm, key):
    match = re.search(rf'^{key}:\s*\n((?:\s+-\s+.+\n?)*)', fm, re.MULTILINE)
    if not match:
        return []
    return [line.strip().lstrip("- ") for line in match.group(1).strip().split("\n")]


def main():
    artifacts = []
    lib = "lib"

    for pattern in ["agents/*.md", "skills/*/SKILL.md", "workflows/*.md", "packs/*.md"]:
        for f in sorted(glob.glob(os.path.join(lib, pattern))):
            with open(f, encoding="utf-8") as fh:
                content = fh.read()
            m = re.match(r'^---\n(.*?)\n---', content, re.DOTALL)
            if not m:
                continue
            fm = m.group(1)

            name = get_field(fm, "name")
            atype = get_field(fm, "type")
            version = get_field(fm, "version")
            desc = get_field(fm, "description")
            deps = get_list_field(fm, "deps")
            tags = get_list_field(fm, "tags")
            supported = get_list_field(fm, "supported")
            pack_artifacts = get_list_field(fm, "artifacts")
            internal_match = re.search(r'^internal:\s*(true|false)$', fm, re.MULTILINE)
            internal = internal_match and internal_match.group(1) == "true"

            rel = os.path.relpath(f, lib).replace("\\", "/")
            path = os.path.dirname(rel) + "/" if atype == "skill" else rel

            entry = {
                "name": name,
                "type": atype,
                "version": version,
                "description": desc,
                "path": path,
                "deps": deps,
                "tags": tags,
            }
            if supported:
                entry["supported"] = supported
            if pack_artifacts:
                entry["artifacts"] = pack_artifacts
            if internal:
                entry["internal"] = True
            artifacts.append(entry)

    index = {
        "version": 1,
        "generated": datetime.datetime.utcnow().strftime("%Y-%m-%dT%H:%M:%SZ"),
        "artifacts": artifacts,
    }
    with open(os.path.join(lib, "index.json"), "w", encoding="utf-8") as fh:
        json.dump(index, fh, indent=2, ensure_ascii=False)
        fh.write("\n")
    print(f"Generated index.json with {len(artifacts)} artifacts")


if __name__ == "__main__":
    main()
