using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Windows.Forms;
using System.Diagnostics;

namespace update_chek
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.ShowInTaskbar = false;
            this.Visible = false;
            revisar_version();
        }
        static async Task revisar_version()
        {
            string filePathExe = @"C:\Deep\DeepControl.exe";
            string filePathIco = @"C:\Deep\logotipo.ico";
            string installedVersion = "";

            if (File.Exists(filePathExe))
            {
                
                FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(filePathExe);
                
                try
                {
                    installedVersion = "" + versionInfo.FileVersion;
                }
                catch (Exception ex)
                {
                    installedVersion = "0";
                }
            }
            string versionCheckUrl = "https://ernestourrutia.com.mx/update_check/version/deepcontrol/";
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(versionCheckUrl);
                    response.EnsureSuccessStatusCode();
                    string content = await response.Content.ReadAsStringAsync();
                    JObject json = JObject.Parse(content);
                    string latestVersion = json["version"].ToString().Trim();
                    string downloadLink = json["url"].ToString();
                    
                    if (installedVersion != latestVersion)
                    {
                        Process.Start("taskkill", $"/f /im DeepControl.exe");
                        string rootPath = Directory.GetCurrentDirectory();
                        string zipFilePath = Path.Combine(rootPath, "update.zip");
                        bool downloadSuccess = await DownloadFileAsync(downloadLink, zipFilePath);
                        if (downloadSuccess)
                        {
                            string extractPath = "C:\\Deep";
                            DescomprimirYReemplazar(zipFilePath, extractPath);
                            Process.Start("attrib", "+s +h C:\\Deep");
                            Process.Start("taskkill", $"/f /im DeepControl.exe");
                            Thread.Sleep(2000);
                            Process.Start(@"C:\Deep\DeepControl.exe");
                            Application.Exit();
                        }
                        else
                        {
                            if (File.Exists(filePathExe))
                            {
                                Process.Start("taskkill", $"/f /im DeepControl.exe");
                                Thread.Sleep(2000);
                                Process.Start(@"C:\Deep\DeepControl.exe");
                            }
                            Application.Exit();
                        }
                        
                    }
                    else
                    {
                        Process.Start("taskkill", $"/f /im DeepControl.exe");
                        Thread.Sleep(2000);
                        Process.Start(@"C:\Deep\DeepControl.exe");
                        Application.Exit();
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show("Error al conectarse al servidor: " + e.Message);
                    if (File.Exists(filePathExe))
                    {
                        Process.Start("taskkill", $"/f /im DeepControl.exe");
                        Thread.Sleep(2000);
                        Process.Start(@"C:\Deep\DeepControl.exe");
                    }
                    Application.Exit();
                }
            }
        }
        public void terminarEiniciar()
        {

        }
        static async Task<bool> DownloadFileAsync(string url, string filePath)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                using (var response = await client.GetAsync(url))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        //MessageBox.Show($"Error al descargar el archivo: {response.StatusCode}");
                        return false; // Error en la descarga
                    }

                    // Crear el archivo en la ruta especificada
                    using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await response.Content.CopyToAsync(fileStream);
                    }

                    // Verificar si el archivo fue descargado correctamente
                    FileInfo fileInfo = new FileInfo(filePath);
                    if (fileInfo.Length > 0)
                    {
                        return true; // La descarga fue exitosa
                    }
                    else
                    {
                        MessageBox.Show("El archivo descargado está vacío.");
                        return false; // El archivo está vacío
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error durante la descarga: " + ex.Message);
                return false; // Manejar errores de descarga
            }
        }

        // Método para descomprimir y reemplazar archivos
        static void DescomprimirYReemplazar(string zipFilePath, string extractPath)
        {
            try
            {
                // Descomprimir y reemplazar archivos existentes
                ZipFile.ExtractToDirectory(zipFilePath, extractPath, overwriteFiles: true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al descomprimir el archivo: " + ex.Message);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Visible = false;
        }
    }
}
