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
using ExCSS;
using Orin.Models;
using Orin.Services;


namespace Orin.ViewModels
{
    //TODO: Window Resizing,, auto pls
    //TODO: Click thru thanks! 
    public partial class OverlayWindowViewModel : ObservableRecipient, IRecipient<ToggleClickThroughMessage>, IRecipient<SessionChangedMessage>
    {
        [ObservableProperty]
        private string? _song;
        [ObservableProperty] 
        private string? _artist;
        [ObservableProperty] 
        private Bitmap? _thumbnail;
        [ObservableProperty]
        private string? _playingStatusImg;

        [ObservableProperty]
        private string _svgColor = "white";

        [ObservableProperty]
        private string _svgColorString;
        
        [ObservableProperty] 
        private bool _isPlaying;

        [ObservableProperty] 
        private bool _displayPlaybackControls;

        private bool _isClickThrough;

        private int _lastLength;

        private IMediaService _mediaService;

        private double lastLength;

        private double newLength;
        
        private GlobalSystemMediaTransportControlsSessionTimelineProperties? _timelineInfo;
        [ObservableProperty]
        
        private GlobalSystemMediaTransportControlsSessionPlaybackInfo? _playbackInfo;
        
        [ObservableProperty]
        private GlobalSystemMediaTransportControlsSession? _currentSession;
        
        private GlobalSystemMediaTransportControlsSessionMediaProperties? _mediaProperties;
        
        //Config Variables
        [ObservableProperty]
        private float _backgroundOpacity = 0;
        
        [ObservableProperty]
        private CornerRadius _borderRadius = new CornerRadius(0);

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
            _ = InitializeOverlay();
            
            if (Config.IsClickThroughDefault)
            {
                DisplayPlaybackControls = false;
            }
            
            if (Config.OverlayBackgroundOpacity > 0)
                BackgroundOpacity = Config.OverlayBackgroundOpacity;
            
            if (Config.OverlayBorderRadius > 0)
                BorderRadius =  new CornerRadius(Config.OverlayBorderRadius);
            
        }
        
        private async Task InitializeOverlay()
        {
            IsActive = true;
            
            DisplayPlaybackControls = true;
            SvgColorString = $".playback {{ fill: {SvgColor};}}";
            
            
            await GetInfo();
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
                await UpdateCurrentSession();
                await GetCurrentMedia();
            }
            else
            {
                await GetCurrentMedia();
                PlaybackInfo = await GetCurrentPlaybackInfo();
                if (PlaybackInfo != null)
                {
                    await UpdatePlaybackStatus(PlaybackInfo);
                }
            }
        }
        
        private async Task UpdateCurrentSession()
        {
            CurrentSession = await _mediaService.UpdateCurrentSession();
        }
        
        
        private async Task GetCurrentMedia()
        {
            GlobalSystemMediaTransportControlsSessionMediaProperties? mediaProperties = null;
            try
            {
                if (CurrentSession != null)
                    mediaProperties = await _mediaService.GetCurrentMediaPropertiesAsync(CurrentSession);

            }
            catch
            {
                CurrentSession = null;
                mediaProperties = null;
            }
            
            _mediaProperties = mediaProperties;
            
            if (_mediaProperties == null)
            {
                DisplayPlaybackControls = false;
                Song = "No media playing";
                Artist = "";
                CalculateWindowSize(Song.Length, false);
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
                    
                    CalculateWindowSize(charCount, true);
                }
            }


        }

        public void CalculateWindowSize(int CharCount, bool MediaPlaying)
        {
            newLength = (DisplayPlaybackControls) ? (CharCount * 9) + 180 : (CharCount * 9) + 70;
            
            if (!MediaPlaying)
                newLength -= 50;
            
            if (!newLength.Equals(lastLength))
            {
                _lastLength = (int)newLength;
                WeakReferenceMessenger.Default.Send(new ResizeWindowMessage(_lastLength));
            }
        }


        private async Task<GlobalSystemMediaTransportControlsSessionPlaybackInfo?> GetCurrentPlaybackInfo()
        {
            if (CurrentSession != null)
            {
                var pbInfo = await _mediaService.GetPlaybackInfoAsync(CurrentSession);
                if (pbInfo != null)
                {
                    if (pbInfo.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing)
                        IsPlaying = true;
                    else if (pbInfo.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Paused)
                        IsPlaying = false;
                    return pbInfo;
                }
                else return null;
            }
            else
            {
                return null;
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
        async Task UpdatePlaybackStatus(GlobalSystemMediaTransportControlsSessionPlaybackInfo PlaybackInfo)
        {
             await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    try
                    {
                        switch (PlaybackInfo.PlaybackStatus)
                        {
                            case GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing:
                                PlayingStatusImg = "avares://Orin/Assets/Pause.svg";
                                break;
                            case GlobalSystemMediaTransportControlsSessionPlaybackStatus.Paused:
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
                PlaybackInfo = await _mediaService.GetPlaybackInfoAsync(Session);

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
                PlaybackInfo = await GetCurrentPlaybackInfo();
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
        

        public void Receive(SessionChangedMessage message)
        {
            CurrentSession = message.Session;
            Task.Run(GetInfo);
        }
        
        public void Receive(ToggleClickThroughMessage message)
        {
            DisplayPlaybackControls = !DisplayPlaybackControls;
            _isClickThrough = !_isClickThrough;
        }
    }
    
}