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
using System.Windows.Shapes;

namespace GuiExample.Helpers
{
    /// <summary>
    /// https://stackoverflow.com/questions/2796470/wpf-create-a-dialog-prompt
    /// Answered by Pythonizo
    /// </summary>
    public partial class PromptDialog : Window
    {
        public PromptDialog(string question, string title, string defaultValue = "")
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(PromptDialog_Loaded);
            txtQuestion.Text = question;
            Title = title;
            txtResponse.Text = defaultValue;
        }

        void PromptDialog_Loaded(object sender, RoutedEventArgs e)
        {
            txtResponse.Focus();
        }

        public static string Prompt(string question, string title, string defaultValue = "")
        {
            PromptDialog inst = new PromptDialog(question, title, defaultValue);
            inst.ShowDialog();
            if (inst.DialogResult == true)
                return inst.ResponseText;
            return null;
        }

        public static T Prompt<T>(string question, string title, string defaultValue = "")
        {
            var result = Prompt(question,title, defaultValue);

            try
            {
                T _result = (T)Convert.ChangeType(result, typeof(T));
                return _result;
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        public string ResponseText
        {
            get
            {
                return txtResponse.Text;
            }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(ResponseText))
            {
                DialogResult = true;
                Close();
            }
            else
            {
                    txtResponse.Focus();
            }
        }
    }
}
