"""Restore the ten original portrait delivery files without changing their pixels."""
import hashlib
import json
import shutil
from pathlib import Path

root = Path(__file__).resolve().parents[2]
records = json.loads((root / "ArtSource/PlayerPortraits/integrity.json").read_text(encoding="utf-8-sig"))
# The manifest describes the approved master/copy pairs; paths are repository-relative.
for item in records["files"]:
    source, target = root / item["master"], root / item["asset"]
    digest = hashlib.sha256(source.read_bytes()).hexdigest()
    if digest.lower() != item["sha256"].lower():
        raise ValueError(f"Master integrity mismatch: {item['master']}")
    target.parent.mkdir(parents=True, exist_ok=True)
    shutil.copyfile(source, target)
print("Restored 10 original character portraits.")
