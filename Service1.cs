using System;
using System.IO;
using System.Net.Http;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Configuration;

namespace MEK7300service
{
    public partial class Service1 : ServiceBase
    {
        private Timer _timer;
        private Timer _timerSendFile;
        private SerialListener _serialListener;
        private readonly ServiceConfig _config;

        public Service1(IConfiguration configuration)
        {
            _config = configuration.GetSection("SerialConfig").Get<ServiceConfig>();
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            string port = _config.PortName;
            int baudRate = _config.BaudRate;
            _serialListener = new SerialListener(port, baudRate);
            _timer = new Timer(CheckPortStatus, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
            _timerSendFile = new Timer(SendFile, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
        }

        protected override void OnStop()
        {
            _serialListener.Dispose();
            _timer.Dispose();
            _timerSendFile.Dispose();
        }

        private void CheckPortStatus(object state)
        {
            if (!_serialListener.IsPortOpen)
            {
                _serialListener.OpenPort();
            }
        }

        private async void SendFile(object state)
        {
            string webHook = _config.Webhook;
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string sourceDirectory = Path.Combine(currentDirectory, "gerados");
            string destinationDirectory = Path.Combine(currentDirectory, "processados");
            string webhookUrl = webHook;

            try
            {
                if (!Directory.Exists(sourceDirectory))
                {
                    Directory.CreateDirectory(sourceDirectory);
                    WriteLog("Diretório 'gerados' não existia e foi criado.");
                    return;
                }

                if (!Directory.Exists(destinationDirectory))
                {
                    Directory.CreateDirectory(destinationDirectory);
                    WriteLog("Diretório 'processados' criado.");
                }

                string[] files = Directory.GetFiles(sourceDirectory);

                if (files.Length == 0)
                {
                    WriteLog("Nenhum arquivo encontrado no diretório 'gerados'.");
                    return;
                }

                using (HttpClient client = new HttpClient())
                {
                    foreach (string file in files)
                    {
                        try
                        {
                            WriteLog($"Arquivo encontrado: {file}");

                            string fileName = Path.GetFileName(file);
                            string fileContent = File.ReadAllText(file);

                            var payload = new
                            {
                                FileName = fileName,
                                Content = fileContent,
                                ExamCode = "HEMO"
                            };

                            string jsonPayload = System.Text.Json.JsonSerializer.Serialize(payload);
                            StringContent httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                            WriteLog($"enviando post com o payload:{jsonPayload} para a url: {webhookUrl}");
                            HttpResponseMessage response = await client.PostAsync(webhookUrl, httpContent);
                            string responsejson = await response.Content.ReadAsStringAsync();

                            if (response.IsSuccessStatusCode && fileName.Length == 17)
                            {
                                WriteLog($"Dados enviados ao webhook para o arquivo: {fileName}");

                                string destFile = Path.Combine(destinationDirectory, fileName);

                                if (!File.Exists(destFile))
                                {
                                    File.Move(file, destFile);
                                    WriteLog($"Arquivo movido para: {destFile}");
                                    WriteLog($"{jsonPayload}");
                                }
                                else
                                {
                                    WriteLog($"O arquivo {fileName} já existe no destino. Não foi movido.");
                                }
                            }
                            else
                            {
                                WriteLog($"Falha ao enviar dados do arquivo {fileName} ao status code {response.StatusCode} message: {responsejson}");
                            }
                        }
                        catch (Exception ex)
                        {
                            WriteLog($"Erro ao processar o arquivo {file}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog($"Erro ao processar arquivos: {ex.Message}");
            }
        }

        public static void WriteLog(string message)
        {
            string logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "service.log");
            using (StreamWriter writer = new StreamWriter(logFile, true))
            {
                writer.WriteLine($"{DateTime.Now}: {message}");
            }
        }
    }
}