using System.Diagnostics;

namespace NgrokSharp.PlatformSpecific
{
    public class PlatformCode
    {
        private readonly IPlatformStrategy _platformStrategy;
        private readonly Process _process;
        
        public PlatformCode(IPlatformStrategy platformStrategy, Process process)
        {
            _platformStrategy = platformStrategy;
            _process = process;
        }

        public void RegisterAuthToken(string authtoken)
        {
            _platformStrategy.RegisterAuthToken(_process,authtoken);
        }

        public void StartNgrok(string region)
        {
            _platformStrategy.StartNgrok(_process,region);
        }
    }
}