using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using System.Drawing;
using System.Collections.Generic;

using Microsoft.Win32;

namespace ForRobot.Model.Settings
{
    public class ApplicationInfo
    {
        public ImageSource Icon { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
    }

    public static class AppsForOpenFile
    {
        public static List<ApplicationInfo> GetInstalledApplications()
        {
            var applications = new List<ApplicationInfo>();

            // Ключи реестра, где хранится информация об установленных приложениях
            string[] registryKeys = 
            {
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
                @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
            };

            foreach (var registryKey in registryKeys)
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

            return applications.OrderBy(item => item.Name).ToList<ApplicationInfo>();
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
    }
}
