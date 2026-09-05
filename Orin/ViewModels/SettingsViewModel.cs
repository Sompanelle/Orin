using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Orin.Helpers;
using Orin.Models;
using Orin.Services;

namespace Orin.ViewModels;

partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty]
    private string? _selectedTheme;

    [ObservableProperty]
    private List<string> _themeList = new()
    {
        "Dark",
        "Light",
        "System",
        "Green"
    };

    [ObservableProperty] 
    private bool _isClickThroughDefault;
    
    private FileHelper _fileHelper;
    
    public SettingsViewModel(Config? Config)
    {
        if (Design.IsDesignMode)
        {
            if (App.Current is App app)
            {
                app.RequestedThemeVariant = ThemeVariant.Dark;
            }
        }
        _fileHelper = new FileHelper();

        if (Config != null)
        {
            SelectedTheme = Config.Theme;
            IsClickThroughDefault = Config.IsClickThroughDefault;
        }
    }

    [RelayCommand]
    private void SaveSettings()
    {
        Config _newConf = new()
        {
            Theme = SelectedTheme ?? "System",
            IsClickThroughDefault = IsClickThroughDefault
        };
        
        _fileHelper.WriteConfig(_newConf);
        
        WeakReferenceMessenger.Default.Send<ConfigChangedMessage>(new ConfigChangedMessage(_newConf));
        
    }
    
    [RelayCommand]
    private void CloseSettings()
    {
        WeakReferenceMessenger.Default.Send<CloseSettingsWindowMessage>(new CloseSettingsWindowMessage());
    }

    
    partial void OnSelectedThemeChanged(string value)
    {
        
        WeakReferenceMessenger.Default.Send<ChangeThemeMessage>(new ChangeThemeMessage(value));
        
    }
    
}