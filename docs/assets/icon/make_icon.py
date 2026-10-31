"""Package icon of LyricsScraperNET: a mosaic eighth note (warm tiles) and three lyric lines (teal and blue tiles)
in a cream circle with a terracotta ring, in the style of skuill.com. Pure Python, no packages needed.

Usage: python make_icon.py <output directory>   (writes icon-128.png for the package and icon-512.png)"""
import math
import os
import random
import struct
import sys
import zlib

OUT_DIR = sys.argv[1]

CREAM = (249, 246, 239)
RING = (200, 85, 55)
WARM = [(232, 163, 61)] * 5 + [(222, 140, 48)] * 3 + [(242, 211, 138)] * 2 + [(196, 80, 50)] * 1
COOL = [(74, 158, 142)] * 4 + [(143, 176, 200)] * 3 + [(46, 106, 158)] * 3 + [(31, 77, 63)] * 1

GRID_SIZE = 32


MOTIF = [
    ".......nnn..........",
    ".......nnnn.........",
    ".......nn.nn........",
    ".......nn..nn.......",
    ".......nn..nn.......",
    ".......nn...n.......",
    ".......nn.....llllll",
    ".......nn...........",
    ".......nn...........",
    "....nnnnn.....lllll.",
    "..nnnnnnn...........",
    ".nnnnnnnn...........",
    ".nnnnnnn......llllll",
    "..nnnnn.............",
]


def build_grid():
    """Places MOTIF ('n' note tile, 'l' lyric line tile) into the grid; centering happens below."""
    cells = [["."] * GRID_SIZE for _ in range(GRID_SIZE)]
    for row, line in enumerate(MOTIF):
        for col, ch in enumerate(line):
            cells[row][col] = ch
    return cells


GRID = ["".join(r) for r in build_grid()]
_used = [(r, c) for r, line in enumerate(GRID) for c, ch in enumerate(line) if ch != "."]
_rows = [r for r, _ in _used]
_cols = [c for _, c in _used]
ROW_SHIFT = GRID_SIZE / 2 - (min(_rows) + max(_rows) + 1) / 2
COL_SHIFT = GRID_SIZE / 2 - (min(_cols) + max(_cols) + 1) / 2


def tiles(seed=7):
    rnd = random.Random(seed)
    result = []
    for row, line in enumerate(GRID):
        for col, ch in enumerate(line):
            if ch == ".":
                continue
            palette = WARM if ch == "n" else COOL
            color = rnd.choice(palette)
            w = rnd.uniform(0.76, 0.9)
            h = rnd.uniform(0.7, 0.86)
            cx = col + 0.5 + COL_SHIFT + rnd.uniform(-0.06, 0.06)
            cy = row + 0.5 + ROW_SHIFT + rnd.uniform(-0.06, 0.06)
            angle = math.radians(rnd.uniform(-7, 7))
            result.append((cx, cy, w, h, angle, color))
    return result


def render(size, supersample):
    big = size * supersample
    cell = big / GRID_SIZE
    center = big / 2.0
    ring_outer = big * 0.485
    ring_inner = ring_outer - big * 0.022
    # Canvas: None = transparent.
    canvas = [[None] * big for _ in range(big)]
    for y in range(big):
        dy = y + 0.5 - center
        row = canvas[y]
        for x in range(big):
            dx = x + 0.5 - center
            d = math.hypot(dx, dy)
            if d <= ring_inner:
                row[x] = CREAM
            elif d <= ring_outer:
                row[x] = RING

    for cx, cy, w, h, angle, color in tiles():
        cos_a, sin_a = math.cos(angle), math.sin(angle)
        hw, hh = w * cell / 2, h * cell / 2
        px, py = cx * cell, cy * cell
        reach = int(math.hypot(hw, hh)) + 2
        for y in range(max(0, int(py) - reach), min(big, int(py) + reach)):
            for x in range(max(0, int(px) - reach), min(big, int(px) + reach)):
                rx, ry = x + 0.5 - px, y + 0.5 - py
                u = rx * cos_a + ry * sin_a
                v = -rx * sin_a + ry * cos_a
                if abs(u) <= hw and abs(v) <= hh:
                    canvas[y][x] = color

    # Downsample with alpha.
    rows = []
    n = supersample * supersample
    for y in range(size):
        out = bytearray([0])
        for x in range(size):
            r = g = b = a = 0
            for sy in range(supersample):
                src = canvas[y * supersample + sy]
                for sx in range(supersample):
                    c = src[x * supersample + sx]
                    if c is not None:
                        r += c[0]; g += c[1]; b += c[2]; a += 1
            if a:
                out += bytes((r // a, g // a, b // a, a * 255 // n))
            else:
                out += b"\x00\x00\x00\x00"
        rows.append(bytes(out))
    return b"".join(rows)


def write_png(path, size, raw):
    def chunk(kind, data):
        return struct.pack(">I", len(data)) + kind + data + struct.pack(">I", zlib.crc32(kind + data) & 0xFFFFFFFF)

    header = struct.pack(">IIBBBBB", size, size, 8, 6, 0, 0, 0)
    open(path, "wb").write(b"\x89PNG\r\n\x1a\n" + chunk(b"IHDR", header) + chunk(b"IDAT", zlib.compress(raw, 9)) + chunk(b"IEND", b""))


_far = max(math.hypot(cx - GRID_SIZE / 2, cy - GRID_SIZE / 2) + math.hypot(w, h) / 2 for cx, cy, w, h, _, _ in tiles())
print(f"motif radius {_far:.2f} cells, ring inner radius {GRID_SIZE * (0.485 - 0.022):.2f} cells, free {1 - _far / (GRID_SIZE * (0.485 - 0.022)):.0%}")

for size, ss in ((128, 4), (512, 2)):
    write_png(os.path.join(OUT_DIR, f"icon-{size}.png"), size, render(size, ss))
