namespace Api.Common.Exceptions;

public class NotificationFailedException(string message) : Exception(message);