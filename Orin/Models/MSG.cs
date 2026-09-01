using System;
using System.Runtime.InteropServices;
using Avalonia;

namespace Orin.Models;

[StructLayout(LayoutKind.Sequential)]
public struct MSG
{
    public IntPtr hwnd;
    public uint message;
    public UIntPtr wParam;
    public IntPtr lParam;
    public uint time;
    public Point pt;
    public uint lPrivate;

}