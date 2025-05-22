namespace Api.Attributes;

[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class BanAttemptLimitAttribute(int limit, int banTime, int observationWindow) : Attribute
{
    public int Limit { get; } = limit;

    /// <summary>
    /// Ban time, unit is second
    /// </summary>
    public int BanTime { get; set; } = banTime;

    /// <summary>
    /// ObservationWindow, unit is hour
    /// </summary>
    public int ObservationWindow { get; set; } = observationWindow;
}