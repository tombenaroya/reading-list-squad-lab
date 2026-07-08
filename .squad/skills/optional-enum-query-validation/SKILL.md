---
name: optional-enum-query-validation
description: Use when an API endpoint accepts an optional enum-like query parameter and must distinguish missing, all/no-filter, and invalid values.
---

# Optional Enum Query Validation

## Purpose
Preserve clear filter semantics for optional enum-like query parameters without letting invalid input silently behave like no filter.

## Process
1. Bind the query parameter as a nullable string, not directly as an enum.
2. Treat `null` as not provided; explicitly map any supported no-filter alias such as `all`.
3. Trim and parse valid enum values case-insensitively.
4. Return `400` validation/problem+json with an error key matching the query parameter for blank or unknown values.
5. Add tests for not provided, no-filter alias, each valid value, casing, blank, and invalid values.

## Anti-patterns
- Do not collapse invalid values into the same path as a missing parameter.
- Do not rely on default enum binding when the API contract needs custom aliases.

## Output
Optional filters are ergonomic for clients while invalid query values fail loudly and predictably.

confidence: low
