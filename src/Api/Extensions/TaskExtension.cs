namespace Api.Extensions;

public static class TaskExtension
{
    public static void FireAndForget(
        this Task task,
        Action<Exception>? errorHandler = null)
    {
        task.ContinueWith(t =>
        {
            if (t.IsFaulted && errorHandler != null)
                errorHandler(t.Exception);
        }, TaskContinuationOptions.OnlyOnFaulted);
    }
}