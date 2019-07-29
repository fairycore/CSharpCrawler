﻿using CSharpCrawler.Model.Dianpin;
using CSharpCrawler.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;

namespace CSharpCrawler.Views
{
    /// <summary>
    /// DishesPrice.xaml 的交互逻辑
    /// </summary>
    public partial class DishesPrice : Page
    {

        public DishesPrice()
        {
            InitializeComponent();
        }

        private void InitDB()
        {

        }

        private async void btn_StartGetCity_Click(object sender, RoutedEventArgs e)
        {
            ShowStatusText($"正在从{UrlUtil.DianpingGetAllProvince}获取省份信息");
            List<Province> provinceList = await  GetProvinces();
            ShowProvincesInfo(provinceList);
            ShowStatusText($"正在从{UrlUtil.DianpingGetCityByProvince}获取城市信息");
            List<City> cityList = await GetAllCities(provinceList);
        }

        public async Task<List<Province>> GetProvinces()
        {
            List<Province> list = new List<Province>();
            var provincesJsonStr = await WebUtil.PostData(UrlUtil.DianpingGetAllProvince, "");
            var provincesObj = JObject.Parse(provincesJsonStr);
            var provincesJToken = provincesObj["provinceList"];
            list =  provincesJToken.Select(x=>new Province {ProvinceID = (int)x["provinceId"],ProvinceName = (string)x["provinceName"] }).ToList();
            return list;
        }

        public void SaveProvinceToDB(List<Province> privinceList)
        {
            //TODO
        }

        public void ShowProvincesInfo(List<Province> list)
        {
            ShowStatusText($"获取到{list.Count}条记录");
            foreach (var item in list)
            {
                ShowStatusText(item.ToString());
            }
        }

        /// <summary>
        /// {"provinceId":id}
        /// </summary>
        /// <param name="provinceList"></param>
        /// <returns></returns>
        public async Task<List<City>> GetAllCities(List<Province> provinceList)
        {
            List<City> cityList = new List<City>();
            var postData = "{\"provinceId\":id}";

            foreach (var item in provinceList)
            {
                var tempPostData = postData.Replace("id", item.ProvinceID.ToString());
                var citiesJsonStr = await WebUtil.PostData(UrlUtil.DianpingGetCityByProvince, tempPostData, "application/json");

                var citiesObj = JObject.Parse(citiesJsonStr);
                var citiesJToken = citiesObj["cityList"];

                var tempCitiesList = citiesJToken.Select(x=>new City {
                    CityID = (string)x["cityId"],
                    CityName = (string)x["cityName"],
                    CityPinYinName = (string)x["cityPyName"],
                    ProvinceID = item.ProvinceID
                }).ToList();

                cityList.AddRange(tempCitiesList);

                ShowStatusText($"***********************************\r\n{item.ProvinceName}\r\n***********************************");
                tempCitiesList.ForEach(x => ShowStatusText(x.ToString()));

                System.Threading.Thread.Sleep(2000);
            }

            return cityList;
        }

        public void SaveCityToDB(List<City> cityList)
        {
            //TODO
        }

        private void ShowStatusText(string str)
        {
            this.Dispatcher.Invoke(()=> {
                this.paragraph.Inlines.Add(str + Environment.NewLine);
            });
        }

        private async void btn_StartDishPrice_Click(object sender, RoutedEventArgs e)
        {
            //这里只是简单的示例，所以仅抓取第一页数据
            //需要添加以下Cookie信息，否则会出现验证码页面

            var url = "https://www.dianping.com/shenzhen/ch10/g1783";
            var userAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.132 Safari/537.36";
            var accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";

            CookieContainer cookieContainer = new CookieContainer();

            System.Net.Cookie cookie = new Cookie("_hc.v", "721d1647-b5e0-18b6-d41a-43453671c5f8.1563348000");
            cookie.Domain = "www.dianping.com";
            cookieContainer.Add(cookie);

            cookie = new Cookie("_lx_utm", "utm_source%3DBaidu%26utm_medium%3Dorganic");
            cookie.Domain = "www.dianping.com";
            cookieContainer.Add(cookie);

            cookie = new Cookie("_lxsdk", "16bfecd5b69c8-070d60ba7b365d-3c604504-1fa400-16bfecd5b69c8");
            cookie.Domain = "www.dianping.com";
            cookieContainer.Add(cookie);

            cookie = new Cookie("_lxsdk_cuid", "16bfecd5b69c8-070d60ba7b365d-3c604504-1fa400-16bfecd5b69c8");
            cookie.Domain = "www.dianping.com";
            cookieContainer.Add(cookie);

            cookie = new Cookie("_lxsdk_s", "16c3b315b86-7bd-ed8-6a4%7C%7C21");
            cookie.Domain = "www.dianping.com";
            cookieContainer.Add(cookie);

            cookie = new Cookie("cy", "7");
            cookie.Domain = "www.dianping.com";
            cookieContainer.Add(cookie);

            cookie = new Cookie("cye", "shenzhen");
            cookie.Domain = "www.dianping.com";
            cookieContainer.Add(cookie);

            cookie = new Cookie("s_ViewType", "10");
            cookie.Domain = "www.dianping.com";
            cookieContainer.Add(cookie);

            var html = await WebUtil.GetHtmlSource(url, accept, userAgent, Encoding.UTF8, cookieContainer);
          
            this.paragraph_Step2.Inlines.Add(html);

            var pattern = "(?<=<li class=\"\")[\\s\\S]*?(?=</li>)";
            var matchCollection = RegexUtil.Match(html, pattern);

            //PC页面价格加密了，可能需要访问移动端页面
        }
    }
}
