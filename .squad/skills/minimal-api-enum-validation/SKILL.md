---
name: minimal-api-enum-validation
description: Use when exposing C# enums over JSON in ASP.NET Core minimal APIs and validating request contracts.
---

# Minimal API Enum Validation

When to use: When a .NET minimal API accepts enum values from JSON and numeric enum coercion would violate the API contract.

## Purpose
Prevent ASP.NET Core JSON binding from accepting integer enum values that are not part of the public string-based API contract.

## Process
1. Write an integration test that posts an integer enum value and expects `400 Bad Request`.
2. Register `JsonStringEnumConverter` with `allowIntegerValues: false`.
3. Keep string enum values as the contract; reject numeric values instead of silently coercing them.

```csharp
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(
        new JsonStringEnumConverter(allowIntegerValues: false));
});
```

## Anti-patterns
- Do not rely on default enum deserialization when the API contract says strings only.
- Do not add ad hoc endpoint validation when a central JSON option enforces the contract.

## Output
Numeric enum JSON like `{ "status": 123 }` returns `400 Bad Request`, while valid string statuses still bind normally.

confidence: medium
