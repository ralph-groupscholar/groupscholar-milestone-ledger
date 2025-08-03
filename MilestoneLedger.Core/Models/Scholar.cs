namespace MilestoneLedger.Core.Models;

public sealed record Scholar(
    int Id,
    string FullName,
    string? Cohort,
    string? Status,
    DateTimeOffset CreatedAt
);
