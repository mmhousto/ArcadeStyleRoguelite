"""
Generates Ricomet's placeholder audio: synthesized sound effects and three short
synthwave music loops, written as WAV files to Assets/Resources/Audio/.

Replace any file with a real sound of the same name and the game picks it up.

Usage (from the repository root):
    pip install numpy
    python3 Tools/generate_audio.py
"""
import os
import wave

import numpy as np

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT = os.path.join(ROOT, "Assets", "Resources", "Audio")
SR = 44100
MUSIC_SR = 22050
rng = np.random.default_rng(7)


# ---------------------------------------------------------------- building blocks

def t_axis(seconds, sr=SR):
    return np.arange(int(seconds * sr)) / sr


def tone(freq, seconds, shape="square", sr=SR):
    """freq may be a number or an array (for sweeps)."""
    t = t_axis(seconds, sr)
    f = np.broadcast_to(np.asarray(freq, dtype=np.float64), t.shape)
    phase = 2 * np.pi * np.cumsum(f) / sr
    if shape == "sine":
        return np.sin(phase)
    if shape == "saw":
        return 2 * ((phase / (2 * np.pi)) % 1.0) - 1
    if shape == "triangle":
        return 2 * np.abs(2 * ((phase / (2 * np.pi)) % 1.0) - 1) - 1
    return np.sign(np.sin(phase)) * 0.8


def noise(seconds, sr=SR):
    return rng.uniform(-1, 1, int(seconds * sr))


def env(n, attack=0.005, decay=None, sr=SR, curve=3.0):
    """Fast attack, then a power-curve decay to silence over the rest."""
    a = max(1, int(attack * sr))
    out = np.ones(n)
    out[:a] = np.linspace(0, 1, a)
    rest = n - a
    if rest > 0:
        out[a:] = (1 - np.linspace(0, 1, rest)) ** curve
    return out


def sweep(f0, f1, seconds, sr=SR):
    return np.geomspace(f0, f1, int(seconds * sr))


def lowpass(x, amount):
    """One-pole low-pass. amount 0..1, higher = darker."""
    y = np.zeros_like(x)
    acc = 0.0
    k = 1 - amount
    for i, v in enumerate(x):
        acc += k * (v - acc)
        y[i] = acc
    return y


def mix(*parts):
    n = max(len(p) for p in parts)
    out = np.zeros(n)
    for p in parts:
        out[:len(p)] += p
    return out


def seq(*parts, gap=0.0, sr=SR):
    pieces = []
    for p in parts:
        pieces.append(p)
        if gap > 0:
            pieces.append(np.zeros(int(gap * sr)))
    return np.concatenate(pieces)


def write(path, x, sr=SR, peak=0.85):
    x = np.asarray(x, dtype=np.float64)
    m = np.max(np.abs(x)) or 1.0
    x = x / m * peak
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with wave.open(path, "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(sr)
        w.writeframes((x * 32767).astype("<i2").tobytes())


def note(n):
    """MIDI note number to frequency."""
    return 440.0 * 2 ** ((n - 69) / 12)


# ---------------------------------------------------------------- sound effects

def sfx():
    s = {}
    d = 0.16
    s["dash"] = lowpass(noise(d) * env(int(d * SR), 0.01, curve=1.5), 0.6) * 0.7 + tone(sweep(300, 900, d), d, "saw") * env(int(d * SR)) * 0.25
    d = 0.11
    s["kill"] = tone(sweep(1400, 500, d), d, "square") * env(int(d * SR), curve=2) + noise(d) * env(int(d * SR), curve=6) * 0.3
    d = 0.14
    s["land"] = tone(sweep(180, 60, d), d, "sine") * env(int(d * SR), curve=2)
    d = 0.4
    s["hit"] = mix(lowpass(noise(d), 0.3) * env(int(d * SR), curve=2), tone(sweep(400, 70, d), d, "saw") * env(int(d * SR), curve=1.5) * 0.7)
    s["shield_repair"] = seq(*[tone(note(n), 0.07, "triangle") * env(int(0.07 * SR), curve=1.5) for n in (72, 76, 79, 84)])
    s["wave_start"] = seq(tone(note(67), 0.12, "square") * env(int(0.12 * SR)), tone(note(74), 0.22, "square") * env(int(0.22 * SR)))
    s["wave_clear"] = seq(*[tone(note(n), 0.09, "square") * env(int(0.09 * SR), curve=1.5) for n in (72, 79, 84, 91)])
    d = 1.6
    t = t_axis(d)
    s["boss_warning"] = tone(600 + 200 * np.sign(np.sin(2 * np.pi * 3 * t)), d, "saw") * np.minimum(1, (d - t) * 3) * 0.6
    d = 1.2
    s["beam_charge"] = tone(sweep(120, 700, d), d, "saw") * np.linspace(0.1, 1, int(d * SR)) * 0.5
    d = 0.5
    s["beam_fire"] = mix(lowpass(noise(d), 0.2) * env(int(d * SR), curve=1.2) * 0.6, tone(90, d, "saw") * env(int(d * SR), curve=1.2))
    d = 0.18
    s["orb_fire"] = tone(sweep(900, 300, d), d, "triangle") * env(int(d * SR), curve=2)
    d = 0.2
    s["deflect"] = mix(tone(1800, d, "sine"), tone(2700, d, "sine") * 0.5) * env(int(d * SR), 0.002, curve=4)
    d = 0.25
    s["boss_hit"] = mix(tone(sweep(260, 80, d), d, "square") * env(int(d * SR), curve=2), noise(d) * env(int(d * SR), curve=4) * 0.5)
    d = 2.2
    s["boss_defeat"] = mix(lowpass(noise(d), 0.5) * env(int(d * SR), curve=2.5), tone(sweep(200, 30, d), d, "saw") * env(int(d * SR), curve=2) * 0.6)
    s["upgrade_pick"] = seq(*[tone(note(n), 0.06, "triangle") * env(int(0.06 * SR)) for n in (84, 88, 91, 96)])
    d = 0.05
    s["ui_click"] = tone(1500, d, "square") * env(int(d * SR), 0.001, curve=3) * 0.6
    d = 0.08
    s["ui_select"] = tone(sweep(700, 1100, d), d, "triangle") * env(int(d * SR), curve=2)
    s["game_over"] = seq(*[tone(note(n), 0.22, "saw") * env(int(0.22 * SR), curve=1) for n in (67, 63, 60, 55)])
    s["victory"] = seq(*[tone(note(n), 0.12, "square") * env(int(0.12 * SR), curve=1) for n in (60, 64, 67, 72, 76, 79)], tone(note(84), 0.6, "square") * env(int(0.6 * SR), curve=1.5))
    d = 0.35
    s["multikill"] = mix(*[tone(note(n), d, "square") * env(int(d * SR), curve=2) for n in (72, 76, 79)])
    s["combo"] = seq(tone(note(84), 0.07, "square") * env(int(0.07 * SR)), tone(note(91), 0.16, "square") * env(int(0.16 * SR)))

    for name, x in s.items():
        write(os.path.join(OUT, "Sfx", name + ".wav"), x)
    return len(s)


# ---------------------------------------------------------------- music loops

def drum_kick(sr):
    d = 0.25
    return tone(sweep(150, 45, d, sr), d, "sine", sr) * env(int(d * sr), 0.002, sr=sr, curve=2)


def drum_snare(sr):
    d = 0.2
    return noise(d, sr) * env(int(d * sr), 0.001, sr=sr, curve=3) * 0.55


def drum_hat(sr):
    d = 0.05
    n = noise(d, sr)
    return (n - lowpass(n, 0.5)) * env(int(d * sr), 0.001, sr=sr, curve=4) * 0.35


def track(bpm, chords, bars_per_chord, arp_pattern, bass_shape, kick_every, hats=True, sr=MUSIC_SR):
    """Loopable synthwave: arpeggiated chords, bass on the root, four-on-the-floor drums."""
    beat = 60.0 / bpm
    step = beat / 4
    bar = beat * 4
    total_bars = len(chords) * bars_per_chord
    out = np.zeros(int(total_bars * bar * sr) + sr)

    def place(x, at, gain=1.0):
        i = int(at * sr)
        out[i:i + len(x)] += x[:max(0, len(out) - i)] * gain

    kick, snare, hat = drum_kick(sr), drum_snare(sr), drum_hat(sr)
    for b in range(total_bars):
        chord = chords[b // bars_per_chord]
        root = chord[0] - 24
        for s16 in range(16):
            at = b * bar + s16 * step
            # Arpeggio.
            n = chord[arp_pattern[s16 % len(arp_pattern)] % len(chord)] + (12 if arp_pattern[s16 % len(arp_pattern)] >= len(chord) else 0)
            place(lowpass(tone(note(n), step * 0.9, "saw", sr), 0.55) * env(int(step * 0.9 * sr), sr=sr, curve=1.5), at, 0.18)
            # Bass on eighths.
            if s16 % 2 == 0:
                place(lowpass(tone(note(root), step * 1.8, bass_shape, sr), 0.7) * env(int(step * 1.8 * sr), sr=sr, curve=0.8), at, 0.35)
            # Drums.
            if s16 % kick_every == 0:
                place(kick, at, 0.9)
            if s16 in (4, 12):
                place(snare, at, 0.6)
            if hats and s16 % 2 == 1:
                place(hat, at, 0.6)

    loop = out[:int(total_bars * bar * sr)]
    # Soft pad underneath for width.
    return loop


def music():
    # A minor: Am - F - C - G
    am, f, c, g = (57, 60, 64), (53, 57, 60), (48, 52, 55), (55, 59, 62)
    write(os.path.join(OUT, "Music", "music_menu.wav"),
          track(92, [am, f, c, g], 2, [0, 1, 2, 3, 2, 1], "triangle", 8, hats=False), MUSIC_SR, 0.7)
    write(os.path.join(OUT, "Music", "music_game.wav"),
          track(116, [am, f, c, g], 2, [0, 2, 1, 3, 2, 4, 1, 2], "saw", 4), MUSIC_SR, 0.75)
    # Boss: darker, faster. Dm - Bb - Gm - A
    dm, bb, gm, a = (62, 65, 69), (58, 62, 65), (55, 58, 62), (57, 61, 64)
    write(os.path.join(OUT, "Music", "music_boss.wav"),
          track(132, [dm, bb, gm, a], 1, [0, 1, 2, 3, 0, 2, 4, 2], "square", 4), MUSIC_SR, 0.8)
    return 3


if __name__ == "__main__":
    count = sfx()
    loops = music()
    print(f"Wrote {count} sound effects and {loops} music loops to {OUT}")
