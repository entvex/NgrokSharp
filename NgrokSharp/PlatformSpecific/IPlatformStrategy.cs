using System;

namespace NgrokSharp.PlatformSpecific
{
    public interface IPlatformStrategy : IDisposable 
    {
        public void RegisterAuthToken(string authtoken);
        public void StartNgrok(string region);
        public void StopNgrok();
    }
}