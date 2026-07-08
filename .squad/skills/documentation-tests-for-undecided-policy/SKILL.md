---
name: documentation-tests-for-undecided-policy
description: Use when observed behavior lacks an agreed product or architectural policy but should not regress accidentally.
---

# Documentation Tests for Undecided Policy

When to use: When behavior is currently allowed, but the team has not decided whether it is the desired long-term rule.

## Purpose
Document current behavior safely without turning an undecided product question into an accidental permanent rule.

## Process
1. Write a test whose name states it documents current behavior.
2. Add a short comment that the policy is undecided.
3. Assert the current behavior exactly; do not invent a stricter rule in tests.
4. Raise the policy question separately for product/lead decision-making.

```csharp
[Fact]
public async Task PostBooks_DuplicateTitleAndAuthor_DocumentsCurrentAllowedBehavior()
{
    // Product policy is undecided; this test documents current behavior only.
    var first = await Client.PostAsJsonAsync("/api/books", request);
    var second = await Client.PostAsJsonAsync("/api/books", request);

    first.StatusCode.Should().Be(HttpStatusCode.Created);
    second.StatusCode.Should().Be(HttpStatusCode.Created);
}
```

## Anti-patterns
- Do not assert a stricter policy before the team has agreed to it.
- Do not leave surprising current behavior undocumented if tests expose it.

## Output
A clearly named test that preserves known behavior and a separate follow-up for the undecided policy.

confidence: low
