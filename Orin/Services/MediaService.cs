using System;
using System.Threading.Tasks;
using Windows.Media.Control;

namespace Orin.Services;




public interface IMediaService
{
    Task<GlobalSystemMediaTransportControlsSessionTimelineProperties?> GetTimeLineInfoAsync();
    Task<GlobalSystemMediaTransportControlsSession> GetCurrentMediaSessionAsync();
    Task<GlobalSystemMediaTransportControlsSessionMediaProperties?> GetCurrentMediaPropertiesAsync();
    Task<GlobalSystemMediaTransportControlsSessionPlaybackInfo?> GetPlaybackInfoAsync();
    Task<GlobalSystemMediaTransportControlsSession?> UpdateCurrentSession();
}




public class MediaService : IMediaService
{
    private GlobalSystemMediaTransportControlsSessionManager? _playbackManager;
    private GlobalSystemMediaTransportControlsSession? _currentSession;

    public async Task InitializeAsync()
    {
        _playbackManager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
        _currentSession = _playbackManager.GetCurrentSession();
        
    }

    public async Task<GlobalSystemMediaTransportControlsSession?>  UpdateCurrentSession()
    {
        if (_playbackManager != null)
        {
            _currentSession = _playbackManager.GetCurrentSession();
            if(_currentSession != null)
                return _currentSession;
            else 
                return null;
        }
        else
        {
            return null;
        }
    }
        
    public async Task<GlobalSystemMediaTransportControlsSessionTimelineProperties?> GetTimeLineInfoAsync()
    {
        if (_playbackManager != null)
        {
            var timelineInfo = _playbackManager.GetCurrentSession().GetTimelineProperties();
            if (timelineInfo != null)
            {
                return timelineInfo;
            }
            else return null;
        }else return null;
    }
    
    public async Task<GlobalSystemMediaTransportControlsSessionPlaybackInfo?> GetPlaybackInfoAsync()
    {
        if (_playbackManager != null)
        {
            var playbackInfo = _currentSession.GetPlaybackInfo();
            if (playbackInfo != null)
            {
                return playbackInfo;
            }
            else
            {
                return null;
            }
        }
        else 
        {
            return null;
        }
    }

    public async Task<GlobalSystemMediaTransportControlsSession> GetCurrentMediaSessionAsync()
    {
        if (_currentSession != null)
        {
            return _currentSession;
        }
        else
        {
            return null;
        }
        
    }   
    
    public async Task<GlobalSystemMediaTransportControlsSessionMediaProperties?> GetCurrentMediaPropertiesAsync()
    {
        if (_currentSession != null)
        {
            var mediaProperties = await _currentSession.TryGetMediaPropertiesAsync();

            if (mediaProperties != null)
            {
                return mediaProperties;
            }
            else
            {
                return null;
            }
        }
        else
        {
            return null;
        }
    }
}