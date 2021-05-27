using System.Diagnostics;

namespace NgrokSharp.PlatformSpecific
{
    public class PlatformCode
    {
        private readonly IPlatformStrategy _platformStrategy;

        public PlatformCode(IPlatformStrategy platformStrategy)
        {
            _platformStrategy = platformStrategy;
        }

        public void RegisterAuthToken(string authtoken)
        {
            _platformStrategy.RegisterAuthToken(authtoken);
        }

        public void StartNgrok(string region)
        {
            _platformStrategy.StartNgrok(region);
        }
        public void StopNgrok()
        {
            _platformStrategy.StopNgrok();
        }
    }
}