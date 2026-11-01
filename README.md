# Chess Planter

A Timberborn mod that plants several crops in one drag. It adds a **Mixed planting**
tool to the Fields group:

- Pick crops in the tool's panel. Click a crop to switch it on or off. The selection
  is remembered.
- Drag an area. The fields are marked in diagonal stripes, one crop per diagonal, so
  each picked crop gets the same share. With two crops the result is a chessboard.
- While you drag, each tile is previewed in the colour of its crop's frame in the
  picker.
- Fields that are already marked stay as they are. Crops that are still locked are
  skipped.
- The stripes follow world coordinates, so areas dragged next to each other line up.

Water crops start switched off because they would only leave gaps on dry land.

## Build

```bash
./build.sh
```

This builds `mod/Scripts/Timberborn.FabianPnke.ChessPlanter.dll` and copies `mod/` to
`~/Documents/Timberborn/Mods/ChessPlanter`. The tool icon is drawn by
`scripts/make_icon.py` and the Workshop thumbnail by `scripts/make_thumbnail.py`.

To publish, open Mods in the game's main menu and click "Upload to Steam Workshop". For an
update, bump `Version` in `mod/manifest.json`, run `./build.sh` and upload again.

Built against Timberborn 1.1.2.8. The decompiled game sources used as reference are
in `../timberborn-companion-v2/reference/1.1.2.8/decompiled`.
