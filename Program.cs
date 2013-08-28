using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;


namespace LightSpeedHarness
{
	public class Partner
	{
		public int PartnerId { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Email { get; set; }
	}

	class Program
	{
		static void Main(string[] args)
		{
			
			DateTime start = DateTime.Now;
			ProcessSuitSleeveTraining();
			DateTime end = DateTime.Now;
			Console.WriteLine(start - end);
			Console.ReadLine();
		}

		static void ProcessSuitSleeveTraining()
		{
			List<Partner> partners = GetUserList("SuitSleeveTraining");

			//get ls token
			string token = LightSpeedAPI.GetToken();
			int i = 0;
			if (token != string.Empty)
			{
				foreach (Partner p in partners)
				{
					if (i % 100 == 0)
					{
						Console.WriteLine("Pausing");
						System.Threading.Thread.Sleep(5000);
					}
					
					Console.WriteLine("{0} - {1} - {2}",LightSpeedAPI.GetUserPassedChapterById(p.Email, token, "57199"),i++, p.Email);
				}

			};
		}

		static List<Partner> GetUserList(string filter)
		{
			List<Partner> partners = new List<Partner>();
			string sql = string.Empty;
			switch (filter)
			{
				case "SuitSleeveTraining":
					sql = "select p.Partner_Id,p.Email, p.First_Name,p.Last_Name from Partner p where p.Partner_Id not in (select x.Partner_ID from Security_Group g join XRefGroupToPartner x on g.Security_GroupID = x.Security_GroupID where GroupName = 'CustomizedSleeveLength') and Rank_ID < 100 and Training_Date < '2013-07-01'";
					break;
				default:
					break;
			}
			

			using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["jhilburn"].ToString()))
			{

				SqlCommand cmd = new SqlCommand(sql, con);
				cmd.CommandType = System.Data.CommandType.Text;
				con.Open();
				SqlDataReader rdr = cmd.ExecuteReader();
				while (rdr.Read())
				{
					partners.Add(new Partner { PartnerId = rdr.GetInt32(0), Email = rdr.GetString(1), FirstName = rdr.GetString(2), LastName = rdr.GetString(3) });
				}
			}
			return partners;

		}
		
	}
}
