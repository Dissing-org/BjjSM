# BjjSM Domain Glossary (Ubiquitous Language)

This document serves as the project's canonical domain glossary. It contains ubiquitous language definitions and is completely devoid of implementation details or code.

---

## 1. Member & Identity Context

* **Academy (Tenant):** The martial arts school / gym boundary. All members, graduation records, programs, and plans belong to a specific Academy.
* **Practitioner (Member):** An individual enrolled in the academy. Can hold multiple operational roles (e.g., Student, Assistant Instructor, Head Instructor, Academy Admin).
* **Instructor (Professor / Coach):** A Practitioner authorized to award belt/stripe promotions and author/publish teaching plans.
* **Belt System:** The ranking framework. Supports two distinct division ladders:
  * **Adult System:** White, Blue, Purple, Brown, Black (with degrees 1–6), Coral (7–8), Red (9–10). Belts from White through Brown hold 0 to 4 stripes.
  * **Junior / Kids System:** White, Grey-White, Grey, Grey-Black, Yellow-White, Yellow, Yellow-Black, Orange-White, Orange, Orange-Black, Green-White, Green, Green-Black (each holding 0 to 4 stripes).
* **Rank / Grade:** A specific combination of Belt System, Belt Color, and Stripe/Degree Count.
* **Graduation (Promotion Record):** A record of a Practitioner achieving a Rank, capturing effective award date, awarding Instructor, and optional notes. Authorized instructors can adjust or remove erroneous entries.
* **Graduation History (Timeline):** The chronological record of all Graduations for a Practitioner.

---

## 2. Curriculum & Planning Context

* **Program:** A distinct curriculum track within an Academy (e.g., Adults Fundamentals, Advanced Gi, No-Gi Grappling, Kids BJJ).
* **Monthly Teaching Plan (Curriculum Plan):** A scoped educational curriculum for a specific Program and Calendar Month/Year.
* **Plan Lifecycle:**
  * **Draft:** Editable working state visible only to instructors.
  * **Published:** Locked/released state visible to students and ready for formal PDF syllabus export.
  * **Archived:** Historical past plan preserved for record-keeping.
* **Monthly Objective:** The overarching strategic learning target for the month.
* **Weekly Focus:** The specific thematic breakdown for each week (Week 1 to Week 4/5) of the month.
* **Technique / Drill:** A specific grappling maneuver, sequence, or drill attached to a Weekly Focus, including coaching notes and reference links (e.g., video URL).
* **Published Plan Document:** The finalized, downloadable PDF syllabus generated from a Published Monthly Teaching Plan adhering to the brand design system.
* **Card Template:** A customizable schema of named Front (prompt) and Back (answer/mechanics) fields (`CardFieldDefinition`) used to structure flashcards.
* **Study Deck:** A collection of flashcards coupled with configurable SM-2 scheduling options (`DeckSchedulingOptions`).
* **Flashcard:** A spaced-repetition item progressing through explicit lifecycle states (`New`, `Learning`, `Review`, `Relearning`, `Suspended`) and maturity tiers (`New`, `Learning`, `Young`, `Mature`, `Suspended`).
* **Review Log:** A timestamped record of a practitioner rating a flashcard (`Again`, `Hard`, `Good`, `Easy`) during a scheduled due queue or custom cram session.
