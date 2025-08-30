# XML Artifacts (Auto-Generated)

This repository maintains **auto-generated** artifacts for Def XMLs:

- `XML_INDEX.md` — bulleted file list with `View · Raw` links (mirrors Code Index) and a schema summary.
- `XML_SNAPSHOT.txt` — normalized, diffable snapshot of all XML Def files.
- `Docs/Templates/Defs/*.xml` — default templates for each discovered schema.

These files are regenerated on every push to `main` (and on manual dispatch) by
the GitHub Actions workflow: `.github/workflows/update-xml-artifacts.yml`.

> Do not hand-edit the generated artifacts; they will be overwritten.

## What gets scanned

By default the scanner looks for Def XMLs under these folders (first match wins):

1. `StreamingAssets/Defs`
2. `Assets/StreamingAssets/Defs`
3. `GameData/Defs`

You can override via environment variables:

- `FC_DEFS_DIR` — absolute or repo-root-relative path to the Defs folder.
- `FC_REPO_ROOT` — override repo root for unusual run contexts.

## Template growth

Templates incorporate the union of **observed fields** across all defs in a schema.
When new fields/components/tags appear in content, templates will grow on the next run.

Field ordering is normalized as:

1. `id`, `schema`, `name_key`, `tags`, `version`, `requires`
2. Known/common fields (from optional `Tools/XmlDefsTools/Config/SchemaOrder.json`)
3. Any newly observed fields (alphabetical)

## Local development

```bash
dotnet build Tools/XmlDefsTools/XmlDefsTools.csproj -c Release
dotnet run --project Tools/XmlDefsTools/XmlDefsTools.csproj -c Release
```

Artifacts will be written to repo root and `Docs/Templates/Defs`.

## Notes

- The snapshot removes comments and normalizes attribute ordering/whitespace.
- Duplicate IDs across files/schemas are reported in the index.
- Invalid XMLs are skipped but listed with an error note.
