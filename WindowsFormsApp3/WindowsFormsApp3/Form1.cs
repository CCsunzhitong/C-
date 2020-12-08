using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Management.Instrumentation;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp3
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }
        private Socket client;
        private String[] STR;
        /*使用tcp/ip协议读取plc寄存器数据
         */
        private  void Button2_Click(object sender, EventArgs e)
        {
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ip = IPAddress.Parse(textBox1.Text);
            IPEndPoint endPoint = new IPEndPoint(ip, int.Parse(textBox2.Text));
            try
            {
                byte[] cmd;             
                 client.Connect(endPoint);
                 cmd= CreateCommand("R", int.Parse(this.textBox3.Text), int.Parse(this.textBox4.Text));
                client.Send(cmd);
               /* string msg = ByteToString(cmd);*/
               // listBox2.Items.Add(@"发送数据"+msg);
                
            }
            catch (Exception)
            {
                MessageBox.Show("地址或端口错误！！！！");
                return;              
            }
            Thread thread = new Thread(ReciveMsg);       
            thread.IsBackground = true;
            thread.Start(client);
        }
        /*
         *使用tcp/ip协议写入plc寄存器数据 
         */
        private void Button3_Click(object sender, EventArgs e)
        {
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ip = IPAddress.Parse(textBox1.Text);
            IPEndPoint endPoint = new IPEndPoint(ip, int.Parse(textBox2.Text));
            try
            {
                byte[] cmd;
                client.Connect(endPoint);
                cmd = CreateCommand("W", int.Parse(this.textBox3.Text), int.Parse(this.textBox4.Text));
                client.Send(cmd);
                string msg = ByteToString(cmd);
                listBox2.Items.Add(@"发送数据" + msg);

            }
            catch (Exception)
            {
                MessageBox.Show("地址或端口错误！！！！");
                return;
            }
            Thread thread = new Thread(ReciveMsg);
            thread.IsBackground = true;
            thread.Start(client);
        }
        /*
         *服务器接收数据
         * 
         */
        private void ReciveMsg(object o)
        {
           Socket client = o as Socket;
            while (true)
            {
                try
                {
                    byte[] arrList = new byte[client.Available];
                    ///接收到的信息大小(所占字节数)
                     client.Receive(arrList);
                    int i = 0;
                    string msg = ByteToString(arrList);
                    if (!msg.Equals("")&&msg.Length!=0) {

                        STR[i] = ByteToAll("S", arrList, int.Parse(this.textBox4.Text));
                        i++;
                       

                        /*string value = Encoding.Default.GetString(arrList);*/
                       // listBox2.Items.Add(@"接收数据" + msg);
                      
                    }
                }
                 catch (Exception)
                {
                                        ///关闭客户端
                    client.Close();
                }
                      
            
            }
      
        }
        /*
         * 根据对应的条件生成报文
         * 输入  type:报文的类型，有W（write）和R（read)
         *       dreg:寄存器起始地址。
         *       data：数据长度。
         * 输出类型 byte：对应的16进制报文         
         */
        private byte[] CreateCommand(string type, int dreg, int data)
        {
            byte[] sendBuf;
            switch (type)
            {
                case "R":
                    {
                        sendBuf = new byte[21];

                        //副标题
                        sendBuf[0] = 0x50;
                        sendBuf[1] = 0x00;
                        //网络号
                        sendBuf[2] = 0x00;
                        //PLC号
                        sendBuf[3] = 0xFF;
                        //IO
                        sendBuf[4] = 0xFF;
                        sendBuf[5] = 0x03;
                        //站号
                        sendBuf[6] = 0x00;
                        //字节数
                        sendBuf[7] = 0x0c;
                        sendBuf[8] = 0x00;
                        //等待长度
                        sendBuf[9] = 0x0A;
                        sendBuf[10] = 0x00;
                        //命令
                        sendBuf[11] = 0x01;
                        sendBuf[12] = 0x04;
                        //子命令
                        sendBuf[13] = 0x00;
                        sendBuf[14] = 0x00;
                        //起始寄存器地址
                        sendBuf[15] = (byte)(dreg & 0xff);
                        sendBuf[16] = (byte)((dreg & 0xff00) >> 8);
                        sendBuf[17] = (byte)((dreg & 0xff0000) >> 16);
                        if (this.comboBox1.Text.Equals("R"))
                        {
                            //寄存器类型-R类
                            sendBuf[18] = 0xAF;
                        }
                        if (this.comboBox1.Text.Equals("D"))
                        {
                            //寄存器类型-D类
                            sendBuf[18] = 0xA8;
                        }
                        if (this.comboBox1.Text.Equals("M"))
                        {
                            //寄存器类型-D类
                            sendBuf[18] = 0x90;
                        }

                        //寄存器连续长度
                        sendBuf[19] = (byte)(data & 0xff);
                        sendBuf[20] = (byte)((data & 0xff00) >> 8);
                        return sendBuf;
                    }
                case "W":
                    {
                        int length = 12 + data * 2;
                        sendBuf = new byte[21+data*2];
                        //副标题
                        sendBuf[0] = 0x50;
                        sendBuf[1] = 0x00;
                        //网络号
                        sendBuf[2] = 0x00;
                        //PLC号
                        sendBuf[3] = 0xFF;
                        //IO
                        sendBuf[4] = 0xFF;
                        sendBuf[5] = 0x03;
                        //站号
                        sendBuf[6] = 0x00;
                        //字节数
                        
                        sendBuf[7] = (byte)(length & 0xff); ;
                        sendBuf[8] = (byte)(length & 0xff >> 8); ;
                        //等待长度
                        sendBuf[9] = 0x0A;
                        sendBuf[10] = 0x00;
                        //命令
                        sendBuf[11] = 0x01;
                        sendBuf[12] = 0x14;
                        //子命令
                        sendBuf[13] = 0x00;
                        sendBuf[14] = 0x00;
                        //起始寄存器地址
                        sendBuf[15] = (byte)(dreg & 0xff);
                        sendBuf[16] = (byte)((dreg & 0xff00) >> 8);
                        sendBuf[17] = (byte)((dreg & 0xff0000) >> 16);
                        if (this.comboBox1.Text.Equals("R"))
                        {
                            //寄存器类型-R类
                            sendBuf[18] = 0xAF;
                        }
                        if (this.comboBox1.Text.Equals("D"))
                        {
                            //寄存器类型-D类
                            sendBuf[18] = 0xA8;
                        }
                        if (this.comboBox1.Text.Equals("M"))
                        {
                            //寄存器类型-D类
                            sendBuf[18] = 0x90;
                        }
                        //寄存器连续长度
                        sendBuf[19] = (byte)(data & 0xff);
                        sendBuf[20] = (byte)((data & 0xff00) >> 8);
                        //写入的数据处理
                         if(data!=0)
                        {
                            int j = 0;
                            int[] intstr = StringToList(this.textBox5.Text, data);
                            for (int i = 21; i<21+2* data; i++)
                            {
                                
                                if (i %2 == 1)
                                {
                                    sendBuf[i] = (byte)(intstr[j] & 0xff);
                                }
                                else
                                {
                                    sendBuf[i] = (byte)((intstr[j] & 0xff00)>>8);
                                    j++;
                                }
                                
                            }
                        }
                       
                       
                        return sendBuf;
                    }
                default: return null;
            }
        }
        /*  /// 客户端发送消息，服务端接收
         private void SendMsg(string str)
         {
              byte[] arrMsg = Encoding.UTF8.GetBytes(str);
              client.Send(arrMsg);
         }*/

/*
        private void Client_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (client != null) client.Close();
        }*/

 
        /*
         * 将报文byte类型转换为string类型，隔两位输出空格
         * 输入 recvBytes:对应的报文
         * 输出 strmsg：转换后的字符串
         */
       
        private string ByteToString(byte[] recvBytes)
        {
            string strmsg="";
            for (int i = 0; i < recvBytes.Length; i++)
            {
                strmsg += recvBytes[i].ToString("X2");
                strmsg +=  " ";
            }
          
            return strmsg;
        }
      /*
       * 执行写入命令时将输入框字符串按，分割,放入int数组中
       * 输入 str: 输入框字符串 data:数据长度
       * 输出 int[] 数组
       */
        private int[] StringToList(string str,int data)
        {
            int[] intstr=new int[data];
            
            string[] sArray = str.Split(',');
            intstr = Array.ConvertAll(sArray, int.Parse);
            return intstr;

        }
        private string ByteToAll(string str, byte[] bytes, int length)
        {
            byte[] sendBuf=new byte[length];
            switch (str)
            {
                case "S":
                    {
                        int j = 0;
                       for( int i = bytes.Length-1;i>15; i--)

                        {
                            if (bytes[i] != 0)
                            {
                                sendBuf[j] = bytes[i];
                                j++;
                            }
                           
                        }
                        
                        string value=Encoding.ASCII.GetString(sendBuf);
                        return value;
                        /*string value = Encoding.Default.GetString(sendBuf);*/
                       // listBox2.Items.Add(@"接收数据" + value);
                    }
                    break;
                case "C":
                    {

                        return "";

                    }
               
            }
            return "";
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
    }
}
