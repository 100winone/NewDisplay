using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Net;

using static NewDisplay.Protocol;


namespace NewDisplay
{
    public partial class Form1 : Form
    {
        public static ArrayList busStatus = new ArrayList();

        public static string log, fmt = "00", justCheck, depart_info;
        public static bool isbreak;
        public static bool isrun;
        public static int init, lastNum, temp;
        public static byte[] getBuf = new byte[1024];
        Form2 f2 = new Form2();
       
        TcpClient tc;
        NetworkStream ns1;
        private void Form1_Load(object sender, EventArgs e)
        {
            timer1.Interval = 1000;
            timer1.Start();
            string initTime = "5000";
            Form2.delay = initTime;
            isbreak = false;
            isrun = false;
            //idstr = "ubis";
            //pwstr = "ubis1234";
            //strdsn = "solsdb";
            //strip = "127.0.0.1";
            //strport = "8002";

            {
                string url = Environment.CurrentDirectory + @"..\..\Config.xml";
                try
                {
                    XmlDocument xml = new XmlDocument();
                    xml.Load(url);

                    //xml문서안의 모든 속성을 가져올수 있는 XmlElement입니다. (끝까지 가져옵니다.)
                    //  XmlElement KeyList = xml.DocumentElement;

                    //XmlNodeList를 쓰게 되면 해당 노드를 선택합니다. 
                    XmlNodeList xnList = xml.SelectNodes("Config");

                    //foreach문으로 하나씩 가져와서 xnList에 추가하여줍니다. 
                    foreach (XmlNode xn in xnList)
                    {
                        f2.strip = xn["IP"].InnerText;
                        f2.strport = xn["PORT"].InnerText;

                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show("XML 문제발생 \r\n" + ex);
                }
            }

        }


        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Connect();
            if (tc.Connected)
            {
                listBox1.Items.Add("연결성공");
                isrun = true;
            }
            button2.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            GetMsg();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            tc.Close();

            isrun = false;
            isbreak = false;
            button1.Enabled = false;
            button2.Enabled = false;
        }

        private void button4_Click(object sender, EventArgs e)
        {

            if (isbreak == true)
                isbreak = false;
            else
                isbreak = true;
        }

        private bool Connect()
        {
            bool bResult = false;
            try
            {
                tc = new TcpClient(f2.strip, Convert.ToInt32(f2.strport));
                bResult = tc.Connected;
                if (bResult)
                {
                    isrun = true;
                    ns1 = tc.GetStream();

                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("error");
            }
            return bResult;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (isrun == true)
            {
                picBoxDBState.Image = Properties.Resources.On;
            }
            else
            {
                picBoxDBState.Image = Properties.Resources.Off;
            }
        }


        private void button5_Click(object sender, EventArgs e)
        {
            //SendDefine();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            f2.ShowDialog();
        }



        public void ReceiveDefine()
        {
            HEAD rhead = new HEAD();

            rhead.STX = 0x02;

            rhead.Bid_no[0] = getBuf[1];
            rhead.Bid_no[1] = getBuf[2];
            rhead.Opcode = 0xD1;
            rhead.Length[0] = getBuf[4];
            rhead.Length[1] = getBuf[5];


            Receive rbody = new Receive();
            Info rinfo = new Info();


            TIME frameSend = new TIME();
            frameSend.hh = getBuf[6];
            frameSend.mm = getBuf[7];
            frameSend.ss = getBuf[8];

            rinfo.FrameSend = frameSend;
            rinfo.bus_info_count[0] = getBuf[9];
            rinfo.bus_info_count[1] = getBuf[10];

            List<byte> recvmsg = new List<byte>();


            List<string> sumList = new List<string>();

            recvmsg.AddRange(TcpUtil.ObjectToByte(rhead));
            recvmsg.AddRange(TcpUtil.ObjectToByte(rinfo));
            double fhh = Convert.ToInt32(recvmsg[6]);
            double fmm = Convert.ToInt32(recvmsg[7]);
            double fss = Convert.ToInt32(recvmsg[8]);





            for (int i = 1; i <= int.Parse(getBuf[9].ToString()); i++)
            {
                init = (i * 18) - 7;
                ListViewItem lvi = new ListViewItem(fhh.ToString(fmt) + ":" + fmm.ToString(fmt) + ":" + fss.ToString(fmt)); // 시간
                lvi.SubItems.Add(recvmsg[9].ToString()); // 운행대수



                rbody.nodeNum[0] = getBuf[init]; // 11
                rbody.nodeNum[1] = getBuf[init + 1];
                justCheck = Cal(rbody.nodeNum[0], rbody.nodeNum[1]);
                rbody.nodeBehind = getBuf[init + 2];

                if (rbody.nodeBehind != 0x00)
                {
                    justCheck = justCheck + "-" + rbody.nodeBehind.ToString(); // 노선 번호
                    sumList.Add(justCheck);
                }
                else sumList.Add(justCheck);
               

                rbody.nodePart = getBuf[init + 3]; // 노선 구분
                if (rbody.nodePart == 0x01) justCheck = "시점출발";
                else if (rbody.nodePart == 0x02) justCheck = "종점출발";
                else justCheck = "시->종->회차경유";
                sumList.Add(justCheck);

                rbody.nodeStat = getBuf[init + 4]; // 노선 형태
                if (rbody.nodeStat == 0x00) justCheck = "일반";
                else if (rbody.nodeStat == 0x09) justCheck = "비상";
                else justCheck = "지원";
                sumList.Add(justCheck);

                rbody.stop_id[0] = getBuf[init + 5];
                rbody.stop_id[1] = getBuf[init + 6];
                justCheck = Cal(rbody.stop_id[0], rbody.stop_id[1]);
                depart_info = justCheck;
                sumList.Add(justCheck); // 최근 통과한 정류소 지점 ID 

                rbody.rest_stop[0] = getBuf[init + 7];
                rbody.rest_stop[1] = getBuf[init + 8];
                justCheck = Cal(rbody.rest_stop[0], rbody.rest_stop[1]);
                sumList.Add(justCheck); //[4] 남은 정류소 수

                rbody.expected_arrival[0] = getBuf[init + 9];
                rbody.expected_arrival[1] = getBuf[init + 10];
                if(depart_info.Equals("0"))
                {
                    depart_info = Cal(rbody.expected_arrival[0], rbody.expected_arrival[1]);
                    justCheck = depart_info.Insert(2, ":");                                       
                }
                else
                { 
                justCheck = Cal(rbody.expected_arrival[0], rbody.expected_arrival[1]);
                temp = int.Parse(justCheck) / 60;
                justCheck = temp.ToString() + "분";
                }
                sumList.Add(justCheck);                //[5] 도착예정시간
                rbody.bus_stat[0] = getBuf[init + 11];
                rbody.bus_stat[1] = getBuf[init + 12];
                justCheck = Cal(rbody.bus_stat[0], rbody.bus_stat[1]);
                if (justCheck.Equals("0")) justCheck = "운행종료";
                else if (justCheck.Equals("1")) justCheck = "첫차";
                else if (justCheck.Equals("2")) justCheck = "막차";
                else if (justCheck.Equals("3")) justCheck = "일반";
                else if (justCheck.Equals("4")) justCheck = "버스차량사고";
                else if (justCheck.Equals("5")) justCheck = "차량고장";
                else justCheck = "긴급상황";
                sumList.Add(justCheck); //[6] 버스유형

                rbody.info_stat = getBuf[init + 13];
                justCheck = InfoCheck(rbody.info_stat);
                sumList.Add(justCheck); //[7]

                rbody.emer_stat = getBuf[init + 14];
                if (rbody.emer_stat == 0x00) justCheck = "정상";
                else justCheck = "비상운행";
                sumList.Add(justCheck); //[8]

                rbody.bus_sort = getBuf[init + 15];
                if (rbody.bus_sort == 0x0D) justCheck = "일반";
                else if (rbody.bus_sort == 0x0C) justCheck = "좌석";
                else if (rbody.bus_sort == 0x0B) justCheck = "리무진";
                else if (rbody.bus_sort == 0x14) justCheck = "마을";
                else if (rbody.bus_sort == 0x1E) justCheck = "지선";
                sumList.Add(justCheck); //[9]


                rbody.vehicle_id[0] = getBuf[init + 16];
                rbody.vehicle_id[1] = getBuf[init + 17];
                justCheck = Cal(rbody.vehicle_id[0], rbody.vehicle_id[1]);
                sumList.Add(justCheck); //[10]

                recvmsg.AddRange(TcpUtil.ObjectToByte(rbody));



                lastNum = init + 18;

                lvi.SubItems.Add(sumList[0].ToString());
                lvi.SubItems.Add(sumList[1].ToString());
                lvi.SubItems.Add(sumList[2].ToString());
                lvi.SubItems.Add(sumList[3].ToString());
                lvi.SubItems.Add(sumList[4].ToString());
                lvi.SubItems.Add(sumList[5].ToString());
                lvi.SubItems.Add(sumList[6].ToString());
                lvi.SubItems.Add(sumList[7].ToString());
                lvi.SubItems.Add(sumList[8].ToString());
                lvi.SubItems.Add(sumList[9].ToString());
                lvi.SubItems.Add(sumList[10].ToString());
                listView1.Items.Add(lvi);

                sumList.Clear();
            }



            byte bCheckSum = 0x00;

            TAIL rtail = new TAIL();
            rtail.Checksum = getBuf[lastNum];


            for (int a = 0; a < recvmsg.Count; a++)
            {
                bCheckSum ^= recvmsg[a];
            }

            if (rtail.Checksum != bCheckSum)
            {
                MessageBox.Show("값이 옳지않습니다.");
                Environment.Exit(0);
            }

            rtail.ETX = 0x03;
            recvmsg.AddRange(TcpUtil.ObjectToByte(rtail));
        }

        //private void SendDefine()
        //{
        //    Connect();
        //    HEAD head = new HEAD();

        //    head.STX = 0x02;
        //    head.Bid_no = 100;
        //    head.Opcode = 0x66;
        //    head.Length = ushort.Parse(Marshal.SizeOf<Send>().ToString());

        //    Send body = new Send();

        //    TIME send_time = new TIME();
        //    send_time.hh = byte.Parse(DateTime.Now.ToString("hh"));
        //    send_time.mm = byte.Parse(DateTime.Now.ToString("mm"));
        //    send_time.ss = byte.Parse(DateTime.Now.ToString("ss"));

        //    body.sendTime = send_time;

        //    body.node_info = Int32.Parse(1.ToString());
        //    body.stop_info = Int32.Parse(1.ToString());
        //    body.node_stop_info = Int32.Parse(1.ToString());
        //    body.message_ver = Int32.Parse(1.ToString());
        //    body.image_ver = Int32.Parse(1.ToString());
        //    body.video_ver = Int32.Parse(1.ToString());
        //    //Int32.Parse(1.ToString());
        //    body.version = new char[20];
        //    body.Reserved = new byte[4];

        //    TAIL tail = new TAIL();
        //    tail.ETX = 0x03;

        //    byte bCheckSum = 0x00;

        //    List<byte> sendmsg = new List<byte>();

        //    sendmsg.AddRange(TcpUtil.ObjectToByte(head));
        //    sendmsg.AddRange(TcpUtil.ObjectToByte(body));

        //    for (int a = 0; a < sendmsg.Count; a++)
        //    {
        //        bCheckSum ^= sendmsg[a];
        //    }
        //    tail.Checksum = bCheckSum;

        //    sendmsg.AddRange(TcpUtil.ObjectToByte(tail));

        //    while (isbreak == true)
        //    {
        //        Delay(1000);
        //    }

        //    var sb = new System.Text.StringBuilder();
        //    for (int k = 0; k < sendmsg.Count; k++)
        //    {
        //        sb.AppendLine(sendmsg[k].ToString());
        //    }
        //    SendMsg(sendmsg.ToArray());
        //    listBox1.Items.Add(sb.ToString());
        //    listBox1.SelectedIndex = listBox1.Items.Count - 1;
        //}
        private void SendMsg(byte[] msg)
        {

            try
            {
                if (tc.Connected == true)
                {
                    isrun = true;

                    byte[] sendbuf = msg;

                    NetworkStream stream = tc.GetStream();
                    stream.Write(sendbuf, 0, sendbuf.Length);

                }
            }

            catch (Exception ex)
            {
                isrun = false;
                tc.Close();
                MessageBox.Show(ex.Message);
            }
        }
        public void printListview()
        {


        }
        public void GetMsg()
        {
            try
            {
                Connect();
                if (tc.Connected == true)
                {
                    isrun = true;

                    NetworkStream stream = tc.GetStream();


                    ArrayList getmsg = new ArrayList();
                    int nbytes = stream.Read(getBuf, 0, getBuf.Length);
                    //string output = Encoding.ASCII.GetString(getBuf, 0, nbytes);
                    //listBox1.Items.Add(nbytes.ToString());
                    ReceiveDefine();

                    for (int i = 0; i < nbytes; i++) {
                        getmsg.Add(getBuf[i]);
                    }

                    var sb = new System.Text.StringBuilder();

                    for (int k = 0; k < getmsg.Count; k++)
                    {
                        sb.AppendLine(getmsg[k].ToString());
                    }
                    //listBox1.Items.Add(sb.ToString());



                    //Delay(500);
                    // Delay(Convert.ToInt32(Form2.delay));                                      
                    //stream.Close();
                    //tc.Close();
                }
            }

            catch (Exception ex)
            {
                isrun = false;
                tc.Close();
                MessageBox.Show(ex.Message);
            }
        }


        public static byte[] cut(int a, string table)
        {
            byte[] result = new byte[a];
            uint temp = uint.Parse(table);
            for (int i = 0; i < a; a--)
            {
                result[a - 1] = Convert.ToByte(temp % 10);
                temp = temp / 10;
            }
            return result;
        }
        private static DateTime Delay(int MS)
        {
            DateTime ThisMoment = DateTime.Now;
            TimeSpan duration = new TimeSpan(0, 0, 0, 0, MS);
            DateTime AfterWards = ThisMoment.Add(duration);
            while (AfterWards >= ThisMoment)
            {
                System.Windows.Forms.Application.DoEvents();
                ThisMoment = DateTime.Now;
            }
            return DateTime.Now;
        }

        public static string Cal(byte pre, byte behind)
        {
            string result;

            if (behind != 0x00 || pre != 0x00) {
                uint x = behind;
                uint y = x << 8;
                y ^= pre;
                result = y.ToString();
                return result;
            }
            else
            {
                if (pre == 0) return behind.ToString();
                else return pre.ToString();
            }
        }

        public static string InfoCheck(byte info)
        {
            string result = null;
            if (info == 0x00) result = "일반버스";
            else if (info == 0x01) result = "저상버스";
            else if (info == 0x02) result = "일반버스지체";
            else if (info == 0x03) result = "저상버스지체";
            else if (info == 0x04) result = "일반버스부분지체";
            else if (info == 0x05) result = "저상버스부분지체";
            else if (info == 0x20) result = "기점 출발 정보";
            else result = "버스 진입 상태";
            return result;
        }

    }
}
