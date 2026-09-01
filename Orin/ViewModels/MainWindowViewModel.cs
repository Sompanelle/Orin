using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Media.Control;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Orin.Services;
using Orin.Views;

namespace Orin.ViewModels
{
    public partial class MainWindowViewModel : ObservableRecipient, IRecipient<ToggleOverlayMessage>
    {

        [ObservableProperty] 
        private OverlayWindow? _overlayWindow;
        [ObservableProperty] 
        private OverlayWindowViewModel? _overlayWindowViewModel;

        [ObservableProperty] 
        private string? _overlayText;
        
        private List<string> _overlayTexts = new() { "Show Overlay", "Hide Overlay", "Open Overlay Failed"};

        [ObservableProperty] 
        private bool? _isOverlayVisible = false;

        private MediaService? _mediaService;
        
        private GlobalSystemMediaTransportControlsSessionManager? _sessionManager;

        [ObservableProperty]
        private List<GlobalSystemMediaTransportControlsSession?> _mediaSessions = new();

        private GlobalSystemMediaTransportControlsSession? _selectedSession;


        public GlobalSystemMediaTransportControlsSession? SelectedSession
        {
            get => _selectedSession;
            set
            {
                _selectedSession = value;
                if (OverlayWindowViewModel != null)
                    OverlayWindowViewModel.CurrentSession = value;
            }
        
        }


        public MainWindowViewModel()
        {
            if (Design.IsDesignMode)
            {
                OverlayText = "Show Overlay";
            }

            IsActive = true;

            Initialize();
        }

        private void Initialize()
        {
            OverlayText = _overlayTexts[0];
        }

        public async void Receive(ToggleOverlayMessage message)
        {
           await OverlayButtonClick();
        }
        
        [RelayCommand]
        private async Task OverlayButtonClick()
        {
            if (IsOverlayVisible == true)
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
                    _sessionManager = await _mediaService.InitializeAsync();
                    if (_sessionManager == null)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        MediaSessions = await _mediaService.GetSessions(_sessionManager);
                        OverlayWindowViewModel = new(_mediaService);
                    }

                    OverlayWindow = new()
                    {
                        DataContext = OverlayWindowViewModel
                    };
                    
                    OverlayWindow.Show();

                    OverlayText = _overlayTexts[1];
                    IsOverlayVisible = true;
                }
                catch(Exception ex)
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
            if (_mediaService != null &&  _sessionManager != null)
                MediaSessions = await _mediaService.GetSessions(_sessionManager);
            else
            {
                throw new NotImplementedException();
            }
        }
        
    }

}