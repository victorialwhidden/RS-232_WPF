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
using System.IO;
using System.Threading;
using System.IO.Ports;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RS_232_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    
    public partial class MainWindow : Window
    {
        private SerialPort mySP;
        private string msgRecieved;
        private string msgSent;

        //Declare delegate for updating listbox
        public delegate void msgDelegate(string msg);

        public MainWindow()
        {
            //Autopopulate COM lines
            InitializeComponent();
            foreach (string s in SerialPort.GetPortNames())
            {
                cbx_ComPorts.Items.Add(s);
            }
            cbx_ComPorts.SelectedIndex = 0;

            //SetUp SP
            mySP = new SerialPort();

            //Mannual setup
            mySP.PortName = cbx_ComPorts.SelectedItem.ToString();
            mySP.BaudRate = 9600;
            mySP.Parity = Parity.Odd;
            mySP.DataBits = 7;
            mySP.StopBits = StopBits.One;

            //Read and write timeouts
            mySP.ReadTimeout = 500;
            mySP.WriteTimeout = 500;

        }

        private void cbx_ComPorts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {   if (cbx_ComPorts.Items.Count > 0)
            {
                mySP.PortName = cbx_ComPorts.SelectedItem.ToString();
            }
        }

        private void mySP_DataRecieved(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                msgRecieved = mySP.ReadLine();

                //using the delegate beginInvoke constructor
                Dispatcher.BeginInvoke(new msgDelegate(outputMessage), msgRecieved);



            }
            catch (IOException ex) 
            {
                MessageBox.Show("Error reading from serial port", ex.Message);
            }
        }

        private void outputMessage(string msg)
        {
            lstB_Messages.Items.Add(msg);
            //Ensure the listBox stays up to date with new items
            lstB_Messages.SelectedItem = lstB_Messages.Items.Count - 1;
        }


        private void btn_Clear_Click(object sender, RoutedEventArgs e)
        {
            lstB_Messages.Items.Clear();
        }

        private void btn_Send_Click(object sender, RoutedEventArgs e)
        {
            msgSent = txtB_Message.Text.ToString();
            try
            {
                if (mySP.IsOpen)
                {
                    //Send text message out and clear
                    mySP.WriteLine(msgSent);
                    txtB_Message.Clear();
                }
            }
            catch (IOException ex)
            {
                MessageBox.Show("There was an issue Sending the message", ex.Message);
            }
        }

        private void MainWindow_Closing(object sender, EventArgs e)
        {
            if (mySP.IsOpen)
            {
                mySP.Close();
            }
            if (mySP != null)
            {
                mySP.Dispose();
            }
        }
        private void txtB_Message_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void btn_OpenSP_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!mySP.IsOpen)
                {
                    mySP.Open();

                    //Event handler Delegate
                    //DataReceived = event delegate
                    //+= operator = mechanism by which classes can register their event handlers with an event
                    //new = new event handler delegate type called SerialDataReceivedEventHandler
                    //mySerialPort_DataReceived = event handler method 
                    mySP.DataReceived += new SerialDataReceivedEventHandler(mySP_DataRecieved);

                    btn_OpenSP.IsEnabled = false;
                    btn_CloseSP.IsEnabled = true;
                }
            }
            catch (IOException ex) { MessageBox.Show("Error opening port", ex.Message); }
        }

        private void btn_CloseSP_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (mySP.IsOpen)
                {

                    mySP.Close();
                    Thread.Sleep(1000);
                    btn_OpenSP.IsEnabled = true;
                    btn_CloseSP.IsEnabled = false;
                }

                if (mySP != null)
                {
                    mySP.Dispose();
                }
            }
            catch (IOException ex)
            {
                MessageBox.Show("Error closing serial port", ex.Message);
            }
        }
    }
}
