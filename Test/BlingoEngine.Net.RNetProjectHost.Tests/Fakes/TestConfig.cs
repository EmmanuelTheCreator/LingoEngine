using BlingoEngine.Net.RNetContracts;
using BlingoEngine.Net.RNetHost.Common;

namespace BlingoEngine.Net.RNetProjectHost.Tests.Fakes;

internal sealed class TestConfig : IRNetConfiguration
{
    public TestConfig(int port) => Port = port;

    public int Port { get; set; }
    public bool AutoStartRNetHostOnStartup { get; set; }
    public string ClientName { get; set; } = "TestHost";
    public RNetRemoteRole RemoteRole { get; set; } = RNetRemoteRole.Host;
    public RNetClientType ClientType { get; set; } = RNetClientType.Project;
}
