using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using System.Drawing;
using System.Threading.Tasks;
using System.Collections.Generic;

using Microsoft.Win32;

using ForRobot.Model.Settings;

namespace ForRobot.Libr.Services
{
    public static class SelectAppsOnDeviceService
    {
        #region Private variables

        private static List<ApplicationInfo> _cachedApplications;
        private static DateTime _lastCacheTime;

        #endregion Private variables

        #region Public variables

        /// <summary>
        /// Игнорируемые системные файлы
        /// </summary>
        private static string[] excludedFiles = new[]
        {
            "svchost", "dllhost", "runtimebroker", "conhost", "smss", "csrss",
            "services", "lsass", "wininit", "winlogon", "taskhost", "dwm"
        };

        /// <summary>
        /// Ключи реестра, где хранится информация об установленных приложениях
        /// </summary>
        public static string[] RegistryKeys =
        {
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
            @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
        };

        /// <summary>
        /// Перечень стандартных папок
        /// </summary>
        public static string[] StandardLocations = new[]
        {
            (Environment.Is64BitProcess ? Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) : Environment.GetEnvironmentVariable("ProgramW6432")), // Для 32 бит всегда будет выводить Program Files X86.
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32")
        };

        #endregion Public variables

        #region Private functions

        //private static List<ApplicationInfo> GetCachedApplications()
        //{
        //    if (_cachedApplications == null || (DateTime.Now - _lastCacheTime).TotalHours > 1)
        //    {
        //        _cachedApplications = GetAllApplicationsOnDevice();
        //        _lastCacheTime = DateTime.Now;
        //    }
        //    return _cachedApplications;
        //}

        /// <summary>
        /// Проверка является ли файл системным
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private static bool ShouldIncludeApplication(string filePath)
        {
            var fileName = Path.GetFileName(filePath).ToLower();
            return !excludedFiles.Any(excluded => fileName.Contains(excluded));
        }

        /// <summary>
        /// Извлечение иконки приложения
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private static ImageSource ExtractIconFromFile(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                    return null;

                Icon systemIcon = Icon.ExtractAssociatedIcon(filePath);
                BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHIcon(systemIcon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                systemIcon.Dispose();

                return bitmapSource;
            }
            catch
            {
                return null;
            }
        }

        #region Async

        private static ValueTask<bool> ShouldIncludeApplicationAsunc(string filePath) => new ValueTask<bool>(Task.Run(() => ShouldIncludeApplication(filePath)));

        private static ValueTask<ImageSource> ExtractIconFromFileAsync(string filePath) => new ValueTask<ImageSource>(Task.Run(() => ExtractIconFromFile(filePath)));

        #endregion

        #endregion Private functions

        #region Public functions

        /// <summary>
        /// Словарь стандартных приложений
        /// </summary>
        public static Dictionary<string, string> StandardAppPaths = new Dictionary<string, string>()
        {
            { "Блокнот", Path.Combine(Environment.SystemDirectory, "notepad.exe")},
            { "Paint", Path.Combine(Environment.SystemDirectory, "mspaint.exe")},
            { "Калькулятор", Path.Combine(Environment.SystemDirectory, "calc.exe")},
            { "WordPad", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Windows NT", "Accessories", "wordpad.exe")},
            { "Командная строка", Path.Combine(Environment.SystemDirectory, "cmd.exe")},
            { "PowerShell", Path.Combine(Environment.SystemDirectory, "WindowsPowerShell", "v1.0", "powershell.exe")},
            { "Браузер Edge", Path.Combine(Environment.SystemDirectory, "SystemApps", "Microsoft.MicrosoftEdge_8wekyb3d8bbwe", "MicrosoftEdge.exe")}
        };

        /// <summary>
        /// Получение всех приложений на устройстве
        /// </summary>
        /// <returns></returns>
        public static List<ApplicationInfo> GetAllApplicationsOnDevice()
        {
            List<ApplicationInfo> applications = new List<ApplicationInfo>();
            applications.AddRange(GetInstalledApplications());
            applications.AddRange(GetStandardApplications());
            applications.AddRange(GetApplicationsFromStandardLocations());

            return applications.GroupBy(a => a.Path, StringComparer.OrdinalIgnoreCase)
                               .Select(g => g.First())
                               .OrderBy(a => a.Name)
                               .ToList();
        }

        /// <summary>
        /// Получение стандартных приложений
        /// </summary>
        /// <returns></returns>
        public static List<ApplicationInfo> GetStandardApplications()
        {
            List<ApplicationInfo> standardApps = new List<ApplicationInfo>();
            foreach (var app in StandardAppPaths)
            {
                if (File.Exists(app.Value))
                {
                    standardApps.Add(new ApplicationInfo
                    {
                        Name = app.Key,
                        Path = app.Value,
                        Icon = ExtractIconFromFile(app.Value)
                    });
                }
            }
            return standardApps;
        }

        /// <summary>
        /// Получение приложений из реестра (установленные)
        /// </summary>
        /// <returns></returns>
        public static List<ApplicationInfo> GetInstalledApplications()
        {
            var applications = new List<ApplicationInfo>();

            foreach (var registryKey in RegistryKeys)
            {
                using (var key = Registry.LocalMachine.OpenSubKey(registryKey))
                {
                    if (key == null) continue;

                    foreach (string subkeyName in key.GetSubKeyNames())
                    {
                        using (var subkey = key.OpenSubKey(subkeyName))
                        {
                            var displayName = subkey?.GetValue("DisplayName") as string;
                            var installLocation = subkey?.GetValue("InstallLocation") as string;
                            var executablePath = subkey?.GetValue("DisplayIcon") as string;

                            if (string.IsNullOrEmpty(displayName) || string.IsNullOrEmpty(executablePath))
                                continue;

                            if (executablePath.Contains(","))
                                executablePath = executablePath.Split(',')[0];

                            if (File.Exists(executablePath))
                            {
                                var icon = ExtractIconFromFile(executablePath);

                                applications.Add(new ApplicationInfo
                                {
                                    Name = displayName,
                                    Path = executablePath
                                });
                            }
                        }
                    }
                }
            }
            return applications;
        }

        /// <summary>
        /// Получение приложений из стандартных папок
        /// </summary>
        /// <returns></returns>
        public static List<ApplicationInfo> GetApplicationsFromStandardLocations()
        {
            var applications = new List<ApplicationInfo>();
            foreach (var location in StandardLocations)
            {
                if (!Directory.Exists(location)) continue;
                try
                {
                    var exeFiles = Directory.EnumerateFiles(location, "*.exe", SearchOption.AllDirectories);
                    foreach (var exeFile in exeFiles)
                    {
                        try
                        {
                            if (ShouldIncludeApplication(exeFile))
                            {
                                applications.Add(new ApplicationInfo
                                {
                                    Name = Path.GetFileNameWithoutExtension(exeFile),
                                    Path = exeFile,
                                    Icon = ExtractIconFromFile(exeFile)
                                });
                            }
                        }
                        catch { continue; }
                    }
                }
                catch { continue; }
            }
            return applications;
        }

        #region Async

        #endregion

        #endregion Public functions
    }
}
