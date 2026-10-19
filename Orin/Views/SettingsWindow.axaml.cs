using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Orin.Services;

namespace Orin.Views;

public partial class SettingsWindow : Window, IRecipient<CloseSettingsWindowMessage>
{
    public SettingsWindow()
    {
        InitializeComponent();
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        WeakReferenceMessenger.Default.Register<CloseSettingsWindowMessage>(this);
    }

    public void Receive(CloseSettingsWindowMessage message)
    {
        Close();
    }
}