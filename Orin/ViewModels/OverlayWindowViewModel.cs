using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Media.Control;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Orin.Models;
using Orin.Services;


namespace Orin.ViewModels
{
    //TODO: Window Resizing,, auto pls
    public partial class OverlayWindowViewModel : ObservableRecipient, IRecipient<ToggleClickThroughMessage>, IRecipient<SessionChangedMessage>
    {
        
        //media variables
        [ObservableProperty]
        private string _song;
        
        [ObservableProperty] 
        private string? _artist;
        
        [ObservableProperty] 
        private Bitmap? _thumbnail;
        
        [ObservableProperty]
        private string? _playingStatusImg;
        
        [ObservableProperty]
        private string _svgColorString = $".playback {{ fill: white;}}";
        
        [ObservableProperty] 
        private bool _isPlaying;

        [ObservableProperty] 
        private bool _displayPlaybackControls = true;

        private bool _isClickThrough = false;

        private int _lastLength;

        private IMediaService _mediaService;
        
        private GlobalSystemMediaTransportControlsSessionTimelineProperties? _timelineInfo;
        
        [ObservableProperty]
        private GlobalSystemMediaTransportControlsSessionPlaybackInfo? _playbackInfo;
        
        [ObservableProperty]
        private GlobalSystemMediaTransportControlsSession? _currentSession;
        
        private GlobalSystemMediaTransportControlsSessionMediaProperties? _mediaProperties;
        
        //Config Variables
        [ObservableProperty]
        private float _backgroundOpacity;
        
        [ObservableProperty]
        private CornerRadius _borderRadius;

        public OverlayWindowViewModel()
        {
            if (Design.IsDesignMode)
            {
                Song = "Crime Pays";
                Artist = "Freddie Gibbs";
            }
            _mediaService = new MediaService();
            _mediaService.InitializeAsync();
            _ = InitializeOverlay();
            
        }
        
        public OverlayWindowViewModel(IMediaService MediaService)
        {
            _mediaService = MediaService;
            _ = InitializeOverlay();
        }

        public OverlayWindowViewModel(IMediaService MediaService,Config Config)
        {
            _mediaService = MediaService;
            ReadConfig(Config);
            _ = InitializeOverlay();
        }
        
        private async Task InitializeOverlay()
        {
            //observable recipient requirement
            IsActive = true;
            GetCurrentMedia().Wait();
            await AppLoop();
        }
        
        private async Task AppLoop()
        {
            bool running = true;
            while (running)
            {
                await GetInfo();
                if (IsPlaying)
                {
                    await Task.Delay(500);
                }
                else if (!IsPlaying)
                {
                    await Task.Delay(1500);
                }
            }
        }
        
        
        private async Task GetInfo()
        {
            if (!IsSessionValid(CurrentSession))
            {
                UpdateCurrentSession();
                await GetCurrentMedia();
            }
            else
            {
                await GetCurrentMedia();
                PlaybackInfo = _mediaService.GetPlaybackInfo(CurrentSession);
                if (PlaybackInfo != null)
                {
                    await UpdatePlaybackStatus(PlaybackInfo);
                }
            }
        }
        
        private void UpdateCurrentSession()
        {
            CurrentSession = _mediaService.UpdateCurrentSession();
        }
        
        
        private async Task GetCurrentMedia()
        {
            GlobalSystemMediaTransportControlsSessionMediaProperties? mediaProperties = null;
            try
            {
                if (CurrentSession != null)
                {
                    mediaProperties = await _mediaService.GetCurrentMediaPropertiesAsync(CurrentSession);
                    await UpdatePlaybackStatus(CurrentSession.GetPlaybackInfo());
                }

            }
            catch
            {
                CurrentSession = null;
                mediaProperties = null;
            }
            
            _mediaProperties = mediaProperties;
            
            if (_mediaProperties == null)
            {
                Song = "No media playing";
                Artist = "";
                Thumbnail = null;
                CalculateWindowSize(Song.Length, DisplayPlaybackControls);
                //Thumbnail = new Bitmap("avares://");
            }
            else
            {
                Song = _mediaProperties.Title;
                Artist = _mediaProperties.Artist;
                if (_mediaProperties.Thumbnail != null)
                {
                    var imgStream = await _mediaProperties.Thumbnail.OpenReadAsync();
                    MemoryStream stream = new MemoryStream();
                    await imgStream.AsStream().CopyToAsync(stream);
                    stream.Position = 0;
                    Bitmap bitmap = new(stream); 
                    Dispatcher.UIThread.Post(() =>
                    {
                        Thumbnail = bitmap;
                    });

                    int charCount = (Artist.Length > Song.Length) ? Artist.Length : Song.Length;
                    
                    CalculateWindowSize(charCount, DisplayPlaybackControls);
                }
            }


        }

        void CalculateWindowSize(int CharCount, bool PlaybackControls)
        {
            double newLength = (CharCount * 9) + 70;

            if (PlaybackControls)
            {
                newLength += 90;
            }
            
            if (!newLength.Equals(_lastLength))
            {
                _lastLength = (int)newLength;
                WeakReferenceMessenger.Default.Send(new ResizeWindowMessage(_lastLength));
            }
        }

        // private async Task GetCurrentTimeLineInfo()
        // {
        //     if (_timelineInfo != null)
        //     {
        //         
        //     }
        //     else
        //     {
        //
        //     }
        // }
        
        //Update Playback Icon Function
        async Task UpdatePlaybackStatus(GlobalSystemMediaTransportControlsSessionPlaybackInfo CurrentPlaybackInfo)
        {
             await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    try
                    {
                        switch (CurrentPlaybackInfo.PlaybackStatus)
                        {
                            case GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing:
                                IsPlaying = true;
                                PlayingStatusImg = "avares://Orin/Assets/Pause.svg";
                                break;
                            case GlobalSystemMediaTransportControlsSessionPlaybackStatus.Paused:
                                IsPlaying = false;
                                PlayingStatusImg = "avares://Orin/Assets/Play.svg";
                                break;
                            case GlobalSystemMediaTransportControlsSessionPlaybackStatus.Stopped:
                                IsPlaying = false;
                                PlayingStatusImg = "avares://Orin/Assets/Play.svg";
                                break;
                        }
                            
                    }
                    
                    catch (Exception e)
                    {
                        Debug.WriteLine($"Failed to change playing status img source {e}");
                    }

                }
            );
        }

        private void ReadConfig(Config Config)
        {
            if (Config.IsClickThroughDefault)
            {
                _isClickThrough = true;
                DisplayPlaybackControls = false;
            }
            
            if (Config.OverlayBackgroundOpacity > 0)
                BackgroundOpacity = Config.OverlayBackgroundOpacity;
            
            if (Config.OverlayBorderRadius > 0)
                BorderRadius =  new CornerRadius(Config.OverlayBorderRadius);
        }
        
        private bool IsSessionValid(GlobalSystemMediaTransportControlsSession? Session)
        {
            if (Session != null && Session.GetPlaybackInfo() != null)
                return true;
            else
                return false;
        }

        [RelayCommand]
        private void ToggleWindow(Window Window)
        {
            Window.IsVisible = !Window.IsVisible;
            Window.ShowInTaskbar = !Window.ShowInTaskbar;
        }
        
        [RelayCommand]
        async Task PausePlay(GlobalSystemMediaTransportControlsSession Session)
        {
            if (IsSessionValid(Session))
            {
                PlaybackInfo = _mediaService.GetPlaybackInfo(Session);

                if (PlaybackInfo != null)
                {
                    switch (PlaybackInfo.PlaybackStatus)
                    {
                        case GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing:
                            await Session.TryPauseAsync();
                            break;
                        case GlobalSystemMediaTransportControlsSessionPlaybackStatus.Paused:
                            await Session.TryPlayAsync();
                            break;
                    
                    }
                }

                await Task.Delay(200);
                PlaybackInfo = _mediaService.GetPlaybackInfo(Session);
                if (PlaybackInfo != null)
                    await UpdatePlaybackStatus(PlaybackInfo);
            }
            //TODO
        }


        [RelayCommand]
        private async Task Skip(GlobalSystemMediaTransportControlsSession Session)
        {
            if (IsSessionValid(Session))
            {
                Session.TrySkipNextAsync();
                await Task.Delay(200);
                await GetCurrentMedia();
            }
        }

        [RelayCommand]
        private async Task Prev(GlobalSystemMediaTransportControlsSession Session)
        {
            if (IsSessionValid(Session))
            {
                Session.TrySkipPreviousAsync();
                await Task.Delay(200);
                await GetCurrentMedia();
            }
        }
        

        public void Receive(SessionChangedMessage Message)
        {
            CurrentSession = Message.Session;
            Task.Run(GetInfo);
        }
        
        public void Receive(ToggleClickThroughMessage Message)
        {
            _isClickThrough = !_isClickThrough;

            DisplayPlaybackControls = !_isClickThrough;
            
            int charCount = Song.Length;
            
            if (Artist != null)
            {
                charCount = (Artist.Length > Song.Length) ? Artist.Length : Song.Length;
            }
            
            CalculateWindowSize(charCount, DisplayPlaybackControls);

        }
    }
    
}