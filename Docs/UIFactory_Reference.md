## UIFactory Reference (FantasyColony)

**Purpose:** Single source of reusable UI helpers. All screens (including UI Creator) should use these instead of ad-hoc code.

### Layout utilities
- `SetAnchorsPercent(RectTransform rt, float xMin, float xMax, float yMin, float yMax)` – Set normalized anchors and zero offsets.
- `ParentHasLayoutGroup(Transform t)` – True if any parent has a `Horizontal/Vertical/GridLayoutGroup`.
- `SplitPercentH(Transform parent, params float[] percents)` – Create children filling a row by percentages (no LayoutGroups). Returns `RectTransform[]`.
- `SplitPercentV(Transform parent, params float[] percents)` – Vertical counterpart.
- `EnsureLayoutElement(GameObject go)` – Return existing or add a `LayoutElement`.

### Sizing policies
- `ApplyDefaultButtonSizing(RectTransform rt, Vector2? freeSize=null)` – If outside a LayoutGroup, set explicit size (default 240×64) and center; inside a layout, suggest a stable preferred height.
- `ApplyDefaultPanelSizing(RectTransform rt, Vector2? freeSize=null)` – Same policy for panels (default 480×320).

### Menus & overlays
- `struct MenuItem { string Label; Action OnClick; bool Separator; }`
- `CreateDropdownMenu(RectTransform overlay, RectTransform anchor, IList<MenuItem> items, float rowHeight=32, float minWidth=160, bool matchAnchorWidth=true)`
  - Overlay: full-screen RectTransform, no LayoutGroup, transparent Image with `raycastTarget=true`.
  - Positions the dropdown under the anchor; sizes with `VerticalLayoutGroup + ContentSizeFitter (Preferred Y)`.

### Feedback widgets (stubs)
- `ShowToast(...)` *(planned)* – small non-blocking message.

### Raycast & borders
- `SetRaycast(GameObject go, bool on)` *(planned)* – toggle raycast on common graphics.
- `JoinBorders(params UIFrame[])` *(planned)* – hide internal seams where panels touch.

**Rules:**
- Structural containers = **Flexible**. Use anchors or LayoutGroup + flexible; do **not** add `ContentSizeFitter` to objects that are also in a LayoutGroup.
- Small widgets (dropdowns, toasts) may use `ContentSizeFitter` on the widget **root** when it isn’t inside a LayoutGroup.
