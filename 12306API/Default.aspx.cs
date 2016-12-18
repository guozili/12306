using _12306Common;
using System;
using System.IO;
using System.Linq;

namespace _12306API
{
    public partial class Default : System.Web.UI.Page
    {
        public static Setting setting;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.QueryString.Count == 0)
            {
                var startTime = DateTime.Now;
                setting = PiaoHelper.GetSetting();
                Response.Write(setting.From + "<br/>" + setting.To + "<br/>CDN IP数：" + setting.Ips.Count + "<br/>加载配置（秒）：" + (DateTime.Now - startTime).TotalSeconds);
                return;
            }

            if (setting == null)
            {
                setting = PiaoHelper.GetSetting();
            }

            if (Request["postStr"] != null)
            {
                var rows = Request["postStr"].Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                setting.From = rows[0].Trim();
                setting.To = rows[1].Trim();
                setting.Code = rows[2].Split(',').ToList();
                setting.Date = rows[3].Trim();
                setting.Passenger = rows[4].Split(',').ToList();
                setting.SeatType = rows[5].Split(',').ToList();

                setting.FromCode = setting.Stations.Where(s => s.Contains(setting.From)).Select(s => s.Split('|')[2]).ToList();
                setting.ToCode = setting.Stations.Where(s => s.Contains(setting.To)).Select(s => s.Split('|')[2]).ToList();
            }

            setting.ThreadCount = 20;
            var piaoData = PiaoHelper.GetPiaoData(setting);

            if (piaoData != null)
            {
                File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "secretStr.txt"), piaoData.secretStr);
            }

            Response.Clear();
            Response.Write(string.Format("{0}(\'{1}\');", Request["callback"], piaoData == null ? string.Empty : (Request["ReturnResult"] == null ? piaoData.secretStr : piaoData.result)));
        }
    }
}