using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;


namespace IMD2txt
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>


    public static class Converter
    {
        public static byte To1(this byte[] b) => b[0];

        public static short To2(this byte[] b) =>
            BitConverter.ToInt16(b, 0);

        public static int To4(this byte[] b) =>
            BitConverter.ToInt32(b, 0);

        public static long To8(this byte[] b) =>
            BitConverter.ToInt64(b, 0);

    }

    public partial class MainWindow : Window
    {
        public static byte[] ReadFile(ref FileStream file,int count)
        {
            byte[] B=new byte[8];
            file.Read(B, 0, count);
            return B;
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Filter = "imd谱面 (*.imd)|*.imd",
                Multiselect = true,
                InitialDirectory = @"/",
            };
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                foreach (string fileName in ofd.FileNames)
                {
                    bool hasSame = false;
                    foreach (string str in listBox.Items)
                        if (fileName==str)
                        {
                            hasSame = true;
                            break;
                        }
                    if (!hasSame)
                        listBox.Items.Add(fileName);
                }
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            string text;
            listBox2.Items.Clear();
            foreach (string fullFileName in listBox.Items)
            {
                text = IMD2txt(fullFileName);
                string fileName = System.IO.Path.GetFileNameWithoutExtension(fullFileName),
                    txtFileName;
                if (isFormatted(fileName))
                {
                    byte diff = 0;
                    switch (fileName.Substring(fileName.LastIndexOf('_')+1).ToLower())
                    {
                        case "ez":
                            diff = 1;
                            break;
                        case "nm":
                            diff = 2;
                            break;
                        case "hd":
                            diff = 3;
                            break;
                    }
                    txtFileName = System.IO.Path.GetDirectoryName(fullFileName) + @"\" +
                        "12" + diff + fileName.Substring(0, fileName.IndexOf('_'))
                        + ".txt";
                }
                else
                {
                    txtFileName = System.IO.Path.GetDirectoryName(fullFileName) + @"\" +
                        fileName + ".txt";
                }
                saveToTxt(txtFileName,text);
                listBox2.Items.Add(txtFileName);
                System.Windows.Forms.Application.DoEvents();
            }
        }

        private void saveToTxt(string txtFileName, string text)
        {
            StreamWriter sw = new StreamWriter(txtFileName, false);
            foreach (string str in text.Split('\n'))
            {
                if (str != "")
                    sw.WriteLine(str);
            }
            sw.Close();
        }

        private bool isFormatted(string fileName)
        {
            return Regex.IsMatch(fileName.ToLower(), @"^\d+_[456]k_(ez|nm|hd)$");
        }

        string IMD2txt(string imdFileName)
        {
            string text = "";
            FileStream imdFile = new FileStream(imdFileName, FileMode.Open);

            imdFile.Seek(4, SeekOrigin.Begin);
            int bpmLineCount = ReadFile(ref imdFile, 4).To4();
            imdFile.Seek(12 * bpmLineCount + 2, SeekOrigin.Current);
            int noteLineCount = ReadFile(ref imdFile, 4).To4(); 
            for (int i = 0; i < noteLineCount; i++)
            {
                short noteType = ReadFile(ref imdFile, 2).To2();
                switch ((int)(noteType / 0x10))
                {
                    case 0:
                        text += "9 ";
                        break;
                    case 6:
                        text += "0 ";
                        break;
                    case 2:
                        text += "1 ";
                        break;
                    case 10:
                        text += "2 ";
                        break;
                }
                text += (noteType % 0x10) + " ";
                text += ReadFile(ref imdFile, 4).To4() + " ";   //按键时间
                text += ReadFile(ref imdFile, 1).To1() + " ";   //按键轨道
                text += ReadFile(ref imdFile, 4).To4() + "\n";  //按键参数
            }
            imdFile.Close();
            return text;
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            while (listBox.SelectedItem != null)
                listBox.Items.Remove(listBox.SelectedItem);
        }
       

        private void Window_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void ListBoxItem_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start((string)((ListBoxItem)sender).Content);
        }
    }
}
