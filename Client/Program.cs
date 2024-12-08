using System;
using System.Collections.Generic;
using System.Net;
using Common;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Text;
using System.IO;

namespace Client
{
    public class Program
    {
        public static IPAddress IpAdress;
        public static int Port;
        public static int Id = -1;
        static void Main(string[] args)
        {
            Console.Write("Введите IP адрес сервера: ");
            string sIpAdress = Console.ReadLine();
            Console.Write("Введите порт: ");
            string sPort = Console.ReadLine();
            if (int.TryParse(sPort, out Port) && IPAddress.TryParse(sIpAdress, out IpAdress))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Данные успешно введены. Подключаюсь к серверу.");
                while (true)
                {
                    ConnectServer();
                }
            }
        }
        public static bool CheckCommand(string message)
        {
            // Создаёи переменную говорящую о том, что команда неверная
            bool BCommand = false;
            // Разбиваем сообщение пользователя на массив
            string[] DataMessage = message.Split(new string[1] { " " }, StringSplitOptions.None);
            // Если длина данных больше 0
            if (DataMessage.Length > 0)
            {
                // проверяем первую строку на наличие команды
                // существующие команды : connect|cdget|set
                string Command = DataMessage[0];
                // Если команда подключение
                if (Command == "connect")
                {
                    // Проверяем что у нас отправляется три данных, команда, логин, пароль
                    if (DataMessage.Length != 3)
                    {
                        // Меняем цвет текста в командной строке
                        Console.ForegroundColor = ConsoleColor.Red;
                        // Выводим текст
                        Console.WriteLine("Использование: connect [login] [password]\nПример: connect User1 Password");
                        // Говорим что команда не верная
                        BCommand = false;
                    }
                    else
                        // Если всё правильно, команда верная
                        BCommand = true;
                }
                // Если команда на переход по директориям
                else if (Command == "cd")
                    // Команда верная
                    BCommand = true;

                // Если команда получения файла
                else if (Command == "get")
                {
                    // Если длина сообщения более одного
                    if (DataMessage.Length == 1)
                    {
                        // Меняем цвет текста в командной строке
                        Console.ForegroundColor = ConsoleColor.Red;
                        // Выводим текст
                        Console.WriteLine("Использование: get [NameFile]\nПример: get Test.txt");
                        // Говорим что команда не верная
                        BCommand = false;
                    }
                    else
                        // Команда верная
                        BCommand = true;
                }
                // Если команда отправки файла
                else if (Command == "set")
                {
                    // Если длина сообщения более одного
                    if (DataMessage.Length == 1)
                    {
                        // Меняем цвет текста в командной строке
                        Console.ForegroundColor = ConsoleColor.Red;
                        // Выводим текст
                        Console.WriteLine("Использование: set [NameFile]\nПример: set Test.txt");
                        // Говорим что команда не верная
                        BCommand = false;
                    }
                    else
                        // Команда верная
                        BCommand = true;

                }
            }
            return BCommand;
        }
        public static void ConnectServer()
        {
            try
            {
                IPEndPoint endPoint = new IPEndPoint(IpAdress, Port);
                Socket socket = new Socket(
                    AddressFamily.InterNetwork,
                    SocketType.Stream,
                    ProtocolType.Tcp);
                socket.Connect(endPoint);
                if (socket.Connected)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    string message = Console.ReadLine();
                    if (CheckCommand(message))
                    {
                        ViewModelSend viewModelSend = new ViewModelSend(message, Id);
                        if (message.Split(new string[1] { " " }, StringSplitOptions.None)[0] == "set")
                        {
                            string[] DataMessage = message.Split(new string[1] { " " }, StringSplitOptions.None);
                            string NameFile = "";
                            for (int i = 1; i < DataMessage.Length; i++)
                            {
                                if (NameFile == "")
                                    NameFile += DataMessage[i];
                                else
                                    NameFile += " " + DataMessage[i];
                            }
                            if (File.Exists(NameFile))
                            {
                                FileInfo FileInfo = new FileInfo(NameFile);
                                FileInfoFTP NewFileInfo = new FileInfoFTP(File.ReadAllBytes(NameFile), FileInfo.Name);
                                viewModelSend = new ViewModelSend(JsonConvert.SerializeObject(NewFileInfo), Id);
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Указанный файл не существует");
                            }
                        }
                        byte[] messageByte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(viewModelSend));
                        int BytesSend = socket.Send(messageByte);
                        byte[] bytes = new byte[10485760];
                        int BytesRec = socket.Receive(bytes);
                        string messageServer = Encoding.UTF8.GetString(bytes, 0, BytesRec);
                        ViewModelMessage viewModelMessage = JsonConvert.DeserializeObject<ViewModelMessage>(messageServer);
                        if (viewModelMessage.Command == "authorization")
                            Id = int.Parse(viewModelMessage.Date);
                        else if (viewModelMessage.Command == "message")
                            Console.WriteLine(viewModelMessage.Date);
                        else if (viewModelMessage.Command == "cd")
                        {
                            List<string> FoldersFiles = new List<string>();
                            FoldersFiles = JsonConvert.DeserializeObject<List<string>>(viewModelMessage.Date);
                            foreach (string Name in FoldersFiles)
                                Console.WriteLine(Name);
                        }
                        else if (viewModelMessage.Command == "file")
                        {
                            string[] DataMessage = viewModelSend.Message.Split(new string[1] { " " }, StringSplitOptions.None);
                            string getFile = "";
                            for (int i = 1; i < DataMessage.Length; i++)
                            {
                                if (getFile == "")
                                    getFile = DataMessage[i];
                                else
                                    getFile += " " + DataMessage[i];
                                byte[] byteFile = JsonConvert.DeserializeObject<byte[]>(viewModelMessage.Date);
                                File.WriteAllBytes(Directory.GetCurrentDirectory() + getFile, byteFile);
                            }
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Подключение не удалось.");
                    }
                    socket.Close();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Что-то случилось: {ex.Message}");
            }
        }
    }
}



