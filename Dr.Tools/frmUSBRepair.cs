using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Suporte
{
    public partial class frmUSBRepair : Form
    {
        //ATTRIB [+R | -R] [+A | -A ] [+S | -S] [+H | -H] [[drive:] [path] filename] [/S [/D]]
        //attrib -r -a -s -h d:\aa /S /D *.*
        //attrib -r b:\public\jones\*.* /s 
        //attrib -r -a -s -h d:\aa /S /D
        private readonly FolderBrowserDialog _folderBrowser = new FolderBrowserDialog();//Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced
        RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced",true);
        private int foldercount = 0;
        private int filecount = 0;
        private int removidos = 0;
        private string malware;
       // private string path;
        public frmUSBRepair()
        {
            InitializeComponent();
        }

        private void btnOpenFolder_Click(object sender, EventArgs e)
        {
            tbxLog.Text = "Selecionando caminho...";
            if (registryKey != null)
            {
                if (registryKey.GetValue("Hidden").ToString() == "0" || registryKey.GetValue("Hidden").ToString() == "2")
                    registryKey.SetValue("Hidden", 1);
                registryKey.Flush();
            }
            _folderBrowser.Description = "Selecionar pasta a reparar";
            if (_folderBrowser.ShowDialog() == DialogResult.OK)
            {
                tbxPath.Text = Path.GetFullPath(_folderBrowser.SelectedPath);
                tbxLog.Text += Environment.NewLine + "Caminho localizado.";
            }
        }

        private void btnRepair_Click(object sender, EventArgs e)
        {
            filecount = 0;
            foldercount = 0;
            tbxLog.Text += Environment.NewLine + "Iniciando reparos na unidade...";
            DirectoryInfo directory = new DirectoryInfo(tbxPath.Text);
            tbxLog.Text += Environment.NewLine + "Reparando pastas e subpastas...";
            try
            {
                foreach (var Dir in directory.GetDirectories())
                {
                    try
                    {
                        if (Dir.Name == "System Volume Information" || Dir.Name == "$RECYCLE.BIN")
                            continue;
                        foldercount++;
                        FileAttributes attributes = File.GetAttributes(Dir.FullName);
                        if ((attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                        { continue;}
                        Dir.Attributes = Dir.Attributes & ~FileAttributes.Hidden;
                        Dir.Attributes = Dir.Attributes & ~FileAttributes.Normal;
                        Dir.Attributes = Dir.Attributes & ~FileAttributes.System;
                    }
                    catch (Exception exception)
                    {
                        tbxLog.Text += Environment.NewLine + "erro na pasta -> " + exception;
                    }

                }
            }
            catch
            {

            }


            try
            {
                foreach (var file in DirRecursiveSearch(tbxPath.Text))
                {
                    try
                    {
                        FileInfo fileInfo = new FileInfo(file);

                        FileAttributes attributes = File.GetAttributes(file);

                        if (fileInfo.Extension == ".lnk")
                        {
                            fileInfo.Delete();
                            removidos++;
                            continue;
                        }
                        if (fileInfo.Extension == ".vbs")
                        {
                            fileInfo.Delete();
                            removidos++;
                            malware = fileInfo.Name;
                            continue;
                        }
                        if (fileInfo.Extension == ".js")
                        {
                            fileInfo.Delete();
                            removidos++;
                            malware = fileInfo.Name;
                            continue;
                        }
                        if (fileInfo.Extension == ".vbe")
                        {
                            fileInfo.Delete();
                            removidos++;
                            malware = fileInfo.Name;
                            continue;
                        }
                        filecount++;
                        if ((attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                        { continue; }
                        fileInfo.Attributes = fileInfo.Attributes & ~FileAttributes.System;
                        fileInfo.Attributes = fileInfo.Attributes & ~FileAttributes.Normal;
                        fileInfo.Attributes = fileInfo.Attributes & ~FileAttributes.Hidden;
                        fileInfo.Attributes = fileInfo.Attributes & ~FileAttributes.ReadOnly;
   

                    }
                    catch (Exception exception)
                    {
                        tbxLog.Text += Environment.NewLine + @"erro no arquivo -> " + exception;
                    }
                }
            }
            catch(Exception exception)
            {
                tbxLog.Text += Environment.NewLine + @"erro no arquivo -> " + exception;
            }
            tbxLog.Text += Environment.NewLine + @"USB/Diretórios Reparados e arquivos suspeitos deletados.";
            tbxLog.Text += Environment.NewLine+@"Total de pastas: "+ foldercount ;
            tbxLog.Text += Environment.NewLine + @"Total de arquivos: " + filecount;
            tbxLog.Text += Environment.NewLine + @"Total Removidos: " + removidos;
            tbxLog.Text += Environment.NewLine + @"Malware ->: " + malware;
        }
        List<string> DirRecursiveSearch(string sDir)
        {
            List<string> files = new List<string>();

            try
            {
                foreach (string f in Directory.GetFiles(sDir))
                {
                    files.Add(f);
                }

                foreach (string d in Directory.GetDirectories(sDir))
                {
                    files.AddRange(DirRecursiveSearch(d));
                }
            }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }

            return files;
        }
        
        private void frmUSBRepair_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (registryKey.GetValue("Hidden").ToString() == "1")
                registryKey.SetValue("Hidden", 0);
            registryKey.Flush();
            registryKey.Close();
        }
    }
}
