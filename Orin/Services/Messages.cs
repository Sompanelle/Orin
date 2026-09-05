using Windows.Media.Control;
using Orin.Models;

namespace Orin.Services;



public record ToggleOverlayMessage();

public record ToggleClickThroughMessage();

public record ResizeWindowMessage(int Width);

public record CloseSettingsWindowMessage();

public record ChangeThemeMessage(string Theme);

public record SessionChangedMessage(GlobalSystemMediaTransportControlsSession Session);

public record ConfigChangedMessage(Config Config);