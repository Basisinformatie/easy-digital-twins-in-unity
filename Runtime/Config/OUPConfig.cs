namespace DefaultNamespace;

public class OUPConfig
{
    public string ApiUrl { get; set; } = "https://api.example.com";
    public int TimeoutSeconds { get; set; } = 30;

    public bool IsValid()
    {
        return !string.IsNullOrEmpty(ApiUrl) && TimeoutSeconds > 0;
    }
}