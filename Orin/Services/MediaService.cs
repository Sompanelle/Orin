using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Control;

namespace Orin.Services;




public interface IMediaService
{
    Task<GlobalSystemMediaTransportControlsSessionManager?> InitializeAsync();
    Task<GlobalSystemMediaTransportControlsSessionTimelineProperties?> GetTimeLineInfoAsync(GlobalSystemMediaTransportControlsSession CurrentSession);
    Task<GlobalSystemMediaTransportControlsSessionMediaProperties?> GetCurrentMediaPropertiesAsync(GlobalSystemMediaTransportControlsSession CurrentSession);
    Task<GlobalSystemMediaTransportControlsSessionPlaybackInfo?> GetPlaybackInfoAsync(GlobalSystemMediaTransportControlsSession CurrentSession);
    Task<GlobalSystemMediaTransportControlsSession?> UpdateCurrentSession(GlobalSystemMediaTransportControlsSession CurrentSession);
}




public class MediaService : IMediaService
{
    private GlobalSystemMediaTransportControlsSessionManager? _playbackManager;

    public async Task<GlobalSystemMediaTransportControlsSessionManager?> InitializeAsync()
    {
        return _playbackManager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
    }

    public async Task<List<GlobalSystemMediaTransportControlsSession?>> GetSessions(
        GlobalSystemMediaTransportControlsSessionManager PlaybackManager)
    {
        if (_playbackManager != null)
        {
            var sessions = PlaybackManager.GetSessions();
            if (sessions != null)
                return sessions.ToList();
            
            else return null;
        }
        else  return null;
    }

    public async Task<GlobalSystemMediaTransportControlsSession?>  UpdateCurrentSession(GlobalSystemMediaTransportControlsSession CurrentSession)
    {
        if (_playbackManager != null)
        {
            CurrentSession = _playbackManager.GetCurrentSession();
            if(CurrentSession != null)
                return CurrentSession;
            else 
                return null;
        }
        else
        {
            return null;
        }
    }
        
    public async Task<GlobalSystemMediaTransportControlsSessionTimelineProperties?> GetTimeLineInfoAsync(GlobalSystemMediaTransportControlsSession CurrentSession)
    {
        if (_playbackManager != null)
        {
            var timelineInfo = CurrentSession.GetTimelineProperties();
            if (timelineInfo != null)
            {
                return timelineInfo;
            }
            else return null;
        }else return null;
    }
    
    public async Task<GlobalSystemMediaTransportControlsSessionPlaybackInfo?> GetPlaybackInfoAsync(GlobalSystemMediaTransportControlsSession CurrentSession)
    {
        if (_playbackManager != null)
        {
            var playbackInfo = CurrentSession.GetPlaybackInfo();
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
    
    public async Task<GlobalSystemMediaTransportControlsSessionMediaProperties?> GetCurrentMediaPropertiesAsync(GlobalSystemMediaTransportControlsSession CurrentSession)
    {
        if (CurrentSession != null)
        {
            var mediaProperties = await CurrentSession.TryGetMediaPropertiesAsync();

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