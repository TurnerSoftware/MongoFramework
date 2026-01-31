# MongoDB.Driver 3.x Migration Guide

This document describes the changes when upgrading MongoFramework to use MongoDB.Driver 3.6.0 (from 2.21.0).

## Overview

MongoDB.Driver 3.0 removes the LINQ2 provider entirely and makes LINQ3 the only option. MongoFramework has been updated to work seamlessly with LINQ3.

## LINQ3 Compatibility

All common LINQ patterns work correctly with LINQ3:

| Pattern | Example | Status |
|---------|---------|--------|
| `Any()` with predicate | `dbSet.Any(x => x.Name == "test")` | **Supported** |
| `FirstOrDefault()` with predicate | `dbSet.FirstOrDefault(x => x.Id == id)` | **Supported** |
| `First()` with predicate | `dbSet.First(x => x.Id == id)` | **Supported** |
| `SingleOrDefault()` with predicate | `dbSet.SingleOrDefault(x => x.Id == id)` | **Supported** |
| `Single()` with predicate | `dbSet.Single(x => x.Id == id)` | **Supported** |
| `Where().FirstOrDefault()` | `dbSet.Where(x => x.Id == id).FirstOrDefault()` | **Supported** |
| `Count()` with predicate | `dbSet.Count(x => x.Active)` | **Supported** |

## Breaking Changes

### 1. DateTime Handling

MongoDB.Driver 3.x returns `DateTime` values in UTC. For consistent round-tripping, use UTC explicitly:

```csharp
// Recommended: Use UTC for dates
var date = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
```

Dates stored with `DateTimeKind.Unspecified` will be treated as local time, converted to UTC for storage, and returned as UTC.

### 2. GUID Serialization

MongoDB.Driver 3.x requires explicit `GuidRepresentation`. MongoFramework now configures `GuidRepresentation.Standard` (UUID binary subtype 4) by default for cross-platform compatibility.

If you were using a different GUID representation, you may need to migrate existing data or configure a different serializer.

### 3. Render() Method Signature (Internal)

If you use MongoDB driver APIs directly, the `Render()` method on definition builders now requires `RenderArgs<T>`:

```csharp
// Before
definition.Render(serializer, registry);

// After
var renderArgs = new RenderArgs<TEntity>(serializer, registry);
definition.Render(renderArgs);
```

### 4. IMongoClient is now IDisposable

`MongoClient` now implements `IDisposable`. MongoFramework's `MongoDbConnection.Dispose()` properly disposes the underlying client.

### 5. Index Options

The `Background` property on `CreateIndexOptions` is deprecated (since MongoDB 4.2). Index builds are now automatically optimized by the server.

## Migration Steps

1. Update the MongoFramework NuGet package to the version using MongoDB.Driver 3.6.0
2. Review any code that uses `DateTime` values - ensure UTC is used for consistent behavior
3. Run your test suite - most LINQ queries should work without changes
4. Review any direct MongoDB driver API usage for `Render()` signature changes

## Troubleshooting

### ExpressionNotSupportedException

If you encounter `ExpressionNotSupportedException`, it indicates a query pattern that LINQ3 cannot translate to the aggregation pipeline. Common solutions:

1. **Simplify the expression** - Break complex queries into simpler steps
2. **Use supported alternatives** - Some projections may need adjustment
3. **Fetch and filter in-memory** - For edge cases, fetch results first then filter

Example:
```csharp
// If a complex expression fails, try fetching first
var results = dbSet.Where(x => x.Category == "Active").ToArray();
var filtered = results.Where(x => ComplexCondition(x));
```

### DateTime Mismatch

If dates appear shifted by your timezone offset:
- Ensure you're using `DateTimeKind.Utc` when creating dates
- Consider using `DateTimeOffset` for timezone-aware timestamps

## Resources

- [MongoDB Driver Upgrade Guide](https://www.mongodb.com/docs/drivers/csharp/current/reference/upgrade/v3/)
- [LINQ3 Documentation](https://www.mongodb.com/docs/drivers/csharp/current/aggregation/linq/)
- [Breaking Changes in v3.0](https://www.mongodb.com/docs/drivers/csharp/v3.0/reference/release-notes/)
