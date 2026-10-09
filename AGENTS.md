# Workspace Guidelines & Brand Design System

## 1. Brand Identity & Visual Theme
The visual identity of this project is derived from the emblem logo:
* **Insignia elements**: Japanese Mon/cherry blossom crest, folding war fan (Tessen), and katana.
* **Canvas background**: Clean, bright white background (the original logo image has a transparent/alpha background which may render dark in some viewers, but the intended design canvas is crisp white).
* **Tone**: Athletic, disciplined, bold, high-contrast Japanese martial arts aesthetic.

---

## 2. Core Color Palette & Hex Values

Always use these exact brand colors and semantic tokens across all UI components and styling:

### Primary Brand
* **Brand Blue (Crest Petals / Dominant)**: `#20409A`
  * Light tint (surfaces, chips, hovers): `#EBF0FA` / `#D7E3F8`
  * Hover / Deep state: `#18337A`
  * Active / Contrast: `#102252`

### Accent & Highlight
* **Brand Yellow / Gold (Border & Fan Accent)**: `#FFEE00`
  * Secondary / Warm gold tint: `#FFD700`
  * Text-accessible dark gold (for readable text on light backgrounds): `#997A00` or `#B38F00`
  * Accent hover: `#E6D600`

### Contrast & Base Neutrals
* **Insignia Black (Outlines, Katana Details, High-Contrast Text)**: `#050505` / `#0B0F19`
* **Pure / Crisp White (Canvas Background & Clean Cards)**: `#FFFFFF`
* **Off-White / Muted Surface**: `#F8FAFC`
* **Border / Divider Gray**: `#E2E8F0`
* **Muted Text / Secondary Foreground**: `#64748B`

---

## 3. UI Implementation Rules for Frontend Work

1. **Strict Palette Adherence**:
   * Do **NOT** use arbitrary random hex colors or mismatched stock Tailwind palettes (such as default generic cyan, orange, or purple) for core theme components.
   * Restrict primary actions, hero elements, navigation accents, and borders to the **Brand Blue (`#20409A`)**, **Brand Yellow (`#FFEE00`)**, and **Insignia Black (`#050505`)** on a **White (`#FFFFFF`)** background.

2. **The 60-30-10 Application**:
   * **60% Base**: White (`#FFFFFF`) and very light slate (`#F8FAFC`) for backgrounds, containers, and cards.
   * **30% Structure**: Insignia Black (`#050505` / `#0F172A`) for typography, outlines, and structural layout, with deep Brand Blue for sidebars/headers where applicable.
   * **10% Accent**: Brand Yellow (`#FFEE00`) reserved for focus highlights, badges, key primary call-to-actions, active indicators, and border trims.

3. **Accessibility & Contrast**:
   * Avoid rendering pure yellow (`#FFEE00`) text on white backgrounds as it fails WCAG contrast standards. Use Brand Yellow as background fills with dark black text (`#050505`), or use the Brand Blue (`#20409A`) for text.
   * When pairing Brand Yellow and Brand Blue together, ensure crisp black outlines or sufficient tonal separation mirroring the logo styling.

4. **Design Tokens & Configuration**:
   * Whenever adding Tailwind CSS, CSS variables, or component libraries (e.g. Shadcn/Radix), register these brand colors under semantic names (`--primary: #20409A`, `--accent: #FFEE00`, `--background: #FFFFFF`, `--foreground: #050505`).

---

## 4. Architecture & Engineering Rules
See detailed rules in [`.agent/rules/architecture.md`](file:///.agent/rules/architecture.md):
* **Stack**: Blazor WebAssembly (.NET C#) Frontend + ASP.NET Core Web API Backend.
* **Pattern**: Clean Architecture combined with Modular Monolith.
* **Boundary Rules**: Strict dependency inversion (Domain has zero external dependencies). Modules communicate only via public interfaces or domain/integration events, never direct DB or internal class access.
* **Blazor Best Practices**: Use strongly-typed API client abstractions (no raw `HttpClient` calls in `.razor` files); isolate complex component logic in code-behind `.razor.cs`.

---

## 5. Coding Standards & Pre-Push Verification Protocol
See full guidelines in [`.agent/rules/coding-standards.md`](file:///.agent/rules/coding-standards.md):
* **DDD**: Rich domain models with encapsulated invariants, ubiquitous BJJ language, explicit Value Objects, and Domain Events. No anemic models.
* **SOLID & DRY**: Single responsibility, open-closed via pipelines/abstractions, interface segregation, and deduplication of domain rules without premature cross-module coupling.
* **TDD (Red-Green-Refactor)**: Always write a failing test first (🔴 Red), write the minimum code to pass (🟢 Green), and refactor with design integrity (🔵 Refactor).
* **Mandatory Pre-Push Gates**: Before ANY `git push`, perform and document:
  1. **Automated Code Review**: Layer boundaries, code hygiene, DRY/SOLID check, and 100% test pass rate (`dotnet test`).
  2. **Security Review**: OWASP Top 10 focus (auth/authz, tenant isolation/IDOR, parameterized queries, input validation via FluentValidation, and secrets scan).


