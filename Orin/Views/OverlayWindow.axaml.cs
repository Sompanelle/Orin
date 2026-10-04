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

public partial class OverlayWindow : Window, IRecipient<ToggleClickThroughMessage>, IRecipient<ResizeWindowMessage>
{

    private IPlatformHandle? _platformHandle;
    //win 32 consts https://learn.microsoft.com/en-us/windows/win32/winmsg/extended-window-styles
    
    //extended Styles
    private const int GWL_EXSTYLE = -20;
    
    //general overlay properties (self-explanatory)
    private const uint WS_EX_TOPMOST = 0x00000008;
    private const uint WS_EX_NOACTIVATE = (uint)0x08000000L;
    private const uint WS_EX_LAYERED = (uint)0x00080000L;
    //allows click through
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
    
    //pos variables (for no change, also self-explanatory)
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_FRAMECHANGED = 0x0020; //refresh

    //Config Variables
    private bool _isClickThrough = false;
    private Config? _config;
    
    
    public OverlayWindow()
    {
        InitializeComponent();
    }


    public OverlayWindow(Config Config)
    {
        InitializeComponent();
        _config = Config;
        _isClickThrough = _config.IsClickThroughDefault;
        
    }
    
    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        WeakReferenceMessenger.Default.Register<ToggleClickThroughMessage>(this);
        WeakReferenceMessenger.Default.Register<ResizeWindowMessage>(this);
        OverlayOpened();
    }
    
    void OverlayOpened()
    {
        //Get platform
        _platformHandle = TryGetPlatformHandle() ?? throw new NotImplementedException("Platform handle is null");
        //If platform is windows
        if (_platformHandle.HandleDescriptor == "HWND")
        {
            uint currentStyle = (uint)GetWindowLong(_platformHandle.Handle, GWL_EXSTYLE);
            uint overlayStyle;
            
            if (_isClickThrough)
                overlayStyle = currentStyle | WS_EX_NOACTIVATE | WS_EX_TOPMOST | WS_EX_LAYERED | WS_EX_TRANSPARENT;
            else
                overlayStyle = currentStyle | WS_EX_NOACTIVATE | WS_EX_TOPMOST | WS_EX_LAYERED;
            
            SetWindowLong(_platformHandle.Handle, GWL_EXSTYLE, (IntPtr)overlayStyle);
            SetWindowPos(_platformHandle.Handle, 0, 0, 0,0,  SWP_NOMOVE |SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);

        }
        

        
    }
    
    //message responses/reception
    
    public void Receive(ToggleClickThroughMessage message)
    {
        ToggleClickThrough();
    }

    public void Receive(ResizeWindowMessage message)
    {
        Width = message.Width;
        Height = 60;
    }
    
 
    private void ToggleClickThrough()
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
        
        _isClickThrough = !_isClickThrough;
    }


}