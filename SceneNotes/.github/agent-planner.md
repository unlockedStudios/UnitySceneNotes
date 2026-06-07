# AGENTS.md

## Purpose

This repository uses a review-first implementation workflow.
For any non-trivial code change, produce a temporary implementation review document before making edits.

A non-trivial change includes:

- edits across more than one file
- edits that change logic, behavior, architecture, or public interfaces
- refactors, migrations, bug fixes, or feature work
- anything where a before/after preview would help a human review safely

Small typo fixes, comments, or tiny one-line safe changes may skip this workflow.

---

## Sidecar planning document behavior

For any non-trivial task, use a temporary sidecar markdown document as the active working artifact.

Required path:
`.codex/tmp/implementation-review.md`

Behavior requirements:

- Create the sidecar file before making any code edits.
- Treat the sidecar file as the primary implementation plan during the task.
- Keep the sidecar file updated as understanding changes.
- When the Codex surface or IDE supports it, prefer opening or focusing this file in the editor so it can be reviewed separately from chat.
- Include clickable file references using `path/to/file.ext:line` or `path/to/file.ext:start-end`.
- Do not begin implementation until the sidecar file contains Goal, Files to Modify, Change Summary, Before, After, Validation Plan, and Status.
- After implementation, update the sidecar file with actual validation results.
- Delete the sidecar file only after the task is complete and validation has succeeded.
- If the task is blocked, incomplete, or validation fails, keep the file and mark Status as `Blocked` or `In Progress`.

---

## Trigger phrases

If the user says any of the following, always use the sidecar planning document workflow even for smaller edits:

- use review-first mode
- use sidecar plan mode
- prepare implementation review
- show before/after plan

---

## Required workflow for non-trivial changes

### 1) Materialize the plan into a sidecar markdown file

Before editing code, create and populate a temporary sidecar markdown file at:

`.codex/tmp/implementation-review.md`

If `.codex/tmp/` does not exist, create it.

This file is not optional for non-trivial changes.

Minimum required contents before any code edit:

- Goal
- Files to Modify
- Change Summary
- Before
- After
- Validation Plan
- Status

The sidecar file should be used as the live implementation document during the task, not as an after-the-fact summary.

## If the Codex client or VS Code integration supports opening files in an editor tab, prefer opening this file and keeping it updated there.

### 2) Fill the review file with readable sections

The review file must be clearly formatted in markdown and easy to scan in VS Code.

Use this structure:

# Implementation Review

## Goal

A short explanation of what is changing and why.

## Files to Modify

A bullet list of files that are likely to change.

For each file, include a clickable VS Code style path with line references when possible, for example:

- `src/features/player/PlayerController.cs:120`
- `src/ui/inventory/InventoryPanel.tsx:48`

If exact lines are not known yet, add them once identified.

## Change Summary

Describe the intended change in plain English:

- what is broken or missing
- what will change
- what will remain unchanged
- risks or edge cases

## Before

For each important edit, include a focused code snippet showing the existing code.
Keep snippets small and relevant.

Use fenced code blocks with the correct language.

Also add a one-line note explaining why this code is being changed.

## After

For each important edit, include the proposed replacement or new code snippet.
Keep snippets small and relevant.

Annotate important additions inline with short comments such as:

- `// add this`
- `// move this`
- `// remove old branch`
- `// new guard`
- `// preserve existing behavior`

Do not use noisy comments on every line. Only annotate meaningful changes.

## Notes for Reviewer

Include concise review notes such as:

- assumptions being made
- follow-up work not included
- potential regressions to watch for
- validation steps

## Validation Plan

List the checks to run after implementation:

- build
- tests
- lint
- manual verification steps

## Status

Set one of:

- Draft
- In Progress
- Implemented
- Validated

---

### 3) Show before/after snippets that are genuinely useful

Before/after snippets must:

- be small enough to read comfortably
- show the exact local area being modified
- avoid dumping full files
- include enough context to understand the change
- prefer the narrowest snippet that still makes sense

For multi-file changes, include at least one before/after pair per materially changed file.

#### Snippet rules

Before/After snippets must:

- show only the local area being changed
- include 5-25 lines when possible
- avoid whole-file dumps
- include the file path and line reference above each snippet
- include short inline annotations only on meaningful changes

Example format:

### `src/player/Health.cs:48-67`

#### Before

````csharp
if (health < 0)
{
    Die();
}

---

### 4) Only then implement

After the temporary review file is prepared, proceed with the code changes.

Keep implementation aligned with the review file.
If the implementation changes direction materially, update the review file first.

---

### 5) Validate

After editing:

- run the smallest relevant validation first
- then run broader validation if needed
- capture concise results in the temporary markdown file

Examples:

- build succeeded
- unit tests passed
- lint passed
- manual smoke test completed
- validation blocked because environment dependency is missing

Be precise. Do not claim validation that was not actually run.

---

### 6) Cleanup

When implementation and validation are complete, delete:

`.codex/tmp/implementation-review.md`

If validation fails or the work is incomplete, keep the file and set Status to:

- `In Progress`, or
- `Blocked`

Do not delete the file in that case.

---

## Formatting requirements

Write for human review, not for maximum verbosity.
Prioritize:

- readable headings
- compact bullet lists
- small code snippets
- plain language
- accurate file and line references

Avoid:

- giant prose blocks
- full-file dumps
- vague claims like "updated logic"
- decorative markdown that reduces readability

---

## Editing behavior

When changing code:

- preserve surrounding style and conventions
- avoid unrelated refactors
- do not rename files or symbols unless necessary
- prefer the smallest correct change
- call out risky edits in the review notes

---

## If file/line linking is possible

When you know the file and line, prefer references like:

- `path/to/file.ext:123`
- `path/to/file.ext:123-145`

These should be placed near each snippet so they are easy to open in VS Code.

---

## Comment style for "after" snippets

Use short end-of-line annotations sparingly, for example:

```csharp
if (!isInitialized) return; // new guard
ApplyDamage(amount);        // existing behavior preserved
RefreshHud();               // add this
````
