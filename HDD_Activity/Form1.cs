#region System Includes
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using System.Management.Instrumentation;
using System.Collections.Specialized;
using System.Threading;
#endregion
namespace HDD_Activity
{
    #region form
    public partial class Form1 : Form
    {
        #region Names
        NotifyIcon hddActivityIcon;
        Icon activeIcon;
        Icon idleIcon;
        Thread hddActivityWorker;
        #endregion
        public Form1()
        {
            InitializeComponent();
            #region Icons 
            activeIcon = new Icon("HDD_Busy.ico");
            idleIcon = new Icon("HDD_Idle.ico");
            // Create notify icons, assign the default icon to use when starting and show it.
            hddActivityIcon = new NotifyIcon();
            hddActivityIcon.Icon = idleIcon;
            hddActivityIcon.Visible = true;
            #endregion
            // create context menu items and add them to tray icon.
            #region Context Menu
            MenuItem CreatedByMenuItem = new MenuItem("HDD Activity App Built by Joshua Wareing.");
            MenuItem updateMenuItem = new MenuItem("Check for updates");
            MenuItem quitMenuItem = new MenuItem("Quit App");
            ContextMenu contextMenu = new ContextMenu();
            contextMenu.MenuItems.Add(CreatedByMenuItem);
            contextMenu.MenuItems.Add(quitMenuItem);
            // Add Context Menu to program tray icon.
            hddActivityIcon.ContextMenu = contextMenu;

            //make quit button to actually close the app.
            quitMenuItem.Click += quitMenuItem_Click;
            #endregion
            //hide form.
            WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            //start worker thread to monitor hdd activity.
            hddActivityWorker = new Thread(new ThreadStart(HDDActivityThread));
            hddActivityWorker.Start();
        }

        /// <summary>
        /// Close App.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        #region Quit Menu Item Setup
        void quitMenuItem_Click(object sender, EventArgs e)
        {
            hddActivityWorker.Abort();
            hddActivityIcon.Dispose();
            this.Close();
        }
        #endregion
        /// <summary>
        /// pulls hdd activity
        /// </summary>
#region Thread Worker Stuff
        public void HDDActivityThread()
        {
            ManagementClass driveDataClass = new ManagementClass("Win32_PerfFormattedData_PerfDisk_PhysicalDisk");
            ManagementObjectCollection driveDataClassCollection = driveDataClass.GetInstances();
            try
            {
                //main loop - Almost EVERYTHING that this program does happens in this loop.
                while (true)
                {
                    foreach (ManagementObject obj in driveDataClassCollection) {
                        if (obj["Name"].ToString() == "_Total")
                        {
                            if (Convert.ToInt64(obj["DiskBytesPersec"]) > 0)
                            {
                                //show busy icon.
                                hddActivityIcon.Icon = activeIcon;
                            }
                            else {
                                //show idle icon.
                                hddActivityIcon.Icon = idleIcon;
                            }
                        }
                    }
                    Thread.Sleep(100);
                }
            }
            catch (ThreadAbortException tbe) {
                //thread aborted.
                driveDataClass.Dispose();
            }
        }
        #endregion
    }
    #endregion
}