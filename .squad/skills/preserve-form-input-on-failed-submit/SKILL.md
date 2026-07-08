---
name: preserve-form-input-on-failed-submit
description: Use when a React form submits asynchronously and should preserve user input after a failed save.
---

# Preserve Form Input on Failed Submit

When to use: When a frontend form calls an async handler and should reset only after the save succeeds.

## Purpose
Avoid losing user-entered data when an API call fails.

## Process
1. Make the submit handler return `Promise<boolean>` or an equivalent success result.
2. Return `true` only after the mutation succeeds; return `false` after errors are surfaced.
3. In the form component, clear/reset inputs only when the handler reports success.
4. Add a test that failed submission preserves input and shows the error.

## Anti-patterns
- Do not reset form state unconditionally after awaiting a submit callback.
- Do not force users to retype data after recoverable API failures.

## Output
Failed submissions keep field values intact; successful submissions reset the form.

confidence: low
