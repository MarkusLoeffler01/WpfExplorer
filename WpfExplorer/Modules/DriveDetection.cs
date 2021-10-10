using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace WpfExplorer
{
    public struct USBBroadcastinterface
    {
        /// <summary>
        /// The size
        /// </summary>
        internal int Size;

        /// <summary>
        /// The device type
        /// </summary>
        internal int USBType;

        /// <summary>
        /// The reserved
        /// </summary>
        internal int Reserved;

        /// <summary>
        /// The class unique identifier
        /// </summary>
        internal Guid ClassGuid;

        /// <summary>
        /// The name
        /// </summary>
        internal short Name;
    }

    /// <summary>
    /// To get DeviceDiscoveryManager
    /// </summary>
    public class USBDetector
    {
        public static DriveInfo[] allDrives;
        public const int NewUsbDeviceConnected = 0x8000;
        public const int UsbDeviceRemoved = 0x8004;
        public const int UsbDevicechange = 0x0219;
        private const int DbtDevtypDeviceinterface = 5;
        private static readonly Guid GuidDevinterfaceUSBDevice = new Guid("A5DCBF10-6530-11D2-901F-00C04FB951ED"); // USB devices
        private static IntPtr notificationHandle;

        public static void RegisterUsbDeviceNotification(IntPtr windowHandle)
        {
            USBBroadcastinterface dbi = new USBBroadcastinterface
            {
                USBType = DbtDevtypDeviceinterface,
                Reserved = 0,
                ClassGuid = GuidDevinterfaceUSBDevice,
                Name = 0
            };

            dbi.Size = Marshal.SizeOf(dbi);
            IntPtr buffer = Marshal.AllocHGlobal(dbi.Size);
            Marshal.StructureToPtr(dbi, buffer, true);

            notificationHandle = RegisterDeviceNotification(windowHandle, buffer, 0);
        }

        /// <summary>
        /// Unregisters the window for USB device notifications
        /// </summary>
        public static void UnregisterUsbDeviceNotification()
        {
            UnregisterDeviceNotification(notificationHandle);
        }

        /// <summary>
        /// Registers the device notification.
        /// </summary>
        /// <param name="recipient">The recipient.</param>
        /// <param name="notificationFilter">The notification filter.</param>
        /// <param name="flags">The flags.</param>
        /// <returns>returns IntPtr</returns>
        [DllImport("user32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr RegisterDeviceNotification(IntPtr recipient, IntPtr notificationFilter, int flags);

        /// <summary>
        /// Unregisters the device notification.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <returns>returns bool</returns>
        [DllImport("user32.dll")]
        private static extern bool UnregisterDeviceNotification(IntPtr handle);



        public static void Detect_Click(object sender, RoutedEventArgs e)
        {
            HwndSource hwndSource = HwndSource.FromHwnd(Process.GetCurrentProcess().MainWindowHandle);
            if (hwndSource != null)
            {
                IntPtr windowHandle = hwndSource.Handle;
                hwndSource.AddHook(UsbNotificationHandler);
                USBDetector.RegisterUsbDeviceNotification(windowHandle);
            }
        }

        private static IntPtr UsbNotificationHandler(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
        {
            if (msg == USBDetector.UsbDevicechange)
            {
                switch ((int)wparam)
                {
                    case USBDetector.UsbDeviceRemoved:
                        MessageBox.Show("USB Removed");
                        break;
                    case USBDetector.NewUsbDeviceConnected:

                        MessageBoxResult res = MessageBox.Show("Neuer USB erkannt. Möchten Sie ihn indizieren?", "Neues USB Gerät", MessageBoxButton.YesNo);
                        if (res == MessageBoxResult.Yes) { USBTools.NewIndex(); }
                        break;
                }
            }
            else
            {

            }

            handled = false;
            return IntPtr.Zero;
        }

        

    }

    public static class USBTools
    {
        public static void NewIndex()
        {
            List<string> drive = ScanUSB();

            if (drive == null || drive.Count == 0) { MessageBox.Show("USB Gerät wurde nicht erkannt\nBitte erneut probieren"); return; }
            MessageBox.Show("Wählen Sie den Ordner aus, den Sie indizieren möchten");
            main.getPathDialog(drive[0]);
        }
        public static List<string> ScanUSB()
        {
            DriveInfo[] currentDrives = DriveInfo.GetDrives();

            List<string> oldD = new List<string> { };
            List<string> newD = new List<string> { };
            foreach (var curr in currentDrives)
            {
                oldD.Add(curr.Name);
            }
            foreach (var d in USBDetector.allDrives)
            {
                newD.Add(d.Name);
            }
            return oldD.Except(newD).Concat(newD.Except(oldD)).ToList();
        }
    }

    
}
