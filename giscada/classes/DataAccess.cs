using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data;
using System.Data.SqlClient;

using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;

using Newtonsoft.Json;

namespace giscada.classes
{
    public class DataAccess
    {
        private readonly string CadCnx = "Data Source=192.168.1.198;Initial Catalog=els_2018;User ID=sa;Password=Els1234!!";
        //private readonly string CadCnx = "Data Source=192.168.1.198;Initial Catalog=els_2018;User ID=sa;Password=Els1234!!";
        private readonly string CommercialCnx = "Data Source=191.168.4.219;Initial Catalog=dbSRSScomercial;User ID=UsuarioConsultaReclamos;Password=WC4wu2+LIf";

        public DataTable ExecuteSelectQuery(string commmand) {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(CadCnx))
            using (SqlCommand command = new SqlCommand(commmand, conn))
            using (SqlDataAdapter dataAdapter = new SqlDataAdapter(command))
                dataAdapter.Fill(dt);
            return dt;
        }

        public DataTable ExecuteCommercialQuery(string commmand)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(CommercialCnx))
            using (SqlCommand command = new SqlCommand(commmand, conn))
            using (SqlDataAdapter dataAdapter = new SqlDataAdapter(command))
                dataAdapter.Fill(dt);
            return dt;
        }

        public static Position ToLatLon(double UTMEasting, double UTMNorthing, string utmZone)
        {
            double rad2deg = 180.0 / Math.PI;
            double k0 = 0.9996;
            double a = 6378137;
            double eccSquared = 0.00669438;
            double eccPrimeSquared;
            double e1 = (1 - Math.Sqrt(1 - eccSquared)) / (1 + Math.Sqrt(1 - eccSquared));
            double N1, T1, C1, R1, D, M;
            double LongOrigin;
            double mu, phi1, phi1Rad;
            double x, y;
            int ZoneNumber;
            string ZoneLetter;
            //int NorthernHemisphere; //1 for northern hemispher, 0 for southern

            x = UTMEasting - 500000.0; //remove 500,000 meter offset for longitude
            y = UTMNorthing;

            //ZoneNumber = strtoul(UTMZone, &ZoneLetter, 10);
            ZoneLetter = utmZone.Last().ToString();
            ZoneNumber = int.Parse(utmZone.Remove(utmZone.Length - 1));
            if (ZoneLetter == "N") { 
                //NorthernHemisphere = 1;//point is in northern hemisphere
            }
            else
            {
                //NorthernHemisphere = 0;//point is in southern hemisphere
                y -= 10000000.0;//remove 10,000,000 meter offset used for southern hemisphere
            }

            LongOrigin = (ZoneNumber - 1) * 6 - 180 + 3;  //+3 puts origin in middle of zone

            eccPrimeSquared = (eccSquared) / (1 - eccSquared);

            M = y / k0;
            mu = M / (a * (1 - eccSquared / 4 - 3 * eccSquared * eccSquared / 64 - 5 * eccSquared * eccSquared * eccSquared / 256));

            phi1Rad = mu + (3 * e1 / 2 - 27 * e1 * e1 * e1 / 32) * Math.Sin(2 * mu)
                        + (21 * e1 * e1 / 16 - 55 * e1 * e1 * e1 * e1 / 32) * Math.Sin(4 * mu)
                        + (151 * e1 * e1 * e1 / 96) * Math.Sin(6 * mu);
            phi1 = phi1Rad * rad2deg;

            N1 = a / Math.Sqrt(1 - eccSquared * Math.Sin(phi1Rad) * Math.Sin(phi1Rad));
            T1 = Math.Tan(phi1Rad) * Math.Tan(phi1Rad);
            C1 = eccPrimeSquared * Math.Cos(phi1Rad) * Math.Cos(phi1Rad);
            R1 = a * (1 - eccSquared) / Math.Pow(1 - eccSquared * Math.Sin(phi1Rad) * Math.Sin(phi1Rad), 1.5);
            D = x / (N1 * k0);

            double Lat = phi1Rad - (N1 * Math.Tan(phi1Rad) / R1) * (D * D / 2 - (5 + 3 * T1 + 10 * C1 - 4 * C1 * C1 - 9 * eccPrimeSquared) * D * D * D * D / 24
                            + (61 + 90 * T1 + 298 * C1 + 45 * T1 * T1 - 252 * eccPrimeSquared - 3 * C1 * C1) * D * D * D * D * D * D / 720);
            Lat = Lat * rad2deg;

            double Long = (D - (1 + 2 * T1 + C1) * D * D * D / 6 + (5 - 2 * C1 + 28 * T1 - 3 * C1 * C1 + 8 * eccPrimeSquared + 24 * T1 * T1)

                            * D * D * D * D * D / 120) / Math.Cos(phi1Rad);
            Long = LongOrigin + Long * rad2deg;
            return new Position(Math.Round(Lat, 5), Math.Round(Long, 5));
        }
    }
}