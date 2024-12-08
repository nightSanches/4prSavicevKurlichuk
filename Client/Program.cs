using System;
using System.Net;

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






    }
}
                    

