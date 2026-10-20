using System;
using System.Threading.Tasks;
using Windows.Media.Control;
using Orin.Services;

namespace Orin.Helpers;

public static class MediaHelper
{
    public static GlobalSystemMediaTransportControlsSession? UpdateCurrentSession(IMediaService MediaService)
    {
        var currentSession = MediaService.UpdateCurrentSession();
        return currentSession;
    }
    
    public static bool IsSessionValid(GlobalSystemMediaTransportControlsSession? Session)
    {
        if (Session != null && Session.GetPlaybackInfo() != null)
            return true;
        else
            return false;
    }
}