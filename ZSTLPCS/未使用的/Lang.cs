//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows.Forms;
//using System.Net;
//using System.Collections;
//using QLib;

//namespace ZSTJPCS
//{
//    public static class Lang
//    {
//        /// <summary>从数据库读取基础基础语言备注表
//        /// 
//        /// </summary>
//        static Lang()
//        {
//            string err = "";
//            var dt = SQLConnect.Oracle_Connect.SelectSQL("select * from zxjc_lang", out err);
//            foreach (System.Data.DataRow dr in dt.Rows)
//            {
//                Langs[dr["zw"].ToString()] = dr["xbyw"].ToString();
//            }
//            AppConfig.LANG = 0;
//            //InitGoogle();
//        }

//        static Dictionary<string, string> Langs = new Dictionary<string, string>();

//        /// <summary>自动对控件所包含的控件的语言进行切换
//        /// 
//        /// </summary>
//        /// <param name="c"></param>
//        public static void ChangeLang(Control f)
//        {
//            ControlGetLang(f);
//            foreach (Control c in f.Controls)
//            {
//                ChangeLang(c);
//            }
//        }

//        /// <summary>自动对控件的语言进行切换
//        /// 
//        /// </summary>
//        /// <param name="c"></param>
//        public static void ControlGetLang(Control c)
//        {
//            if (c is Form)
//            {
//                Form lb = c as Form;
//                lb.Text = GetLang(lb.Text);
//            }
//            if (c is System.Windows.Forms.Label)
//            {
//                Label lb = c as Label;
//                lb.Text = GetLang(lb.Text);
//                if (AppConfig.LANG == 1)
//                {
//                    lb.Font = new System.Drawing.Font(lb.Font.Name, lb.Font.Size / 2);
//                }
//            }
//            else if (c is System.Windows.Forms.Button)
//            {
//                Button lb = c as Button;
//                lb.Text = GetLang(lb.Text);
//                if (AppConfig.LANG == 1)
//                {
//                    lb.Font = new System.Drawing.Font(lb.Font.Name, lb.Font.Size / 2);
//                }
//            }
//            else if (c is System.Windows.Forms.ToolStrip)
//            {
//                ToolStrip lb = c as ToolStrip;
//                foreach (ToolStripItem tsi in lb.Items)
//                {
//                    if (tsi is ToolStripButton)
//                    {
//                        ToolStripButton tsb = tsi as ToolStripButton;
//                        tsb.Text = GetLang(tsb.Text);
//                    }
//                }
//                if (AppConfig.LANG == 1)
//                {
//                    lb.Font = new System.Drawing.Font(lb.Font.Name, lb.Font.Size / 2);
//                }
//            }
//        }

//        /// <summary>输入一段文字,根据目前的语言配置项返回相应的文字
//        /// 
//        /// </summary>
//        /// <param name="s"></param>
//        /// <returns></returns>
//        public static string GetLang(string ss)
//        {
//            string result = "";
//            int 找到汉字起始位置 = -1;
//            for (int i = 0; i < ss.Length; i++)
//            {
//                char c = ss[i];
//                if (c >= 0x4e00 && c <= 0x9fbb)//是汉字
//                {
//                    if (找到汉字起始位置 == -1)
//                    {
//                        找到汉字起始位置 = i;
//                    }
//                }
//                else//不是汉字
//                {
//                    //把之前的汉字转换后输出到结果集中
//                    if (找到汉字起始位置 != -1)
//                    {
//                        string 汉字 = ss.Substring(找到汉字起始位置, i - 找到汉字起始位置);
//                        找到汉字起始位置= - 1;
//                        result += GetLangFromDB(汉字);
//                    }
//                    result += c.ToString();//直接添加到结果集中
//                }

//                if (i == ss.Length - 1 && 找到汉字起始位置!=-1)
//                {
//                    string 汉字 = ss.Substring(找到汉字起始位置, i - 找到汉字起始位置+1);
//                    找到汉字起始位置 = -1;
//                    result += GetLangFromDB(汉字);
//                }
//            }
//            return result;
//        }

//        /// <summary>从数据库中查询汉字对应的西班牙文字[根据当前系统模式]
//        /// 
//        /// </summary>
//        /// <param name="s"></param>
//        /// <returns></returns>
//        static string GetLangFromDB(string s)
//        {
//            switch (AppConfig.LANG)
//            {
//                case 1:
//                    if (Langs.ContainsKey(s))
//                    {
//                        return s +"["+ Langs[s]+"]";
//                    }
//                    return s;
//                case 2:
//                    if (Langs.ContainsKey(s))
//                    {
//                        return Langs[s];
//                    }
//                    return s;
//                default:
//                    return s;
//            }
//        }

//        #region 谷歌翻译接口

//        //static Hashtable array = new Hashtable();

//        //static void InitGoogle()
//        //{
//        //    array.Add("sq", "阿尔巴尼亚语");
//        //    array.Add("ar", "阿拉伯语");
//        //    array.Add("ga", "爱尔兰语");
//        //    array.Add("et", "爱沙尼亚语");
//        //    array.Add("be", "白俄罗斯语");
//        //    array.Add("bg", "保加利亚语");
//        //    array.Add("is", "冰岛语");
//        //    array.Add("pl", "波兰语");
//        //    array.Add("fa", "波斯语");
//        //    array.Add("af", "布尔文(南非荷兰语)");
//        //    array.Add("da", "丹麦语");
//        //    array.Add("de", "德语");
//        //    array.Add("ru", "俄语");
//        //    array.Add("fr", "法语");
//        //    array.Add("tl", "菲律宾语");
//        //    array.Add("fi", "芬兰语");
//        //    array.Add("ko", "韩语");
//        //    array.Add("nl", "荷兰语");
//        //    array.Add("gl", "加利西亚语");
//        //    array.Add("ca", "加泰罗尼亚语");
//        //    array.Add("cs", "捷克语");
//        //    array.Add("hr", "克罗地亚语");
//        //    array.Add("lv", "拉脱维亚语");
//        //    array.Add("lt", "立陶宛语");
//        //    array.Add("ro", "罗马尼亚语");
//        //    array.Add("mt", "马耳他语");
//        //    array.Add("ms", "马来语");
//        //    array.Add("mk", "马其顿语");
//        //    array.Add("no", "挪威语");
//        //    array.Add("pt", "葡萄牙语");
//        //    array.Add("ja", "日语");
//        //    array.Add("sv", "瑞典语");
//        //    array.Add("sr", "塞尔维亚语");
//        //    array.Add("sk", "斯洛伐克语");
//        //    array.Add("sl", "斯洛文尼亚语");
//        //    array.Add("sw", "斯瓦希里语");
//        //    array.Add("th", "泰语");
//        //    array.Add("tr", "土耳其语");
//        //    array.Add("cy", "威尔士语");
//        //    array.Add("uk", "乌克兰语");
//        //    array.Add("es", "西班牙语");
//        //    array.Add("iw", "希伯来语");
//        //    array.Add("el", "希腊语");
//        //    array.Add("hu", "匈牙利语");
//        //    array.Add("it", "意大利语");
//        //    array.Add("yi", "意第绪语");
//        //    array.Add("hi", "印地语");
//        //    array.Add("id", "印尼语");
//        //    array.Add("en", "英语");
//        //    array.Add("vi", "越南语");
//        //    array.Add("zh", "中文");
//        //}

//        ///// <summary>
//        ///// 翻译方法
//        ///// </summary>
//        ///// <param name="textstr">需要翻译的内容</param>
//        ///// <param name="language">被翻译的语言</param>
//        ///// <param name="tolanguage">翻译成的语言</param>
//        ///// <returns></returns>
//        //public static string GetGoogtextStr(string textstr, string language = "zh-CN", string tolanguage = "es")
//        //{
//        //    WebClient web = new WebClient();
//        //    WebHeaderCollection headers = new WebHeaderCollection();
//        //    headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded; charset=utf-8";
//        //    headers[HttpRequestHeader.Referer] = "http://translate.google.cn/";
//        //    web.Headers = headers;
//        //    //string url = string.Format("http://ajax.googleapis.com/ajax/services/language/translate?v=1.0&q={0}&langpair={1}%7C{2}", textstr, language, tolanguage);
//        //    string url = String.Format("https://translate.google.cn/#view=home&op=translate&sl={1}&tl={2}&text={0}", textstr, language, tolanguage);
//        //    //string url = String.Format("https://fanyi.baidu.com/#zh/spa/{0}", textstr/*, language, tolanguage*/);

//        //    byte[] bystr = web.DownloadData(url);
//        //    string sss = web.DownloadString(url);
//        //    //web.
//        //    WebBrowser wb = new WebBrowser();
//        //    wb.Url = new Uri(url);
//        //    //wb.Refresh();
//        //    //string wbstr = wb.
//        //    //string s = 
//        //    //string urldata = GetText(System.Web.HttpUtility.UrlDecode(bystr, Encoding.UTF8));
//        //    //return urldata;
//        //    string result = System.Text.Encoding.UTF8.GetString(bystr);
//        //    //wb.DocumentText = result;
//        //    //wb.Document.Body.All
//        //    //var sss  wb.Document.GetElementById("")
//        //    //HtmlDocument doc = new HtmlDocument();
//        //    //doc.LoadHtml(result);
//        //    //return doc.DocumentNode.SelectSingleNode("//textarea[@name='utrans']").InnerText;
//        //    return result;
//        //}


//        #endregion
//    }
//}
