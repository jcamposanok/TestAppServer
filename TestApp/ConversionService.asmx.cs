using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Script.Services;
using System.Web.Script.Serialization;
using System.IO;
using System.Text;

namespace TestApp
{
    /// <summary>
    /// This service provides basic base64 conversion functions
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [ScriptService]
    public class ConversionService : WebService
    {

        protected string file = @"c:\\db.txt"; // Change the location of the file as required and check permissions

        protected string JavascriptSerialize(object obj)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();
            return js.Serialize(obj);
        }

        protected string ConvertTo64(string plainText)
        {
            var textBytes = Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(textBytes);
        }

        protected string ConvertFrom64(string encodedText)
        {
            var textBytes = System.Convert.FromBase64String(encodedText);
            return Encoding.UTF8.GetString(textBytes);
        }

        [WebMethod]
        [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Json)]
        public void SaveFile(string plainText)
        {
            Context.Response.Clear();
            Context.Response.ClearHeaders();
            Context.Response.ContentType = "application/json";
            Context.Response.AppendHeader("Access-Control-Allow-Methods", "POST,GET,OPTIONS");
            Context.Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Context.Response.AppendHeader("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept");
            
            var writeThis = this.ConvertTo64(plainText) + Environment.NewLine;

            if (!File.Exists(this.file))
            {
                // Create a file to write to 
                File.WriteAllText(this.file, writeThis);
            }
            else
            {
                // Append to existing file
                File.AppendAllText(this.file, writeThis);
            }

            var callback = System.Web.HttpContext.Current.Request.QueryString["callback"]; // Todo: Add some validation to prevent XSS
            Context.Response.Write(callback + "([]);");
        }

        [WebMethod]
        [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Json)]
        public void ReadFile()
        {
            Context.Response.Clear();
            Context.Response.ClearHeaders();
            Context.Response.ContentType = "application/json";
            Context.Response.AppendHeader("Access-Control-Allow-Methods", "POST,GET,OPTIONS");
            Context.Response.AppendHeader("Access-Control-Allow-Origin", "*");
            Context.Response.AppendHeader("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept");
            
            List<string> decodedLines = new List<string>();

            if (File.Exists(this.file))
            {
                string[] lines = File.ReadAllLines(this.file);
                foreach (string line in lines) {
                    decodedLines.Add(this.ConvertFrom64(line));
                }
            }

            Context.Response.Write(this.JavascriptSerialize(decodedLines));
        }

    }
}
