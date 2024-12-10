using Common;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClientWPF.Pages.DirectoryPage
{
    /// <summary>
    /// Логика взаимодействия для DirectoryPage.xaml
    /// </summary>
    public partial class DirectoryPage : Page
    {
        public static DirectoryPage init;
        public DirectoryPage()
        {
            InitializeComponent();
            LoadDirectories();
            init = this;
        }

        private void Download(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                SendFileToServer(filePath);
            }
        }

        public void LoadDirectories()
        {
            try
            {
                var response = MainWindow.init.SendCommand("cd");
                if (response?.Command == "cd")
                {
                    var items = JsonConvert.DeserializeObject<List<string>>(response.Date);
                    stackPanelDirectories.Children.Clear();

                    foreach (var item in items)
                    {
                        bool isDirectory = Directory.Exists(item);
                        var directoryItem = new Item.Item(item, isDirectory);
                        stackPanelDirectories.Children.Add(directoryItem);
                    }
                }
                else
                {
                    MessageBox.Show("Не удалось загрузить список", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void DownloadFile(string fileName)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    FileName = System.IO.Path.GetFileName(fileName),
                    Filter = "All Files (*.*)|*.*",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                };

                if (saveFileDialog.ShowDialog() != true)
                {
                    return;
                }

                string localSavePath = saveFileDialog.FileName;

                Console.WriteLine($"Скачивание файла: {fileName}");
                var socket = Connecting(MainWindow.init.ipAddress, MainWindow.init.port);
                if (socket == null)
                {
                    MessageBox.Show("Не удалось подключиться к серверу", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string command = $"get {fileName}";
                ViewModelSend viewModelSend = new ViewModelSend(command, MainWindow.init.userId);
                byte[] messageBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(viewModelSend));
                socket.Send(messageBytes);

                byte[] buffer = new byte[10485760];
                int bytesReceived = socket.Receive(buffer);
                string serverResponse = Encoding.UTF8.GetString(buffer, 0, bytesReceived);

                ViewModelMessage responseMessage = JsonConvert.DeserializeObject<ViewModelMessage>(serverResponse);
                socket.Close();

                if (responseMessage.Command == "file")
                {
                    byte[] fileData = JsonConvert.DeserializeObject<byte[]>(responseMessage.Date);
                    File.WriteAllBytes(localSavePath, fileData);
                    MessageBox.Show($"Файл сохранён по пути: {localSavePath}", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Ошибка скачивания файла", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public Socket Connecting(IPAddress ipAddress, int port)
        {
            IPEndPoint endPoint = new IPEndPoint(ipAddress, port);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                socket.Connect(endPoint);
                return socket;
            }
            catch (SocketException ex)
            {
                Debug.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                if (socket != null && !socket.Connected)
                {
                    socket.Close();
                }
            }
            return null;
        }
        private string GetUniqueFilePath(string directory, string fileName)
        {
            string uniqueFilePath = System.IO.Path.Combine(directory, fileName);
            return uniqueFilePath;
        }

        public void SendFileToServer(string filePath)
        {
            try
            {
                var socket = Connecting(MainWindow.init.ipAddress, MainWindow.init.port);
                if (socket == null)
                {
                    MessageBox.Show("Не удалось подключиться к серверу", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (!File.Exists(filePath))
                {
                    MessageBox.Show("Файл не существует", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                FileInfo fileInfo = new FileInfo(filePath);
                FileInfoFTP fileInfoFTP = new FileInfoFTP(File.ReadAllBytes(filePath), fileInfo.Name);
                ViewModelSend viewModelSend = new ViewModelSend(JsonConvert.SerializeObject(fileInfoFTP), MainWindow.init.userId);
                byte[] messageByte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(viewModelSend));
                socket.Send(messageByte);
                byte[] buffer = new byte[10485760];
                int bytesReceived = socket.Receive(buffer);
                string serverResponse = Encoding.UTF8.GetString(buffer, 0, bytesReceived);
                ViewModelMessage responseMessage = JsonConvert.DeserializeObject<ViewModelMessage>(serverResponse);
                socket.Close();
                LoadDirectories();
                if (responseMessage.Command == "message")
                {
                    MessageBox.Show(responseMessage.Date);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
