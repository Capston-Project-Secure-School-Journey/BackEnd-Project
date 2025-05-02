using System.Globalization;

namespace Api.Common.Exceptions;

public class DatabaseException : Exception
{
    public DatabaseException(string message) : base(message)
    {
    }

    public DatabaseException(string message, params object[] args)
        : base(string.Format(CultureInfo.CurrentCulture, message, args))
    {
    }
}