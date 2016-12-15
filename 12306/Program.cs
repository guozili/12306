using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Net;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using _12306Common;
using Newtonsoft.Json;
using System.Globalization;
using System.Web;

namespace _12306
{
    class Program
    {
        static void Main(string[] args)
        {
            //var wc = new WebClient();
            //var ipStr = wc.DownloadString("http://www.fishlee.net/apps/cn12306/ipservice/getlist");
            //foreach (Match m in Regex.Matches(ipStr, @"""ip"":""(.*?)"""))
            //{
            //    ips.Add(m.Groups[1].Value);
            //}

            string[] stations = null;
            while (true)
            {
                try
                {
                    Console.WriteLine(DateTime.Now + " 获取站点信息 station_name.js");
                    var wc = new WebClient();
                    wc.Encoding = Encoding.UTF8;
                    stations = wc.DownloadString("https://kyfw.12306.cn/otn/resources/js/framework/station_name.js").Split('@');
                    break;
                }
                catch
                {
                }
            }

            var setting = new Setting();

            foreach (var row in File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ips.txt")))
            {
                if (!string.IsNullOrEmpty(row))
                    setting.Ips.Add(row);
            }

            var rows = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "setting.txt"));
            Console.WriteLine(DateTime.Now + " 获取设置信息setting.txt：" + string.Join(" ", rows.Take(6)));
            setting.From = rows[0].Trim();
            setting.To = rows[1].Trim();
            setting.Code = rows[2].Split(',').ToList();
            setting.Date = rows[3].Trim();
            setting.Passenger = rows[4].Split(',').ToList();
            setting.SeatType = rows[5].Split(',').ToList();

            setting.UserAgent = rows[6].Trim();
            setting.JSESSIONID = rows[7].Trim();
            setting.BIGipServerotn = rows[8].Trim();
   

            setting.FromCode = stations.Where(s => s.Contains(setting.From)).Select(s => s.Split('|')[2]).ToList();
            setting.ToCode = stations.Where(s => s.Contains(setting.To)).Select(s => s.Split('|')[2]).ToList();

            proccess(setting);

            Console.Read();
        }

        static PiaoData GetPiaoData(Setting setting)
        {
            var task = new PiaoTask();
            var piaoData = task.Run(setting);
            //Console.WriteLine(piaoData);

            if (piaoData != null)
            {
                File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "secretStr.txt"), piaoData.secretStr);
            }

            return piaoData;
        }

        static void proccess(Setting setting)
        {
            string text = string.Empty;

            try
            {
                var cc = new CookieContainer();
                cc.Add(new Cookie("JSESSIONID", setting.JSESSIONID, @"/otn", "kyfw.12306.cn") { Expires = DateTime.Now.AddMonths(1) });
                cc.Add(new Cookie("BIGipServerotn", setting.BIGipServerotn, @"/", "kyfw.12306.cn") { Expires = DateTime.Now.AddMonths(1) });

                Console.WriteLine(DateTime.Now + " 访问订票列表页 leftTicket/init");
                var myReq = (HttpWebRequest)WebRequest.Create("https://kyfw.12306.cn/otn/leftTicket/init");
                myReq.Method = "GET";
                myReq.CookieContainer = cc;
                myReq.UserAgent = setting.UserAgent;
              
                var resp = myReq.GetResponse() as HttpWebResponse;
                var s = resp.GetResponseStream();
                var sr = new StreamReader(s);
                text = sr.ReadToEnd();

                Thread.Sleep(5000);

                Console.WriteLine(DateTime.Now + " 得到用户的联系人 getPassengerDTOs");
                myReq = (HttpWebRequest)WebRequest.Create("https://kyfw.12306.cn/otn/confirmPassenger/getPassengerDTOs");
                myReq.Method = "POST";
                myReq.CookieContainer = cc;
                myReq.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                myReq.UserAgent = setting.UserAgent;
                myReq.Referer = "https://kyfw.12306.cn/otn/leftTicket/init";
                myReq.Headers.Add("X-Requested-With:XMLHttpRequest");
                using (var writer = new StreamWriter(myReq.GetRequestStream()))
                {
                    writer.Write("");
                }
                resp = myReq.GetResponse() as HttpWebResponse;
                s = resp.GetResponseStream();
                sr = new StreamReader(s);
                text = sr.ReadToEnd();

                Thread.Sleep(2000);
                Console.WriteLine(DateTime.Now + " 扫描票源中，请耐心等待");
                var piaoData = GetPiaoData(setting);

                if (piaoData == null)
                {
                    text = null;
                    throw new Exception("扫描所有的CDN没有找到票源！");
                }

                string passengerTicketStr = string.Empty;
                string oldPassengerStr = string.Empty;
                foreach (var passenger in setting.Passenger)
                {
                    var passengerId = Regex.Match(text, passenger + @"[^\.]+?passenger_id_no"":""(.+?)""").Groups[1].Value;

                    passengerTicketStr += string.Format("{0},0,1,{1},1,{2},,N_", setting.SeatCode, passenger, passengerId);
                    oldPassengerStr += string.Format("{0},1,{1},1_", passenger, passengerId);
                }

                passengerTicketStr = passengerTicketStr.TrimEnd('_');

                Console.WriteLine(DateTime.Now + " 提交订单中 autoSubmitOrderRequest");
                myReq = (HttpWebRequest)WebRequest.Create("https://kyfw.12306.cn/otn/confirmPassenger/autoSubmitOrderRequest");
                myReq.Method = "POST";
                myReq.CookieContainer = cc;
                myReq.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                myReq.UserAgent = setting.UserAgent;
                myReq.Referer = "https://kyfw.12306.cn/otn/leftTicket/init";
                myReq.Headers.Add("X-Requested-With:XMLHttpRequest");
                using (var writer = new StreamWriter(myReq.GetRequestStream()))
                {
                    writer.Write(string.Format("secretStr={0}&train_date={1}&tour_flag=dc&purpose_codes=ADULT&query_from_station_name={2}&query_to_station_name={3}&&cancel_flag=2&bed_level_order_num=000000000000000000000000000000&passengerTicketStr={4}&oldPassengerStr={5}",
                        piaoData.secretStr, setting.Date, setting.From, setting.To, passengerTicketStr, oldPassengerStr));
                }
                resp = myReq.GetResponse() as HttpWebResponse;
                s = resp.GetResponseStream();
                sr = new StreamReader(s);
                text = sr.ReadToEnd();

                var result = JsonConvert.DeserializeObject<AutoSubmitOrderResponse>(text);

                Console.WriteLine(DateTime.Now + " 确认余票中 getQueueCountAsync");
                myReq = (HttpWebRequest)WebRequest.Create("https://kyfw.12306.cn/otn/confirmPassenger/getQueueCountAsync");
                myReq.Method = "POST";
                myReq.CookieContainer = cc;
                myReq.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                myReq.UserAgent = setting.UserAgent;
                myReq.Referer = "https://kyfw.12306.cn/otn/leftTicket/init";
                myReq.Headers.Add("X-Requested-With:XMLHttpRequest");
                using (var writer = new StreamWriter(myReq.GetRequestStream()))
                {
                    writer.Write(string.Format("train_date={0}&train_no={1}&stationTrainCode={2}&seatType={3}&fromStationTelecode={4}&toStationTelecode={5}&leftTicket={6}&purpose_codes=ADULT&_json_att=",
                        (Convert.ToDateTime(setting.Date).ToString("ddd MMM dd yyy ", DateTimeFormatInfo.InvariantInfo) + DateTime.Now.ToString("HH:mm:ss").Replace(":", "%3A") + " GMT%2B0800 (China Standard Time)").Replace(' ', '+'),
                        piaoData.queryLeftNewDTO.train_no,
                        piaoData.queryLeftNewDTO.station_train_code,
                        setting.SeatCode,
                        piaoData.queryLeftNewDTO.from_station_telecode,
                        piaoData.queryLeftNewDTO.to_station_telecode,
                        result.data.result.Split('#')[2]));
                }
                resp = myReq.GetResponse() as HttpWebResponse;
                s = resp.GetResponseStream();
                sr = new StreamReader(s);
                text = sr.ReadToEnd();

                Console.WriteLine(DateTime.Now + " 订单确认排队中 confirmSingleForQueueAsys");
                myReq = (HttpWebRequest)WebRequest.Create("https://kyfw.12306.cn/otn/confirmPassenger/confirmSingleForQueueAsys");
                myReq.Method = "POST";
                myReq.CookieContainer = cc;
                myReq.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                myReq.UserAgent = setting.UserAgent;
                myReq.Referer = "https://kyfw.12306.cn/otn/leftTicket/init";
                myReq.Headers.Add("X-Requested-With:XMLHttpRequest");
               
                using (var writer = new StreamWriter(myReq.GetRequestStream()))
                {
                    writer.Write(string.Format("passengerTicketStr={0}&oldPassengerStr={1}&randCode=&purpose_codes=ADULT&key_check_isChange={2}&leftTicketStr={3}&train_location={4}&choose_seats=&seatDetailType=&_json_att=",
                        HttpUtility.UrlEncode(passengerTicketStr).ToUpper(),
                        HttpUtility.UrlEncode(oldPassengerStr).ToUpper(),
                        result.data.result.Split('#')[1],
                        result.data.result.Split('#')[2],
                        result.data.result.Split('#')[0]));
                }
                resp = myReq.GetResponse() as HttpWebResponse;
                s = resp.GetResponseStream();
                sr = new StreamReader(s);
                text = sr.ReadToEnd();
                if (text.Contains("\"submitStatus\":true"))
                {
                    Console.WriteLine(DateTime.Now + " 订单提交成功，请进网页查看未完成的订单并支付");
                    text = null;
                }
            }
            catch (Exception e)
            {
                if (!(e is NullReferenceException))
                    Console.WriteLine(e.Message);
            }
            finally
            {
                Console.WriteLine(text);
            }
        }


        private static List<string> ips = new List<string>();
    }

    public class AutoSubmitOrderResponse
    {
        public string validateMessagesShowId { get; set; }
        public bool status { get; set; }
        public int httpstatus { get; set; }
        public AutoSubmitOrderData data { get; set; }
    }

    public class AutoSubmitOrderData
    {
        public string result { get; set; }
        public bool submitStatus { get; set; }
    }

}
