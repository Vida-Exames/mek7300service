using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Text;

namespace MEK7300service
{
    public class SerialListener : IDisposable
    {
        private readonly SerialPort _serialPort;
        private readonly object _lock = new object();
        private readonly StringBuilder _buffer = new StringBuilder();
        private readonly List<string> _lines = new List<string>();
        private const char ETX = (char)0x03;

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                WriteLog($"Dando inicio ao recebimento de dados via serial");
                string incoming = _serialPort.ReadExisting();
                WriteLog(incoming);

                lock (_lock)
                {
                    WriteLog($"incoming dentro do lock {incoming}");
                    _buffer.Append(incoming);
                    WriteLog($"Printando o buffer apos a append");
                    WriteLog(_buffer.ToString());
                    WriteLog("Buffer printado");


                    if (isEnd())
                    {

                        WriteLog("É para ter acabado o arquivo, basedo no ETX");

                        string exameCompleto = string.Join(Environment.NewLine, _buffer.ToString());
                        WriteLog($"Printando o exame por completo");
                        WriteLog(exameCompleto);

                        ProcessFile processFile = new ProcessFile();
                        processFile.CreateInitializationFile(exameCompleto);

                        WriteLog($"enviado para criar o arquivo e sera limpo o buffer");

                        _buffer.Clear();

                        WriteLog($"buffer limpo -> {_buffer.ToString()}");
                    }



                }

                WriteLog($"Saiu do lock");
            }
            catch (Exception ex)
            {
                _buffer.Clear();
                WriteLog($"Erro no DataReceived: {ex.Message}");
            }
        }

        private bool isEnd()
        {
            return _buffer.ToString().IndexOf(ETX) >= 0;

        }

        public bool IsPortOpen => _serialPort.IsOpen;

        public SerialListener(string portName, int baudRate)
        {
            _serialPort = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One)
            {
                ReadTimeout = 1000,
                WriteTimeout = 1000,
                ReadBufferSize = 4096,
                WriteBufferSize = 2048
            };
            _serialPort.DataReceived += SerialPort_DataReceived;
        }

        public void OpenPort()
        {
            lock (_lock)
            {
                if (!_serialPort.IsOpen)
                {
                    try
                    {
                        _serialPort.Open();
                        WriteLog("Porta serial aberta com sucesso.");
                    }
                    catch (Exception ex)
                    {
                        WriteLog($"Erro ao abrir porta: {ex.Message}");
                    }
                }
            }
        }

        public void Dispose()
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
            }
            _serialPort.Dispose();
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