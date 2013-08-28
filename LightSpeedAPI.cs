using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Security.Cryptography;

namespace LightSpeedHarness
{
	public class LightSpeedAPI
	{
		/// <summary>
		/// Checks to see if the userId is availble.  False means userId already exists; True means you can create the user
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="token"></param>
		/// <returns></returns>
		public static bool CheckUser(string userName, string token)
		{
			bool b = false;
			string postString = string.Format("command=checkUserID&token={0}&UserId={1}", token, userName);
			XmlDocument response = MakeRequest(postString);
			b = bool.Parse(response.SelectSingleNode("//result").InnerXml);
			return b;

		}

		public static void CreateUser(string token)
		{
			XmlDocument response = new XmlDocument();
			string userName = "userid-Jhilburn";
			string password = "password";
			string firstName = "fname";
			string lastName = "lname";
			string emailAddress = "emailaddy@email.com";

			string postString = string.Format("command=addUser&token={0}&login={1}&password={2}&FName={3}&LName={4}&Email={5}&isActive=true&gdlrid=56824&UserAccessLevel=12&userUpdateMyProfile=false", token, userName, password, firstName, lastName, emailAddress);
			response = MakeRequest(postString);

		}

		public static string GetToken()
		{
			string token = string.Empty;
			XmlDocument response = new XmlDocument();
			string postString = "authKey=E2AF93CA&command=getLGToken";
			response = MakeRequest(postString);
			XmlNode node = response.SelectSingleNode("//result");
			if (node != null && node.InnerXml == "true")
			{
				token = response.SelectSingleNode("//data").InnerXml;
			}

			return token;
		}

		/// <summary>
		/// Generic HTTP Post Method
		/// </summary>
		/// <param name="postString">body of post message</param>
		/// <returns>returns XMLDocument with results - up to the caller to parse results</returns>
		public static XmlDocument MakeRequest(string postString)
		{

			XmlDocument response = new XmlDocument();
			const string URL = "https://webservices.lightspeedvt.net/lsvt_api_v35.ashx";
			const string contentType = "application/x-www-form-urlencoded; charset=UTF-8";

			CookieContainer cookies = new CookieContainer();
			HttpWebRequest webRequest = WebRequest.Create(URL) as HttpWebRequest;
			webRequest.Method = "POST";
			webRequest.ContentType = contentType;
			webRequest.CookieContainer = cookies;
			webRequest.ContentLength = postString.Length;

			StreamWriter requestWriter = new StreamWriter(webRequest.GetRequestStream());
			requestWriter.Write(postString);
			requestWriter.Close();

			StreamReader responseReader = new StreamReader(webRequest.GetResponse().GetResponseStream());
			string responseData = responseReader.ReadToEnd();
			response.LoadXml(responseData);

			return response;
		}

		public static string GenerateLoginUrl(string userName, string password)
		{
			string returnUrl = string.Empty;
			return returnUrl = string.Format("https://webservices.lightspeedvt.net/lsvt_api_v35.ashx?command=goLogin&login={0}&h={1}", userName, System.Web.HttpUtility.UrlEncode(password));
		}


		public static string HashPassword(string userName, string password)
		{
			string returnUrl = string.Empty;
			var hash = System.Security.Cryptography.MD5.Create().ComputeHash(System.Text.Encoding.ASCII.GetBytes(password));

			var b64 = Convert.ToBase64String(hash);
			//var b64 = Convert.ToString(hash);
			return returnUrl = string.Format("https://webservices.lightspeedvt.net/lsvt_api_v35.ashx?command=goLogin&login={0}&h={1}", userName, System.Web.HttpUtility.UrlEncode(b64));
			//return returnUrl = string.Format("https://webservices.lightspeedvt.net/lsvt_api_v35.ashx?command=goLogin&login={0}&h={1}", userName, b64);
		}

		public static string GetMd5Hash(string input)
		{
			StringBuilder sBuilder = new StringBuilder();
			using (MD5 md5Hash = MD5.Create())
			{
				byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
				for (int i = 0; i < data.Length; i++)
				{
					sBuilder.Append(data[i].ToString("x2"));
				}
			}
			return sBuilder.ToString();
		}

		public static XmlDocument GetCourses(string token)
		{
			XmlDocument response = new XmlDocument();

			string postBody = string.Format("command=getCourses&token={0}", token);
			response = MakeRequest(postBody);

			return response;
		}

		public static void GetUserCourses(string userName, string token)
		{
			string userId = GetUserId(userName, token);
			string postBody = string.Format("command=getUserCourses&token={0}&UserId={1}", token, userId);
			XmlDocument response = new XmlDocument();
			response = MakeRequest(postBody);
		}

		public static bool GetUserPassedChapterById(string userName, string token, string chapterID)
		{
			bool passed = false;

			string userId = GetUserId(userName, token);
			string postBody = string.Format("command=getUserChapters&token={0}&UserId={1}", token, userId);
			XmlDocument response = new XmlDocument();
			response = MakeRequest(postBody);
			//XmlNode chapter = response.SelectSingleNode(string.Format("//userCourseChapter/chapterID[text()='{0}']",chapterID)).ParentNode;

			XmlNodeList list = response.SelectNodes(string.Format("//userCourseChapter/chapterID[text()='{0}']", chapterID));

			if (list.Count > 0)
			{
				if (list[0].SelectSingleNode("//chapterAttemptStatus").InnerText == "pass")
				{
					passed = true;
				}
			}

			return passed;

		}

		public static string GetUserId(string userName, string token)
		{
			XmlDocument response = new XmlDocument();
			string postBody = string.Format("command=getUserDataByLogin&token={0}&login={1}", token, userName);
			string userId = string.Empty;


			response = MakeRequest(postBody);
			XmlNode n = response.SelectSingleNode("//GUserID");
			if (n != null)
			{
				userId = n.InnerText;
			}

			return userId;
		}
	}
}
