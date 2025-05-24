using System.ComponentModel.DataAnnotations;

public class PayPalOptions
{
    [Required]
    public string ClientId { get; set; } = string.Empty;

    [Required]
    public string ClientSecret { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^(sandbox|live)$", ErrorMessage = "Mode must be 'sandbox' or 'live'.")]
    public string Mode { get; set; } = string.Empty;
}