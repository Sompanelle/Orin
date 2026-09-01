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
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Orin.Helpers;
using Orin.Models;
using Orin.Services;
using Orin.ViewModels;
using WinRT;

namespace Orin.Views;

public partial class OverlayWindow : Window, IRecipient<ToggleClickThroughMessage>
{

    private IPlatformHandle? _platformHandle;
    //win 32 consts https://learn.microsoft.com/en-us/windows/win32/winmsg/extended-window-styles
    //TODO: Label these maybe..
    private const int GWL_EXSTYLE = -20;
    private const uint WS_EX_TOPMOST = 0x00000008;
    private const uint WS_EX_NOACTIVATE = (uint)0x08000000L;
    private const uint WS_EX_LAYERED = (uint)0x00080000L;
    private const uint WS_EX_TRANSPARENT = (uint)0x00000020L;
    
    //Get Window Style Function
    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
    private static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);
    
    //Set Window Style
    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
    private static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
    
    //Set Window Pos (Causes Refresh)
    [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
    private static extern bool  SetWindowPos(IntPtr hWnd, int x, int y, int cx, int cy, uint wFlags);
    
    //pos variables (for no change)
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_FRAMECHANGED = 0x0020; //refresh
    
    public OverlayWindow()
    {
        InitializeComponent();
    }

    public void Receive(ToggleClickThroughMessage message)
    {
        ToggleClickThrough();
    }
    
    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        WeakReferenceMessenger.Default.Register<ToggleClickThroughMessage>(this);
        OverlayOpened();
    }

    
    void OverlayOpened()
    {
        //Get platform
        _platformHandle = GetPlatform();
        
        //If platform is windows
        if (_platformHandle != null && _platformHandle.HandleDescriptor == "HWND")
        {
            uint currentStyle = (uint)GetWindowLong(_platformHandle.Handle, GWL_EXSTYLE);

            uint overlayStyle = currentStyle | WS_EX_NOACTIVATE | WS_EX_TOPMOST | WS_EX_LAYERED;
            SetWindowLong(_platformHandle.Handle, GWL_EXSTYLE, (IntPtr)overlayStyle);
            SetWindowPos(_platformHandle.Handle, 0, 0, 0,0,  SWP_NOMOVE |SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
        }
        
    }

    public IPlatformHandle? GetPlatform()
    {
        var platform = TryGetPlatformHandle();
        if (platform != null)
            return platform;
        else return null;
    }

 
    public void ToggleClickThrough()
    {
        uint currentStyle = (uint)GetWindowLong(_platformHandle.Handle, GWL_EXSTYLE);
        uint newStyle;
        
        if ((currentStyle & WS_EX_TRANSPARENT) == WS_EX_TRANSPARENT)
        {
            newStyle = currentStyle & ~WS_EX_TRANSPARENT;
            
        }
        else   
        {
            newStyle = currentStyle | WS_EX_TRANSPARENT;
        }
        
        SetWindowLong(_platformHandle.Handle, GWL_EXSTYLE, (IntPtr)newStyle);
        SetWindowPos(_platformHandle.Handle, 0,0,0,0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER |  SWP_FRAMECHANGED);
    }


}