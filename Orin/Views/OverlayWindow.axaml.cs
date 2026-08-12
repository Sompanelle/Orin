using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Windows.Storage.Streams;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Orin.Helpers;
using Orin.ViewModels;
using WinRT;

namespace Orin.Views;

public partial class OverlayWindow : Window
{
    //win 32 consts https://learn.microsoft.com/en-us/windows/win32/winmsg/extended-window-styles
    //TODO: Label these maybe..
    private const int GWL_EXSTYLE = -20;
    private const uint WS_EX_TOPMOST = 0x00000008;
    private const uint WS_EX_NOACTIVATE = (uint)0x08000000L;
    private const uint WS_EX_LAYERED = (uint)0x00080000L;
    
    
    //Get Window Style Function
    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
    private static extern uint GetWindowLong(IntPtr hWnd, int nIndex);
    
    //Set
    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);
    
    public OverlayWindow()
    {
        InitializeComponent();
        OverlayOpened();
    }
    
    void OverlayOpened()
    {
        //Get platform
        var platformHandle = TryGetPlatformHandle();
        
        
        //If platform is windows
        if (platformHandle != null && platformHandle.HandleDescriptor == "HWND")
        {
            uint currentStyle = GetWindowLong(platformHandle.Handle, GWL_EXSTYLE);
                
            uint overlayStyle = currentStyle | WS_EX_NOACTIVATE | WS_EX_TOPMOST | WS_EX_LAYERED;
                
            SetWindowLong(platformHandle.Handle, GWL_EXSTYLE, overlayStyle);
        }
        
    }
    
}