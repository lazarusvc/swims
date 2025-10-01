namespace SWIMS.Models.Email;

public readonly record struct EmailAddress(string Address, string? DisplayName = null);
