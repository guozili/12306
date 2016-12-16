using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;

namespace _12306Common
{
    public class PiaoTask
    {
        private ConcurrentQueue<string> ips;
        private Setting setting;
        private PiaoData piaoData;
        public string ipStr;

        public PiaoTask()
        {

        }

        public PiaoData Run(Setting setting, int threadCount = 10)
        {
            this.setting = setting;
            this.ips = new ConcurrentQueue<string>(setting.Ips);
            //现在ips里有600个IP，大概10个线程有了，从ips队列里出列查询
            for (var i = 0; i < threadCount; i++)
            {
                var worker = new BackgroundWorker();
                worker.DoWork += worker_DoWork;
                worker.RunWorkerAsync();
            }

            while (true)
            {
                Thread.Sleep(300);

                if (piaoData != null || this.ips.Count() == 0)
                    break;
            }

            return piaoData;
        }

        void worker_DoWork(object s, DoWorkEventArgs e)
        {
            while (true)
            {
                //所有线程看到其他线程找到了secretStr，退出
                if (piaoData != null)
                    break;

                var ip = string.Empty;
                if (this.ips.TryDequeue(out ip))
                {
                    ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
                    try
                    {
                        Console.Write(".");
                        for (int i = 0; i < setting.FromCode.Count; i++)
                        {
                            for (int j = 0; j < setting.ToCode.Count; j++)
                            {
                                var dt = DateTime.Now;
                                var request = WebRequest.Create(
                                    string.Format(
                                        "https://{0}/otn/leftTicket/queryA?leftTicketDTO.train_date={1}&leftTicketDTO.from_station={2}&leftTicketDTO.to_station={3}&purpose_codes=ADULT",
                                        ip,
                                        setting.Date,
                                        setting.FromCode[i],
                                        setting.ToCode[j]
                                        )) as HttpWebRequest;
                                request.Host = "kyfw.12306.cn";
                                request.Method = "GET";
                                request.Referer = "https://kyfw.12306.cn/otn/leftTicket/init";
                                request.Headers.Add("X-Requested-With:XMLHttpRequest");
                                request.Headers.Add("Cache-Control", "no-cache");
                                request.UserAgent = "Mozilla/5.0 (Linux; U; Android 2.3.6; zh-cn; GT-S5660 Build/GINGERBREAD) AppleWebKit/533.1 (KHTML, like Gecko) Version/4.0 Mobile Safari/533.1 MicroMessenger/4.5.255";
                                request.Timeout = 5000;
                                request.ServicePoint.ConnectionLimit = 10;
                                using (var response = request.GetResponse())
                                {
                                    var h = response.Headers[3];
                                    //Console.WriteLine(h);
                                    if ((DateTime.Now - dt).Milliseconds < 2000)
                                    {
                                        //ipStr = ipStr + "\r\n" + ip;
                                    }

                                    using (var sr = new StreamReader(response.GetResponseStream()))
                                    {
                                        PiaoData currentPiaoData = null;
                                        var result = sr.ReadToEnd();
                                        var currentPiaoDatas = JsonConvert.DeserializeObject<Piao>(result).data;
                                        foreach (var t in setting.SeatType)
                                        {
                                            if (t.Trim() == "硬卧")
                                            {
                                                currentPiaoData = currentPiaoDatas.FirstOrDefault(p => setting.Code.Contains(p.queryLeftNewDTO.station_train_code) && p.queryLeftNewDTO.YouPiao(p.queryLeftNewDTO.yw_num));
                                                if (currentPiaoData != null)
                                                {
                                                    setting.SeatCode = "3";
                                                    break;
                                                }
                                            }
                                            else if (t.Trim() == "软卧")
                                            {
                                                currentPiaoData = currentPiaoDatas.FirstOrDefault(p => setting.Code.Contains(p.queryLeftNewDTO.station_train_code) && p.queryLeftNewDTO.YouPiao(p.queryLeftNewDTO.rw_num));
                                                if (currentPiaoData != null)
                                                {
                                                    setting.SeatCode = "4";
                                                    break;
                                                }
                                            }
                                            else if (t.Trim() == "二等座")
                                            {
                                                currentPiaoData = currentPiaoDatas.FirstOrDefault(p => setting.Code.Contains(p.queryLeftNewDTO.station_train_code) && p.queryLeftNewDTO.YouPiao(p.queryLeftNewDTO.ze_num));
                                                if (currentPiaoData != null)
                                                {
                                                    setting.SeatCode = "O";
                                                    break;
                                                }
                                            }
                                        }
                                        
                                        //Console.WriteLine("查询IP {0} {1}", ip, setting.Code.First());
                                        if (currentPiaoData != null && !string.IsNullOrEmpty(currentPiaoData.secretStr))
                                        {
                                            piaoData = currentPiaoData;
                                            piaoData.result = result;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //Console.WriteLine(ip + ":" + ex.Message);
                    }
                }
                else
                {
                    //IP里队列都查完了，退出
                    break;
                }
            }
        }
    }

    public class Setting
    {
        public Setting()
        {
            this.Ips = new List<string>();
            this.FromCode = new List<string>();
            this.ToCode = new List<string>();
            this.Code = new List<string>();
        }

        //所有cdnIP
        public List<string> Ips { get; set; }
        //出发站代码，如BJP,BXP,VNP都代表北京，可以去12306页面里去找这个数据
        public List<string> FromCode { get; set; }
        //到站代码
        public List<string> ToCode { get; set; }
        //车次，如Z5
        public List<string> Code { get; set; }
        //乘车日期
        public string Date { get; set; }

        public string From { get; set; }
        public string To { get; set; }
        public List<string> Passenger { get; set; }

        public List<string> SeatType { get; set; }
        public string SeatCode { get; set; }

        public string JSESSIONID { get; set; }
        public string BIGipServerotn { get; set; }
        public string UserAgent { get; set; }

        public string[] Stations { get; set; }
    }

    //映射/otn/leftTicket/queryX 返回的json对象
    public class Piao
    {
        public string validateMessagesShowId { get; set; }
        public bool status { get; set; }
        public List<PiaoData> data { get; set; }
    }

    public class PiaoData
    {
        public PiaoDTO queryLeftNewDTO { get; set; }
        public string secretStr { get; set; }
        public string result { get; set; }
    }

    public class PiaoDTO
    {
        //硬座
        public string yz_num { get; set; }
        //一等座
        public string zy_num { get; set; }
        //二等座
        public string ze_num { get; set; }
        //硬卧
        public string yw_num { get; set; }
        //软卧
        public string rw_num { get; set; }

        //如 Z5
        public string station_train_code { get; set; }
        //如 24000000Z506
        public string train_no { get; set; }

        public string from_station_telecode { get; set; }
        public string to_station_telecode { get; set; }
        public string seat_types { get; set; }

        public bool YouPiao(string numStr)
        {
            int num = 0;

            if (numStr == "有")
                return true;

            if (int.TryParse(numStr, out num))
            {
                if (num > 0)
                    return true;
            }

            return false;
        }
    }
}