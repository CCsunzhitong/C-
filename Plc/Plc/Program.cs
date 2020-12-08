using NLog;
using Plc.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using ZXing;
using ZXing.QrCode;

namespace Plc
{
    static class Program
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        //初始化串口
        private static SerialPort serialPort = new SerialPort();
        //plcIP地址
        private static string address = Settings.Default.IPAddress;
        //PLC端口号
        private static int Number = Convert.ToInt32(Settings.Default.Number);
        //PLC数据库表
       // private static TestContainer entities = new TestContainer();
        private static ATRS_QUALEntities codeEntities = new ATRS_QUALEntities();
        private static PrintDocument printDocument;
        private static Thread plcthread;
        private static Thread comthread;
        private static Thread pdathread;
        public static List<PLCPoint> plc;

        static void Main()
        {

            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                    new PlcService()
            };
            ServiceBase.Run(ServicesToRun);
            /* PlcService s1 = new PlcService();
             s1.On*/
        }

        public  static void Start()

        {

            logger.Info("线程开始！");
            //通信端口
            serialPort.PortName = Settings.Default.PortName;
            //波特率
            serialPort.BaudRate = Settings.Default.BaudRate;
            //数据位
            serialPort.DataBits = Settings.Default.DataBits;
            //忽略null字节
            serialPort.DiscardNull = Settings.Default.DtrEnable;
            //将读到的点位信息存到List中
         /*  plc = ReadXML();*/
            /*comthread = new Thread(Com_Main);
            comthread.Start();
            plcthread = new Thread(Plc);
            plcthread.Start(plc);*/


        }

        /// <summary>
        /// com线程
        /// 1.读取点位对应软元件数据
        /// 2.对数据根据点表进行处理
        /// 3.将数据存储到数据库中
        /// </summary>
        public static void Com_Main()
        {
            logger.Info("com线程" );
            try
            {
                serialPort.Open();
                serialPort.DataReceived += SerialPort_DataReceived;

            }
            catch (Exception ex)
            {
                logger.Info("线程"+ex);
                Console.WriteLine("串口错误信息" + ex.Message);
            }
        }

        private static void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            logger.Info("com读取线程");
            Thread.Sleep(100);
            int length = serialPort.BytesToRead;
            //定义字节存储器数组
            byte[] byteReceive = new byte[length];
            //接收的字节存入字节存储器数组
            serialPort.Read(byteReceive, 0, length);
            //把接收的的字节数组转成字符串
            string strReceive = Encoding.UTF8.GetString(byteReceive);
           strReceive= strReceive.Replace("\r", "").Replace("\n","");
     
            if (strReceive.Length == 30)
            {
                Model.AssCode = strReceive;
                Model.SingCode = strReceive.Substring(15,3);
                
                Console.WriteLine("总成条码为" + Model.AssCode);
                Console.WriteLine("总成条码解析的车型信息为" + Model.SingCode);
                pdathread = new Thread(PDAThread);
                pdathread.Start();
                logger.Info("总成条码" + strReceive);
            }
            else
            {

                Console.WriteLine("总成条码出错");
            }
        }

       
        /// <summary>
        /// 读取XML文件
        /// return:PLC点位信息集合
        /// </summary>

       public static List<PLCPoint> ReadXML()
        {

            logger.Info("XML读取线程");
            List<PLCPoint> pLCPoints=new List<PLCPoint>();

            //加载XML文件
            try
            {

                logger.Info(AppDomain.CurrentDomain.BaseDirectory);
                XElement element = XElement.Load(AppDomain.CurrentDomain.BaseDirectory+ "XMLFile1.xml");
                logger.Info("加载成功");
                IEnumerable<XElement> xElements = from plc in element.Elements("point") select plc;


                foreach (var plc in xElements)
                {
                    PLCPoint pLCPoint = new PLCPoint();

                    pLCPoint.Address = Convert.ToInt16(plc.Element("address").Value);
                    pLCPoint.Type = plc.Element("type").Value;
                    pLCPoint.AddressType = plc.Element("addressType").Value;
                    pLCPoint.Length = Convert.ToInt16(plc.Element("length").Value);
                    pLCPoints.Add(pLCPoint);

                }
                

                

            }
            catch (Exception ex)
            {

                logger.Info(ex.Message);
            }
            return pLCPoints;
        }
        /// <summary>
        /// plc线程
        /// 1.读取点位对应软元件数据
        /// 2.对数据根据点表进行处理
        /// 3.将数据存储到数据库中
        /// </summary>


        public static void Plc()
        {
           
            logger.Info("plc线程");
            List<PLCPoint> pLCPoints = (List<PLCPoint>) ReadXML();
            IPAddress ip = IPAddress.Parse(address);
            IPEndPoint ipe = new IPEndPoint(ip, Number);
            //创建Socket
            Socket plcSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            byte[] byteCmd;
            ArrayList listPlc = new ArrayList();
            try
            {
                plcSocket.Connect(ipe);
                // 1.读取点位对应软元件数据


            }
            catch (Exception ex)

            {
                Console.WriteLine("连接plc失败" + ex.Message);
                plcSocket.Close();
                throw;
            }
            while (plcSocket.Connected)
            {
                try
                {

                    if (Model.B_judge)
                    {
                        foreach (var plcpoint in pLCPoints)
                        {
                            //根据点位调用Class1.CreateCommand，生成对应的报
                            byteCmd = Class1.CreateCommand(plcpoint.AddressType, plcpoint.Address, plcpoint.Length);

                            //向plc发送数据
                            plcSocket.Send(byteCmd);

                            Thread.Sleep(100);
                            byte[] arrList = new byte[plcSocket.Available];
                            //从plc接收数据
                            plcSocket.Receive(arrList);
                            //2.对数据根据点表进行处理
                            byte[] receiveByte = Class1.ByteToAll(arrList, plcpoint.Length);
                            // 转换成点表对应的类型
                            if (plcpoint.Type.Equals("string"))
                            {
                                string strTrim = Encoding.ASCII.GetString(receiveByte).Replace("\0", "");
                                listPlc.Add(strTrim);
                            }
                            else if (plcpoint.Type.Equals("float"))
                            {
                                float fTemp = BitConverter.ToSingle(receiveByte, 0);

                                listPlc.Add(fTemp);
                            }
                            else if (plcpoint.Type.Equals("boolean"))
                            {
                                listPlc.Add(BitConverter.ToBoolean(receiveByte, 0));
                            }
                            //将读取到的点位存储到数据库中



                        }
                        Class1.SaveDatabase(listPlc);
                        printDocument = new PrintDocument();
                        printDocument.PrinterSettings.PrinterName = "Canon MF230";
                        printDocument.PrintPage += Print1;
                        printDocument.Print();
                        Model.B_judge = false;
                        Model.AssCode = "";
                        Model.SingCode = "";
                       
                    }
                }

                catch (Exception ex)
                {
                    logger.Info("线程" + ex);
                    Console.WriteLine(ex.Message);
                }
                Thread.Sleep(50);
            }





        }
        /*
         * PDA线程，判断总成条码和工位信息是否匹配
         * 
         */
        private static void PDAThread()
        {
            logger.Info("PDA线程");
            var data = codeEntities.CODE_RECORED.ToList().Last();
            //获取数据库中的车型信息
            
            string strCode = data.CHR_HYP_PS.Substring(13, 3);
            //判断数据库中的车型信息是否与扫码枪中扫到的车型信息匹配
            logger.Info(strCode);
            Console.WriteLine("数据库中车型信息"+ strCode);
            if (Model.SingCode == strCode)
            {
                Model.B_judge = true;
            }
            else
            {
                Console.WriteLine("车型信息不匹配");
            }
        }


        //打印二维码事件
        // 将拿到的总成条码变为二维码
        //*


        private static void Print1(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            
           Image image = Class1.QrCode();
            Rectangle destRect = new Rectangle(20, 20, 150, 150);
            e.Graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);

           
        }



    }
}
