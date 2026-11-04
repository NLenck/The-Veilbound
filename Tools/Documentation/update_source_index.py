"""Regenerate Docs/Code/SourceIndex.md from the live Assets C# sources."""
from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[2]
ASSETS = ROOT / "Assets"
OUTPUT = ROOT / "Docs" / "Code" / "SourceIndex.md"

# Remove comments and string literals before looking for type declarations.
TOKENS = re.compile(r'@"(?:""|[^\"])*"|"(?:\\.|[^"\\])*"|\'(?:\\.|[^\'\\])*\'|//[^\n]*|/\*[\s\S]*?\*/')
DECLARATIONS = re.compile(r'\b(class|struct|interface|enum)\s+([A-Za-z_]\w*)')
GROUPS = ["Equipment domain", "Main runtime gameplay and UI", "Runtime smoke and review harness", "Editor tooling", "Model and presentation support"]


def classify(relative: str) -> str:
    name = Path(relative).stem.lower()
    if "/Editor/" in relative:
        return "Editor tooling"
    if "/EquipmentDomain/" in relative:
        return "Equipment domain"
    if relative.startswith("Assets/Veilbound/Scripts/"):
        if any(marker in name for marker in ("smoke", "review", "validation", "runtimecheck")):
            return "Runtime smoke and review harness"
        return "Main runtime gameplay and UI"
    return "Model and presentation support"


def generate() -> None:
    groups = {key: [] for key in GROUPS}
    all_files = sorted(ASSETS.rglob("*.cs"), key=lambda p: p.relative_to(ROOT).as_posix().lower())
    for path in all_files:
        relative = path.relative_to(ROOT).as_posix()
        source = path.read_text(encoding="utf-8-sig")
        source = TOKENS.sub(" ", source)
        names = []
        for kind, name in DECLARATIONS.findall(source):
            label = f"`{name}` ({kind})"
            if re.search(r"\bpartial\s+class\s+" + re.escape(name) + r"\b", source):
                label = f"`{name}` (partial {kind})"
            if label not in names:
                names.append(label)
        groups[classify(relative)].append((relative, ", ".join(names) or "No type declaration found"))
    lines = [
        "# Complete C# source index", "", "[Back to the code guide](README.md) · [Class reference](Classes.md)", "",
        "Generated from the live `Assets/` C# files by `Tools/Documentation/update_source_index.py`.",
        "Root art-production copies, Packages and local archives are excluded. Nested types are listed by their declared short names.",
        "Repeated `VeilGame`, `Settlement` or `RunBuild` entries refer to partial declarations of the same class.",
        "Category grouping is based on path/filename; a review helper can still be compiled into the player unless its source is editor-only.", "",
        f"**{len(all_files)} C# files indexed.**", ""
    ]
    for group in GROUPS:
        lines += [f"## {group}", "", "| Source file | Declared types |", "| --- | --- |"]
        for relative, names in groups[group]:
            lines.append(f"| [{relative}](../../{relative}) | {names} |")
        lines.append("")
    OUTPUT.parent.mkdir(parents=True, exist_ok=True)
    OUTPUT.write_text("\n".join(lines), encoding="utf-8", newline="\n")
    print(f"Indexed {len(all_files)} C# files into {OUTPUT.relative_to(ROOT).as_posix()}")


if __name__ == "__main__":
    generate()
