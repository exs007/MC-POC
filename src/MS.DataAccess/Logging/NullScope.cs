using System;

namespace MS.DataAccess.Logging
{
    public class NullScope : IDisposable
    {
        public void Dispose()
        {
        }
    }
}