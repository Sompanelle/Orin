using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Windows.Media.Control;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Avalonia.Svg;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Orin.Helpers;
using Orin.Services;
using Svg.Skia;


namespace Orin.ViewModels
{
    public partial class OverlayWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _song;
        [ObservableProperty] 
        private string _artist;
        [ObservableProperty] 
        private Bitmap _thumbnail;
        [ObservableProperty]
        private string _playingStatusImg;
    
        [ObservableProperty] 
        private bool _isPlaying;

        private IMediaService _mediaService;
        
        private GlobalSystemMediaTransportControlsSessionTimelineProperties? _timelineInfo;
        private GlobalSystemMediaTransportControlsSessionPlaybackInfo? _playbackInfo;
        [ObservableProperty]
        private GlobalSystemMediaTransportControlsSession? _currentSession;
        private GlobalSystemMediaTransportControlsSessionMediaProperties? _mediaProperties;

        
        public OverlayWindowViewModel(IMediaService MediaService)
        {
            if (Design.IsDesignMode)
            {
                Song = "Crime Pays";
                Artist = "Freddie Gibbs";
                
            }

            
            _mediaService = MediaService;
            _ = InitializeOverlay();
            
        }
        
        
        
        
        public async Task InitializeOverlay()
        {
            await GetInfo();

            await AppLoop();
        }
        
        public async Task AppLoop()
        {
            bool running = true;
            while (running)
            {
                await Task.Run(async () =>
                {
                    while (IsPlaying)
                    {
                        await GetInfo();
                        await Task.Delay(500);
                    }
                    

                    while (!IsPlaying)
                    {
                        await GetCurrentPlaybackInfo();
                        await Task.Delay(2000);
                    }

                });
            }
        }
        
        public async Task GetInfo()
        {
            _playbackInfo = await _mediaService.GetPlaybackInfoAsync();
            CurrentSession = await _mediaService.GetCurrentMediaSessionAsync();
            _timelineInfo = await _mediaService.GetTimeLineInfoAsync();
            _mediaProperties = await _mediaService.GetCurrentMediaPropertiesAsync();
            
            await GetCurrentMedia();
            UpdatePlaybackStatus(_playbackInfo);
            await GetCurrentPlaybackInfo();
            await GetCurrentTimeLineInfo();
            
        }
        
        private async Task GetCurrentMedia()
        {
            if (_mediaProperties != null)
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
                }
               
            }

        }

        private async Task GetCurrentPlaybackInfo()
        {
            _playbackInfo = await _mediaService.GetPlaybackInfoAsync();
            if (_playbackInfo != null)
            {
                if (_playbackInfo.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing)
                    IsPlaying = true;
                else if (_playbackInfo.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Paused)
                    IsPlaying = false;
                await Task.Delay(100);
                await _mediaService.UpdateCurrentSession();
                _playbackInfo = await _mediaService.GetPlaybackInfoAsync();
                UpdatePlaybackStatus(_playbackInfo);
            }
            else
            {
                

            }
        }

        private async Task GetCurrentTimeLineInfo()
        {
            if (_timelineInfo != null)
            {
                
            }
            else
            {

            }
        }
        
        //Update Playback Icon Function
        void UpdatePlaybackStatus(GlobalSystemMediaTransportControlsSessionPlaybackInfo PlaybackInfo)
        {
            Dispatcher.UIThread.Invoke(() =>
                {
                    try
                    {
                        if (PlaybackInfo.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing)
                            PlayingStatusImg = "avares://Orin/Assets/Pause.svg";
                        else if (PlaybackInfo.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Paused)
                            PlayingStatusImg = "avares://Orin/Assets/Play.svg";
                    }
                    
                    catch (Exception e)
                    {
                        Debug.WriteLine($"Failed to change playing status img source {e}");
                    }

                }
            );
        }

        [RelayCommand]
        async Task PausePlay(GlobalSystemMediaTransportControlsSession CurrentSession)
        {
            await _mediaService.GetCurrentMediaSessionAsync();
            _playbackInfo = await _mediaService.GetPlaybackInfoAsync();
            
            
            if (_playbackInfo.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing)
            {
                await CurrentSession.TryPauseAsync();
                
            }

            if (_playbackInfo.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Paused)
            {
                await CurrentSession.TryPlayAsync();
            }
            
            await GetCurrentPlaybackInfo();
            
        }
        
        
        [RelayCommand]
        private async Task Skip(GlobalSystemMediaTransportControlsSession CurrentSession)
        {
            CurrentSession.TrySkipNextAsync();
            await Task.Delay(100);
            await _mediaService.UpdateCurrentSession();
            _mediaProperties = await _mediaService.GetCurrentMediaPropertiesAsync();
            await GetCurrentMedia();
        }

        [RelayCommand]
        private async Task Prev(GlobalSystemMediaTransportControlsSession CurrentSession)
        {
            CurrentSession.TrySkipPreviousAsync();
            await Task.Delay(100);
            await _mediaService.UpdateCurrentSession();
            _mediaProperties = await _mediaService.GetCurrentMediaPropertiesAsync();
            await GetCurrentMedia();
        }
    }
    
}