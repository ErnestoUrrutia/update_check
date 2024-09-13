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

            // Revisar la versi�n al iniciar
            revisar_version();
        }

        // M�todo para revisar la versi�n
        static async Task revisar_version()
        {

            string exePath = @"C:\Deep\DeepControl.exe";

            // Obtener la informaci�n de versi�n del archivo
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(exePath);

            // Imprimir el n�mero de versi�n del ejecutable
            MessageBox.Show($"Versi�n del ejecutable: {versionInfo.FileVersion}");

            // Opcional: Imprimir otros detalles
            MessageBox.Show($"Producto: {versionInfo.ProductName}");
            MessageBox.Show($"Descripci�n: {versionInfo.FileDescription}");
             MessageBox.Show($"Empresa: {versionInfo.CompanyName}");





            string installedVersion = ""+versionInfo.FileVersion; // Versi�n actual de la aplicaci�n

            // URL del script PHP que devuelve la versi�n m�s reciente
            string versionCheckUrl = "https://ernestourrutia.com.mx/update_check/version/deepcontrol/";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(versionCheckUrl);
                    response.EnsureSuccessStatusCode();

                    // Leer el contenido de la respuesta
                    string content = await response.Content.ReadAsStringAsync();

                    // Parsear el contenido JSON
                    JObject json = JObject.Parse(content);
                    
                    // Obtener la versi�n y el enlace de descarga
                    string latestVersion = json["version"].ToString();
                    string downloadLink = json["url"].ToString();
                    MessageBox.Show(latestVersion+" "+installedVersion);
                    // Mostrar informaci�n de la versi�n
                    string filePathExe = @"C:\Deep\DeepControl.exe";
                    string filePathIco = @"C:\Deep\logotipot";

                    // Comparar la versi�n instalada con la m�s reciente
                    if (installedVersion != latestVersion|| !File.Exists(filePathExe) || !File.Exists(filePathIco))
                    {
                        //MessageBox.Show($"Hay una nueva versi�n disponible: {latestVersion}");
                        Process.Start("taskkill", $"/f /im DeepControl.exe");
                        // Guardar el archivo en la ruta ra�z del proyecto
                        string rootPath = Directory.GetCurrentDirectory();
                        string zipFilePath = Path.Combine(rootPath, "update.zip");

                        // Descargar el archivo y verificar el resultado
                        bool downloadSuccess = await DownloadFileAsync(downloadLink, zipFilePath);
                        if (downloadSuccess)
                        {
                            //MessageBox.Show("La actualizaci�n se ha descargado correctamente.");

                            // Ruta donde se descomprimir� el contenido
                            string extractPath = "C:\\Deep"; // Descomprimir en la ra�z del proyecto

                            // Descomprimir y reemplazar archivos existentes
                            DescomprimirYReemplazar(zipFilePath, extractPath);
                            MessageBox.Show("Actualizaci�n completada y archivos reemplazados.");
                            Process.Start(@"C:\Deep\DeepControl.exe");
                            Application.Exit();
                        }
                        else
                        {
                            MessageBox.Show("Error al descargar el archivo.");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Tu aplicaci�n est� actualizada.");
                    }
                }
                catch (HttpRequestException e)
                {
                    MessageBox.Show("Error al conectarse al servidor: " + e.Message);
                }
            }
        }

        // M�todo para descargar el archivo
        static async Task<bool> DownloadFileAsync(string url, string filePath)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                using (var response = await client.GetAsync(url))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        MessageBox.Show($"Error al descargar el archivo: {response.StatusCode}");
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
                        MessageBox.Show("El archivo descargado est� vac�o.");
                        return false; // El archivo est� vac�o
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error durante la descarga: " + ex.Message);
                return false; // Manejar errores de descarga
            }
        }

        // M�todo para descomprimir y reemplazar archivos
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
