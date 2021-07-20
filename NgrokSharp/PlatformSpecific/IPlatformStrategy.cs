using System;
using System.Threading.Tasks;

namespace NgrokSharp.PlatformSpecific
{
    public interface IPlatformStrategy : IDisposable 
    {
        public Task RegisterAuthTokenAsync(string authtoken);
        public void StartNgrok(string region);
        public void StopNgrok();
    }
}