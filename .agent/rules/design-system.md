# Brand Design System & Frontend Color Rules

This workspace follows the official brand design system extracted from the project logo emblem. All frontend UI work must adhere strictly to these rules.

## Core Palette

- **Brand Blue (Primary Brand)**: `#20409A` (Mon / Crest Petals)
- **Brand Yellow (Accent / Action)**: `#FFEE00` (War Fan & Trim Borders)
- **Insignia Black (Text & Outlines)**: `#050505` (Katana & Linework)
- **Crisp White (Primary Canvas)**: `#FFFFFF` (Intended background)
- **Surface Light (Secondary Canvas)**: `#F8FAFC`

## Enforcement Guidelines

1. **No Arbitrary Colors**: Do not introduce ad-hoc color palettes (e.g. random greens, purples, generic blues). Use the brand tokens above.
2. **Background**: The default background for pages, cards, and canvas elements must always be clean White (`#FFFFFF`) or subtle off-white (`#F8FAFC`), never dark black by default unless an explicit dark mode toggle is requested.
3. **Contrast Compliance**: Yellow (`#FFEE00`) must never be used as text on white backgrounds. Use yellow for badges, buttons with black text, focus states, or trims.
4. **Tokenization**: In CSS/Tailwind, map these to `--brand-primary`, `--brand-accent`, `--background`, and `--foreground`.
