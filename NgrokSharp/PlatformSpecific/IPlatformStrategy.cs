using System.Diagnostics;

namespace NgrokSharp.PlatformSpecific
{
    public interface IPlatformStrategy
    {
        public void RegisterAuthToken(string authtoken);
        public void StartNgrok(string region);
        public void StopNgrok();
    }
}