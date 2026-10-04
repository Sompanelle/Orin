using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Control;

namespace Orin.Services;




public interface IMediaService
{
    Task<GlobalSystemMediaTransportControlsSessionManager?> InitializeAsync();
    GlobalSystemMediaTransportControlsSessionTimelineProperties? GetTimeLineInfo(GlobalSystemMediaTransportControlsSession CurrentSession);
    Task<GlobalSystemMediaTransportControlsSessionMediaProperties?> GetCurrentMediaPropertiesAsync(GlobalSystemMediaTransportControlsSession CurrentSession);
    GlobalSystemMediaTransportControlsSessionPlaybackInfo? GetPlaybackInfo(GlobalSystemMediaTransportControlsSession CurrentSession);
    GlobalSystemMediaTransportControlsSession? UpdateCurrentSession();
}




public class MediaService : IMediaService
{
    private GlobalSystemMediaTransportControlsSessionManager? _playbackManager;

    public async Task<GlobalSystemMediaTransportControlsSessionManager?> InitializeAsync()
    {
        return _playbackManager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
    }

    public List<GlobalSystemMediaTransportControlsSession?> GetSessions(GlobalSystemMediaTransportControlsSessionManager PlaybackManager)
    {
        if (_playbackManager != null)
        {
            var sessions = PlaybackManager.GetSessions();
            if (sessions != null)
                return sessions.ToList();
        }
        return new List<GlobalSystemMediaTransportControlsSession?>();
    }

    public GlobalSystemMediaTransportControlsSession? UpdateCurrentSession()
    {
        if (_playbackManager != null)
        {
            GlobalSystemMediaTransportControlsSession currentSession = _playbackManager.GetCurrentSession();
            if(currentSession != null)
                return currentSession;
        }

        return null;
        
    }
        
    public GlobalSystemMediaTransportControlsSessionTimelineProperties? GetTimeLineInfo(GlobalSystemMediaTransportControlsSession CurrentSession)
    {
        if (_playbackManager != null)
        {
            var timelineInfo = CurrentSession.GetTimelineProperties();
            if (timelineInfo != null)
            {
                return timelineInfo;
            }
        } 
        return null;
    }
    
    public GlobalSystemMediaTransportControlsSessionPlaybackInfo? GetPlaybackInfo(GlobalSystemMediaTransportControlsSession CurrentSession)
    {
        if (_playbackManager != null)
        {
            var playbackInfo = CurrentSession.GetPlaybackInfo();
            if (playbackInfo != null)
            {
                return playbackInfo;
            }
        }

        return null;
        
    }
    
    public async Task<GlobalSystemMediaTransportControlsSessionMediaProperties?> GetCurrentMediaPropertiesAsync(GlobalSystemMediaTransportControlsSession? CurrentSession)
    {
        if (CurrentSession != null)
        {
            var mediaProperties = await CurrentSession.TryGetMediaPropertiesAsync();

            if (mediaProperties != null)
            {
                return mediaProperties;
            }
        }

        return null;
    }
}