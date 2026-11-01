#!/usr/bin/env python3
"""Draws the Steam Workshop thumbnail: a field of six crops in diagonal stripes.

The game's uploader picks up mod/thumbnail.png; it must stay under 1 MB and is best
at 16:9. Tile colours are the preview palette from CropSelection, so the picture
matches what the tool draws while dragging. Text uses Futura from macOS.

    python3 scripts/make_thumbnail.py
"""

import math
import pathlib

from PIL import Image, ImageDraw, ImageFilter, ImageFont

OUT = pathlib.Path(__file__).resolve().parent.parent / "mod" / "thumbnail.png"

W, H = 1280, 720
SUPERSAMPLE = 2

PALETTE = [
    (92, 217, 77),
    (250, 199, 38),
    (89, 158, 255),
    (242, 97, 77),
    (199, 115, 250),
    (51, 219, 214),
]
SOIL_TOP = (58, 42, 30)
SOIL_BOTTOM = (28, 20, 15)
TEXT = (250, 244, 230)
SUBTEXT = (214, 200, 176)

FONT = "/System/Library/Fonts/Supplemental/Futura.ttc"


def font(style, size):
    """The face in Futura.ttc whose style name matches, at the given size."""
    for index in range(8):
        try:
            face = ImageFont.truetype(FONT, size, index=index)
        except OSError:
            break
        if face.getname()[1].lower() == style.lower():
            return face
    return ImageFont.truetype(FONT, size)


def background(w, h):
    image = Image.new("RGB", (w, h))
    draw = ImageDraw.Draw(image)
    for y in range(h):
        t = y / (h - 1)
        colour = tuple(round(a + (b - a) * t) for a, b in zip(SOIL_TOP, SOIL_BOTTOM))
        draw.line([(0, y), (w, y)], fill=colour)
    return image


def field(w, h):
    """The tiles, drawn flat on a large canvas and then rotated, so stripes run diagonally."""
    size = int(max(w, h) * 1.6)
    layer = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(layer)
    cell = 74 * SUPERSAMPLE
    gap = 9 * SUPERSAMPLE
    radius = 12 * SUPERSAMPLE
    count = size // (cell + gap) + 1
    for x in range(count):
        for y in range(count):
            colour = PALETTE[(x + y) % len(PALETTE)]
            left = x * (cell + gap)
            top = y * (cell + gap)
            draw.rounded_rectangle([left, top, left + cell, top + cell], radius, fill=colour + (255,))
            sprout(draw, left + cell / 2, top + cell / 2, cell, colour)
    return layer.rotate(-14, resample=Image.BICUBIC, expand=False)


def sprout(draw, cx, cy, cell, colour):
    """Three leaves on a tile, a shade darker than the tile, so it reads as a crop bed."""
    dark = tuple(round(c * 0.62) for c in colour) + (255,)
    r = cell * 0.13
    for angle in (90, 210, 330):
        a = math.radians(angle)
        x = cx + math.cos(a) * r * 1.1
        y = cy - math.sin(a) * r * 1.1
        draw.ellipse([x - r, y - r, x + r, y + r], fill=dark)


def fade_mask(w, h):
    """Solid on the left, fading out under the text on the right."""
    mask = Image.new("L", (w, h))
    draw = ImageDraw.Draw(mask)
    start, end = int(w * 0.40), int(w * 0.66)
    for x in range(w):
        if x <= start:
            value = 255
        elif x >= end:
            value = 0
        else:
            t = (x - start) / (end - start)
            value = round(255 * (1 - t) ** 1.6)
        draw.line([(x, 0), (x, h)], fill=value)
    return mask


def main():
    w, h = W * SUPERSAMPLE, H * SUPERSAMPLE
    image = background(w, h)

    tiles = field(w, h)
    offset = ((w - tiles.width) // 2 - int(w * 0.18), (h - tiles.height) // 2)
    placed = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    placed.paste(tiles, offset, tiles)
    alpha = Image.composite(placed.getchannel("A"), Image.new("L", (w, h)), fade_mask(w, h))
    placed.putalpha(alpha)

    shadow = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    shadow.putalpha(alpha.point(lambda a: a * 0.5))
    shadow = shadow.filter(ImageFilter.GaussianBlur(10 * SUPERSAMPLE))
    image.paste(shadow, (6 * SUPERSAMPLE, 10 * SUPERSAMPLE), shadow)
    image.paste(placed, (0, 0), placed)

    draw = ImageDraw.Draw(image)
    right = w - 80 * SUPERSAMPLE
    title = font("Bold", 118 * SUPERSAMPLE)
    subtitle = font("Medium", 40 * SUPERSAMPLE)
    for text, face, y, colour in (
        ("Chess", title, 168, TEXT),
        ("Planter", title, 296, TEXT),
        ("Every crop in one drag,", subtitle, 490, SUBTEXT),
        ("in even diagonal stripes", subtitle, 542, SUBTEXT),
    ):
        width = draw.textlength(text, font=face)
        draw.text((right - width, y * SUPERSAMPLE), text, font=face, fill=colour)

    # A strip of the six colours under the title ties the text to the field.
    swatch = 34 * SUPERSAMPLE
    step = swatch + 10 * SUPERSAMPLE
    top = 452 * SUPERSAMPLE
    for i, colour in enumerate(reversed(PALETTE)):
        left = right - swatch - i * step
        draw.rounded_rectangle([left, top, left + swatch, top + swatch // 3], 4 * SUPERSAMPLE, fill=colour)

    image = image.resize((W, H), Image.LANCZOS)
    image.save(OUT, optimize=True)
    print(f"{OUT} {OUT.stat().st_size // 1024} KB")


if __name__ == "__main__":
    main()
