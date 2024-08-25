using System;

namespace Palmalytics.SqlServer.Locking
{
    public class CouldNotAcquireLockException(string message) : Exception(message)
    {
    }
}
