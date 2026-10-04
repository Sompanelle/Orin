namespace Orin.Models;

public class Config
{
    public bool IsClickThroughDefault { get; set; }
    public required string Theme { get; set; }
    public float OverlayBackgroundOpacity { get; set; }
    public int OverlayBorderRadius { get; set; }
}