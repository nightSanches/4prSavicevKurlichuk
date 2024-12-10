using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

namespace ClientWPF.Pages.DirectoryPage.Item
{
    /// <summary>
    /// Логика взаимодействия для Item.xaml
    /// </summary>
    public partial class Item : UserControl
    {
        public string DirectoryPath { get; private set; }

        public Item(string directoryPath, bool isDirectory)
        {
            InitializeComponent();
            DirectoryPath = directoryPath;
            if (isDirectory)
            {
                txtDirectory.Text = Path.GetFileName(directoryPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            }
            else
            {
                txtDirectory.Text = Path.GetFileName(directoryPath);
            }

            if (isDirectory)
            {
                imgIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Images/directory.png")); // Путь к иконке папки
            }
            else
            {
                imgIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Images/file.png")); // Путь к иконке файла
            }
            if (directoryPath == "Назад" && isDirectory == false)
            {
                imgIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Images/arrow.png"));
            }
        }

        private void ucMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string selectedItem = DirectoryPath;

            if (selectedItem == "Назад")
            {
                if (MainWindow.init.directoryStack.Count > 0)
                {
                    MainWindow.init.directoryStack.Pop();
                    DirectoryPage.init.LoadDirectories();
                }
            }
            else if (selectedItem.EndsWith("\\"))
            {
                MainWindow.init.directoryStack.Push(selectedItem);
                var response = MainWindow.init.SendCommand($"cd {selectedItem.TrimEnd('/')}");

                if (response?.Command == "cd")
                {
                    var items = JsonConvert.DeserializeObject<List<string>>(response.Date);
                    DirectoryPage.init.stackPanelDirectories.Children.Clear();
                    var directoryItem = new Item("Назад", false);
                    DirectoryPage.init.stackPanelDirectories.Children.Add(directoryItem);
                    foreach (var item in items)
                    {
                        bool isDirectory = Directory.Exists(item);
                        var Item = new Item(item, isDirectory);
                        DirectoryPage.init.stackPanelDirectories.Children.Add(Item);
                    }
                }
                else
                {
                    MessageBox.Show($"Ошибка: {response?.Date}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                DirectoryPage.init.DownloadFile(selectedItem);
            }
        }

        private void ucMouseEnter(object sender, MouseEventArgs e)
        {
            border.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#90828282"));
        }

        private void ucMouseLeave(object sender, MouseEventArgs e)
        {
            border.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#99000000"));
        }
    }
}
