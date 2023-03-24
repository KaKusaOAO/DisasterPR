namespace DisasterPR.Attributes;

[AttributeUsage(AttributeTargets.All)]
public class WebGlUnavailableAttribute : Attribute
{
    public WebGlUnavailableReason Reason { get; set; }

    public WebGlUnavailableAttribute(WebGlUnavailableReason reason)
    {
        Reason = reason;
    }
}

public enum WebGlUnavailableReason
{
    Multithreading
}