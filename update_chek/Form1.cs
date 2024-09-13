using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Windows.Forms;

namespace update_chek
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.ShowInTaskbar = false;
            this.Visible = false;

            // Revisar la versión al iniciar
            revisar_version();
        }

        // Método para revisar la versión
        static async Task revisar_version()
        {
            string installedVersion = "1.2.1"; // Versión actual de la aplicación

            // URL del script PHP que devuelve la versión más reciente
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

                    // Obtener la versión y el enlace de descarga
                    string latestVersion = json["version"].ToString();
                    string downloadLink = json["url"].ToString();

                    // Mostrar información de la versión
                    MessageBox.Show(latestVersion + " " + downloadLink);

                    // Comparar la versión instalada con la más reciente
                    if (installedVersion != latestVersion)
                    {
                        MessageBox.Show($"Hay una nueva versión disponible: {latestVersion}");

                        // Guardar el archivo en la ruta raíz del proyecto
                        string rootPath = Directory.GetCurrentDirectory();
                        string zipFilePath = Path.Combine(rootPath, "update.zip");

                        // Descargar el archivo y verificar el resultado
                        bool downloadSuccess = await DownloadFileAsync(downloadLink, zipFilePath);
                        if (downloadSuccess)
                        {
                            MessageBox.Show("La actualización se ha descargado correctamente.");

                            // Ruta donde se descomprimirá el contenido
                            string extractPath = "C:\\Deep"; // Descomprimir en la raíz del proyecto

                            // Descomprimir y reemplazar archivos existentes
                            DescomprimirYReemplazar(zipFilePath, extractPath);
                            MessageBox.Show("Actualización completada y archivos reemplazados.");
                        }
                        else
                        {
                            MessageBox.Show("Error al descargar el archivo.");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Tu aplicación está actualizada.");
                    }
                }
                catch (HttpRequestException e)
                {
                    MessageBox.Show("Error al conectarse al servidor: " + e.Message);
                }
            }
        }

        // Método para descargar el archivo
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
