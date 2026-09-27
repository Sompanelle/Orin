using System;
using System.Security.Authentication.ExtendedProtection;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Orin.Services;
using Orin.ViewModels;
using Orin.Views;

namespace Orin
{
    public partial class App : Application, IRecipient<ChangeThemeMessage>
    {
        public IServiceProvider Services { get; set; }
        public static readonly ThemeVariant Green = new("Green", ThemeVariant.Dark);
        public static readonly ThemeVariant Pink = new("Pink", ThemeVariant.Dark);
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public App()
        {
            Services = ConfigureServices();
            WeakReferenceMessenger.Default.Register<ChangeThemeMessage>(this);
        }

        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IMediaService,MediaService>();
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<OverlayWindowViewModel>();
            
            return services.BuildServiceProvider();
        }
        
        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
                // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }

        public void Receive(ChangeThemeMessage message)
        {
            if (Current == null)
            {
                throw new NotImplementedException("current App was null");
            }
            
            switch (message.Theme)
            {
                case "Dark":
                    Current.RequestedThemeVariant = ThemeVariant.Dark;
                    break;
                case "Light":
                    Current.RequestedThemeVariant = ThemeVariant.Light;
                    break;
                case "System":
                    Current.RequestedThemeVariant = ThemeVariant.Default;
                    break;
                case "Green":
                    Current.RequestedThemeVariant = Green;
                    break;
                case "Pink":
                    Current.RequestedThemeVariant = Pink;
                    break;
            }
            
        }
    }
}