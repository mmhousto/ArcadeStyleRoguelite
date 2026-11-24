"""
Generates Ricomet's neon vector art.

Writes SVG source shapes to Art/Vector/ and exports glowing PNG sprites to
Assets/Resources/Art/ for the game to load. Shapes are white so the game can tint
them with the gameplay colors (player cyan, enemy magenta, danger orange...).

Usage (from the repository root):
    pip install cairosvg pillow numpy
    python3 Tools/generate_art.py

Edit a shape below (or an SVG, then remove it from SHAPES) and run again.
"""
import io
import math
import os

import cairosvg
import numpy as np
from PIL import Image, ImageFilter

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
SVG_DIR = os.path.join(ROOT, "Art", "Vector")
PNG_DIR = os.path.join(ROOT, "Assets", "Resources", "Art")

SIZE = 256
# Every gameplay shape fits inside this radius around the center, leaving room for glow.
# The game uses it to size sprites in world units (see SpriteLibrary.ShapeRadiusPixels).
R = 88
C = SIZE / 2


def svg(body, stroke=10, size=SIZE):
    return (f'<svg xmlns="http://www.w3.org/2000/svg" width="{size}" height="{size}" viewBox="0 0 {size} {size}">'
            f'<g fill="none" stroke="#ffffff" stroke-width="{stroke}" stroke-linejoin="round" stroke-linecap="round">'
            f'{body}</g></svg>')


def poly(points, closed=True, extra=""):
    pts = " ".join(f"{x:.1f},{y:.1f}" for x, y in points)
    tag = "polygon" if closed else "polyline"
    return f'<{tag} points="{pts}" {extra}/>'


def regular(n, radius, rotation_deg=0.0, cx=C, cy=C):
    # SVG y points down; rotate counter-clockwise like Unity by negating the angle.
    return [(cx + radius * math.cos(math.radians(rotation_deg + i * 360 / n)),
             cy - radius * math.sin(math.radians(rotation_deg + i * 360 / n))) for i in range(n)]


def circle(r, cx=C, cy=C, extra=""):
    return f'<circle cx="{cx:.1f}" cy="{cy:.1f}" r="{r:.1f}" {extra}/>'


def filled(body):
    return f'<g fill="#ffffff">{body}</g>'


# ---------------------------------------------------------------- gameplay shapes

SHAPES = {
    # Comet: bright ring with a hot core.
    "comet": svg(circle(R - 4) + filled(circle(R * 0.32)), stroke=12),
    # Drifter: triangle pointing +X, with an inner triangle.
    "drifter": svg(poly(regular(3, R)) + poly(regular(3, R * 0.42), extra='stroke-width="7"')),
    # Dart: arrowhead pointing +X.
    "dart": svg(poly([(C + R, C), (C - R * 0.6, C - R * 0.55), (C - R * 0.3, C), (C - R * 0.6, C + R * 0.55)])),
    # Splitter: hexagon with a crack line showing it will break apart.
    "splitter": svg(poly(regular(6, R, 30)) + poly(regular(6, R * 0.5, 30), extra='stroke-width="7"')),
    "splitter_cracked": svg(poly(regular(6, R, 30))
                            + poly([(C - R * 0.5, C - R * 0.75), (C - R * 0.1, C - R * 0.2), (C + R * 0.2, C + R * 0.1), (C + R * 0.05, C + R * 0.8)], closed=False, extra='stroke-width="7"')),
    # Prism core: outer and inner triangles drawn separately so they can spin opposite ways.
    "prism_outer": svg(poly(regular(3, R, 90)), stroke=12),
    "prism_inner": svg(poly(regular(3, R * 0.7, 270)) + filled(circle(R * 0.16)), stroke=9),
    # Enemy orb.
    "orb": svg(circle(R * 0.8) + filled(circle(R * 0.38)), stroke=12),
    # Charge pip and Shatter fragment.
    "pip": svg(filled(circle(R * 0.7))),
    "shard": svg(poly([(C + R, C), (C, C - R * 0.28), (C - R, C), (C, C + R * 0.28)]), stroke=10),
    # Thin ring for landing / shockwave pulses.
    "ring": svg(circle(R + 20), stroke=6),
    # HUD shield.
    "shield": svg('<path d="M128 44 L196 70 L190 140 Q180 196 128 216 Q76 196 66 140 L60 70 Z"/>', stroke=14),
    "shield_full": svg('<path d="M128 44 L196 70 L190 140 Q180 196 128 216 Q76 196 66 140 L60 70 Z" fill="#ffffff" fill-opacity="0.45"/>', stroke=14),
    # Pause icon.
    "icon_pause": svg('<line x1="100" y1="70" x2="100" y2="186"/><line x1="156" y1="70" x2="156" y2="186"/>', stroke=22),
}

# ---------------------------------------------------------------- upgrade icons (one per upgrade ID)

ICONS = {
    # Rebound: dash bouncing off a wall.
    "icon_bounce_wall": svg('<line x1="196" y1="48" x2="196" y2="208" stroke-width="14"/>'
                            + poly([(56, 70), (176, 128), (56, 186)], closed=False)
                            + poly([(70, 160), (56, 186), (84, 192)], closed=False, extra='stroke-width="8"'), stroke=12),
    # Seeker: curved path ending at a target.
    "icon_bounce_enemy": svg('<path d="M50 200 Q60 80 170 90"/>' + circle(28, 192, 88) + filled(circle(8, 192, 88)), stroke=12),
    # Shockwave: dot with expanding arcs.
    "icon_impact_shockwave": svg(filled(circle(18)) + circle(50) + circle(84, extra='stroke-dasharray="40 22"'), stroke=10),
    # Plasma Trail: wavy streak behind a dot.
    "icon_trail_fire": svg('<path d="M40 170 Q80 110 110 150 T170 110"/><path d="M40 210 Q80 150 110 190 T170 150" stroke-opacity="0.6"/>'
                           + filled(circle(22, 196, 96)), stroke=12),
    # Arc: lightning bolt.
    "icon_chain_lightning": svg(poly([(150, 36), (86, 136), (132, 136), (104, 220), (176, 112), (128, 112)]), stroke=11),
    # Shatter: burst of shards.
    "icon_split_shatter": svg("".join(
        f'<line x1="{C + 30 * math.cos(math.radians(a)):.1f}" y1="{C - 30 * math.sin(math.radians(a)):.1f}" '
        f'x2="{C + 92 * math.cos(math.radians(a)):.1f}" y2="{C - 92 * math.sin(math.radians(a)):.1f}"/>'
        for a in range(0, 360, 60)) + filled(circle(16)), stroke=12),
    # Reserve: battery with a plus.
    "icon_energy_capacity": svg('<rect x="58" y="84" width="128" height="88" rx="12"/><line x1="200" y1="110" x2="200" y2="146" stroke-width="16"/>'
                                '<line x1="122" y1="104" x2="122" y2="152"/><line x1="98" y1="128" x2="146" y2="128"/>', stroke=12),
    # Siphon: circular refund arrows.
    "icon_energy_refund": svg('<path d="M70 110 A64 64 0 0 1 186 104"/><path d="M186 146 A64 64 0 0 1 70 152"/>'
                              + poly([(172, 78), (190, 108), (156, 112)], closed=False) + poly([(84, 178), (66, 148), (100, 144)], closed=False), stroke=12),
    # Clarity: eye.
    "icon_energy_focus": svg('<path d="M36 128 Q128 40 220 128 Q128 216 36 128 Z"/>' + circle(30) + filled(circle(12)), stroke=11),
    # Overdrive: double chevrons.
    "icon_energy_rush": svg(poly([(70, 64), (134, 128), (70, 192)], closed=False) + poly([(130, 64), (194, 128), (130, 192)], closed=False), stroke=16),
    # Bulwark: shield with a plus.
    "icon_def_shield": svg('<path d="M128 40 L198 68 L192 140 Q182 198 128 218 Q74 198 64 140 L58 68 Z"/>'
                           '<line x1="128" y1="96" x2="128" y2="168"/><line x1="92" y1="132" x2="164" y2="132"/>', stroke=12),
    # Phase: ghosted, dashed circle.
    "icon_def_phase": svg(circle(80, extra='stroke-dasharray="30 20"') + circle(36, extra='stroke-opacity="0.6"'), stroke=12),
}

# ---------------------------------------------------------------- UI (nine-sliced at runtime)

UI_SIZE = 96
UI = {
    "ui_frame": svg('<rect x="18" y="18" width="60" height="60" rx="14"/>', stroke=4, size=UI_SIZE),
    "ui_fill": f'<svg xmlns="http://www.w3.org/2000/svg" width="{UI_SIZE}" height="{UI_SIZE}"><rect x="18" y="18" width="60" height="60" rx="14" fill="#ffffff"/></svg>',
}


def export(name, source, size, glow_radius, glow_strength):
    with open(os.path.join(SVG_DIR, name + ".svg"), "w") as f:
        f.write(source)

    png = cairosvg.svg2png(bytestring=source.encode(), output_width=size, output_height=size)
    shape = Image.open(io.BytesIO(png)).convert("RGBA")
    alpha = shape.getchannel("A")

    # Neon glow: a blurred copy of the shape behind the crisp line.
    if glow_radius > 0:
        glow = alpha.filter(ImageFilter.GaussianBlur(glow_radius))
        glow = np.clip(np.array(glow, dtype=np.float32) * glow_strength, 0, 255)
        alpha = Image.fromarray(np.maximum(np.array(alpha, dtype=np.float32), glow).astype(np.uint8))

    # White everywhere (even fully transparent pixels) so filtering never adds dark fringes.
    out = Image.new("RGBA", (size, size), (255, 255, 255, 0))
    out.putalpha(alpha)
    out.save(os.path.join(PNG_DIR, name + ".png"), optimize=True)


def spark():
    """Soft round dot for particles."""
    size = 64
    img = Image.new("RGBA", (size, size), (255, 255, 255, 0))
    px = img.load()
    for y in range(size):
        for x in range(size):
            d = math.hypot(x - size / 2 + 0.5, y - size / 2 + 0.5) / (size / 2)
            a = max(0.0, 1.0 - d) ** 2
            px[x, y] = (255, 255, 255, int(255 * a))
    img.save(os.path.join(PNG_DIR, "spark.png"), optimize=True)


def main():
    os.makedirs(SVG_DIR, exist_ok=True)
    os.makedirs(PNG_DIR, exist_ok=True)

    for name, source in SHAPES.items():
        export(name, source, SIZE, glow_radius=9, glow_strength=1.6)
    for name, source in ICONS.items():
        export(name, source, SIZE, glow_radius=7, glow_strength=1.3)
    for name, source in UI.items():
        export(name, source, UI_SIZE, glow_radius=5 if name == "ui_frame" else 0, glow_strength=1.5)
    spark()

    print(f"Wrote {len(SHAPES) + len(ICONS) + len(UI)} SVGs to {SVG_DIR}")
    print(f"Wrote PNG sprites to {PNG_DIR}")


if __name__ == "__main__":
    main()
