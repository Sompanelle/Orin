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
    //TODO: Window Resizing,, auto pls
    //TODO: Click thru thanks! 
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

        [ObservableProperty] 
        private bool _displayPlaybackControls = true;

        private IMediaService _mediaService;
        
        private GlobalSystemMediaTransportControlsSessionTimelineProperties? _timelineInfo;
        [ObservableProperty]
        private GlobalSystemMediaTransportControlsSessionPlaybackInfo? _playbackInfo;
        [ObservableProperty]
        private GlobalSystemMediaTransportControlsSession? _currentSession;
        private GlobalSystemMediaTransportControlsSessionMediaProperties? _mediaProperties;


        public OverlayWindowViewModel()
        {
            if (Design.IsDesignMode)
            {
                Song = "Crime Pays";
                Artist = "Freddie Gibbs";
                
            }
        }
        
        public OverlayWindowViewModel(IMediaService MediaService)
        {
            
            _mediaService = MediaService;
            _ = InitializeOverlay();
            
        }

        public async Task UpdateCurrentSession()
        {
            CurrentSession = await _mediaService.UpdateCurrentSession(CurrentSession);
        }
        
        
        public async Task InitializeOverlay()
        {
            await GetInfo();

            await AppLoop();
        }
        
        public async Task AppLoop()
        {
            bool running = true;
            await Task.Run(async () =>
            {
                while (running)
                {
                    await GetInfo();
                    if (IsPlaying)
                    {
                        await Task.Delay(500);
                    }
                    else if (!IsPlaying)
                    {
                        await Task.Delay(2000);
                    }

                }
            });
        }
        
        public async Task GetInfo()
        {
            if (CurrentSession == null)
            {
                DisplayPlaybackControls = false;
                await UpdateCurrentSession();
            }
            else
            {
                DisplayPlaybackControls = true;
                await GetCurrentMedia();
                PlaybackInfo = await GetCurrentPlaybackInfo();
                if (PlaybackInfo != null)
                {
                    await UpdatePlaybackStatus(PlaybackInfo);
                }
            }

            
        }
        
        private async Task GetCurrentMedia()
        {
            _mediaProperties = await _mediaService.GetCurrentMediaPropertiesAsync(CurrentSession);
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

            if (_mediaProperties == null)
            {
                Song = "No media playing";
                Artist = "";
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
        async Task UpdatePlaybackStatus(GlobalSystemMediaTransportControlsSessionPlaybackInfo PlaybackInfo)
        {
             await Dispatcher.UIThread.InvokeAsync(() =>
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
            if (CurrentSession != null)
            {
                _playbackInfo = await _mediaService.GetPlaybackInfoAsync(CurrentSession);


                if (_playbackInfo.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing)
                {
                    await CurrentSession.TryPauseAsync();

                }

                if (_playbackInfo.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Paused)
                {
                    await CurrentSession.TryPlayAsync();
                }

                await Task.Delay(200);
                PlaybackInfo = await GetCurrentPlaybackInfo();
                if (PlaybackInfo != null)
                    await UpdatePlaybackStatus(PlaybackInfo);
            }
            //TODO
        }


        [RelayCommand]
        private async Task Skip(GlobalSystemMediaTransportControlsSession CurrentSession)
        {
            if (CurrentSession != null)
            {
                CurrentSession.TrySkipNextAsync();
                await Task.Delay(200);
                await GetCurrentMedia();
            }
        }

        [RelayCommand]
        private async Task Prev(GlobalSystemMediaTransportControlsSession CurrentSession)
        {
            if (CurrentSession != null)
            {
                CurrentSession.TrySkipPreviousAsync();
                await Task.Delay(200);
                await GetCurrentMedia();
            }
        }
    }
    
}