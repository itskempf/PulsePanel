namespace PulsePanel.Blueprints;

public class ValidationResult
{
    public string Status { get; set; } = "fail";
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public Models.Blueprint? Blueprint { get; set; }
    public string LicenseCheckResult { get; set; } = "NotChecked";

    public bool IsValid => Status == "pass" && Errors.Count == 0;
}
