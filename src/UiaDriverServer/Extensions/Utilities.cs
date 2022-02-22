﻿/*
 * CHANGE LOG - keep only last 5 threads
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using UIAutomationClient;

namespace UiaDriverServer.Extensions
{
    internal static class Utilities
    {
        /// <summary>
        /// web-driver element reference key - must be returned with element object value
        /// </summary>
        public const string EelementReference = "element-6066-11e4-a52e-4f735466cecf";

        /// <summary>
        /// gets the local IP address of the host machine
        /// </summary>
        /// <returns>local ip address if exists</returns>
        public static string GetLocalEndpoint()
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork))
                {
                    if (string.IsNullOrEmpty(ip.ToString())) continue;
                    return ip.ToString();
                }
                throw new KeyNotFoundException("local IPvP address not found");
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// get element runtime-id based on it's COM runtime property
        /// </summary>
        /// <param name="domRuntime"></param>
        /// <returns>automation element runtime id</returns>
        public static IEnumerable<int> GetRuntime(string domRuntime)
            => JsonSerializer.Deserialize<IEnumerable<int>>(domRuntime);

        /// <summary>
        /// create global cache request for elements properties
        /// </summary>
        /// <param name="automation">automation to get request for</param>
        /// <returns>cache request</returns>
        public static IUIAutomationCacheRequest GetCacheRequest(this CUIAutomation8 automation)
        {
            // create request
            var r = automation.CreateCacheRequest();

            // add patterns
            r.AddPattern(UIA_PatternIds.UIA_TextChildPatternId);
            r.AddPattern(UIA_PatternIds.UIA_TextEditPatternId);
            r.AddPattern(UIA_PatternIds.UIA_TextPattern2Id);
            r.AddPattern(UIA_PatternIds.UIA_TextPatternId);

            // add properties
            r.AddProperty(UIA_PropertyIds.UIA_AcceleratorKeyPropertyId);
            r.AddProperty(UIA_PropertyIds.UIA_AccessKeyPropertyId);

            // tree scope
            r.TreeScope = TreeScope.TreeScope_Descendants;
            r.TreeFilter = automation.CreateTrueCondition();
            return r;
        }

        /// <summary>
        /// Gets the primary screen full resolution.
        /// </summary>
        /// <returns>Primary screen full resolution.</returns>
        public static (int Width, int Height) GetScreenResultion()
        {
            // setup
            var query = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
            var queryCollection = query.Get();

            // build
            var videoModeDescription = GetVideoModeDescription(queryCollection);
            var x = Regex.Match(videoModeDescription, @"^\d+").Value;
            var y = Regex.Match(videoModeDescription, @"(?<=x\s+)\d+(?=\s+x)").Value;

            // parse
            _ = int.TryParse(x, out int xOut);
            _ = int.TryParse(y, out int yOut);

            // get
            return (xOut, yOut);
        }

        private static string GetVideoModeDescription(ManagementObjectCollection queryCollection)
        {
            foreach (var managementObject in queryCollection)
            {
                var propertyDataCollection = managementObject.Properties;
                foreach (var propertyData in propertyDataCollection)
                {
                    if (propertyData.Name.Equals("VideoModeDescription", StringComparison.OrdinalIgnoreCase))
                    {
                        return $"{propertyData.Value}";
                    }
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// start an interactive process.
        /// </summary>
        /// <param name="fileName">The application or document to start.</param>
        /// <param name="arguments">The set of command-line arguments to use when starting the application.</param>
        /// <returns>A new instance of the <see cref="Process"/> class.</returns>
        public static Process StartProcess(string fileName,  string arguments)
        {
            // setup conditions
            var isDirectory = Directory.Exists(fileName);

            // build process
            var startInfo = isDirectory
                ? new ProcessStartInfo { FileName = "explorer.exe", Arguments = fileName }
                : new ProcessStartInfo { FileName = fileName, Arguments = arguments };

            var process = new Process
            {
                StartInfo = startInfo
            };
            process.Start();
            Thread.Sleep(3000);
            return process;
        }

        /// <summary>
        /// Close the driver with exit code 0.
        /// </summary>
        public static void CloseDriver() => Task.Run(() =>
        {
            Trace.TraceInformation("Shutting down...");
            Thread.Sleep(1000);
            Environment.Exit(0);
        });

        /// <summary>
        /// Render UiA Driver logo.
        /// </summary>
        public static void RednerLogo()
        {
            Console.WriteLine("  ▄▄▄▄▄▄▄     ▄▄▄▄▄▄   ▄▄▄▄▄         ▄▄▄▄▄         ");
            Console.WriteLine(" ████████     ██████  █████▀        ███████▄       ");
            Console.WriteLine("  ██████       ████     ▄▄▄▄        ████████▄      ");
            Console.WriteLine("  ██████       ████  ▄██████       ▄███▀██████     ");
            Console.WriteLine("  ██████       ████   ██████      ▄███▀ ▀██████    ");
            Console.WriteLine("  ██████       ████   ██████     ▄██████████████   ");
            Console.WriteLine("  ███████▄   ▄▄████   ██████    ▄███▀▀▀▀▀▀▀██████  ");
            Console.WriteLine("   ▀█████████████▀   ████████ ████████   ██████████");
            Console.WriteLine("      ▀▀▀▀▀▀▀▀▀      ▀▀▀▀▀▀▀  ▀▀▀▀▀▀▀     ▀▀▀▀▀▀▀▀ ");
            Console.WriteLine(" WebDriver implementation for Windows native.      ");
            Console.WriteLine();
            Console.WriteLine(" Powered by IUIAutomation: https://docs.microsoft.com/en-us/windows/win32/api/_winauto/");
            Console.WriteLine(" GitHub Project URL:       https://github.com/gravity-api/gravity-uia-server");
            Console.WriteLine();
            Console.WriteLine();
        }
    }
}