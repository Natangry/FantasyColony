# Base Game UI Style (Locked)

**Purpose:** This document freezes the default look-and-feel for all in-game UI. Any time we say "use base style," screens and controls must follow this spec. No themes yet—one consistent style across the whole game.

---

## 1) Visual identity

- **Vibe:** warm, painterly fantasy; readable over dark tavern tones.
- **Primary accent:** warm gold.
- **Surfaces:** deep brown/charcoal, soft keylines—not flat black.

### Palette (hex)
- **Primary (gold):** `#D6B25E`  • **Hover:** `#E4C77D`  • **Pressed:** `#B99443`
- **Panel surface:** `#1F1A14` @ 95% opacity
- **Secondary fill:** `#2A231B`  • **Keyline:** `#5A4C38` @ 60%
- **Text primary:** `#F1E9D2`  • **Text secondary:** `#C9BDA2`
- **Danger:** `#B34844`  • **Hover:** `#C8625E`  • **Pressed:** `#953A37`

> Aim for ≥4.5:1 contrast for body text over panels.

---

## 2) Typography (TMP)

- **Titles (serif):** classic serif with small-caps feel (e.g., *Cinzel*, *IM Fell*)
    - H1: 48 sp, H2: 40 sp, H3: 32 sp
- **UI text (sans):** clean sans (e.g., *Inter*, *Noto Sans*)
    - Button: 24 sp, Body: 20 sp, Caption: 18 sp
- **Effects:** avoid heavy drop shadows; use subtle outline only if needed.

---

## 3) Layout & sizing

- Canvas Scaler: **Scale With Screen Size**, Reference 1920×1080, Match = 0.5.
- Spacing scale: **8 / 12 / 16 / 24 / 32 px**.
- Corner radius: **8 px buttons**, **12 px panels**.
- Elevation: soft shadow only on interactive surfaces.

---

## 4) Components (variants & states)

### Buttons (Primary / Secondary / Ghost / Danger)
- Height: **56 px** (min). Horizontal padding: **16–24 px**.
- Optional icon: **20–24 px**, left of label (12 px gap).
- **Primary** (gold fill, dark text): main progression actions.
- **Secondary** (panel-tone fill, light text, keyline): normal actions.
- **Ghost** (transparent fill, keyline only): tertiary actions.
- **Danger** (red fill): destructive/exit actions.

**States:** Default, Hover (+4–6% brightness), Pressed (−6–10% brightness, translate Y 1 px), Disabled (40% opacity), Focus (2 px accent outline, slight scale 1.02).

### Panels / Cards / Modals
- **Surface Panel:** fill `#1F1A14` @ 95%, 12 px radius, 1 px inner keyline `#5A4C38` @ 60%, soft shadow.
- **Scrim Panel:** black @ 50–60% (or gradient) for readability over art.
- **Modal:** surface panel + darker scrim (70–80%); headline + primary/secondary buttons.

### Inputs (World Setup later)
- Text field / Dropdown: surface fill, 1 px keyline; focus glow in gold.
- Slider: groove `#5A4C38`, thumb `#D6B25E` with hover ring.

### Lists / Menus
- Row height: **48–56 px**; selected row has **2 px gold left rule**.
- Scrollbars: thin; track `#5A4C38`, thumb `#C9BDA2` @ 60%.
- Menu Bar: horizontal surface panel; items styled as **Ghost** buttons.

---

## 5) Motion & SFX

- Hover: **60–80 ms**, Press: **80–100 ms**, Panel fade: **120–160 ms**.
- Easing: cubic-out (hover), cubic-in-out (panel fades).
- UI sounds: `ui_hover`, `ui_click`, `ui_back` (soft wood taps).

---

## 6) Main Menu spec (applies base style)

- Background: painterly tavern art.
- Bottom-right **MenuPanel** (Surface panel + vertical stack, 24 px padding, 12 px spacing).
- **Button order:** Start, Continue, Load, Mods, Creator, Restart, Quit.
- **Variants:** Start = Primary, Quit = Danger, others = Secondary.
- Scrim under panel for readability over art.

---

## 7) Mapping rules (automatic choices)

- **Primary** → confirm/advance/start/major apply.
- **Secondary** → open/close panels, navigation, options.
- **Ghost** → tertiary links or tool panels.
- **Danger** → delete/reset/quit.
- **Disabled** when action not available (e.g., Continue with no saves).

---

## 8) Minimal assets (optional)

We can ship with **no art** (square corners) using simple Image fills. For rounded corners later, add these 9-slice sprites and tint them:

```
Assets/Resources/ui/sprites/9slice/panel_card_9s.png     # 12 px corners
Assets/Resources/ui/sprites/9slice/button_fill_9s.png    # 8 px corners
```

Set Image Type = **Sliced** and enter matching borders (12 or 8 px) in Sprite Editor.

---

## 9) Acceptance checklist

- Buttons/panels look identical across screens.
- Clear hover/pressed/disabled/focus states.
- Readable at 1080p and 4K.
- No ad-hoc colors/fonts; everything follows this doc.

---

## 10) Reference mocks (optional to commit later)

Keep the two reference images in LFS when ready:

- `Docs/Design/Art/UI_Buttons_Reference.png`
- `Docs/Design/Art/UI_Menus_Reference.png`

These are not required to use the style; they’re visual guides.

