using System.ComponentModel.DataAnnotations;

namespace Tgs.PlayerService.ViewModels;

public record PlayerQueryPlayerBirthdayViewModel
{
    [Required]
    [DataType(DataType.Date)]
    public DateOnly Birthday { get; init; }
    [Required]
    [Range(minimum: 1, maximum: int.MaxValue)]
    public int Page { get; init; }
    [Required]
    [Range(minimum: 1, maximum: 1000)]
    public int Rows { get; init; }
}
