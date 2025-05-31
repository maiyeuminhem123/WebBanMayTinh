using System.ComponentModel.DataAnnotations;

public class PayPalOptions
{
    
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;

    [RegularExpression("^(sandbox|live)$", ErrorMessage = "Mode must be 'sandbox' or 'live'.")]
    public string Mode { get; set; } = string.Empty;
}