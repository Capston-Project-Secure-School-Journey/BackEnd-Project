using System.Globalization;

namespace Api.Common.Utilities.Exceptions;

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