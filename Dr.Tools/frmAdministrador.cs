using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Principal;
using System.Windows.Forms;
using System.Xml;
using Microsoft.Win32;
using Suporte;

namespace Dr.Tools
{

    /// <summary>
    /// cscript //H:CScript
    /// </summary>
    public partial class frmAdministrador : Form
    {
        static readonly XmlDocument Doc = new XmlDocument();
        private string FileToDownload;
        private bool _doRegClean = false;
        private bool _forcedExit = false;
        readonly bool _isAdmin = IsAdministrator(); //<-- set to allow Delete
        public frmAdministrador()
        {
            InitializeComponent();
        }
        
        private static bool IsAdministrator()
        {
        WindowsIdentity identity = WindowsIdentity.GetCurrent();
        WindowsPrincipal principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        private void LoadControlsDefaults()
        {
            tConsole.Text = "Olá Administrador!\r\n";
            tConsole.Focus();
        }

        private void frmAdmCadastrar_Load(object sender, EventArgs e)
        {
            LoadControlsDefaults();

            DownloadXmls();

            //if (!File.Exists(AssemblyDirectory + "\\VirusDatabase.xml"))
            //    return;
            //try
            //{
            //   Doc.Load(AssemblyDirectory + "\\VirusDatabase.xml");
            //}
            //catch (Exception)
            //{
            //    tConsole.Text += "Erro ao acessar o VDB.";
            //}
        }
        #region Botoes

        //INICAR PROCESSO COM ARGUMENTOS
        private void StartWithArgs(string processStart, string args)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo { FileName = processStart, Arguments = args };
                Process.Start(startInfo);
            }
            catch (Exception)
            {
                throw;
            }
        }
        static public string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
        private void btnForceQuit_Click(object sender, EventArgs e)
        {
            tConsole.Text = "Sayonara!";
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Application.Exit();
        }

        private void tbnLoadThreats_Click(object sender, EventArgs e)
        {
            LoadCheckList();
        }

        private void btnRestAdm_Click(object sender, EventArgs e)
        {
            _forcedExit = true;
            if (!IsAdministrator())
            {
                // Restart program and run as admin
                var exeName = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                ProcessStartInfo startInfo = new ProcessStartInfo(exeName);
                startInfo.Verb = "runas";
                Application.Exit();
                Application.ExitThread();
                System.Diagnostics.Process.Start(startInfo);

            }
        }

        private void btnUSBRepair_Click(object sender, EventArgs e)
        {
            frmUSBRepair frmUsb = new frmUSBRepair();
            frmUsb.ShowDialog();
        }

        private void btnCleanRegTraces_Click(object sender, EventArgs e)
        {
            if (!_isAdmin)
            {
                tConsole.Text = "Acesso restrito -> inicie o modo Administrador";
                return;
            }

            _doRegClean = true;
            MalwareScanner();
            RogueScanner();
            AdwareScanner();
            tConsole.Text += "\rChaves Removidas!";
            _doRegClean = false;
        }
        #endregion

        #region SCANNER
        private void LoadCheckList()
        {
            tConsole.Clear();
            tConsole.Text += "--------------------------Iniciado--------------------------------\r";

            _doRegClean = false;

            RogueScanner();
            AdwareScanner();
            MalwareScanner();

            tConsole.Text += "\r--------------------------Finalizado-------------------------------";
        }
        private void RogueScanner()
        {

            XmlNodeList LMNodes = Doc.SelectNodes("DBVirus/Rogue/hlmk/key");//Adwares LocalM

            foreach (XmlNode node in LMNodes)
            {
                if (Registry.LocalMachine.OpenSubKey(node.InnerText, _isAdmin) != null)
                {
                    tConsole.Text += node.InnerText + "\n";
                    if (_doRegClean)
                    {
                        try
                        {
                            tConsole.Text += "\r" + node.InnerText + " <- Removido";
                            Registry.LocalMachine.DeleteSubKey(node.InnerText);

                        }
                        catch (Exception)
                        {
                            tConsole.Text += "\r" + node.InnerText + " <- Removido";
                            Registry.LocalMachine.DeleteSubKeyTree(node.InnerText);
                        }
                    }
                }
            }

            XmlNodeList cuserNodes = Doc.SelectNodes("DBVirus/Rogue/hcuk/key");//Adwares Currentuser
            foreach (XmlNode node in cuserNodes)
            {
                if (Registry.CurrentUser.OpenSubKey(node.InnerText, _isAdmin) != null)
                {
                    tConsole.Text += node.InnerText + "\n";
                    if (_doRegClean)
                    {
                        try
                        {
                            tConsole.Text += "\r" + node.InnerText + " <- Removido";
                            Registry.CurrentUser.DeleteSubKey(node.InnerText);

                        }
                        catch (Exception)
                        {
                            tConsole.Text += "\r" + node.InnerText + " <- Removido";
                            Registry.CurrentUser.DeleteSubKeyTree(node.InnerText);
                        }
                    }
                }
            }
        }
        private void MalwareScanner()
        {
            XmlNodeList LMNodes = Doc.SelectNodes("DBVirus/Malware/hlmk/key");//Adwares LocalM
            foreach (XmlNode node in LMNodes)
            {
                if (Registry.LocalMachine.OpenSubKey(node.InnerText,_isAdmin) != null)
                {
                    tConsole.Text += node.InnerText + "\n";
                    if (_doRegClean)
                    {
                        try
                        {
                            tConsole.Text += "\r" + node.InnerText +" <- Removido";
                            Registry.LocalMachine.DeleteSubKey(node.InnerText);

                        }
                        catch (Exception)
                        {
                            tConsole.Text += "\r" + node.InnerText + " <- Removido";
                            Registry.LocalMachine.DeleteSubKeyTree(node.InnerText);
                        }
                    }
                }
            }

            XmlNodeList cuserNodes = Doc.SelectNodes("DBVirus/Malware/hcuk/key");//Adwares Currentuser
            foreach (XmlNode node in cuserNodes)
            {
                if (Registry.CurrentUser.OpenSubKey(node.InnerText, _isAdmin) != null)
                {
                    tConsole.Text += node.InnerText + "\n";
                    if (_doRegClean)
                    {
                        try
                        {
                            tConsole.Text += "\r" + node.InnerText + " <- Removido";
                            Registry.CurrentUser.DeleteSubKey(node.InnerText);

                        }
                        catch (Exception)
                        {
                            tConsole.Text += "\r" + node.InnerText + " <- Removido";
                            Registry.CurrentUser.DeleteSubKeyTree(node.InnerText);
                        }
                    }
                }
            }
        }
        private void AdwareScanner()
        {
            XmlNodeList LMNodes = Doc.SelectNodes("DBVirus/Adware/hlmk/key");//Adwares LocalM
            foreach (XmlNode node in LMNodes)
            {
                if (Registry.LocalMachine.OpenSubKey(node.InnerText, _isAdmin) != null)
                {
                    tConsole.Text += node.InnerText + "\n";
                    if (_doRegClean)
                    {
                        try
                        {
                            tConsole.Text += "\r" + node.InnerText + " <- Removido";
                            Registry.LocalMachine.DeleteSubKey(node.InnerText);

                        }
                        catch (Exception)
                        {
                            tConsole.Text += "\r" + node.InnerText + " <- Removido";
                            Registry.LocalMachine.DeleteSubKeyTree(node.InnerText);
                        }
                    }
                }
            }

            XmlNodeList cuserNodes = Doc.SelectNodes("DBVirus/Adware/hcuk/key");//Adwares Currentuser
            foreach (XmlNode node in cuserNodes)
            {
                if (Registry.CurrentUser.OpenSubKey(node.InnerText, _isAdmin) != null)
                {
                    tConsole.Text += node.InnerText + "\n";
                    if (_doRegClean)
                    {
                        try
                        {
                            tConsole.Text += "\r" + node.InnerText + " <- Removido";
                            Registry.CurrentUser.DeleteSubKey(node.InnerText);

                        }
                        catch (Exception)
                        {
                            tConsole.Text += "\r" + node.InnerText + " <- Removido";
                            Registry.CurrentUser.DeleteSubKeyTree(node.InnerText);
                        }
                    }
                }
            }
        }
        #endregion

        #region Download

        public void DownloadXmls()
        {
            if (File.Exists(AssemblyDirectory + "\\VirusDatabase.xml"))
                return;
            DownloadFile("https://dl.dropboxusercontent.com/s/b4ow784br0xkoxz/VirusDatabase.xml", "VirusDatabase.xml");
        }
        public static void DownloadFile(string url, string filename)
        {
            try
            {
                WebClient webDownup = new WebClient();
                webDownup.DownloadFileAsync(new Uri(url), AssemblyDirectory + "\\" + filename);
            }
            catch (WebException)
            {
            }

        }
        private void DownloadApplication()
        {
            try
            {
                try
                {
                    if (!Directory.Exists(AssemblyDirectory))
                        return;
                }
                catch (Exception)
                {
                    return;
                }

                using (WebClient webDownup = new WebClient())
                {
                    if (FileToDownload.Contains("AdwCleaner"))
                        webDownup.DownloadFileAsync(new Uri(FileToDownload), AssemblyDirectory + @"\AdwCleaner.exe");

                    if (FileToDownload.Contains("mbam-setup"))
                        webDownup.DownloadFileAsync(new Uri(FileToDownload), AssemblyDirectory + @"\mbam-setup-2.0.1.1004.exe");

                    if (FileToDownload.Contains("ComboFix"))
                        webDownup.DownloadFileAsync(new Uri(FileToDownload), AssemblyDirectory + @"\ComboFix.exe");

                    if (FileToDownload.Contains("ccsetup"))
                        webDownup.DownloadFileAsync(new Uri(FileToDownload), AssemblyDirectory + @"\ccsetup.exe");

                    if (FileToDownload.Contains("HijackThis"))
                        webDownup.DownloadFileAsync(new Uri(FileToDownload), AssemblyDirectory + @"\HijackThis.exe");

                    webDownup.DownloadFileCompleted += WebDownupOnDownloadFileCompleted;
                }
            }
            catch (WebException)
            {
            }
        }

        private void WebDownupOnDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            tConsole.Text += "\nDownload Concluído";
            tConsole.Text += "\nIniciando aplicativo...";
            if (FileToDownload.Contains("AdwCleaner"))
            {
                Process.Start(AssemblyDirectory + @"\AdwCleaner.exe");
            }

            if (FileToDownload.Contains("mbam-setup"))
            {
                Process.Start(AssemblyDirectory + @"\mbam-setup-1.75.0.1300.exe");
            }
            if (FileToDownload.Contains("ComboFix"))
            {
                Process.Start(AssemblyDirectory + @"\ComboFix.exe");
            }
            if (FileToDownload.Contains("ccsetup"))
            {
                Process.Start(AssemblyDirectory + @"\ccsetup.exe");
            }
            if (FileToDownload.Contains("HijackThis"))
            {
                Process.Start(AssemblyDirectory + @"\HijackThis.exe");
            }
            FileToDownload = "";
        }

        #endregion

        #region read Log
        private void ReadADWCleanerLog()
        {
            if (!Directory.Exists(@"C:\AdwCleaner"))
                return;

            const string path = @"C:\AdwCleaner";
            var dir = new DirectoryInfo(path);
            string filepath = null;
            var LastWrite = DateTime.Now;

            foreach (var file in dir.GetFiles())
            {
                if (file.CreationTime < LastWrite)
                {
                    filepath = file.FullName;
                }
            }

            tConsole.Clear();
            if (filepath != null) tConsole.LoadFile(filepath, RichTextBoxStreamType.PlainText);
        }

        private void ReadMalwarebytesLog()
        {
            //C:\Users\EncryptorX\AppData\Roaming\Malwarebytes\Malwarebytes' Anti-Malware\Logs
            if (!Directory.Exists(@"C:\Users\" + Environment.UserName + @"\AppData\Roaming\Malwarebytes\Malwarebytes' Anti-Malware\Logs"))
                return;

            string path = @"C:\Users\" + Environment.UserName + @"\AppData\Roaming\Malwarebytes\Malwarebytes' Anti-Malware\Logs";
            var dir = new DirectoryInfo(path);
            string filepath = null;
            var LastWrite = DateTime.Now;

            foreach (var file in dir.GetFiles())
            {
                if (file.CreationTime < LastWrite)
                {
                    filepath = file.FullName;
                }
            }

            tConsole.Clear();
            if (filepath != null) tConsole.LoadFile(filepath, RichTextBoxStreamType.PlainText);
        }
        private void ReadCombofixLog()
        {
            //C:\Users\EncryptorX\AppData\Roaming\Malwarebytes\Malwarebytes' Anti-Malware\Logs
            if (!File.Exists(@"C:\Combofix.txt"))
                return;

            //const string path = @"C:\Combofix.txt";
            //var dir = new DirectoryInfo(path);
            string filepath = @"C:\Combofix.txt";
            tConsole.Clear();
            tConsole.LoadFile(filepath, RichTextBoxStreamType.PlainText);
        }

        private void btnAdwcLog_Click(object sender, EventArgs e)
        {
            ReadADWCleanerLog();
        }
        private void btnMalwareLog_Click(object sender, EventArgs e)
        {
            ReadMalwarebytesLog();
        }
        private void btnCombLog_Click(object sender, EventArgs e)
        {
            ReadCombofixLog();
        }
        #endregion

        #region Aplicativos

        private void StartAdwcleaner()
        {
            if (File.Exists(AssemblyDirectory + @"\AdwCleaner.exe"))
            {
                File.Delete(AssemblyDirectory + @"\AdwCleaner.exe");
                tConsole.Text = "Atualizando...";
                tConsole.Text += "\nIniciando Download... Aguarde.";
                FileToDownload = "https://downloads.malwarebytes.com/file/adwcleaner";
                DownloadApplication();
            }
            else
            {
                tConsole.Text += "\nIniciando Download... Aguarde.";
                FileToDownload = "https://downloads.malwarebytes.com/file/adwcleaner";
                DownloadApplication();
            }
        }

        private void StartMalwareBytes()
        {
            if (!File.Exists(@"C:\Program Files (x86)\Malwarebytes' Anti-Malware\mbam.exe"))
            {
                if (!File.Exists(AssemblyDirectory + @"\mbam-setup-2.0.1.1004.exe"))
                {
                    tConsole.Text = "Arquivo não existe...";
                    tConsole.Text += "\nIniciando Download...";
                    FileToDownload = "https://www.malwarebytes.com/api/downloads/mb-windows?filename=MBSetup.exe";//http://data-cdn.mbamupdates.com/v0/program/data/mbam-setup-1.75.0.1300.exe
                    DownloadApplication();
                }
                else
                {
                    Process.Start(AssemblyDirectory + @"mbam-setup-2.0.1.1004.exe");
                }

            }
            else
                try
                {
                    Process.Start(@"C:\Program Files (x86)\Malwarebytes' Anti-Malware\mbam.exe");
                }
                catch (Exception)
                {
                    tConsole.Text = "Erro: Verificar Existencia do arquivo Mbam";

                }
        }

        private void StartHijackThis()
        {
            if (!File.Exists(@"C:\ProgramData\SuporteUpdater\HijackThis.exe"))
            {
                if (!File.Exists(AssemblyDirectory + @"\HijackThis.exe"))
                {
                    tConsole.Text = "Arquivo não existe...";
                    tConsole.Text += "\nIniciando Download...";
                    FileToDownload = "http://ufpr.dl.sourceforge.net/project/hjt/2.0.5%20beta/HijackThis.exe";
                    DownloadApplication();
                }
                else
                {
                    Process.Start(AssemblyDirectory + @"\HijackThis.exe");
                }

            }
            else
                try
                {
                    Process.Start(AssemblyDirectory + @"\HijackThis.exe");
                }
                catch (Exception)
                {
                    tConsole.Text = "Erro: Verificar Existencia do arquivo HJT";

                }
        }
        private void StartCombofix()
        {
            if (File.Exists(AssemblyDirectory + @"\ComboFix.exe"))
            {
                File.Delete(AssemblyDirectory + @"\ComboFix.exe");
                tConsole.Text = "Atualizando...";
                tConsole.Text += "\nAtenção - Detectado o uso de ferramenta avançada.";
                tConsole.Text += "\nIniciando Download...";
                FileToDownload = "http://download.bleepingcomputer.com/sUBs/ComboFix.exe";
                DownloadApplication();
            }
            else
                try
                {
                    tConsole.Text += "\nIniciando Download...";
                    FileToDownload = "http://download.bleepingcomputer.com/sUBs/ComboFix.exe";
                    DownloadApplication();
                }
                catch (Exception)
                {
                    tConsole.Text = "Erro: Verificar Existencia do arquivo ComboFix";

                }
        }

        private void StartCCleaner()//Precisa de Atualização
        {
            if (!File.Exists(@"C:\Program Files\CCleaner\CCleaner64.exe"))
            {
                tConsole.Text += "Iniciando o download - CCleaner";
                FileToDownload = "http://download.piriform.com/ccsetup411.exe";
                DownloadApplication();
            }
            else
            {
                try
                {
                    Process.Start(@"C:\Program Files\CCleaner\CCleaner64.exe");
                }
                catch (Exception)
                {
                }

            }
        }
        private void btnAdwCleaner_Click(object sender, EventArgs e)
        {
            StartAdwcleaner();
        }

        private void btnMalwareBytes_Click(object sender, EventArgs e)
        {
            StartMalwareBytes();

        }
        private void btnCCleaner_Click(object sender, EventArgs e)
        {
            StartCCleaner();
        }
        private void btnComboFix_Click(object sender, EventArgs e)
        {
            StartCombofix();

        }

        private void btnHijackthis_Click(object sender, EventArgs e)
        {
            StartHijackThis();
        }
        #endregion

        #region Remover BTN
        internal enum MoveFileFlags
        {
            MOVEFILE_REPLACE_EXISTING = 1,
            MOVEFILE_COPY_ALLOWED = 2,
            MOVEFILE_DELAY_UNTIL_REBOOT = 4,
            MOVEFILE_WRITE_THROUGH = 8
        }

        [System.Runtime.InteropServices.DllImportAttribute("kernel32.dll", EntryPoint = "MoveFileEx")]
        internal static extern bool MoveFileEx(string lpExistingFileName, string lpNewFileName,
        MoveFileFlags dwFlags);
        private void MarktoDelete()
        {
           // MoveFileEx(Program.ApplicationExe, null, MoveFileFlags.MOVEFILE_DELAY_UNTIL_REBOOT);
        }
        #endregion

        #region Console

        List<string> normaList = new List<string>
        {
            "olá","ola","oi","tchau","oii","sim","nao","n","não","s","ok","bye","bobo","coco","puto",
        };
        List<string> execList = new List<string>
        {
            "exec","executar","iniciar","start","get",
        };

        private void FindinLineaString()
        {
            // string tosearch = search.ToLower();
            string[] tLineStrings = tConsole.Lines;
            foreach (var line in tLineStrings)
            {

                if (line.Equals("help") || line.Equals("Help"))
                {
                    tConsole.Clear();
                    tConsole.Text += "Mostrando Cvars\n";
                    tConsole.Text += "\nclear;cls;limpar";
                    tConsole.Text += "\nquit;exit;sair;close";
                    tConsole.Text += "\nexecutar;exec;set;iniciar;force;forçar";
                    tConsole.Text += "\nget adobe";
                    break;
                }
                
                //cVars
                if (line.Equals("clear") || line.Equals("cls") || line.Equals("limpar"))
                {
                    tConsole.Clear();
                    break;
                }
                if (line.Equals("scan"))
                {
                    tConsole.Clear();
                    tConsole.Text += "Iniciando Scan de integridade...";
                   // cIntegridade.StartSystemCheck();
                    tConsole.Text += "Finalizado";
                    break;
                }
                if (line.Equals("forcequit"))
                {
                    Application.Exit();
                    break;
                }
                if (line.Equals("quit", StringComparison.OrdinalIgnoreCase) || line.Equals("exit", StringComparison.OrdinalIgnoreCase) || line.Equals("sair", StringComparison.OrdinalIgnoreCase) || line.Equals("close", StringComparison.OrdinalIgnoreCase))
                {
                    Close();
                    break;
                }
                if (normaList.Contains(line))//Normal words
                {
                    tConsole.Clear();
                    tConsole.Text += "\nComando Inválido: Você é algum retardado ?";
                }
                if (execList.Contains(line))// Execs cvars
                {
                    if (line.Contains("combofix"))//combofix
                    {
                        StartCombofix();
                        break;
                    }
                    if (line.Contains("malwarebytes"))//combofix
                    {
                        StartMalwareBytes();
                        break;
                    }
                    if (line.Contains("adwcleaner"))//combofix
                    {
                        StartAdwcleaner();
                        break;
                    }
                    if (line.Contains("hijack"))//combofix
                    {
                        StartHijackThis();
                        break;
                    }
                }

            }
            //tConsole.Clear();
        }
        private void ExecFromConsole()
        {
            FindinLineaString();
        }

        private void tConsole_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                ExecFromConsole();
            }
        }
        #endregion
        
        private void frmAdministrador_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (!_forcedExit) return;
            Application.Exit();
        }

        #region ToolStripBTN

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            StartWithArgs(@"C:\Windows\regedit.exe", null);
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            StartWithArgs(@"eventvwr.msc", "/s");
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            StartWithArgs(@"msconfig.exe", null);
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            StartWithArgs(@"gpedit.msc", null);
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            StartWithArgs(@"compmgmt.msc", "/s");
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            StartWithArgs(@"services.msc", null);
        }

        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            StartWithArgs(@"WF.msc", null);
        }

        private void toolStripButton8_Click(object sender, EventArgs e)
        {
            StartWithArgs(@"Netplwiz.exe", null);
        }

        private void toolStripButton9_Click(object sender, EventArgs e)
        {
            StartWithArgs(@"rstrui.exe", null);
        }
        
        #endregion

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }


    }
}
