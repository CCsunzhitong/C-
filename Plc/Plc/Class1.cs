using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXing;
using ZXing.QrCode;

namespace Plc
{
  public  class Class1
    {
        private static TestContainer entities = new TestContainer();
        public static  byte[] CreateCommand(string type, int dreg, int data)
        {
            byte[] sendBuf=new byte[21];
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
            sendBuf[9] = 0x10;
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
             if (type.Equals("R"))
            {
                //寄存器类型-R类
                sendBuf[18] = 0xAF;
            }
            if (type.Equals("D"))
            {
                //寄存器类型-D类
                sendBuf[18] = 0xA8;
            }
            if (type.Equals("M"))
            {
                //寄存器类型-D类
                sendBuf[18] = 0x90;
            }
            //寄存器连续长度
            sendBuf[19] = (byte)(data & 0xff);
             sendBuf[20] = (byte)((data & 0xff00) >> 8);
             return sendBuf;                                                        
        }
        public static  byte[] ByteToAll( byte[] bytes, int length)
        {
            byte[] sendBuf=new byte[length*2];
            int j = 0;
            for (int i = 11; i < bytes.Length ; i++)

            {
               
                    sendBuf[j] = bytes[i];
                    j++;
                

            }

            return sendBuf;
        }
        public static void SaveDatabase(ArrayList arrayList)
        {
            Console.WriteLine("数据开始存入数据库，");
            T_Test plctest = new T_Test
            {
                Id = Guid.NewGuid(),
                D600 = (string)arrayList[0],
                D610 = (string)arrayList[1],
                D620 = (string)arrayList[2],
                D640 = (string)arrayList[3],
                D660 = (string)arrayList[4],
                D670 = (string)arrayList[5],
                D816 = (float)arrayList[6],
                D818 = (float)arrayList[7],
                D820 = (float)arrayList[8],
                M1007 = (bool)arrayList[9],
                CreateTime = DateTime.Now

            };
            entities.T_TestSet.Add(plctest);
            entities.SaveChanges();
            Console.WriteLine("数据成功存入数据库，");
        }
        public static Image QrCode()
        {
            QrCodeEncodingOptions qr = new QrCodeEncodingOptions();
            //设置编码格式,否则会乱码
            qr.CharacterSet = "UTF-8";
            qr.Height = 200;
            qr.Width = 200;
            //设置二维码图片周围空白边距
            qr.Margin = 1;
            BarcodeWriter wr = new BarcodeWriter();
            //指定二维码规格
            wr.Format = BarcodeFormat.QR_CODE;
            wr.Options = qr;
            //生成二维码
            Image img = wr.Write(Model.AssCode);
            return img;

        }

    }
}
