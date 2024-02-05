using System.ComponentModel.DataAnnotations;

namespace Tgs.PlayerService.ViewModels;

public record PlayerLoginRecordQueryGroupPlayerViewModel
{
    [Required]
    [DataType(DataType.DateTime)]
    public DateTimeOffset StartOn { get; init; }
    [Required]
    [DataType(DataType.DateTime)]
    public DateTimeOffset EndOn { get; init; }
    [Required]
    [Range(minimum: 1, maximum: int.MaxValue)]
    public int Page { get; init; }
    [Required]
    [Range(minimum: 1, maximum: 1000)]
    public int Rows { get; init; }
}
