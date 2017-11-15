﻿using Microsoft.Win32;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Bzway
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] arg)
        {
            if (arg.Length == 0)
            {
                try
                {
                    Register();
                }
                catch { }
            }

            var root = Directory.GetCurrentDirectory();
            VirtualDrive.Create('N', root);
            var git = new Git();
            git.Set(root, "", "");
            var version = Console.ReadLine();
            while (!string.IsNullOrEmpty(version))
            {
                git.Get(root, version);
                version = Console.ReadLine();
            }

            return;
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //var form = new MainForm();

            //if (arg.Length == 1)
            //{
            //    double minutes;
            //    if (double.TryParse(arg[0], out minutes))
            //    {
            //        form.workMinutes = minutes;
            //    }
            //}
            //Application.Run(form);
        }

        public static void Register()
        {
            var path = "Folder\\shell\\NetDisk|directory\\Background\\shell\\NetDisk".Split('|');

            foreach (var item in path)
            {
                using (RegistryKey mainMenu = Registry.ClassesRoot.CreateSubKey(item))
                {
                    mainMenu.SetValue("Icon", "shell32.dll,5");
                    mainMenu.SetValue("MUIVerb", "NetDisk");
                    mainMenu.SetValue("SubCommands", "");
                    var menus = "get|set|diff|push|pull".Split('|');

                    for (int i = 0; i < menus.Length; i++)
                    {
                        var m = menus[i];
                        using (var subMenu = mainMenu.CreateSubKey(string.Format("shell\\menu{0}", i)))
                        {
                            subMenu.SetValue("Icon", "shell32.dll,6");
                            subMenu.SetValue("", m);
                        }
                        using (var subMenu = mainMenu.CreateSubKey(string.Format("shell\\menu{0}\\command", i)))
                        {
                            subMenu.SetValue("", string.Format("{0} {1}", Application.ExecutablePath, m));
                        }
                    }
                }
            }
        }
        public static void Unregister(string fileType, string shellKeyName)
        {
            var path = "Folder\\shell\\NetDisk|directory\\Background\\shell\\NetDisk".Split('|');
            foreach (var item in path)
            {
                Registry.ClassesRoot.DeleteSubKeyTree(item);
            }

        }
    }
}