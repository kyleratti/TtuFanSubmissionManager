using Microsoft.AspNetCore.Components;

namespace Frontend.Util;

[EventHandler("onfullscreenmodechange", typeof(FullScreenModeChangeArgs), enableStopPropagation: true, enablePreventDefault: true)]
public static class EventHandlers
{
	//
}

public class FullScreenModeChangeArgs : EventArgs
{
	public bool IsInFullScreenMode { get; set; }
}