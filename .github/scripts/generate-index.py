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


def unique(values):
    out = []
    seen = set()
    for value in values:
        if not value:
            continue
        if value in seen:
            continue
        seen.add(value)
        out.append(value)
    return out


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

            # Frontmatter v2 keeps role relations as explicit fields.
            # Convert them into index dependencies/member ids used by installer logic.
            agent_members = get_list_field(fm, "agents")
            skill_members = get_list_field(fm, "skills")
            workflow_member = get_field(fm, "workflow")

            if atype == "agent":
                deps = unique(deps + [f"skill:{name}" for name in skill_members])
            elif atype == "workflow":
                deps = unique(
                    deps
                    + [f"agent:{name}" for name in agent_members]
                    + [f"skill:{name}" for name in skill_members]
                )
            elif atype == "pack":
                synthesized = []
                if workflow_member:
                    synthesized.append(f"workflow:{workflow_member}")
                synthesized.extend([f"agent:{name}" for name in agent_members])
                synthesized.extend([f"skill:{name}" for name in skill_members])
                pack_artifacts = unique(pack_artifacts + synthesized)

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
