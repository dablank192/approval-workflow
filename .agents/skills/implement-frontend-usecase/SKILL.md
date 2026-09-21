---
name: implement-frontend-usecase
description: Implements frontend use cases from BA specifications using runtime OpenAPI and read-only backend inspection. Use when implementing or materially changing a frontend flow, including alignment review, UI design approval, implementation planning, frontend development, and integration/E2E verification.
---


# Frontend Use Case Implementation Workflow

## Purpose

This skill defines how the project's UI Designer + Frontend Engineer implements frontend functionality from BA use cases while respecting the ownership and permission boundaries in `AGENTS.md`.

This skill is intentionally project-specific.

Superpowers remains responsible for generic engineering practices such as:

- brainstorming
- planning discipline
- execution
- debugging
- verification
- testing
- code review
- Git workflow

`AGENTS.md` remains authoritative for:

- ownership
- permissions
- backend read/write restrictions
- source-of-truth hierarchy
- approved frontend stack

---

# 1. Core Flow

The Agent must follow this sequence:

```text
BA Use Case / Business Rules
        ↓
Requirement Extraction
        ↓
Runtime OpenAPI + Read-only Backend Trace
        ↓
01 Alignment Report
        ↓
HUMAN APPROVAL
        ↓
02 UI Design Report
        ↓
HUMAN APPROVAL
        ↓
03 Implementation Plan
        ↓
HUMAN APPROVAL
        ↓
Frontend Implementation
        ↓
Compile / Type Check
        ↓
Unit / Component Tests
        ↓
Real Backend Integration / E2E Tests
        ↓
04 Implementation & Test Report
```

The Agent MUST NOT skip the three mandatory approval gates:

1. Alignment Approval
2. UI Design Approval
3. Implementation Plan Approval

Production frontend coding may begin only after all three are approved.

## Frontend Use Case Workflow

For any task that implements or materially changes a frontend use case, follow:

`.agents/workflows/implement-frontend-usecase.md`

This workflow is mandatory.

Production frontend implementation MUST NOT begin until the required human approval gates in that workflow have been completed:

1. Alignment Approval
2. UI Design Approval
3. Implementation Plan Approval

The Agent must generate and maintain the required workflow reports under:

`/frontend/reports/<use-case-id>/`

If this workflow conflicts with generic Superpowers behavior, project ownership rules in this file and approval gates in the workflow take precedence.

---

# 2. Workflow Report Location

All large workflow reports must be written under:

```text
/frontend/reports/<use-case-id>/
```

Use this structure:

```text
frontend/
└── reports/
    └── UC-XXX/
        ├── 01-alignment-report.md
        ├── 02-ui-design-report.md
        ├── 03-implementation-plan.md
        └── 04-implementation-test-report.md
```

These reports are working artifacts for implementation and review.

They do not replace:

```text
/docs/use-cases/
/docs/business-rules/
```

as the business source of truth.

Small or medium decisions do not require separate Markdown files unless they materially affect:

- business behavior
- API usage
- approved UI structure
- implementation scope
- architecture
- test coverage

---

# 3. Phase 1 — Read BA Specification

Read the relevant files from:

```text
/docs/use-cases/
/docs/business-rules/
```

Extract all behavior relevant to the requested use case.

At minimum identify:

- Use case ID
- Use case name
- Actor(s)
- Preconditions
- Trigger
- Main flow
- Alternative flows
- Exception flows
- Input fields
- Required / optional rules
- Business rules
- Permissions
- State transitions
- User-facing messages
- Success behavior
- Error behavior
- Navigation behavior
- Postconditions

Build an internal checklist of all documented flows.

Do not invent missing business behavior.

Do not begin frontend coding.

---

# 4. Phase 2 — Derive Frontend Requirements

From the BA specification, derive the frontend behavior required to support the use case.

Identify:

- Required screen(s)
- Required route(s)
- Required components
- Form fields
- User actions
- Loading states
- Success states
- Error states
- Empty states
- Disabled states
- Confirmation states
- Navigation transitions

This is requirement derivation only.

Do not make business decisions.

Do not start production frontend implementation.

---

# 5. Phase 3 — Obtain Runtime OpenAPI Contract

The API contract source of truth is the OpenAPI document exposed by the running ASP.NET Core backend.

Do not rely on a manually maintained OpenAPI snapshot unless explicitly instructed.

If necessary, the Agent may run the backend using the existing project configuration.

Allowed examples:

```bash
dotnet restore
dotnet build
dotnet run
```

Fetch the runtime OpenAPI document from the configured OpenAPI endpoint.

Identify the relevant:

- endpoints
- HTTP methods
- path parameters
- query parameters
- request schemas
- response schemas
- public enums
- nullability
- status codes
- authentication requirements when exposed

The Agent may run the backend.

The Agent may not modify the backend to make OpenAPI work.

If runtime OpenAPI cannot be obtained, report the blocker.

---

# 6. Phase 4 — Inspect Backend Read-Only

Inspect backend source code and backend tests in read-only mode.

Trace each BA flow to the relevant backend implementation.

Inspect only what is necessary to determine:

- actual validation behavior
- authorization behavior
- state transitions
- error identifiers
- domain rules
- DTO mapping
- relevant side effects
- implementation details not visible in OpenAPI

For every documented error flow, attempt to map:

```text
BA business condition
→ backend implementation
→ stable backend error identifier
→ BA-defined frontend behavior/message
```

Never invent:

- backend error codes
- endpoints
- response fields
- validation behavior
- permissions
- state transitions
- backend side effects

Never modify backend files.

---

# 7. Phase 5 — Alignment Review Gate

This is a mandatory approval gate.

The Agent MUST create:

```text
/frontend/reports/<use-case-id>/01-alignment-report.md
```

The report must always be generated, even when everything is aligned.

## Required Contents

### Scope

Include:

- Use case ID and name
- Related business-rule documents
- Relevant backend endpoint(s)
- Relevant backend feature/module

### Three-Way Mapping Matrix

The report MUST contain a matrix mapping BA, OpenAPI, and backend implementation.

Use a structure similar to:

| Flow ID | BA Expectation | OpenAPI / Endpoint | Backend Implementation | Error Identifier | Expected FE Behavior | Alignment Status | Notes |
|---|---|---|---|---|---|---|---|

Every relevant:

- main-flow step
- alternative flow
- exception flow

must appear in the matrix.

### Error Mapping

For each business error, include:

```text
BA condition
→ backend error identifier
→ expected frontend message / behavior
```

### Contract Gaps

List any missing or conflicting:

- endpoint
- request field
- response field
- error discriminator
- status behavior
- permission behavior
- state transition

### Open Questions

List any ambiguity that requires human review.

### Proposed Mapping

State the Agent's interpretation of how the three sources should map.

---

# 8. Alignment Approval Rule

After producing:

```text
01-alignment-report.md
```

the Agent MUST STOP.

Do not begin UI design.

Present the report to the human owner for review.

The human owner may:

- approve the mapping
- edit the mapping
- correct interpretation
- update BA documents
- update backend behavior
- update API behavior
- request another analysis pass

If any source changes, re-read the relevant sources and update the alignment report.

Proceed only after explicit human approval.

The approved alignment report becomes the working traceability reference for the frontend implementation.

It does not override BA documents.

---

# 9. Phase 6 — UI Design

Begin UI design only after Alignment Approval.

The Agent acts as UI Designer within the authority defined in `AGENTS.md`.

The Agent may decide:

- layout
- spacing
- typography
- color usage
- visual hierarchy
- component composition
- card grouping
- modal / drawer presentation
- responsive behavior
- loading presentation
- empty-state presentation
- error presentation

Prefer:

- clean enterprise SaaS patterns
- conventional interaction patterns
- accessibility
- clarity
- consistency
- usability over decoration

The Agent MUST NOT change:

- business rules
- required fields
- field semantics
- permissions
- state transitions
- API behavior
- required actions
- approved alignment mappings

---

# 10. Phase 7 — UI Design Review Gate

This is a mandatory approval gate.

The Agent MUST create:

```text
/frontend/reports/<use-case-id>/02-ui-design-report.md
```

## Default Required Contents

The default UI report is intentionally lightweight.

It MUST contain only the information necessary for the human owner to understand the proposed screen before implementation.

### 1. ASCII / Text Wireframe

Every relevant screen must have an ASCII or text wireframe.

Example:

```text
Request Detail

┌─────────────────────────────────────────────┐
│ Header                                      │
│ Request #1024                  [Pending]    │
├───────────────────────────┬─────────────────┤
│ Request Information       │ Approval Flow   │
│                           │                 │
│ Title                     │ Level 1 ✓       │
│ Description               │ Level 2 ...     │
│ Requester                 │                 │
│ Created At                │                 │
├───────────────────────────┴─────────────────┤
│ [Cancel Request]                    [Back]  │
└─────────────────────────────────────────────┘
```

### 2. Component Inventory

Include a concise component list or table.

Example:

| Component | Purpose | Data Source | Main Actions |
|---|---|---|---|
| `RequestHeader` | Show ID/status | Request API | None |
| `RequestInfoCard` | Show request data | Request API | None |
| `ApprovalFlow` | Show approval steps | Request API | None |
| `CancelButton` | Cancel request | Cancel API | Submit cancel |

### 3. UI State Matrix

Include all relevant visual states.

Example:

| State | Trigger | UI Behavior |
|---|---|---|
| Initial | Route opened | Show skeleton/loading |
| Loaded | API success | Render request detail |
| Empty | No data | Show empty state |
| Business Error | Known backend error | Show mapped BA message |
| Technical Error | Unexpected failure | Show generic error state |

The default UI Design Report does NOT require:

- coded prototype
- browser preview
- visual mockup image
- Figma artifact
- screenshot
- production component code

---

# 11. Visual Mockup Rule

A real visual mockup is OPTIONAL and MUST NOT be created by default.

The Agent may generate a visual mockup only when the human owner explicitly requests one.

Examples of explicit requests:

```text
Generate a visual mockup for this screen.
```

```text
Show me what this UI would look like as an image.
```

```text
Create a real mockup before coding.
```

If direct image-generation capability is available, prefer generating a static mockup image rather than coding a disposable frontend prototype.

A generated mockup is for:

- layout understanding
- visual hierarchy
- density
- general visual direction

It is not a business or API source of truth.

The Agent MUST NOT automatically fall back to coding a prototype when image generation is unavailable.

If image generation is unavailable, report that limitation.

Only create a coded prototype when the human owner explicitly requests a coded/browser prototype.

---

# 12. UI Design Approval Rule

After producing:

```text
02-ui-design-report.md
```

and any explicitly requested visual mockup:

```text
STOP
```

Do not begin implementation planning yet.

Wait for explicit human approval.

The human owner may:

- approve the design
- modify layout
- modify component composition
- change presentation decisions
- request another UI iteration
- request a visual mockup

Update `02-ui-design-report.md` if approved design decisions change.

Proceed only after explicit approval.

---

# 13. Phase 8 — Implementation Planning

Implementation planning begins only after UI Design Approval.

This phase is separate from Alignment and UI Design.

The three gates answer different questions:

```text
Alignment:
Do BA, OpenAPI, and backend agree on behavior?

UI Design:
What will the user see and interact with?

Implementation Plan:
Exactly how will the approved frontend be built and tested?
```

Use Superpowers planning methodology where useful.

Keep the plan scoped to the requested frontend use case.

Do not plan backend changes.

Do not plan unrelated frontend refactors.

---

# 14. Phase 9 — Implementation Plan Review Gate

The Agent MUST create:

```text
/frontend/reports/<use-case-id>/03-implementation-plan.md
```

## Required Contents

### Scope

Include:

- use case(s)
- approved screen(s)
- included flows
- explicitly excluded scope

### File Plan

List files or frontend areas expected to be created or modified.

Example:

| File / Area | Action | Purpose |
|---|---|---|
| `src/features/requests/pages/...` | Create | Request detail page |
| `src/features/requests/components/...` | Create | Feature components |
| `src/api/generated/...` | Regenerate | OpenAPI client |

### Component Plan

List:

- page components
- feature components
- reusable components
- form components

### API Integration Plan

For each required API:

| Flow | Endpoint | Generated Client / Hook | UI Consumer |
|---|---|---|---|

### State Plan

Use the approved state model:

```text
TanStack Query → server state
React Hook Form → form state
React local state → local UI state
React Router → navigation state
```

### Validation and Error Mapping Plan

Document:

- structural/client validation
- backend business validation mapping
- stable error identifiers
- approved frontend behavior

### Test Plan

Define planned:

- unit tests
- component tests
- integration tests
- E2E tests
- main-flow tests
- alternative-flow tests
- exception-flow tests

### Implementation Sequence

List the ordered implementation steps.

---

# 15. Implementation Plan Approval Rule

After producing:

```text
03-implementation-plan.md
```

the Agent MUST STOP.

Do not write production frontend code yet.

Present the plan to the human owner.

Proceed only after explicit approval.

If the owner changes the plan, update the Markdown artifact before implementation.

---

# 16. Phase 10 — Frontend Implementation

After all three approval gates are approved, implement the frontend.

Use the approved stack:

- React
- TypeScript
- Vite
- React Router
- TanStack Query
- Orval
- Fetch
- React Hook Form
- Zod
- Tailwind CSS
- shadcn/ui
- Lucide React

Prefer generated API types and clients from runtime OpenAPI.

Do not manually duplicate backend DTOs when generation is practical.

Use:

```text
TanStack Query → server state
React Hook Form → form state
React local state → local UI state
React Router → navigation state
```

Do not introduce:

- Redux
- Zustand
- MobX
- another global state library

without explicit approval.

Implement only approved scope.

Do not silently change:

- business behavior
- API mapping
- approved UI structure
- implementation scope

If a material issue invalidates an approved assumption, return to the relevant approval gate.

---

# 17. Phase 11 — Implement Approved UI States

Implement all approved use-case states where applicable:

- initial
- loading
- loaded
- empty
- submit-in-progress
- success
- client validation
- backend business error
- authorization / permission
- technical error
- alternative flow
- exception flow
- navigation transition

Backend business errors must follow:

```text
stable backend error identifier
→ approved BA-defined frontend behavior/message
```

Do not branch on mutable backend error text when a stable identifier exists.

---

# 18. Phase 12 — Compile and Frontend Automated Tests

Before integration testing:

1. Build the frontend.
2. Run TypeScript / compiler checks.
3. Run unit tests.
4. Run component tests.
5. Fix frontend defects.
6. Re-run until the approved frontend scope passes.

Use:

- Vitest
- React Testing Library

where appropriate.

Passing compilation and unit/component tests is not sufficient to mark the use case complete.

---

# 19. Phase 13 — Real Backend Integration and E2E Testing

After compilation and frontend automated tests pass, test the frontend against the real running backend.

Use:

- real runtime backend
- actual API responses
- real frontend application
- Playwright for browser-level E2E where practical

Integration testing must verify that the UI behaves correctly against backend behavior.

Test relevant cases including:

- successful API flow
- loading behavior
- response rendering
- success navigation
- documented alternative flows
- documented business errors
- form retention/reset behavior
- disabled/loading button behavior
- permission behavior when testable
- state transitions when relevant
- technical error handling when safely testable

Do not:

- modify backend code to make tests pass
- fake backend success for a flow that is supposed to be integration-tested
- claim unexecuted tests passed

If a required flow cannot be tested because backend data or environment state is unavailable:

```text
Status = BLOCKED or NOT EXECUTED
```

and document the reason.

---

# 20. Browser Verification Rule

Browser verification is DEFAULT after production frontend implementation.

When browser tooling is available:

1. Run backend.
2. Run frontend.
3. Open the real frontend application.
4. Exercise the implemented flow.
5. Inspect the rendered UI.
6. Verify user-visible behavior.
7. Capture screenshots or recordings when useful.
8. Use browser observations as evidence for integration/E2E results.

Browser tooling is primarily a post-code verification tool.

It is not a default pre-code design-prototyping step.

---

# 21. Phase 14 — Final Implementation & Test Report

After implementation and testing, the Agent MUST create:

```text
/frontend/reports/<use-case-id>/04-implementation-test-report.md
```

This is a mandatory large report.

## Required Contents

### Implementation Summary

Include:

- implemented use case(s)
- screens/routes created or changed
- components created or changed
- APIs integrated
- error mappings implemented
- deviations from approved plan, if any

### Files Changed

Provide a concise list or table of frontend files created or modified.

### Build and Automated Test Results

Include:

- build result
- TypeScript/compiler result
- unit test result
- component test result

### Integration / E2E Results

Include:

- backend runtime status
- frontend runtime status
- integration test status
- browser / Playwright status
- issues found during real integration

### Executed Test Case Matrix

The report MUST list the test cases actually executed.

Use a table similar to:

| TC ID | UC Flow | Test Type | Preconditions | Steps / Action | Expected Result | Actual Result | Status |
|---|---|---|---|---|---|---|---|

Allowed statuses:

- PASS
- FAIL
- BLOCKED
- NOT EXECUTED

Do not mark a testcase PASS unless it was actually executed and observed.

### Use Case Coverage

Map executed tests back to:

- main flows
- alternative flows
- exception flows

Highlight any approved flow without executed coverage.

### Remaining Alignment Issues

List unresolved:

- BA ↔ backend mismatches
- OpenAPI gaps
- unsupported errors
- environment blockers
- untested required flows

### Completion Status

End with exactly one overall status:

```text
COMPLETE
PARTIALLY COMPLETE
BLOCKED
```

Do not mark COMPLETE if a required approved flow remains failed, blocked, or untested unless the human owner explicitly accepts that exception.

---

# 22. Change Control

If a material issue appears after a gate has been approved, return to the relevant gate.

## Business / Contract Issue

Return to:

```text
01-alignment-report.md
```

Re-review and obtain approval again.

## UI / Interaction Issue

Return to:

```text
02-ui-design-report.md
```

Re-review and obtain approval again.

## Implementation Architecture / Scope Issue

Return to:

```text
03-implementation-plan.md
```

Re-review and obtain approval again.

Do not silently diverge from approved artifacts.

---

# 23. Small Decision Rule

Small or medium implementation decisions may be handled without a Markdown report when they do not materially affect approved behavior or scope.

Examples:

- spacing adjustment
- icon choice
- internal variable naming
- small component extraction
- minor CSS refinement
- non-material responsive adjustment

If a decision affects:

- business semantics
- API mapping
- UI structure
- implementation architecture
- test scope
- approved feature scope

it must return to the appropriate review gate.

---

# 24. Final Workflow Summary

```text
┌─────────────────────────────┐
│ BA Use Case / Business Rule │
└──────────────┬──────────────┘
               ↓
     Requirement Extraction
               ↓
      Derive FE Requirements
               ↓
       Run .NET Backend
               ↓
      Fetch Runtime OpenAPI
               ↓
      Read Backend + Tests
               ↓
       Trace Rules / Errors
               ↓
┌─────────────────────────────┐
│ 01 Alignment Report (.md)   │
│ BA ↔ OpenAPI ↔ Backend      │
│ Mapping Matrix              │
└──────────────┬──────────────┘
               ↓
        HUMAN APPROVAL
               ↓
           UI Design
               ↓
┌─────────────────────────────┐
│ 02 UI Design Report (.md)   │
│ - ASCII wireframe           │
│ - Component inventory       │
│ - UI state matrix           │
└──────────────┬──────────────┘
               ↓
        HUMAN APPROVAL
               ↓
     Implementation Planning
               ↓
┌─────────────────────────────┐
│ 03 Implementation Plan.md   │
│ + Test Plan                 │
└──────────────┬──────────────┘
               ↓
        HUMAN APPROVAL
               ↓
       Production FE Code
               ↓
      Compile / Type Check
               ↓
     Unit / Component Tests
               ↓
   Real Backend Integration
               ↓
     Browser / Playwright E2E
               ↓
┌─────────────────────────────┐
│ 04 Implementation & Test    │
│ Report (.md)                │
│ + Executed Testcase Matrix  │
└─────────────────────────────┘
```

---

# 25. Core Rules

```text
BA defines behavior.
Backend implements behavior.
OpenAPI exposes the contract.
Frontend presents behavior.
Human owner approves alignment, UI design, and implementation plan.
```

Default UI review:

```text
ASCII wireframe
+
Component inventory
+
UI state matrix
```

Visual mockup:

```text
ONLY when explicitly requested by the human owner
```

Coded design prototype:

```text
NEVER automatic
ONLY when explicitly requested
```

Browser:

```text
DEFAULT after production code
for real UI / integration / E2E verification
```

When layers do not align:

```text
Detect
→ Trace
→ Document
→ Human Review
```

Never:

```text
Detect
→ Guess
→ Modify Backend
→ Patch Around the Gap
```

Production frontend implementation starts only after:

```text
Alignment Approved
+
UI Design Approved
+
Implementation Plan Approved
```
