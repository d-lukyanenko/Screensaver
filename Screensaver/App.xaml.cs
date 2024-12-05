using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Screensaver.API;
using Screensaver.API.pInvoke;

namespace Screensaver
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            nint parentHandle = default;

            var args = Environment.GetCommandLineArgs();
            
            if (args.Length > 1)
            {
                foreach (string arg in args)
                {
                    switch (arg.ToLower())
                    {
                        // Параметр запуску повного екрану
                        case "/s":
                            CreateMainWindows(parentHandle);
                            break;
                        // Параметр налаштування програми
                        case "/c":
                            MessageBox.Show("Налаштування відсутні", "Налаштування");
                            Application.Current.Shutdown();
                            break;
                        // Попередній перегляд
                        case "/p":
                            CreateWindow(parentHandle).Show();
                            break;
                    }
                }
            }
            else
            {
                MessageBox.Show("Параметри не були передані. Використовуйте /s, /p або /c.");
                Application.Current.Shutdown();
            }
        }

        private static void CreateMainWindows(nint ParentHandle)
        {
            foreach (var (left, top, width, height) in Screen.AllScreens.Select(s => s.Bounds))
            {
                var window = CreateWindow(ParentHandle);

                (window.Left, window.Top, window.Width, window.Height) = (left, top, width, height);

                window.Show();
            }
        }

        private static Window CreateWindow(nint ParentHandle)
        {
            Window window = new MainWindow();

            if (ParentHandle == default)
            {
                window.Loaded += (s, _) => ((Window)s).WindowState = WindowState.Maximized;
                window.MouseDown += (_, _) => Current.Shutdown();

                return window;
            }

            User32.GetClientRect(ParentHandle, out var parent_rect);

            var parent = new HwndSource(new("sourceParams")
            {
                PositionX = 0,
                PositionY = 0,
                Height = parent_rect.Bottom - parent_rect.Top,
                Width = parent_rect.Right - parent_rect.Left,
                ParentWindow = ParentHandle,
                WindowStyle = (int)(WindowStyles.WS_VISIBLE | WindowStyles.WS_CHILD | WindowStyles.WS_CLIPCHILDREN)

            })
            {
                RootVisual = window
            };

            parent.Disposed += (_, _) => window.Close();

            return window;
        }
    }

}
