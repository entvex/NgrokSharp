using System;

namespace NgrokSharp.Tests
{
    public class NgrokManagerOneTimeSetUp : IDisposable
    {
        public string? environmentVariableNgrokYml;
        public NgrokManagerOneTimeSetUp()
        {
            environmentVariableNgrokYml = Environment.GetEnvironmentVariable("ngrokYml");
        }

        public void Dispose()
        {
            
        }
    }
}