# TODO

## In progress
- [ ] Resizing a layer with the "Move layer" tool (the handle is shown, the scaling itself is still being tuned)
- [ ] Image import: keyboard focus issues after using some tools (fixed via Focus, needs verification)

## Planned
- [ ] UI redesign: icons on tool buttons instead of letters
- [ ] Multiple languages (resx files)
- [ ] Real-size preview (1x, 2x, 4x) in a separate panel
- [ ] More tools: circle, selection, layer flip and rotate
- [ ] Animation and GIF export

## Code cleanup
- [ ] Remove duplicate conditions (`GetCenterLineColor`, `InitKeyboard`)
- [ ] Remove unused variables and leftovers from experiments
- [ ] Split `Form1.cs` into smaller parts (palette, tools, layers, menu)
- [ ] Performance: replace per-pixel `FillRectangle` rendering with a bitmap
- [ ] Undo: store only changed layers instead of all of them
