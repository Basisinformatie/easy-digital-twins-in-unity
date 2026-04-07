namespace DefaultNamespace;

public class ToolkitSettings
{
    public bool EnableLogging { get; set; } = true;
    public string LogPrefix { get; set; } = "[Toolkit]";

    public string FormatMessage(string message)
    {
        if (EnableLogging)
        {
            return $"{LogPrefix} {message}";
        }
        return message;
    }
}