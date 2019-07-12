using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.NetworkInformation;
using System.Threading;
using System.IO;
using System.Timers;

namespace Ping_Recorder
{
    public partial class Form1 : Form
    {
        private Thread thread;
        private int S = 0;//成功次数
        private int F = 0;//失败次数
        private int E = 0;//异常次数
        private int time_limit=50;//超时提示设置
        private bool P_enable = false;//ping使能
        private int Sleep_time = 1000;//ping间隔时间设置
        //private string IP_adr = "192.168.8.1";//要Ping的IP地址
        //IP地址有效性判断要对格式范围判断，太麻烦不做了，
        //格式不对的话，会出现在ping异常里面
        public Form1()
        {
            InitializeComponent();
            // 启动线程
            thread = new Thread(new ThreadStart(Ping_cmd));
            thread.IsBackground = true;
            thread.Start();
            
        }


        /// <summary>
        /// 使用子线程，间隔若干ms执行ping命令并返回结果
        /// </summary>
        private void Ping_cmd()
        {

            while (true)
            {

                Thread.Sleep(Sleep_time);
                if (P_enable)
                {
                    //获取ping时间间隔设定值
                    try
                    {
                        Sleep_time = int.Parse(textBox4.Text);
                        if (Sleep_time < 100)
                        {
                            Sleep_time = 100;//如果设置间隔小于100ms,则置为100ms
                        }
                        Invoke(new Action(() =>
                        {
                            //设置动态指示灯的时间间隔
                            this.timer1.Interval = Sleep_time / 2;
                        }));

                    }
                    catch (Exception e)
                    {
                        P_enable = false;
                        Invoke(new Action(() =>
                        {
                            //停止ping后，参数可编辑
                            this.textBox1.Enabled = true;
                            // this.textBox2.Enabled = true;
                            this.textBox3.Enabled = true;
                            this.textBox4.Enabled = true;
                        }));
                        Sleep_time = 1000;//缺省值1000ms
                        MessageBox.Show("时间间隔" + e.Message);
                    }
                    //获取耗时超限制值打印的限制值
                    try
                    {
                        time_limit = int.Parse(textBox3.Text);
                    }
                    catch (Exception e)
                    {
                        P_enable = false;
                        Invoke(new Action(() =>
                        {
                            //停止ping后，参数可编辑
                            this.textBox1.Enabled = true;
                            // this.textBox2.Enabled = true;
                            this.textBox3.Enabled = true;
                            this.textBox4.Enabled = true;
                        }));
                        time_limit = 50;//缺省值
                        MessageBox.Show("耗时异常阀值" + e.Message);
                    }
                    //尝试开始ping并显示结果
                    try
                    {
                        Ping PingInfo = new Ping();
                        PingOptions PingOpt = new PingOptions();
                        PingOpt.DontFragment = true;
                        string myInfo = "jld5256_PingTest";
                        byte[] bufferInfo = Encoding.ASCII.GetBytes(myInfo);
                        int TimeOut = 1000;
                        PingReply reply = PingInfo.Send(this.textBox1.Text, TimeOut, bufferInfo, PingOpt);

                        // 跨UI更新界面
                        Invoke(new Action(() =>
                        {
                            if (reply.Status == IPStatus.Success)
                            {
                                this.success_f.Text = "成功！";
                                S++;
                                this.label11.Text = S.ToString();
                                this.time.Text = reply.RoundtripTime.ToString();
                                //判断耗时是否大于限制值，大于则打印该条信息，否则不打印
                                if (reply.RoundtripTime > time_limit)
                                {
                                    this.textBox2.AppendText(DateTime.Now.ToString("yyyy-MM-dd hh-mm-ss.fff") + " ping时间大于设定的" + time_limit.ToString() + "ms，为" + reply.RoundtripTime.ToString() + "ms\r\n");
                                }
                                this.TTL.Text = reply.Options.Ttl.ToString();
                                // this.textBox4.Text = (reply.Options.DontFragment ? "发生分段" : "没有发生分段");
                                this.label5.Text = reply.Buffer.Length.ToString();
                            }
                            else
                            {
                                this.success_f.Text = "失败！";
                                this.textBox2.AppendText(DateTime.Now.ToString("yyyy-MM-dd hh-mm-ss.fff") + " ping失败" + "\r\n");
                                F++;
                                this.label10.Text = F.ToString();
                                this.time.Text = "0";
                                this.TTL.Text = "0";
                                this.label5.Text = "0";
                                // MessageBox.Show("无法Ping通");
                            }


                        }));



                    }
                    //出现ping错误，则转到此处，异常计数+1，同时打印异常
                    catch (Exception ey)

                    {
                        Invoke(new Action(() =>
                        {
                            E++;
                            this.label12.Text = E.ToString();
                            this.textBox2.AppendText(DateTime.Now.ToString("yyyy-MM-dd hh-mm-ss.fff") + " Ping异常：" + ey.Message + "\r\n");

                        }));
                        //  MessageBox.Show(ey.Message);

                    }

                }


            }
        }
        /// <summary>
        /// 开始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button1_Click(object sender, EventArgs e)
        {
            P_enable = true;
            //开始ping后，参数不可编辑
            this.textBox1.Enabled = false;
           // this.textBox2.Enabled = false;
            this.textBox3.Enabled = false;
            this.textBox4.Enabled = false;
        }
        /// <summary>
        /// 停止
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button2_Click(object sender, EventArgs e)
        {
            P_enable = false;
            //停止ping后，参数可编辑
            this.textBox1.Enabled = true;
            this.textBox2.Enabled = true;
            this.textBox3.Enabled = true;
            this.textBox4.Enabled = true;
        }
        /// <summary>
        /// 保存文件到txt
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button3_Click(object sender, EventArgs e)
        {
            //**************弹出保存路径************************************************************
            try
            {
                if (this.folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                {
                    string path = this.folderBrowserDialog1.SelectedPath + @"\Ping_log_" + DateTime.Now.ToString("yyyy-MM-dd hh-mm-ss") + ".txt";

                    using (StreamWriter writer = new StreamWriter(path))
                    {
                        writer.Write(this.textBox2.Text);
                    }
                    MessageBox.Show("保存成功！");
                }
            }
             catch(Exception error1)
            {
                MessageBox.Show(error1.Message);
            }



            //*****************直接给出保存路径**********************************************************
            //直接给路径缺点是有些路径不允许写入，如下面路径。当然应该也可以获取当前工程路径写入。。。
            //try
            //{
            //    string path = "C:\\Users\\Ping_log_" + DateTime.Now.ToString("yyyy-MM-dd hh-mm-ss") + ".txt";
            //    using (StreamWriter sw = new StreamWriter(path, true))
            //    {
            //        sw.Write(textBox2.Text);

            //        MessageBox.Show("保存成功！");

            //    }
            //}
            //catch(Exception error1)
            //{
            //    MessageBox.Show(error1.Message);
            //}
            //*****************直接给出保存路径*******************************************

        }
        #region 通过timer定时来做闪烁指示灯
        /// <summary>
        /// 通过timer定时来做闪烁指示灯，网上看到有人用label写个点来做，真是脑洞大开，请收下我的膝盖！
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer1_Tick(object sender, EventArgs e)
        {
            if(P_enable )//Ping使能时设置颜色
            {
                if (this.label18.ForeColor == Color.Lime)
                {
                    this.label18.ForeColor = Color.Red;
                }
                else
                {
                    this.label18.ForeColor = Color.Lime;
                }
            }
            else//Ping不使能时设置灰色
            {
                this.label18.ForeColor = Color.LightGray;
            }
            
            
        }
        #endregion

    }
}
