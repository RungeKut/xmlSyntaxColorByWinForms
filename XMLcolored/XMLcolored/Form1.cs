using ScintillaNET;
using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace XMLcolored
{
    public partial class Form1 : Form
    {
        private Scintilla scintilla;
        private string filePath;

        public Form1()
        {
            InitializeComponent();
            InitializeScintilla();
        }

        private void InitializeScintilla()
        {
            // Создаем и настраиваем Scintilla
            scintilla = new Scintilla();

            // Сброс всех стилей
            scintilla.StyleResetDefault();

            scintilla.LexerLanguage = "xml";
            scintilla.LexerName = "xml";

            // Настройка стилей для XML Lexer.SCLEX_XML
            scintilla.Styles[Style.Xml.Default].ForeColor = System.Drawing.Color.Black; // Обычный текст
            scintilla.Styles[Style.Xml.Tag].ForeColor = System.Drawing.Color.Blue; // Теги
            scintilla.Styles[Style.Xml.TagEnd].ForeColor = System.Drawing.Color.Blue; // Закрывающие теги
            scintilla.Styles[Style.Xml.Attribute].ForeColor = System.Drawing.Color.IndianRed; // Атрибуты
            scintilla.Styles[Style.Xml.Number].ForeColor = System.Drawing.Color.DarkGreen; // Числа
            scintilla.Styles[Style.Xml.DoubleString].ForeColor = System.Drawing.Color.DarkOrange; // Двойные кавычки
            scintilla.Styles[Style.Xml.SingleString].ForeColor = System.Drawing.Color.DarkOrange; // Одинарные кавычки
            scintilla.Styles[Style.Xml.Comment].ForeColor = System.Drawing.Color.Green; // Комментарии
            scintilla.Styles[Style.Xml.Entity].ForeColor = System.Drawing.Color.Purple; // Сущности (например, &amp;)
            scintilla.Styles[Style.Xml.CData].ForeColor = System.Drawing.Color.Gray; // CDATA-секции

            scintilla.Margins[0].Width = 32; // Ширина поля для номеров строк
            scintilla.ScrollWidth = 1000;  // Ширина прокрутки
            scintilla.LexerLanguage = scintilla.GetLexerIDFromLexer(Lexer.SCLEX_XML);

            // Настройка шрифта (опционально)
            scintilla.Styles[Style.Default].Font = "Consolas";
            scintilla.Styles[Style.Default].Size = 12;

            // Подключаем обработчик изменения текста
            scintilla.TextChanged += Scintilla_TextChanged;

            // Добавляем Scintilla на форму
            this.Controls.Add(scintilla);
            scintilla.Dock = DockStyle.Fill;

            LoadXmlContent();
        }

        private bool isValidationInProgress = false;

        private void Scintilla_TextChanged(object sender, EventArgs e)
        {
            UpdateLineNumbersWidth();
            // Если валидация уже выполняется, выходим
            if (isValidationInProgress)
                return;

            isValidationInProgress = true;

            try
            {
                // Запускаем асинхронную валидацию
                ValidateXml(scintilla.Text);
            }
            finally
            {
                isValidationInProgress = false;
            }
        }

        private void LoadXmlContent()
        {
            // Пример XML-содержимого
            string xmlContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<root>
    <element attribute=""value"">Text</element>
    <nested>
        <child>Content</child>
    </nested>
    <!-- Это комментарий -->
</root>";

            // Загружаем XML в Scintilla
            scintilla.Text = xmlContent;
        }

        private void ValidateXml(string xmlContent)
        {
            try
            {
                scintilla.MarkerDeleteAll(-1);
                // Пытаемся загрузить XML
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlContent);
            }
            catch (XmlException ex)
            {
                // Если XML невалиден, выделяем ошибку
                HighlightError(ex.LinePosition, ex.LineNumber);
            }
        }

        private void HighlightError(int linePosition, int lineNumber)
        {
            // Преобразуем номер строки и позицию в индекс символа
            int position = scintilla.Lines[lineNumber - 1].Position + linePosition - 1;

            // Устанавливаем маркер для выделения ошибки
            scintilla.Markers[0].Symbol = MarkerSymbol.Circle; // Тип маркера
            scintilla.Markers[0].SetBackColor(System.Drawing.Color.MediumVioletRed); // Цвет маркера
            //scintilla.Markers[0].SetForeColor(System.Drawing.Color.White); // Цвет текста маркера

            // Добавляем маркер на строку с ошибкой
            scintilla.Lines[lineNumber - 1].MarkerAdd(0);

            // Прокручиваем Scintilla до строки с ошибкой
            scintilla.GotoPosition(position);
            scintilla.ScrollCaret();
        }

        private void Open_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            //openFileDialog.InitialDirectory = this.text;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                filePath = openFileDialog.FileName;
                try
                {
                    // чтение из файла
                    using (FileStream fstream = File.OpenRead(filePath))
                    {
                        // выделяем массив для считывания данных из файла
                        byte[] buffer = new byte[fstream.Length];
                        // считываем данные
                        fstream.Read(buffer, 0, buffer.Length);
                        // декодируем байты в строку
                        string textFromFile = Encoding.Default.GetString(buffer);
                        scintilla.Text = textFromFile;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка чтения файла!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void UpdateLineNumbersWidth()
        {
            int digit = 1;
            int lineCount = scintilla.Lines.Count;
            while (lineCount >= 10)
            {
                lineCount /= 10;
                digit++;
            }
            scintilla.Margins[0].Width = digit * scintilla.Styles[Style.Default].Size;
        }

        private void Save_Click(object sender, EventArgs e)
        {
            try
            {
                using (FileStream fstream = new FileStream(filePath, FileMode.OpenOrCreate))
                {
                    // преобразуем строку в байты
                    byte[] input = Encoding.Default.GetBytes(scintilla.Text);
                    // запись массива байтов в файл
                    fstream.Write(input, 0, input.Length);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка сохранения файла!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string findFileMess = null;
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false)) //Если дропаются файлы то
            {
                bool allowFilesDrop;
                //Извлекаем пути перетаскиваемых файлов
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, true);
                int fileQuantity = files.Length;
                string[] filePaths = new string[fileQuantity];

                for (int i = 0; i < fileQuantity; i++)
                {
                    //Проверяем того ли они расширения
                    string fileExtension = (new System.IO.FileInfo(files[i])).Extension;
                    allowFilesDrop = (fileExtension == ".xml") |
                                     (fileExtension == ".Xml") |
                                     (fileExtension == ".XML");
                    if (allowFilesDrop)
                    {
                        filePaths[i] = files[i];
                    }
                    else
                    {
                        filePaths[i] = null;
                        findFileMess += System.IO.Path.GetFileName(files[i]) + "\n";
                    }
                }
                if (filePaths.Length > 0)
                {
                    e.Effect = DragDropEffects.All;
                    try
                    {
                        scintilla.Text = XElement.Load(filePaths[0]).ToString();
                        filePath = filePaths[0];
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Ошибка чтения Drag&Drop файла!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    e.Effect = DragDropEffects.None;
                    MessageBox.Show("Поддерживаются только xml файлы!", "Формат не поддерживается!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            if (findFileMess != null)
            {
                MessageBox.Show("Файлы:\n" + findFileMess + "имеют неподдерживаемый формат!", "Эти файлы не поддерживаются!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                findFileMess = null;
            }
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false)) //Если дропаются файлы то
            {
                //Тут можно прописать условия при которых будет меняться курсор мыши и выполняться что-то в зависимости от дропаемых вещей до отпускания кнопки мыши
                e.Effect = DragDropEffects.All;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Обработка дропа файлов на ярлык приложения, "открыть с помощью" и поддержка ассоциации фалов с приложением
            string findFileMess = null;
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length >= 2)
            {
                bool allowFilesDrop = false;
                //Удалим первый аргумент командной строки (путь к нашей программе)
                string[] filePaths = new string[args.Length - 1];
                for (int i = 0; i < args.Length - 1; i++)
                {
                    //За одно проверим расширения файлов
                    string fileExtension = (new System.IO.FileInfo(args[i + 1])).Extension;
                    allowFilesDrop = (fileExtension == ".xml") |
                                     (fileExtension == ".Xml") |
                                     (fileExtension == ".XML");
                    if (allowFilesDrop)
                    {
                        filePaths[i] = args[i + 1];
                    }
                    else
                    {
                        filePaths[i] = null;
                        findFileMess += System.IO.Path.GetFileName(args[i + 1]) + "\n";
                    }
                }
                try
                {
                    scintilla.Text = XElement.Load(filePaths[0]).ToString();
                    filePath = filePaths[0];
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка чтения файла!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            if (findFileMess != null)
            {
                MessageBox.Show("Файлы:\n" + findFileMess + "имеют неподдерживаемый формат!", "Эти файлы не поддерживаются!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                findFileMess = null;
            }
        }
    }
}
