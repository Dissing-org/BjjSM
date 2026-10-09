# Coding Standards, Development Workflow & Git Verification Rules

This document establishes the mandatory software engineering standards, quality practices, and pre-push verification gates for **BjjSM**.

---

## 1. Core Engineering Principles

### 1.1. Domain-Driven Design (DDD)
* **Ubiquitous Language**: Use consistent, precise BJJ and club management domain terminology across code, database schemas, and documentation (e.g., `Practitioner`, `Academy`, `BeltRank`, `MatSession`, `TournamentBracket`).
* **Rich Domain Models**:
  * Avoid anemic domain models with public setters and empty business logic.
  * Encapsulate state mutations behind expressive domain methods (e.g., `member.PromoteBelt(newRank, promotionDate)` instead of `member.Belt = newRank`).
  * Enforce entity invariants inside private/protected constructors and factory methods (`Create(...)`).
* **Value Objects**:
  * Model concepts that have no identity as immutable Value Objects (e.g., `Email`, `Money`, `BeltGrade`, `WeightCategory`).
  * Value Objects must implement structural equality (C# `record` or custom `ValueObject` base).
* **Aggregate Roots & Boundaries**:
  * External access and persistence must operate through Aggregate Roots.
  * Aggregates maintain their own consistency boundaries and communicate state changes outward via **Domain Events**.

### 1.2. SOLID Principles
* **Single Responsibility (SRP)**: Each class/handler must have exactly one reason to change. Separate commands (write) from queries (read).
* **Open/Closed (OCP)**: Extend behavior through polymorphism, delegates, or pipeline behaviors (e.g., MediatR pipeline behaviors for validation/logging) rather than modifying core logic.
* **Liskov Substitution (LSP)**: Derived types must be fully substitutable for their base types without altering correctness.
* **Interface Segregation (ISP)**: Design fine-grained, role-focused interfaces rather than bloated monolithic interfaces.
* **Dependency Inversion (DIP)**: High-level business logic must depend on abstractions, never on low-level infrastructure implementations.

### 1.3. DRY (Don't Repeat Yourself)
* Eliminate duplicate business rules and domain logic.
* *Pragmatic balance*: Do not prematurely couple independent modules just to avoid trivial syntax repetition. Duplicate code across module boundaries is preferable to false coupling; unify only genuine domain rules and reusable core primitives.

---

## 2. Test-Driven Development (TDD) — Red-Green-Refactor

All features and bug fixes must follow the strict **Red-Green-Refactor** cycle:

1. **🔴 Red (Failing Test First)**:
   * Write a focused, deterministic unit/integration test capturing the expected behavior or reproducing the reported bug.
   * Run the test suite and confirm the test **fails for the expected reason**. Never skip verifying the failure first.
2. **🟢 Green (Minimal Implementation)**:
   * Write the minimum amount of production code required to make the test pass.
   * Run the test suite and confirm all tests pass.
3. **🔵 Refactor (Clean Up)**:
   * Clean up the implementation: eliminate duplication, improve naming, ensure DDD encapsulation, and adhere to SOLID principles.
   * Verify all tests remain green after refactoring.

### Test Structure & Conventions
* Use the **Arrange-Act-Assert (AAA)** pattern.
* Test naming convention: `MethodName_ShouldExpectedBehavior_WhenCondition` (e.g., `PromoteBelt_ShouldRaiseBeltPromotedEvent_WhenEligible`).
* Unit tests must be fast, in-memory, and isolated. Use mocks/fakes only for external boundary dependencies, never for domain entities or value objects.

---

## 3. Mandatory Pre-Push Verification Protocol

**Before executing ANY `git push` or declaring a feature/PR ready for upstream submission**, the following two reviews must be executed and explicitly documented:

### Gate 1: Automated Code Review
* **Standards & Architecture Conformance**:
  * Verify Clean Architecture layer boundaries (e.g., Domain has zero references to EF Core/Web API).
  * Check that no cross-module boundaries are violated.
  * Ensure Blazor components do not make raw `HttpClient` calls and respect the Brand Design System (`#20409A`, `#FFEE00`, `#050505`, `#FFFFFF`).
* **Code Cleanliness**:
  * Verify no dead code, commented-out blocks, or debug log statements remain.
  * Check nullability warnings and ensure proper C# idiom usage.
* **Full Test Suite Execution**:
  * Execute the entire automated test suite (`dotnet test`) and verify 100% pass rate.

### Gate 2: Security Review
* **Input Validation & Sanitization**: Ensure all external inputs (API payloads, query params) are validated via FluentValidation or domain invariants before execution.
* **Injection Vulnerabilities**:
  * Check for SQL/ORM injection (ensure parameterized EF Core queries; no raw string concatenation in SQL).
  * Check for Cross-Site Scripting (XSS) in Blazor markup (ensure proper HTML encoding; avoid raw unescaped HTML injection).
* **Authentication & Authorization**:
  * Verify every endpoint is guarded by appropriate authentication and authorization policies (e.g., role/permission checks).
  * Confirm tenants/users cannot access or mutate records belonging to other tenants/users (IDOR checks).
* **Secrets & Sensitive Data**:
  * Verify no API keys, connection strings, credentials, or development certificates are committed to Git.
  * Ensure sensitive data (passwords, tokens) is not logged or exposed in API error payloads.

---

## 4. Git Commit & Review Summary Format
When a push or PR preparation is completed, output a structured review summary:
```markdown
### 🛡️ Pre-Push Verification Summary
- **Tests**: [x] All unit/integration tests passing (Red-Green-Refactor verified)
- **Code Review**: [x] DDD invariants respected, SOLID & DRY checked, Clean Architecture preserved
- **Security Review**: [x] Auth, input validation, injection, and secrets scanned — NO ISSUES
```
