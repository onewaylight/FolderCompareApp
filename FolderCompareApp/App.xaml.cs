using FolderCompareApp.ViewModels;
using FolderCompareApp.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace FolderCompareApp
{
    public partial class App : Application
    {
        private ServiceProvider _serviceProvider;

        public App()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            // Register services and viewmodels
            services.AddSingleton<MainViewModel>();
            services.AddTransient<MainWindow>();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var mainViewModel = _serviceProvider.GetService<MainViewModel>();
            var mainWindow = _serviceProvider.GetService<MainWindow>();

            // Process command line arguments if provided
            if (e.Args.Length >= 2)
            {
                mainViewModel.LeftFolderPath = e.Args[0];
                mainViewModel.RightFolderPath = e.Args[1];
                mainViewModel.CompareCommand.Execute(null);
            }

            mainWindow.Show();
        }
    }

}
