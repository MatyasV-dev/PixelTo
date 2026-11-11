# PixelTo

A simple pixel art editor with layers, built with C# and Windows Forms.

> Status: work in progress. The core features are done, some are still in development (see [TODO.md](TODO.md)).

## Features

**Drawing**
- Brush with adjustable size (1-8 px)
- Eraser, bucket fill, eyedropper
- Line (Bresenham's algorithm), outlined and filled rectangle
- Primary and secondary color (left / right mouse button)
- Color opacity slider
- Middle mouse button = quick eraser
- Symmetry on the X and Y axes

**Layers**
- Add, delete, hide and rename (right-click the layer name)
- Reorder layers, merge the active layer with the one below
- Highlight a layer's pixels (hold the middle mouse button on the layer)
- Move tool for the contents of a layer

**Images and files**
- Import an image with moving, resizing and snapping to the canvas center and edges
- Undo / Redo (Ctrl+Z / Ctrl+Y) across all layers
- Save and open projects (`.ptx` format, JSON)
- PNG export with selectable scale (1x-10x)

**View**
- Grid, transparency checkerboard, canvas center guide, pixel coordinates
- Zoom (mouse wheel, Ctrl +/-/0)
- Canvas presets: 16x16, 64x64, 128x128, 256x256 or custom size

## Keyboard shortcuts

| Shortcut | Action |
|---|---|
| Ctrl+Z / Ctrl+Y | Undo / Redo |
| Ctrl+S / Ctrl+O | Save / Open |
| Ctrl+E | Export PNG |
| Ctrl + / - / 0 | Zoom in / out / reset |
| Enter / Esc | Confirm / cancel image import |

## Getting started

Requirements: Windows, Visual Studio 2022 or newer, .NET (version as set in `PixelTo.csproj`).

1. Clone the repository
2. Open `PixelTo.sln` in Visual Studio
3. Run (F5)

## Tech stack

C#, Windows Forms, System.Text.Json

## Author

Matyáš Vrbka
