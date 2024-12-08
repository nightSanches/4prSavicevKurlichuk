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
        public static IPAddress IPAddress;
        public static int Port;
        public static int Id = -1;
        static void Main(string[] args)
        {

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
                        Console.WriteLine("Использование: connect [login] [password]\nПример: connect User1 P@ssword");
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
                // Создаём конечную точку, состоящую из IP-адреса и порта
                IPEndPoint endPoint = new IPEndPoint(IPAddress, Port);
                // Создаём сокет для подключения
                Socket socket = new Socket(
                // Задаём схему адресации, которую будет использовать сокет ІРѵ4
                AddressFamily.InterNetwork,
                // Задаём тип сокета, двоичный код
                SocketType.Stream,
                // Указываем протокол сокета
                ProtocolType.Tcp);
                // Подключаемся к серверу
                socket.Connect(endPoint);
                // Если состояние подключено
                if (socket.Connected)
                {
                    // Изменяем цвет текста в консоле
                    Console.ForegroundColor = ConsoleColor.White;
                    // Ждём сообщения от пользователя
                    string message = Console.ReadLine();
                    // Проверяем сообщение пользователя на соответствие команд
                    if (CheckCommand(message))
                    {
                        // Создаём модель, в которой хранится сообщние пользователя, и ID пользователя
                        ViewModelSend viewModelSend = new ViewModelSend(message, Id);
                        // Если команда set
                        if (message.Split(new string[1] { " " }, StringSplitOptions.None)[0] == "set")
                        {
                            // Разбиваем сообщение на данные
                            string[] DataMessage = message.Split(new string[1] { " " }, StringSplitOptions.None);
                            // Собираем наименование файла, аналагично сервера
                            string NameFile = "";
                            for (int i = 1; i < DataMessage.Length; i++)
                                if (NameFile == "")
                                    NameFile += DataMessage[i];
                                else
                                    NameFile += " " + DataMessage[i];
                            // проверяем существование файла
                            if (File.Exists(NameFile))
                            {
                                // Получаем информаццию о файле
                                FileInfo FileInfo = new FileInfo(NameFile);
                                // Создаём объект состоящий из наименования файл и массива байт
                                FileInfoFTP NewFileInfo = new FileInfoFTP(File.ReadAllBytes(NameFile), FileInfo.Name);
                                // Создаём модель, состающую из данных о файле и коде пользователя
                                viewModelSend = new ViewModelSend(JsonConvert.SerializeObject(NewFileInfo), Id);
                            }
                            else
                            {
                                // Изменяем цвет текста в консоле
                                Console.ForegroundColor = ConsoleColor.Red;
                                // Выводим текст
                                Console.WriteLine("Указанный файл не существует");
                            }
                        }
                        // Разбиваем сообщения на массив байт, предварительно преобразов в Json
                        byte[] messageByte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(viewModelSend));
                        // Отправляем сообщение
                        int BytesSend = socket.Send(messageByte);
                        // Создаём массив байт
                        byte[] bytes = new byte[10485760];
                        // Получаем ответ от сервера
                        int BytesRec = socket.Receive(bytes);
                        // Преобразовываем ответ в строку
                        string messageServer = Encoding.UTF8.GetString(bytes, 0, BytesRec);
                        // Преобразовываем Json в объект
                        ViewModelMessage viewModelMessage = JsonConvert.DeserializeObject<ViewModelMessage>(messageServer);
                        // Если метод авторизации
                        if (viewModelMessage.Command == "autorization")
                            // Запоминаем код пользоватля
                            Id = int.Parse(viewModelMessage.Date);
                        // Если сообщение
                        else if (viewModelMessage.Command == "message")
                            // Выводи на экран
                            Console.WriteLine(viewModelMessage.Date);

                        // Если команда перехода в категорию
                        else if (viewModelMessage.Command == "cd")
                        {
                            // Создаём список
                            List<string> FoldersFiles = new List<string>();
                            // Преобразовываем список пришедший с сервера в список
                            FoldersFiles = JsonConvert.DeserializeObject<List<string>>(viewModelMessage.Date);
                            // Выводим папки и файлы
                            foreach (string Name in FoldersFiles)
                                Console.WriteLine(Name);
                        }

                        // Если команда файл
                        else if (viewModelMessage.Command == "file")
                        {
                            // Получаем имя файла из команды пользователя
                            string[] DataMessage = viewModelSend.Message.Split(new string[1] { " " }, StringSplitOptions.None);
                            // Собираем наименование файла, аналагично сервера
                            string getFile = "";
                            for (int i = 1; i < DataMessage.Length; i++)
                                if (getFile == "")
                                    getFile = DataMessage[i];
                                else
                                    getFile += " " + DataMessage[i];
                            // Преобразовываем набор байт в массив из Json
                            byte[] byteFile = JsonConvert.DeserializeObject<byte[]>(viewModelMessage.Date);
                            // Сохраняем файл на клиенте
                            File.WriteAllBytes(getFile, byteFile);
                        }
                    }
                }
                else
                {
                    // Изменяем цвет текста в консоле
                    Console.ForegroundColor = ConsoleColor.Red;
                    // Выводим текст
                    Console.WriteLine("Подключение не удалось.");
                }
                socket.Close();
            }
            catch (Exception exp)
            {
                // Изменяем цвет текста в консоле
                Console.ForegroundColor = ConsoleColor.Red;
                // Выводим текст
                Console.WriteLine($"Что-то случилось: + {exp.Message}");
            }
        }
    }
}



