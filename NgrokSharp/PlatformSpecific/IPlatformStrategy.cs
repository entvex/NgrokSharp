using System.Diagnostics;

namespace NgrokSharp.PlatformSpecific
{
    public interface IPlatformStrategy
    {
        public void RegisterAuthToken(Process process ,string authtoken);
        public void StartNgrok(Process process, string region);
    }
}