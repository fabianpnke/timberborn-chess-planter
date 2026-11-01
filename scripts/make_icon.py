#!/usr/bin/env python3
"""Draws the tool button icon: a small field in diagonal stripes.

Colours are the first entries of the preview palette in CropSelection, so the icon
looks like what the tool draws on the map. Drawn large and downsampled for
antialiasing.

    python3 scripts/make_icon.py
"""

import json
import pathlib

from PIL import Image, ImageDraw

OUT = pathlib.Path(__file__).resolve().parent.parent / "mod" / "Sprites" / "BottomBar"

SUPERSAMPLE = 8
SIZE = 128
S = SIZE * SUPERSAMPLE

COLOURS = [
    (92, 217, 77, 255),
    (250, 199, 38, 255),
    (89, 158, 255, 255),
]
CELLS = 4
MARGIN = int(S * 0.10)
GAP = int(S * 0.035)
RADIUS = int(S * 0.04)


def main():
    image = Image.new("RGBA", (S, S), (0, 0, 0, 0))
    draw = ImageDraw.Draw(image)
    cell = (S - 2 * MARGIN - (CELLS - 1) * GAP) / CELLS
    for x in range(CELLS):
        for y in range(CELLS):
            # Same rule as StripePattern, flipped so the stripes run like on the map.
            colour = COLOURS[(x + (CELLS - 1 - y)) % len(COLOURS)]
            left = MARGIN + x * (cell + GAP)
            top = MARGIN + y * (cell + GAP)
            draw.rounded_rectangle([left, top, left + cell, top + cell], RADIUS, fill=colour)
    image = image.resize((SIZE, SIZE), Image.LANCZOS)

    OUT.mkdir(parents=True, exist_ok=True)
    image.save(OUT / "ChessPlanterTool.png")
    (OUT / "ChessPlanterTool.png.meta.json").write_text(json.dumps({"isSprite": True}, indent=2) + "\n")


if __name__ == "__main__":
    main()
