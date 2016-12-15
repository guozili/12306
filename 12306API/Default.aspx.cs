using _12306Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace _12306API
{
    public partial class Default : System.Web.UI.Page
    {
        public static Setting setting;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (setting == null)
            {
                setting = new Setting();

                foreach (var row in File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ips.txt")))
                {
                    if (!string.IsNullOrEmpty(row))
                        setting.Ips.Add(row);
                }

                var rows = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "setting.txt"));
                setting.FromCode = rows[0].Split(',').ToList();
                setting.ToCode = rows[1].Split(',').ToList();
                setting.Code = rows[2].Split(',').ToList();
                setting.Date = rows[3].Trim();
            }

            var task = new PiaoTask();
            var piaoData = task.Run(setting);
            Console.WriteLine(piaoData);

            if (piaoData != null)
            {
                File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "secretStr.txt"), piaoData.secretStr);
            }

            Response.Clear();
            Response.Write(string.Format("{0}(\"{1}\");", Request["callback"], piaoData));
        }
    }
}