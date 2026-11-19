namespace MowTrackr.Core.Contracts.Maintenance;

/// <summary>
/// Result of saving a template: the saved template (absent when blocked) plus any validation findings.
/// </summary>
/// <param name="Template">The saved template, or <see langword="null"/> when errors blocked the save.</param>
/// <param name="Issues">Errors and warnings from interval validation.</param>
public sealed record ServiceTemplateSaveResponse(ServiceTemplateResponse? Template, IReadOnlyList<IntervalIssue> Issues);