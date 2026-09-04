namespace Orin.Services;



public record ToggleOverlayMessage();

public record ToggleClickThroughMessage();

public record ResizeWindowMessage(int Width);

public record CloseSettingsWindowMessage();

public record ChangeThemeMessage(string Theme);