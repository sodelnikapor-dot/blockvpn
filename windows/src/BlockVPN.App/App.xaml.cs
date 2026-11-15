using System.Diagnostics;
using System.Threading;
using System.Windows;

namespace BlockVPN.App
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Режим службы: SCM запускает наш exe с "/service <конфиг> <pid UI>".
            // Туннель поднимает tunnel.dll (официальный userspace WireGuard).
            if (e.Args.Length >= 2 && e.Args[0] == "/service")
            {
                var configPath = e.Args[1];
                if (e.Args.Length >= 3 && int.TryParse(e.Args[2], out var uiPid))
                {
                    // Эфемерный режим: если UI-процесс умер — снять службу.
                    var watcher = new Thread(() =>
                    {
                        try
                        {
                            var ui = Process.GetProcessById(uiPid);
                            if (ui.MainModule.FileName != Process.GetCurrentProcess().MainModule.FileName)
                                return;
                            ui.WaitForExit();
                            Tunnel.Service.Remove(configPath, false);
                        }
                        catch { }
                    });
                    watcher.IsBackground = true;
                    watcher.Start();
                }

                bool ok = false;
                var runner = new Thread(() => { ok = Tunnel.Service.Run(configPath); });
                runner.SetApartmentState(ApartmentState.MTA);
                runner.Start();
                runner.Join();
                Shutdown(ok ? 0 : 1);
                return;
            }

            var window = new MainWindow();
            MainWindow = window;
            window.Show();
        }
    }
}
