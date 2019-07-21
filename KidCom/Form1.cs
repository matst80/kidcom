using System;
using System.Windows.Forms;

namespace KidCom
{
    public partial class Form1 : Form
    {


        public Form1()
        {
            InitializeComponent();


            var messageHandler = new MessageHandler((message) =>
            {
                Invoke((Action)(() =>
                {
                    label1.Text = message;
                    WindowState = FormWindowState.Maximized;
                    Visible = true;
                    TopMost = true;
                }));
            }, () =>
            {
                Invoke((Action)(() =>
                {
                    Visible = false;
                    TopMost = false;
                }));
            });

            var rnd = new Random();
            var backgroundHandler = new BackgroundMessageHandler();
            backgroundHandler.Start(new MessageConfig()
            {
                CilentId = "kid"+rnd.Next(1000).ToString()
            }, messageHandler);

           
            WindowState = FormWindowState.Minimized;
        }
    }
}
