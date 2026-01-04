using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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