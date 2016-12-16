using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace _12306Common
{
    public class PiaoHelper
    {
        public static Setting GetSetting()
        {
            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

            var setting = new Setting();

            while (true)
            {
                try
                {
                    Console.WriteLine(DateTime.Now + " 获取站点信息 station_name.js");
                    var wc = new WebClient();
                    wc.Encoding = Encoding.UTF8;
                    setting.Stations = wc.DownloadString("https://kyfw.12306.cn/otn/resources/js/framework/station_name.js").Split('@');
                    break;
                }
                catch
                {
                }
            }

            

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


            setting.FromCode = setting.Stations.Where(s => s.Contains(setting.From)).Select(s => s.Split('|')[2]).ToList();
            setting.ToCode = setting.Stations.Where(s => s.Contains(setting.To)).Select(s => s.Split('|')[2]).ToList();

            return setting;
        }

        public static PiaoData GetPiaoData(Setting setting, int threadCount = 10)
        {
            var task = new PiaoTask();
            var piaoData = task.Run(setting, threadCount);
            //Console.WriteLine(piaoData);

            if (piaoData != null)
            {
                File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "secretStr.txt"), piaoData.secretStr);
            }

            return piaoData;
        }
    }
}
