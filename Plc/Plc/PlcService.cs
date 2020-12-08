using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Plc
{
    partial class PlcService : ServiceBase
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
       /* private Thread comthread;*/
        private Thread plcthread;
        private static List<PLCPoint> pLCPoints;
        public PlcService()
        {
            InitializeComponent();
           

        }


        protected override void OnStart(string[] args)
        {
            logger.Info("程序启动！");

            Program.Start();
           /* pLCPoints = Program.ReadXML();*/
           Program.Com_Main();
          
            plcthread = new Thread(Program.Plc);
            plcthread.Start();



        }
        /*   public  void OnStart()
           {
               Thread plcthread = new Thread(Program.Plc);
               plcthread.Start();
           }*/



        protected override void OnStop()
        {
            plcthread.Abort();
        /*    comthread.Abort();*/
            logger.Info("程序停止！");
         
        }
      


    }
}
