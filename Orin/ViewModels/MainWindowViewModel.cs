using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Media.Control;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Orin.Services;
using Orin.Views;

namespace Orin.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {

        [ObservableProperty] 
        private OverlayWindow _overlayWindow;
        [ObservableProperty] 
        private OverlayWindowViewModel _overlayWindowViewModel;

        [ObservableProperty] 
        private string _overlayText;

        [ObservableProperty] 
        private bool? _isOverlayVisible;

        [ObservableProperty]
        private List<GlobalSystemMediaTransportControlsSession> _mediaSessions = new();

        private GlobalSystemMediaTransportControlsSession _selectedSession;


        public GlobalSystemMediaTransportControlsSession SelectedSession
        {
            get => _selectedSession;
            set
            {
                _selectedSession = value;
                OverlayWindowViewModel.CurrentSession = value;
            }
        
        }


        public MainWindowViewModel()
        {
            if (Design.IsDesignMode)
            {
                OverlayText = "Show Overlay";
            }

            Initialize();
        }

        public void Initialize()
        {
            if (IsOverlayVisible == null)
            {
                OverlayText = "Show Overlay";
                IsOverlayVisible = false;
            }
        }
        
        
        [RelayCommand]
        public async Task OverlayButtonClick()
        {
            if (IsOverlayVisible == true)
            {
                OverlayWindow.Close();
                IsOverlayVisible = false;
                OverlayText = "Show Overlay";
            }
            else if (IsOverlayVisible == false)
            {
                MediaService mediaService = new();
                try
                {
                    var pbManager = await mediaService.InitializeAsync();
                    MediaSessions = await mediaService.GetSessions(pbManager);
                    OverlayWindowViewModel = new(mediaService);

                    Debug.WriteLine($"Song: {OverlayWindowViewModel.Song}");
                    

                    OverlayWindow = new()
                    {
                        DataContext = OverlayWindowViewModel
                    };
                    OverlayWindow.Show();
                    
                    
                    OverlayText = "Hide Overlay";
                    IsOverlayVisible = true;
                }
                catch(Exception ex)
                {
                    IsOverlayVisible = false;
                    OverlayText = "Opened Failed";
                }
                

                
                //help it start up

                

            }
        }

    }

}