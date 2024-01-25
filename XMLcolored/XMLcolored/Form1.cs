using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace XMLcolored
{
    public partial class Form1 : Form
    {
        #region xml
        private string text = "<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<AppSettings xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\r\n";
        private string path;
        #endregion
        public Form1()
        {
            InitializeComponent();
            xmlRichTextBox.Text = text;
            FormatXML(this.xmlRichTextBox);
        }

        private void Open_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = this.text;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                this.xmlRichTextBox.SuspendLayout();
                try
                {
                    xmlRichTextBox.Text = XElement.Load(openFileDialog.FileName).ToString();
                    this.xmlRichTextBox.Text = XElement.Parse(this.xmlRichTextBox.Text).ToString();
                    path = openFileDialog.FileName;
                    FormatXML(this.xmlRichTextBox);
                }
                catch (Exception) { }
                this.xmlRichTextBox.ResumeLayout();
            }
        }

        public static void FormatXML(RichTextBox rtb)
        {
            rtb.SuspendLayout();
            try
            {
                Color colorEquality = Color.FromArgb(255, 200, 200, 200);
                Color colorQuotationMarks = Color.FromArgb(255, 215, 150, 120);
                Color colorText = Color.FromArgb(255, 215, 150, 120);
                Color colorAttribute = Color.FromArgb(255, 156, 220, 255);
                Color colorSymbol = Color.FromArgb(255, 128, 128, 100);
                Color colorElement = Color.FromArgb(255, 86, 156, 215);

                int nextIndex = 0;
                int endIndex = 0;
                while (rtb.Find("=", nextIndex, RichTextBoxFinds.MatchCase) >= 0 && nextIndex < rtb.TextLength)
                {
                    rtb.SelectionColor = colorEquality;
                    nextIndex = rtb.SelectionStart + rtb.SelectionLength;
                    rtb.Find("\"", nextIndex, RichTextBoxFinds.MatchCase);
                    rtb.SelectionColor = colorQuotationMarks;
                    int textStartIndex = rtb.SelectionStart + rtb.SelectionLength;
                    rtb.Find("\"", textStartIndex, RichTextBoxFinds.MatchCase);
                    rtb.SelectionColor = colorQuotationMarks;
                    endIndex = rtb.SelectionStart;
                    rtb.SelectionStart = textStartIndex;
                    rtb.SelectionLength = endIndex - textStartIndex;
                    rtb.SelectionColor = colorText;
                    rtb.Find(" ", 0, nextIndex, RichTextBoxFinds.Reverse);
                    rtb.SelectionStart = rtb.SelectionStart + rtb.SelectionLength;
                    rtb.SelectionLength = nextIndex - 1 - rtb.SelectionStart;
                    rtb.SelectionColor = colorAttribute;
                }
                nextIndex = 0;
                endIndex = 0;
                while (rtb.Find("<", nextIndex, RichTextBoxFinds.None) >= 0 && nextIndex < rtb.TextLength)
                {
                    rtb.SelectionColor = colorSymbol;
                    nextIndex = rtb.SelectionStart + rtb.SelectionLength;
                    endIndex = nextIndex;
                    while (rtb.Text[endIndex] != '?' && rtb.Text[endIndex] != '/' && rtb.Text[endIndex] != ' ' && rtb.Text[endIndex] != '>')
                    {
                        endIndex++;
                    }
                    switch (rtb.Text[endIndex])
                    {
                        case '?':
                            rtb.SelectionLength++;
                            rtb.SelectionColor = colorSymbol;
                            nextIndex++;
                            rtb.SelectionStart = nextIndex;
                            char[] find2 = { '?', ' ' };
                            rtb.Find(find2, nextIndex, rtb.TextLength);
                            endIndex = nextIndex;
                            while (rtb.Text[endIndex] != '?' && rtb.Text[endIndex] != ' ')
                            {
                                endIndex++;
                            }
                            rtb.SelectionLength = endIndex - nextIndex;
                            rtb.SelectionColor = colorElement;
                            rtb.Find("?>", endIndex, RichTextBoxFinds.MatchCase);
                            rtb.SelectionColor = colorSymbol;
                            nextIndex = rtb.SelectionStart + rtb.SelectionLength;
                            break;
                        case '/':
                            rtb.SelectionLength++;
                            rtb.SelectionColor = colorSymbol;
                            nextIndex++;
                            int index = rtb.Find(">", nextIndex, RichTextBoxFinds.MatchCase);
                            rtb.SelectionColor = colorSymbol;
                            rtb.SelectionStart = nextIndex;
                            rtb.SelectionLength = index - nextIndex;
                            rtb.SelectionColor = colorElement;
                            nextIndex = index;
                            break;
                        case ' ':
                            rtb.SelectionLength = endIndex - nextIndex;
                            rtb.SelectionStart = nextIndex;
                            rtb.SelectionColor = colorElement;
                            rtb.Find(">", nextIndex, RichTextBoxFinds.MatchCase);
                            rtb.SelectionColor = colorSymbol;
                            nextIndex = rtb.SelectionStart + rtb.SelectionLength;
                            break;
                        default:
                            endIndex = nextIndex;
                            while (rtb.Text[endIndex] != '>' && rtb.Text[endIndex] != ' ')
                            {
                                endIndex++;
                            }
                            rtb.SelectionStart = nextIndex;
                            rtb.SelectionLength = endIndex - nextIndex;
                            rtb.SelectionColor = colorElement;
                            nextIndex = rtb.SelectionStart + rtb.SelectionLength;
                            break;
                    }
                }
            }
            catch (Exception ex) { }
            rtb.ResumeLayout(false);
        }

        private void Save_Click(object sender, EventArgs e)
        {
            try
            {
                XDocument xDocument = new XDocument(new XElement(XElement.Parse(this.xmlRichTextBox.Text)));
                xDocument.Save(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    //LoadDropFile(filePaths);
                    this.xmlRichTextBox.SuspendLayout();
                    try
                    {
                        xmlRichTextBox.Text = XElement.Load(filePaths[0]).ToString();
                        this.xmlRichTextBox.Text = XElement.Parse(this.xmlRichTextBox.Text).ToString();
                        path = filePaths[0];
                        FormatXML(this.xmlRichTextBox);
                    }
                    catch (Exception) { }
                    this.xmlRichTextBox.ResumeLayout();
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
                //LoadDropFile(filePaths);
                this.xmlRichTextBox.SuspendLayout();
                try
                {
                    xmlRichTextBox.Text = XElement.Load(filePaths[0]).ToString();
                    this.xmlRichTextBox.Text = XElement.Parse(this.xmlRichTextBox.Text).ToString();
                    path = filePaths[0];
                    FormatXML(this.xmlRichTextBox);
                }
                catch (Exception) { }
                this.xmlRichTextBox.ResumeLayout();
            }
            if (findFileMess != null)
            {
                MessageBox.Show("Файлы:\n" + findFileMess + "имеют неподдерживаемый формат!", "Эти файлы не поддерживаются!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                findFileMess = null;
            }
        }
    }
}
