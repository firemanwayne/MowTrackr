namespace MowTrackr.Core.Attributes;

/// <summary>
/// Keeps a property out of the change history. Use it for secrets and hashes, so the history table never becomes a
/// second place they are stored.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class NotAuditedAttribute : Attribute
{
}
