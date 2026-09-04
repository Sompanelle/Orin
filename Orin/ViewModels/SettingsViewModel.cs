using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
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

    [RelayCommand]
    private void SaveSettings()
    {
        
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