using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.Messaging;
using Orin.Models;
using Orin.Services;

namespace Orin.Helpers;
//TODO: THIS...
public class FileHelper
{
    private string _configDir;
    

    public FileHelper()
    {
        _configDir = @$"{Directory.GetCurrentDirectory()}\config\config.json";
        
    }
    
    public Config? ReadConfig()
    {
        
        if (!File.Exists(_configDir) || !Directory.Exists(_configDir.Remove(_configDir.Length - 11, 11)))
            return null;
        
        var configText = File.ReadAllText(_configDir);

        Config? config = JsonSerializer.Deserialize<Config>(configText);
        
        if (config == null)
            return null;
        
        WeakReferenceMessenger.Default.Send(new ChangeThemeMessage(config.Theme));
        
        return config;
    }

    public bool WriteConfig(Config config)
    {
        string dir = _configDir.Remove(_configDir.Length - 11, 11);

        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        
        string configText = JsonSerializer.Serialize<Config>(config);
        
        File.WriteAllText(_configDir, configText);


        return false;
    }
    
    
}