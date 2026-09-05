using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.UI.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.Messaging;
using Orin.Services;
using Orin.ViewModels;
using Tmds.DBus.Protocol;

namespace Orin.Views;

public partial class MainWindow : Window
{
    
    //`https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-registerhotkey
    [DllImport("user32.dll", SetLastError = true, EntryPoint = "RegisterHotKey")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
    
    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    
    //related variables 
    private const uint WM_HOTKEY = 0x0312;
    
    //Modifier Keys
    //ctrl
    private const uint MOD_CONTROL = 0x0002;

    private const uint VK_RMENU = 0x0001;
    
    //Virutal Keys
    private Dictionary<int, (string, uint)> _hotkeys = new() { {0, ("O", 0x4F) }, {1, ("I", 0x49)} };
    
    public MainWindow()
    {
        InitializeComponent();

        if (!Design.IsDesignMode)
        {

            IPlatformHandle platform =
                TryGetPlatformHandle() ?? throw new NotImplementedException("Platform handle was null");

            if (platform.HandleDescriptor == "HWND")
            {
                RegisterHotKey(platform.Handle, 0, VK_RMENU, _hotkeys[0].Item2);
                RegisterHotKey(platform.Handle, 1, VK_RMENU, _hotkeys[1].Item2);

                RegisterWindowsHook(platform);
            }

        }

    }

    //add windows hook
    private void RegisterWindowsHook(IPlatformHandle PlatformHandle)
    {
        if (PlatformHandle.HandleDescriptor == "HWND")
            Win32Properties.AddWndProcHookCallback(this, HotkeyCallback);
    }

    //Hotkey handling
    private IntPtr HotkeyCallback(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_HOTKEY)
        {
            var id = wParam.ToInt32();
                
            var windowViewModel = DataContext as MainWindowViewModel;

            if (windowViewModel == null)
                throw new NotImplementedException("Error casting DataContext into Viewmodel"); 
            
            switch (id)
            {
                case 0:
                    WeakReferenceMessenger.Default.Send(new ToggleOverlayMessage());
                    break;
                case 1:
                    WeakReferenceMessenger.Default.Send(new ToggleClickThroughMessage());
                    break;
            }
            
            handled = true;
        }
        
        return IntPtr.Zero;
    }
    
    //closing
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        var platform = TryGetPlatformHandle();
        if (platform == null)
        {
            return;
        }
        else if (platform.HandleDescriptor == "HWND")
        {
            //important I think...
            UnregisterHotKey(platform.Handle, 0);
            UnregisterHotKey(platform.Handle, 1); 
        }
    }
}