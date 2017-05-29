using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
namespace WeatherAndCamera
{
    class Weather
    {
        public Dictionary<string, string> ProvinceCodes { get; private set; }
        public Weather()
        {
            ProvinceCodes = new Dictionary<string, string>();
            FileStream fs = new FileStream("省份或直辖市代码.txt", FileMode.Open);
            StreamReader sw = new StreamReader(fs,Encoding.Default);
            string line = sw.ReadLine();
            while (line!=null&& line!="")
            {
                string[] strs=line.Split(new char[] { '\"', ':',' ' }, StringSplitOptions.RemoveEmptyEntries);
                ProvinceCodes.Add(strs[1], strs[0]);
                line = sw.ReadLine();
            }
        }
        Dictionary<string, string> CityCodes = new Dictionary<string, string>();
        public Dictionary<string, string> getCityCodes(string provinceName)
        {
            CityCodes.Clear();
            string request = "http://www.weather.com.cn/data/citydata/district/";
            request += ProvinceCodes[provinceName] + ".html";
            string str = SendRequestByGetMethod(request, System.Text.Encoding.UTF8);
            stringHandle(str,ref CityCodes, ProvinceCodes[provinceName]);
            return CityCodes;
        }
        Dictionary<string, string> CountyTownCodes = new Dictionary<string, string>();
        public Dictionary<string, string> getCountyTownCodes(string cityName)
        {
          
            CountyTownCodes.Clear();
            string request = "http://www.weather.com.cn/data/citydata/city/";
            request += CityCodes[cityName] + ".html";
            string str = SendRequestByGetMethod(request, System.Text.Encoding.UTF8);
            string s = CityCodes[cityName];
            if (cityName == "重庆" || cityName == "北京" || cityName == "天津" || cityName == "上海")
                s = ProvinceCodes[cityName];
            stringHandle(str, ref CountyTownCodes, s,"00");
            return CountyTownCodes;
        }
        public Dictionary<string, string> stringHandle(string str)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            string[] lines = str.Split(new char[] { '{', '}', ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                string[] strs = line.Split(new char[] { '\"', ':', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (strs.Length == 2)
                    dic.Add(strs[0], strs[1]);
            }
            return dic;
        }
        public Dictionary<string, string> stringHandle(string str,ref Dictionary<string, string> dic,string t1,string t2="")
        {
            string[] lines = str.Split(new char[] { '{', '}', ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                string[] strs = line.Split(new char[] { '\"', ':', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (strs.Length == 2)
                    dic.Add(strs[1], t1+strs[0]+t2);
            }
            return dic;
        }
        public Dictionary<string,string> getTodayWeather(string city,string CountyTown)
        {
            string request = "http://www.weather.com.cn/data/cityinfo/{0}.html";
            string s = null;
            if(CountyTown.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length != 0)
            {
                s = CountyTownCodes[CountyTown];
            }
            else if (city.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length != 0)
            {
                s =  CityCodes[city]+"01";
            }
            if (s != null)
            {
                request = String.Format(request, s);               
                string str = SendRequestByGetMethod(request, System.Text.Encoding.UTF8);
                return stringHandle(str);
            }
            return null;
        }
        public Dictionary<string, string> getRealTimeWeather(string city, string CountyTown)
        {
            string request = "http://www.weather.com.cn/data/sk/{0}.html";
            string s = null;
            if (CountyTown.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length != 0)
            {
                s = CountyTownCodes[CountyTown];
            }
            else if (city.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length != 0)
            {
                s = CityCodes[city]+"01";
            }
            if (s != null)
            {
                request = String.Format(request, s);
                string str = SendRequestByGetMethod(request, System.Text.Encoding.UTF8);
                return stringHandle(str);
            }
            return null;
        }
        private string SendRequestByGetMethod(String url, Encoding encoding)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Method = "GET";
            HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
            StreamReader sr = new StreamReader(webResponse.GetResponseStream(), encoding);
            return sr.ReadToEnd();
        }
    }
}
