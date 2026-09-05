using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Media.Control;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Orin.Helpers;
using Orin.Models;
using Orin.Services;
using Orin.Views;

namespace Orin.ViewModels
{
    public partial class MainWindowViewModel : ObservableRecipient, IRecipient<ToggleOverlayMessage>, IRecipient<ConfigChangedMessage>
    {
        
        //config variables
        private FileHelper _fileHelper;
        
        [ObservableProperty]
        private Config? _config;
        
        //overlay variables 
        [ObservableProperty] 
        private OverlayWindow? _overlayWindow;
        
        [ObservableProperty] private string? _overlayText;

        private List<string> _overlayTexts = new() { "Show Overlay", "Hide Overlay", "Open Overlay Failed" };

        [ObservableProperty] 
        private bool _isOverlayVisible;
        
        
        //media service variables
        private MediaService? _mediaService;

        private GlobalSystemMediaTransportControlsSessionManager? _sessionManager;

        [ObservableProperty] 
        private List<GlobalSystemMediaTransportControlsSession?> _mediaSessions = new();
        
        
        [ObservableProperty]
        private GlobalSystemMediaTransportControlsSession? _selectedSession;

        public MainWindowViewModel()
        {
            if (Design.IsDesignMode)
            {
                OverlayText = _overlayTexts[0];
            }
            _fileHelper =  new FileHelper();
            _config = _fileHelper.ReadConfig();
            RegisterMessages();
            Initialize();
            
        }

        private void Initialize()
        {
            OverlayText = _overlayTexts[0];
        }

        private void RegisterMessages()
        {
            WeakReferenceMessenger.Default.Register<ToggleOverlayMessage>(this);
            WeakReferenceMessenger.Default.Register<ConfigChangedMessage>(this);
        }
        
        public async void Receive(ToggleOverlayMessage message)
        {
            await OverlayButtonClick();
        }

        [RelayCommand]
        private async Task OverlayButtonClick()
        {
            if (IsOverlayVisible)
            {
                if (OverlayWindow != null)
                {
                    OverlayWindow.Close();
                    IsOverlayVisible = false;
                    OverlayText = _overlayTexts[0];
                }
            }
            else
            {
                _mediaService = new();
                try
                {
                    OverlayWindowViewModel overlayDataContext = new();

                    _sessionManager = await _mediaService.InitializeAsync();
                    if (_sessionManager == null)
                    {
                        throw new NotImplementedException("Failed to initialize media service");
                    }

                    MediaSessions = await _mediaService.GetSessions(_sessionManager);

                    foreach(GlobalSystemMediaTransportControlsSession? session in MediaSessions)
                    {
                        Debug.WriteLine(session.SourceAppUserModelId);
                    }

                    overlayDataContext = new(_mediaService, Config);

                    OverlayWindow = new()
                    {
                        DataContext = overlayDataContext
                    };

                    OverlayWindow.Show();

                    OverlayText = _overlayTexts[1];
                    IsOverlayVisible = true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    IsOverlayVisible = false;
                    OverlayText = _overlayTexts[2];
                }

            }
        }


        [RelayCommand]
        public async Task RefreshMediaSessions()
        {
            if (_mediaService != null && _sessionManager != null)
                MediaSessions = await _mediaService.GetSessions(_sessionManager);
            else
            {
                throw new NotImplementedException();
            }
        }

        [RelayCommand]
        private void ToggleSettings()
        {
            SettingsViewModel svm = new SettingsViewModel(Config); 
            SettingsWindow settingsWindow = new()
            {
                DataContext = svm
            };

            settingsWindow.Show();
        }

        partial void OnSelectedSessionChanged(GlobalSystemMediaTransportControlsSession Session)
        {
            WeakReferenceMessenger.Default.Send<SessionChangedMessage>(new SessionChangedMessage(Session));
        }


        public void Receive(ConfigChangedMessage message)
        {
            Config = _fileHelper.ReadConfig();
        }
    }

}