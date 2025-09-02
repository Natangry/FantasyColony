# UI Creator – Functional & Technical Specification (v1)

> Status: Draft for Step 1 (Dev entry). This document will evolve as features land.

## 1. Purpose
The UI Creator is an in-game, runtime editor for building menus and screens without Unity scenes. It outputs a versioned JSON "blueprint" that the game can load, preview, and wire to logic. It standardizes styling (wood fill + border), sizing, and layout across the project.

## 2. Non-Goals (v1)
- No 3D layout or world-space canvases.
- No animation authoring (future).
- No custom shader graph editing.

## 3. Design Tenets
- **Data-first**: JSON blueprints (versioned). No scenes.
- **Deterministic layout**: Structural panels are **Flexible** (fill); small widgets can be Auto* (shrink). No unintended ContentSizeFitter + LayoutGroup conflicts.
- **Composable**: Overlay/panel bases; ratios (splits/rows/cols) or anchors; joinable borders.
- **Self-documenting**: Every node supports **notes** (not rendered) for intent.
- **Safe & reversible**: Preview sandbox, hot-swap with rollback for self-editing.

## 4. User Flow
1. Open Creator via hotkey (**F10**; dev only).
2. Choose **New** or **Modify Existing**.
3. Designer workspace: Palette (widgets), Canvas (overlay/board), Inspector (properties), Toolbar (save/preview), Outline (hierarchy).
4. Add items → drag/resize with snap → set anchors or ratios → **Place** (fill required info + notes) → Save.
5. Preview → iterate.

## 5. Blueprint JSON (v1) – Schema Overview
- **Containers**: `overlay`, `inset`, `split-horizontal`, `split-vertical`, `row`, `col`, `panel`, `viewstack`.
- **Widgets**: `button`, `label`, `list`, `rule`, `spacer`.
- **Common properties**
  - `id` (string, unique in tree)
  - `notes` (string, optional; not rendered)
  - `skin` (optional): `{ "wood": true, "border": true, "edges": { "left": true, "right": true, "top": true, "bottom": true } }`
  - `anchors` (freeform mode): `{ "min": [x,y], "max": [x,y], "pos": [x,y], "size": [w,h] }`
  - `ratio` (ratio mode): numbers or arrays depending on container
  - `padding` (int or 4-array), `spacing` (int)
  - `children`: array of nodes
- **Overlay background** (optional):
  ```json
  "background": {
    "sprite": "UI/Backgrounds/MainMenu",
    "mode": "cover",  
    "tint": [1,1,1,1],
    "dim": 0.2,
    "vignette": true
  }
  ```
- **Panel properties**: `panelSizing` ("Flexible"|"AutoHeight"|"AutoWidth"|"AutoBoth"), optional `title`.
- **Button properties**: `label`, `kind` ("primary"|"secondary"), `onClick` (string id), `tooltip`, `targetView` (for nav→viewstack).
- **Label properties**: `text` or `textKey`, `style` ("title"|"body"|"caption"|"italic").
- **List properties**: `title`, `minHeight`.
- **Rule properties**: `thickness`, `alpha`.
- **ViewStack**: `{ "id": "detail", "defaultView": "infoView", "children": [ { "type": "panel", "id": "infoView" }, ... ] }`

### 5.1 Example (Master–Detail Character Sheet)
```json
{
  "v": 1,
  "type": "overlay",
  "children": [{
    "type": "split-horizontal", "ratio": [0.28, 0.72], "joinEdges": true, "children": [
      { "type": "panel", "id": "nav", "children": [
        { "type": "col", "children": [
          { "type": "button", "id": "nav_info",  "label": "Info",  "onClick": "UI.ShowView", "targetView": "infoView"  },
          { "type": "button", "id": "nav_stats", "label": "Stats", "onClick": "UI.ShowView", "targetView": "statsView" }
        ] }
      ]},
      { "type": "viewstack", "id": "detail", "defaultView": "infoView", "children": [
        { "type": "panel", "id": "infoView" },
        { "type": "panel", "id": "statsView" }
      ] }
    ]
  }]
}
```

## 6. Sizing Model
- **Flexible**: structural containers (fills available width/height via LayoutElement.flex). No ContentSizeFitter attached.
- **AutoHeight/AutoWidth/AutoBoth**: shrink-to-content widgets or stacks. ContentSizeFitter is attached appropriately.

## 7. Layout Modes
- **Freeform (Anchors)**: Absolute placement with anchors; supports snap-to-grid/edges.
- **Ratio (Splits/Rows/Cols)**: Percent/weight-based containers; exact math for W/H equalities; perfect scaling.

## 8. Skinning & Borders
- Panels/buttons spawn pre-decorated with wood tile and a frame. `UIFrame` offers per-edge toggles to hide inner seams when panels touch.
- Overlays/panels can have image backgrounds (cover/contain/stretch/tile) instead of wood; optional tint/dim/vignette.

## 9. Placement Flow & Required Info
- Add → drag/resize → **Place** → fill dialog:
  - Common: `id` (unique), **Notes** (multiline), Decor (wood/border), Edges (L/R/T/B)
  - Panel: `panelSizing`, optional `title`
  - Button: `label`, `kind`, `onClick`, `tooltip`, optional `targetView`
  - Label: `text|textKey`, `style`
  - List: `title`, `minHeight`
  - Rule: `thickness`, `alpha`
- Validation: unique ids, sane sizes, mandatory fields.

## 10. ViewStack (Master–Detail)
- Contains multiple child panels; only one visible.
- `targetView` on buttons with `onClick: "UI.ShowView"` switches current view.
- Optional Nav Group highlights selected button.

## 11. Linting & Preview
- Lint rules:
  - Duplicate ids
  - Zero-size or off-screen elements
  - Fitter + LayoutGroup on structural nodes
  - Invalid ratios (<=0)
- Preview instantiates a temporary runtime build of the blueprint.

## 12. Binding & Actions
- `IUIBinding` + `ActionMapBinding` wire `onClick` ids to game code.
- Localization-ready via `textKey` (future Loc service).

## 13. Self-Editing & Safety
- Creator loads its own blueprint (`creator_screen.json`).
- Sandbox preview on separate canvas; **Apply to Self** hot-swaps the UI.
- Rollback: `.backup.json`; Safe Mode (Shift on open).

## 14. Migration Plan
- Convert Confirm Dialog → Main Menu → Mods.
- Keep router feature flag until parity; remove legacy after sign-off.

## 15. Troubleshooting
- **Panel shrinks unexpectedly** → ensure `PanelSizing.Flexible` for structural containers.
- **Duplicate LayoutGroup warning** → only one LayoutGroup per GameObject; configure the one that exists.
- **Double borders between panels** → use `UIFrame.SetEdges` or factory `JoinHorizontal/JoinVertical`.
- **Background image stretches weirdly** → check `background.mode` (cover/contain/stretch/tile).

## 16. Glossary
- **Structural panel**: container meant to fill available space (columns, rails, boards).
- **Widget**: interactive or content element (button, label, list, rule).
- **ViewStack**: container that shows exactly one child panel at a time.

