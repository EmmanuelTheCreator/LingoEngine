using BlingoEngine.IO.Data.DTO;
using BlingoEngine.Net.RNetContracts;
using BlingoEngine.Net.RNetProjectClient;
using BlingoEngine.Net.RNetTerminal.Datas;
using BlingoEngine.Net.RNetTerminal.Dialogs;
using BlingoEngine.Net.RNetTerminal.Views;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Terminal.Gui;
using Terminal.Gui.App;
using Terminal.Gui.Configuration;
using Terminal.Gui.Drawing;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using Timer = System.Timers.Timer;

namespace BlingoEngine.Net.RNetTerminal;

public sealed class BlingoRNetTerminal : System.IAsyncDisposable
{
    private readonly RNetTerminalConnection _connection;
    private RNetTerminalConnectionOptions _connectionOptions;
    private readonly RNetTerminalSettings _settings;
    private static int _port;
    private static bool _connected;
    public static bool IsConnected => _connected;
    public static int Port => _port;
    private RootWindow _rootWindow = null!;
    private bool IsRemoteMode => _connection.IsConnected;

    public BlingoRNetTerminal()
    {
        _settings = RNetTerminalSettings.Load();
        _connectionOptions = new RNetTerminalConnectionOptions(_settings.Port, _settings.PreferredTransport);
        _connection = new RNetTerminalConnection();
        _connection.ConnectionStateChanged += OnConnectionStateChanged;
        _connection.PlayFrameReceived += OnPlayFrameReceived;
        _connection.LogMessage += Log;
        _port = _settings.Port;
    }

    public async Task RunAsync()
    {
        Application.Init();
        RNetTerminalStyle.SetMyTheme();
        _rootWindow = new RootWindow();
        
        var startupSelection = new StartupDialog(_connectionOptions.Port, _connectionOptions.Transport).Show();
        var transport = startupSelection.Mode switch
        {
            StartupSelectionMode.Http => RNetTerminalTransport.Http,
            StartupSelectionMode.Pipe => RNetTerminalTransport.Pipe,
            _ => _connectionOptions.Transport
        };
        _connectionOptions = _connectionOptions with { Port = startupSelection.Port, Transport = transport };
        if (startupSelection.Mode != StartupSelectionMode.Standalone)
        {
            SaveSettings();
            var options = _connectionOptions;
            _ = Task.Run(async () =>
            {
#if DEBUG
                await Task.Delay(300);
#else
                await Task.Delay(100);
#endif
                try
                {
                    await _connection.ConnectAsync(options).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Log($"Connection failed: {ex.Message}");
                }
            });
        }
        else
            TerminalDataStore.Instance.LoadTestData();
        Application.Begin(new Toplevel());
        _rootWindow.BuildUi(SendCommandAsync, ToggleConnectionAsync, SetPort);

        Application.Run();
        Application.Shutdown();
    }
   

    public async ValueTask DisposeAsync()
    {
        _connection.ConnectionStateChanged -= OnConnectionStateChanged;
        _connection.PlayFrameReceived -= OnPlayFrameReceived;
        _connection.LogMessage -= Log;
        await _connection.DisposeAsync().ConfigureAwait(false);
       
    }
    private void SetPort(int port)
    {
        _port = port;
        SaveSettings();
    }
    private void SaveSettings()
    {
        _settings.Port = _port;
        _settings.Save();
    }

    private void Log(string message)
    {
        _rootWindow.Log(message);
    }


    #region Connection
    public Task SendCommandAsync(RNetCommand cmd, CancellationToken? ct = default) => _connection.SendCommandAsync(cmd, ct);
    private async Task ToggleConnectionAsync()
    {
        try
        {
            if (!_connection.IsConnected)
                await _connection.ConnectAsync(_connectionOptions).ConfigureAwait(false);
            else
                await _connection.DisconnectAsync().ConfigureAwait(false);
        }
        catch (System.Exception ex)
        {
            Log($"Connection error: {ex.Message}");
        }
    }

    private void OnConnectionStateChanged(BlingoNetConnectionState state)
    {
        _connected = state == BlingoNetConnectionState.Connected;
        _rootWindow.UpdateConnectionStatus(state);
        UpdateLocalChangeMode();
    }
    private void UpdateLocalChangeMode()
    {
        var remote = IsRemoteMode;
        var store = TerminalDataStore.Instance;
        store.ApplyLocalChanges = !remote;
        _rootWindow.UpdateIsRemove(remote);
        
    }
    private void OnPlayFrameReceived(int frame)
    {
        TerminalDataStore.Instance.SetFrame(frame + 1, updateMovieState: false);
        _rootWindow.SetPlayFrame(frame);
    }
    #endregion

 

 
}


