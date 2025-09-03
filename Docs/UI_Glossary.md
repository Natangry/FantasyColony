## UI Glossary (FantasyColony — UI Creator)

Authoritative terms and patterns used by the runtime uGUI Creator. This aligns with `/Docs/UI_Creator_Spec.md` rules: runtime-only uGUI, structural containers are **Flexible**, no `ContentSizeFitter` on LayoutGroup children, pixel-accurate borders, default wood skin for app UI.

### A. Core objects (uGUI)
- **Canvas** – Root for all uGUI. Our project uses Screen Space – Overlay and a CanvasScaler.
- **CanvasScaler (Scale With Screen Size)** – Scales the whole UI to a reference resolution; exposes `scaleFactor`.
- **GraphicRaycaster** – Enables pointer events on a Canvas.
- **EventSystem** – Input router for UI; exactly one input module active.
- **RectTransform** – UI transform with anchors/pivot/offsets.
- **Image** – Basic visual (sprite, color, sliced/tiled).
- **Text (uGUI)** – Label. We use Best Fit selectively.

### B. Layout & sizing
- **Anchors** – 0–1 normalized min/max → true percent sizing.
- **Pivot** – 0–1 reference point for rotation/scaling and offset.
- **Offsets (sizeDelta)** – Extra margins relative to anchors.
- **LayoutGroup** – Auto layout for children (Horizontal/Vertical/Grid).
- **LayoutElement** – Per-child min / preferred / flexible sizes.
- **Flexible width/height** – Ratio-like weight to divide remaining space.
- **Preferred size** – Natural size requested by a child.
- **Child Force Expand** – LayoutGroup option to stretch children.
- **ContentSizeFitter** – Sizes to content. **Never** combine with LayoutGroup on the same object.
- **PanelSizing.Flexible** – Project rule: containers expand via anchors or flexible sizes; no Fitters.

### C. Pixels & resolution
- **Reference resolution** – Target resolution for CanvasScaler.
- **scaleFactor** – Multiplier from reference pixels to device pixels.
- **referencePixelsPerUnit** – Sprite density (does not affect RectTransform size).
- **Pixel-perfect** – Canvas option to round to device pixels.
- **Device pixels vs UI units** – To get N px thickness: `size = N / scaleFactor`.

### D. Sprites & 9-slice
- **Sprite.border (L,B,R,T)** – Nine-slice margins in pixels.
- **Sliced** – Stretch center, keep borders.
- **Tiled** – Repeat texture to fill.
- **Sprite PPU** – Geometry density for sprites.

### E. Interaction & input
- **Raycast Target** – If true, element blocks clicks. Keep non-interactive fills as false.
- **Navigation** – D-pad/keyboard move between Selectables.
- **Hotkeys** – F10 (open Creator), F11 (fullscreen stage toggle).

### F. Project building blocks
- **UIFactory** – Helpers to build panels, rows/cols, buttons with the default skin and correct layout.
- **UIFrame** – Wood borders via four edge images; pixel-accurate thickness.
- **BaseUIStyle** – Tints, fonts, `TargetBorderPx = 10f`, shared look.
- **UIPixelSnap** – Avoid sub-pixel blur on thin borders.
- **GrayscaleSpriteCache** – Grayscale sprite preparation.

### G. Screen/flow framework
- **IScreen** – Runtime screen (`Enter`, `Exit`).
- **UIRouter** – Screen stack (`Push`, `Pop`, `PopAll`).
- **UIRoot** – Boots Canvas, EventSystem, initial screen.

### H. Creator-specific terms
- **Toolbar (Creator)** – Top menu (File, Edit, View, Tools, Close). Default height 5%, togglable to 0%.
- **Stage** – Large work area where widgets appear.
- **Menu Overlay** – Fullscreen transparent catcher that hosts dropdowns and closes on outside click/Esc.
- **Blueprint** – JSON representation of screens/widgets (Step 2).
- **Binding / IUIBinding** – Interface to connect widgets to data/actions (Step 2).
- **ActionMapBinding** – Maps Creator commands to input actions (Step 2).
- **Designer Shell** – Multi-pane UI for advanced editing (Step 4).
- **Placement Flow** – Guided creation of a widget (Step 5).
- **Drag/Resize/Snap/Anchors** – Manipulation tools (Step 6).
- **Ratio Containers / ViewStack / Nav Group** – Responsive containers and navigable stacks (Step 7).
- **Preview & Lint** – Live preview and rule checks (Step 8).
- **Background Image Options** – Stage/panel backdrop controls (Step 9).
- **Self-editing & Rollback** – Edit the Creator with itself; undo/versions (Step 10).

### I. Layout patterns
- **Master–Detail / Split View** – Left list, right detail; horizontal container with right pane flexible.
- **Two-Pane (Stacked)** – Vertical stack at narrow widths, split horizontally at wide widths.
- **Docked Panels** – Movable side panels; fixed for v1.
- **Grid / List View** – ScrollRect + LayoutGroup.
- **Tabs** – Row of tabs + ViewStack.
- **Accordion** – Collapsible sections.
- **Wizard / Stepper** – Multi-step flow.
- **Breadcrumbs** – Hierarchy trail navigation.
- **Sidebar / Drawer** – Collapsible vertical navigation.

### J. Menus & overlays
- **Menu Bar** – Top-level categories.
- **Dropdown Menu** – Anchored panel below a button.
- **Context Menu** – Pointer-positioned actions for selection.
- **Command Palette** – Type-to-run action list.
- **Modal Dialog** – Blocks until resolved.
- **Sheet / Drawer Modal** – Slides from an edge.
- **Popover** – Small contextual bubble.
- **Tooltip** – Hover help.
- **Toast / Snackbar** – Temporary non-blocking notice.
- **Banner Alert** – Persistent message at top.

### K. Editor features to consider
- Palette, Inspector, Outline/Hierarchy, Layers, Rulers & Guides, Snap Grid, Marquee Select, Gizmo Handles, Alignment & Distribute, Search/Filter, Empty State, Loading Skeleton, Progress Indicators, Status Bar.

### L. Widgets (Creator → Tools → Add …)
- **Primary Button** – Prominent action (wood + primary tint).
- **Secondary Button** – Neutral action.
- **Danger Button** – Destructive/confirm.
- **Panel** – Framed container (wood fill + border).
- **Background Panel** – Full-bleed non-interactive backdrop.
- *(Future)* Label, Toggle, Slider, Dropdown, Input Field, Image (Decor).

