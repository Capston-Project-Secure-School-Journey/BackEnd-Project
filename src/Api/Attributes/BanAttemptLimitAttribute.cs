namespace Api.Attributes;

[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class BanAttemptLimitAttribute : Attribute
{
    public int Limit { get; }

    /// <summary>
    /// Ban time, unit is second
    /// </summary>
    public int BanTime { get; set; }

    /// <summary>
    /// ObservationWindow, unit is hour
    /// </summary>
    public int ObservationWindow { get; set; }

    public BanAttemptLimitAttribute(int limit, int banTime, int observationWindow)
    {
        Limit = limit;
        BanTime = banTime;
        ObservationWindow = observationWindow;
    }
}