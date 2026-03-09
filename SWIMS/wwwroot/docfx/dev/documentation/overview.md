# Documentation System

SWIMS documentation is built and served using **DocFX v2.78.3**, embedded directly into the project as a local .NET tool. The generated static site lives at `wwwroot/docs/` and is served alongside the application at `/docs/`.

## Structure

```
wwwroot/
├── docfx/                  ← source (you edit files here)
│   ├── docfx.json          ← DocFX build configuration
│   ├── index.md            ← landing page
│   ├── toc.yml             ← top-level navigation
│   ├── api/                ← API reference extensions + generated toc
│   │   └── index.md        ← API landing page
│   ├── dev/                ← Developer Guide content (markdown)
│   │   ├── toc.yml
│   │   ├── introduction.md
│   │   └── [module folders]/
│   └── user-guide/         ← User Guide content (markdown)
│       ├── toc.yml
│       ├── introduction.md
│       └── [module folders]/
└── docs/                   ← generated output (do not edit directly)
    ├── api/                ← HTML API reference
    ├── dev/                ← HTML developer guide
    ├── user-guide/         ← HTML user guide
    └── public/             ← JS/CSS assets
```

## Building

### Via MSBuild (default off)

DocFX generation is toggled by a project property — off by default to keep normal builds fast:

```bash
# One-off full build including docs
dotnet build -p:DocFxBuildEnabled=true
```

The `GenerateDocFX` MSBuild target (in `SWIMS.csproj`) runs after `Build` when the flag is true:
1. `dotnet tool restore` — restores DocFX from `.config/dotnet-tools.json`
2. `dotnet tool run docfx metadata` — extracts C# XML comments → `api/` YAML
3. `dotnet tool run docfx build` — compiles all markdown + metadata → `wwwroot/docs/`

### Manually

```bash
cd wwwroot/docfx
dotnet tool restore
dotnet tool run docfx metadata --logLevel verbose
dotnet tool run docfx build --logLevel verbose
```

### Live preview (local server)

```bash
cd wwwroot/docfx
dotnet tool run docfx build --serve
# Opens at http://localhost:8080/docs/
```

## Adding Content

### New page in an existing section

1. Create the `.md` file in the appropriate subfolder (e.g. `dev/cases/new-topic.md`).
2. Add an entry to the section's `toc.yml`:
   ```yaml
   - name: New Topic
     href: new-topic.md
   ```
3. Rebuild.

### New section

1. Create a new folder under `dev/` or `user-guide/`.
2. Add a `toc.yml` inside it listing the section's pages.
3. Add an entry in the parent `toc.yml` pointing to the folder:
   ```yaml
   - name: My New Section
     href: my-section/
     items:
       - name: Overview
         href: my-section/overview.md
   ```
4. Rebuild.

## DocFX Markdown Features

DocFX v2 extends standard Markdown with:

### Alert boxes

```markdown
> [!NOTE]
> Informational note.

> [!TIP]
> Helpful tip.

> [!WARNING]
> Something to watch out for.

> [!IMPORTANT]
> Critical information.
```

### Cross-references

Link to other pages using relative paths:

```markdown
[See Cases Overview](../cases/overview.md)
```

Link to a C# type in the API reference using its UID:

```markdown
<xref:SWIMS.Services.Cases.ICaseLifecycleService>
```

### Code blocks

Use language hints for syntax highlighting:

````markdown
```csharp
var result = await _caseLifecycle.RefreshAsync(caseId);
```
````

## Configuration Reference (`docfx.json`)

| Key | Value | Notes |
|-----|-------|-------|
| `metadata[].src.src` | `../../` | Path from `wwwroot/docfx/` to the `.csproj` directory |
| `metadata[].src.files` | `SWIMS.csproj` | Single project |
| `metadata[].dest` | `api` | Output subfolder for generated YAML |
| `build.dest` | `../docs` | Output relative to `wwwroot/docfx/` → lands in `wwwroot/docs/` |
| `build.template` | `["default","modern"]` | Uses DocFX modern theme |
| `_enableSearch` | `true` | Enables full-text search in the generated site |
| `_appTitle` | `SWIMS Developer Documentation` | Browser tab / header title |

## Tooling

DocFX is installed as a local .NET tool pinned to v2.78.3:

```json
// .config/dotnet-tools.json
{
  "tools": {
    "docfx": {
      "version": "2.78.3",
      "commands": ["docfx"]
    }
  }
}
```

To upgrade: update the version in `dotnet-tools.json` and run `dotnet tool restore`.

## CI / Deployment Notes

- The `wwwroot/docs/` output folder is **gitignored** — it is generated at build time and should not be committed.
- In CI pipelines, add `dotnet build -p:DocFxBuildEnabled=true` as a step if you want to publish updated docs.
- The generated site is static HTML — it can be deployed to any static host independently of the SWIMS application if needed.
