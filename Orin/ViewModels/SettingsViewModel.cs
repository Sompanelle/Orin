using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Orin.Helpers;
using Orin.Models;
using Orin.Services;

namespace Orin.ViewModels;

partial class SettingsViewModel : ObservableObject, IRecipient<ConfigChangedMessage>
{
    
    [ObservableProperty]
    private bool _isFileSaved;
    
    private FileHelper? _fileHelper;

    [ObservableProperty]
    private List<string> _themeList = new()
    {
        "Dark",
        "Light",
        "System",
        "Green",
        "Pink"
    };

    //Config Variables
    [ObservableProperty]
    private string? _selectedTheme;
    
    [ObservableProperty] 
    private bool _clickThroughDefault;
    
    [ObservableProperty]
    private double _overlayBackgroundOpacity = 0.0;
    
    [ObservableProperty]
    private int _overlayBorderRadius = 0;

    public SettingsViewModel()
    {
        if (Design.IsDesignMode)
        {
            if (App.Current is App app)
            {
                app.RequestedThemeVariant = ThemeVariant.Dark;
            }
        }
    }
    
    public SettingsViewModel(Config? Config)
    {
        
        _fileHelper = new FileHelper();
        if (Config != null)
        {
            SelectedTheme = Config.Theme;
            ClickThroughDefault = Config.IsClickThroughDefault;
            OverlayBackgroundOpacity = Config.OverlayBackgroundOpacity*100;
            OverlayBorderRadius = Config.OverlayBorderRadius;
        }

        WeakReferenceMessenger.Default.Register(this );
    }

    [RelayCommand]
    private void SaveSettings()
    {
        Config newConf = new()
        {
            Theme = SelectedTheme ?? "System",
            IsClickThroughDefault = ClickThroughDefault,
            OverlayBackgroundOpacity = (float)OverlayBackgroundOpacity/100,
            OverlayBorderRadius = OverlayBorderRadius
        };
        
        _fileHelper.WriteConfig(newConf);
        
        WeakReferenceMessenger.Default.Send<ConfigChangedMessage>(new ConfigChangedMessage(newConf));
        
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

    public async void Receive(ConfigChangedMessage message)
    {
        IsFileSaved = true;
        await Task.Delay(3500);
        IsFileSaved = false;
    }
}