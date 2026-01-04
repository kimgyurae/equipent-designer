using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using EquipmentDesigner.Services;

namespace EquipmentDesigner
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            // Add global exception handlers for debugging
            DispatcherUnhandledException += (s, args) =>
            {
                File.WriteAllText("crash_log.txt", $"Dispatcher Exception:\n{args.Exception}");
                MessageBox.Show($"오류 발생:\n{args.Exception.Message}\n\n{args.Exception.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                args.Handled = true;
            };

            AppDomain.CurrentDomain.UnhandledException += (s, args) =>
            {
                var ex = args.ExceptionObject as Exception;
                File.WriteAllText("crash_log.txt", $"AppDomain Exception:\n{ex}");
            };
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Configure dependency injection for production
            ServiceLocator.ConfigureForProduction();

            // Initialize language service with Korean as default language
            LanguageService.Instance.Initialize();
        }
    }
}