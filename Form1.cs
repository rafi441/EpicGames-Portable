using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace EpicPortable
{
    

    public partial class Form1 : Form
    {
        [DllImport("kernel32.dll")]
        static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, int dwFlags);
        private const int WM_NCHITTEST = 0x84;
        private const int HTCLIENT = 0x1;
        private const int HTCAPTION = 0x2;
        private const string path = @"EpicGamesCFG.INI";
        const string EpicDataPath = @"C:/ProgramData/Epic";
        const string EpicPath = @"EpicData";
        const string LauncherPath = @"Launcher";



        public Form1()
        {
            
            InitializeComponent();
        }
        protected override void WndProc(ref Message message)    //Dragable Borderless
        {
            base.WndProc(ref message);

            if (message.Msg == WM_NCHITTEST && (int)message.Result == HTCLIENT)
                message.Result = (IntPtr)HTCAPTION;
        }

        //Config And Key/License(simple)
        static void ReadKey()
        {
            List<string> line = new List<string>();
            //line = File.ReadLines(path).ToList();

            var line2 = File.ReadLines(path).Skip(0).Take(1).First();

            var message = string.Join(Environment.NewLine, line2);

            if (message != "neogaming223")
            {
                MessageBox.Show(new Form { TopMost = true }, "Key Error\n" + message , "Launcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return;
            }

        }

        //Check Symbolic or Real Directory
        private bool IsSymbolic(string path)
        {
            FileInfo pathInfo = new FileInfo(path);
            return pathInfo.Attributes.HasFlag(FileAttributes.ReparsePoint);
        }

        //Check EmptyDirectory
        public bool IsDirectoryEmpty(string path)
        {
            return !Directory.EnumerateFileSystemEntries(path).Any();
        }

        //Copy
        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the destination directory doesn't exist, create it.       
            Directory.CreateDirectory(destDirName);

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
                }
            }
        }

        public static void DeleteDirectory(string target_dir)
        {
            string[] files = Directory.GetFiles(target_dir);
            string[] dirs = Directory.GetDirectories(target_dir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(target_dir, false);
        }

        private void KillEpic()
        {
            Process[] localByName = Process.GetProcessesByName("EpicGamesLauncher");
            foreach (Process p in localByName)
            {
                p.Kill();
            }

            Process[] localByName2 = Process.GetProcessesByName("EpicWebHelper");
            foreach (Process p in localByName2)
            {
                p.Kill();
            }
        }
        private void CreateSymlink()
        {
            ReadKey();


            KillEpic();
            if (Directory.Exists(EpicDataPath))
            {
                if (!IsSymbolic(EpicDataPath))
                {

                    DirectoryCopy(EpicDataPath, "EpicData", true);
                    DeleteDirectory(EpicDataPath);
                }
            }

            progressBar1.Value = 280;
            var Symlink = new ProcessStartInfo("cmd.exe", " /C mklink /j \"" + EpicDataPath + "\" \"" + EpicPath + "\"");
            progressBar1.Value = 300;
            Symlink.CreateNoWindow = true;
            Symlink.UseShellExecute = false;
            Process.Start(Symlink);
            progressBar1.Value = 350;

        }


        //Main
        private void Form1_Load(object sender, EventArgs e)
        {
            this.CenterToScreen();  //StartPos CENTER
            this.TopMost = true;

            if (!Directory.Exists(LauncherPath))
            {
                MessageBox.Show(new Form { TopMost = true }, "Cannot Find Launcher", "Rafi Launcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }

            if (File.Exists(path))
            {
                ReadKey();
                CreateSymlink();
                timer1.Start(); //Timer
            }
            else
            {
                MessageBox.Show(new Form { TopMost = true }, "Key Error, Config File Missing", "Rafi Launcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            progressBar1.Increment(2);
            if (progressBar1.Value == 400)
            {
                timer1.Stop();
                Process.Start(@"Launcher\Engine\Binaries\Win32\EpicGamesLauncher.exe");
                Environment.Exit(0);
                return;
            }
            return;
        }
    }
}
