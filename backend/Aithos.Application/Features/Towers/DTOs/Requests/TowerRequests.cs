using System.ComponentModel.DataAnnotations;

namespace Aithos.Application.Contracts.Requests;

public record CreateTowerDto(
    [Required] string SiteCode,
    [Required] string Name,
    [Required] string Location,
    string? Status
);

public record UpdateTowerDto(string Name, string Location, string Status);
