using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ConsoleApp1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Console.WriteLine(CamelToString("MyName")); 
            //string data = "cats AND*Dogs-are Awesome"; 
            //string removeSpace = RemoveSpacFromString(data);  
            //string removeChar = RemoveSpecChar(removeSpace);  
            //Console.WriteLine(removeChar); 

            // GetCategories(); 
            // Validate(); 
             Notify(); 

            Console.ReadLine(); 
        }

        //MY METHOD 
        private async static void Notify()
        {
            BillerRequest myModel = new BillerRequest() { data = new Data() { } };

            var memoryStream = new MemoryStream();
            TextWriter stringWriter = new StreamWriter(memoryStream, System.Text.Encoding.UTF8);
            XmlSerializer x = new XmlSerializer(myModel.GetType());
            x.Serialize(stringWriter, myModel);

            string mraw = Encoding.UTF8.GetString(memoryStream.ToArray());

            string resp = await PostAsycXML("notify", mraw);
            Console.WriteLine(resp);
        }

        private async static void Validate()
        {
            BillerRequest myModel = new BillerRequest() { data = new Data() }; // remove debit id and amount

            var memoryStream = new MemoryStream();
            TextWriter stringWriter = new StreamWriter(memoryStream, System.Text.Encoding.UTF8);
            XmlSerializer x = new XmlSerializer(myModel.GetType());
            x.Serialize(stringWriter, myModel);

            string mraw = Encoding.UTF8.GetString(memoryStream.ToArray());

            string resp = await PostAsycXML("validate", mraw);
            Console.WriteLine(resp);
        }

        private async static void GetCategories()
        {
            string res = await GetAsyc("getcategories");
            Console.WriteLine(res);
        }

        //My API CALLING METHOD 
        //static string baseUrlVarseIP = "http://196.46.20.20:8002/api/biller/";
        static string baseUrlVarseIP = "http://196.46.20.114:8002/api/biller/";

        public static async Task<string> GetAsyc(string actionName)
        {
            string mresult = "";
            try
            {
                using (var mclient = new HttpClient() { BaseAddress = new Uri(baseUrlVarseIP) })
                {
                    mclient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    mclient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json; charset=utf-8");
                    mclient.Timeout = TimeSpan.FromMinutes(1);
                    var response = await mclient.GetAsync(actionName);
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        return result;
                    }
                    else
                    {
                        return response.StatusCode.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                mresult = "error";
            }
            return mresult;
        }

        public static async Task<string> PostAsyc(string actionName, string mrawData)
        {
            string mresult = "";
            try
            {
                using (var mclient = new HttpClient() { BaseAddress = new Uri(baseUrlVarseIP) })
                {
                    mclient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    mclient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json; charset=utf-8");

                    mclient.Timeout = TimeSpan.FromMinutes(1);
                    var response = await mclient.PostAsync($"{actionName}/", new StringContent(mrawData, Encoding.UTF8, "application/json")).ConfigureAwait(false);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        return result;
                    }
                    else
                    {
                        return response.StatusCode.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                mresult = "";
            }
            return mresult;
        }
        
        public static async Task<string> PostAsycXML(string actionName, string mrawData)
        {
            string mresult = "";
            try
            {
                using (var mclient = new HttpClient() { BaseAddress = new Uri(baseUrlVarseIP) })
                {
                    mclient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
                    mclient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/xml; charset=utf-8");

                    //hash = date+clientid+customerid (20210629 + 3acd1796-893f-45b5-b53f-2568598e0a5e + 45701727336) with sha 512 lowercase
                    //mclient.DefaultRequestHeaders.Add("hash", "451763ac6be7d0a161f277a0b3434b84d5de14dd24ab2278bf9c1b8a5f32da8b10ca3f7cac8402436354ef072478b0019ec4802cd4e0c0e89b5f68844e71705a");

                    //hash = date+clientid+customerid (20211013 + 8937887c-ba88-4415-983d-76e147e9f881 + 2347083817146) with sha 512 lowercase //customerid is the phone number crediting
                    
                    string hash = SHA512("20211013" + "8937887c-ba88-4415-983d-76e147e9f881" + "2347083817146");
                    mclient.DefaultRequestHeaders.Add("hash", "6b6c9450244554de24a016ca81d8f1f45cb606a2cbe07718cf66fb15116636f0768ec4b21ea5b21611a11473dd1e30284ce9e3c9ccdcaa03fc8afc0fbaa62d9c");

                    mclient.Timeout = TimeSpan.FromMinutes(1);
                    var response = await mclient.PostAsync($"{actionName}/", new StringContent(mrawData, Encoding.UTF8, "application/xml")).ConfigureAwait(false);

                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        return result; 
                    }
                    else
                        return response.StatusCode.ToString();
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                mresult = "error";
            }
            return mresult;
        }

        public static string SHA512(string input)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(input);
            using (var hash = System.Security.Cryptography.SHA512.Create())
            {
                var hashedInputBytes = hash.ComputeHash(bytes); 
                // Convert to text 
                // StringBuilder Capacity is 128, because 512 bits / 8 bits in byte * 2 symbols for byte 
                var hashedInputStringBuilder = new System.Text.StringBuilder(128);
                foreach (var b in hashedInputBytes)
                    hashedInputStringBuilder.Append(b.ToString("X2"));
                return hashedInputStringBuilder.ToString().ToLower();
            }
        }

        public static string CamelToString(string value)
        {
            return Regex.Replace(value, "(?!^)([A-Z])", " $1");
        }

        public static string RemoveSpacFromString(string value)
        {
            return value.Replace(" ", "").ToString();
        }

        public static string RemoveSpecChar(string value)
        {
            return Regex.Replace(value, @"[^0-9a-zA-Z]+", "");
        }

    }

    public class BillerRequest
    {
        public string VendorCode { get; set; } = "2347087214896";
        public string MerchantId { get; set; } = "ELKANAH";
        public string TerminalId { get; set; } = "VAS002";
        public string DebitId { get; set; } = "2"; //transref
        public Data data { get; set; }
    }
    public class Data
    {
        public string customerid { get; set; } = "2347083817146";
        public string productKey { get; set; } = "EXRCTRFREQ";
        public string email { get; set; } = "oluwasegunababatunde@elkanahtech.com";
        public string phone { get; set; } = "2347083817146";
        public string amount { get; set; } = "50";
    }

}