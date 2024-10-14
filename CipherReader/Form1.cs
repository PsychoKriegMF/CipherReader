using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CipherReader
{
    public partial class Form1 : Form
    {
        private string saveFilePath;


        public Form1()
        {
            InitializeComponent();
        }

        private List<string> GetSelectedFiles()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                return openFileDialog.FileNames.ToList();
            }
            return new List<string>();
        }

        private void btnFileOpen_Click(object sender, EventArgs e)
        {
            GetSelectedFiles();
        }
        public static byte[] CaesarCipher(byte[] data, int shift)
        {
            byte[] result = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                result[i] = (byte)((data[i] + shift) % 256); // Шифрование с побайтовым сдвигом
            }
            return result;
        }

        private async Task EncryptFilesAsync(IEnumerable<string> files, int shift, bool deleteOriginal)
        {
            foreach (var filePath in files)
            {
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    // Получаем текущее расширение исходного файла
                    string fileExtension = Path.GetExtension(filePath);

                    // Устанавливаем фильтр для сохранения того же типа файлов, что и исходный файл
                    saveFileDialog.Filter = $"Files (*{fileExtension})|*{fileExtension}|All Files (*.*)|*.*";
                    saveFileDialog.Title = "Сохранить зашифрованный файл";
                    saveFileDialog.FileName = $"encrypted_{Path.GetFileName(filePath)}"; // Имя по умолчанию

                    // Открываем диалог для выбора пути
                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string saveFilePath = saveFileDialog.FileName;

                        // Выполняем шифрование в асинхронном режиме
                        await Task.Run(() => EncryptFile(filePath, saveFilePath, shift, deleteOriginal));
                    }
                }
            }

            MessageBox.Show("Шифрование завершено!");
        }

        private void EncryptFile(string filePath, string saveFilePath, int shift, bool deleteOriginal)
        {
            try
            {
                // Чтение файла в бинарном формате
                byte[] fileContent = File.ReadAllBytes(filePath);
                if (fileContent.Length == 0)
                {
                    MessageBox.Show($"Файл {filePath} пуст или не найден.");
                    return;
                }

                // Шифрование
                byte[] encryptedContent = CaesarCipher(fileContent, shift);

                // Запись зашифрованного файла
                File.WriteAllBytes(saveFilePath, encryptedContent);
                MessageBox.Show($"Файл успешно зашифрован и сохранен: {saveFilePath}");

                // Удаление исходного файла, если это было выбрано
                if (deleteOriginal)
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при шифровании файла: {ex.Message}");
            }
        }






        private void EnsureEncryptedFolderExists()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "Зашифровать");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private async void btnEncrypt_Click(object sender, EventArgs e)
        {
            var files = GetSelectedFiles(); // Получить выбранные файлы
            int shift = int.Parse(tbShift.Text); // Получить сдвиг шифра
            bool deleteOriginal = DeleteFile.Checked; // Удалить ли оригинал

            // Обрабатываем каждый файл
            foreach (var file in files)
            {
                await Task.Run(() => EncryptFile(file, shift, DeleteFile)); // Асинхронная обработка файлов
            }

            MessageBox.Show("Шифрование завершено!");
        }

        




        private async void btnDecrypt_Click(object sender, EventArgs e)
        {
            var files = GetSelectedFiles();
            int shift = -int.Parse(tbShift.Text);
            bool deleteOriginal = DeleteFile.Checked;

            await EncryptFilesAsync(files, shift, deleteOriginal);
            MessageBox.Show("Расшифровка завершена!");
        }

        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            GetSelectedFiles();
        }
    }
}
