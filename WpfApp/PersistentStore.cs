using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace UiPathTeam.OrchestratorMaintenanceMode
{
    internal class PersistentStore
    {
        private static readonly string DIR1 = "UiPathTeam";
        private static readonly string DIR2 = "OrchestratorMaintenanceMode";
        private static readonly string FNAME = "Configuration.xml";
        private static readonly string CONF = "configuration";
        private static readonly string VER = "version";
        private static readonly string AUTH = "authentication";
        private static readonly string URL = "url";
        private static readonly string CRED = "credentials";
        private static readonly string TN = "tenancyname";
        private static readonly string UN = "username";
        private static readonly string PW = "password";
        private static readonly string TS = "timestamp";
        private static readonly string TS_FORMAT = "yyyy-MM-ddTHH:mm:ss";
        private const byte SALT = 109;

        public string Path { get; }

        private XmlDocument _doc;
        private int _version;

        public PersistentStore()
        {
            Path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), DIR1);
            if (!Directory.Exists(Path))
            {
                Directory.CreateDirectory(Path);
            }
            Path = System.IO.Path.Combine(Path, DIR2);
            if (!Directory.Exists(Path))
            {
                Directory.CreateDirectory(Path);
            }
            Path = System.IO.Path.Combine(Path, FNAME);
            if (!File.Exists(Path))
            {
                Create(Path);
            }
        }

        private static void Create(string path)
        {
            File.WriteAllText(path, "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
                + "<" + CONF + " " + VER + "=\"1\">\r\n"
                + "<" + AUTH + "/>\r\n"
                + "</" + CONF + ">\r\n");
        }

        public void Load()
        {
            _doc = new XmlDocument();
            _doc.Load(Path);
            _version = int.Parse(_doc.DocumentElement.GetAttribute(VER));
            if (_version == 1)
            {
                // Nothing to do
            }
            else
            {
                throw new InvalidDataException(string.Format("Invalid version={0}", _version));
            }
        }

        public void Save()
        {
            var ws = new XmlWriterSettings();
            ws.Encoding = Encoding.UTF8;
            ws.Indent = true;
            using (var xw = XmlWriter.Create(Path, ws))
            {
                _doc.WriteContentTo(xw);
                xw.Flush();
            }
        }

        public void EnumUrls(Action<string> action)
        {
            var a = _doc.DocumentElement[AUTH];
            foreach (XmlNode node1 in a.ChildNodes)
            {
                if (node1.NodeType == XmlNodeType.Element)
                {
                    var element1 = node1 as XmlElement;
                    if (element1.Name == CRED)
                    {
                        action(element1.GetAttribute(URL));
                    }
                }
            }
        }

        public void FindLastCredentials(Action<string, string, string, string, string> action)
        {
            string url = null;
            string tn = null;
            string un = null;
            string pw = null;
            string tk = null;
            DateTime ts = new DateTime(0);
            var a = _doc.DocumentElement[AUTH];
            foreach (XmlNode node1 in a.ChildNodes)
            {
                if (node1.NodeType == XmlNodeType.Element)
                {
                    var element1 = node1 as XmlElement;
                    if (element1.Name == CRED)
                    {
                        try
                        {
                            var dt = DateTime.ParseExact(element1.GetAttribute(TS), TS_FORMAT, System.Globalization.DateTimeFormatInfo.InvariantInfo);
                            if (ts < dt)
                            {
                                ts = dt;
                                url = element1.GetAttribute(URL);
                                tn = element1.GetAttribute(TN);
                                un = element1.GetAttribute(UN);
                                pw = element1.GetAttribute(PW);
                                tk = DateTime.Now.AddHours(-1) < dt ? element1.InnerText : null;
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
            if (url != null)
            {
                action(url, tn, un, DecodePassword(pw), tk);
            }
        }

        public void FindCredentials(string url, Action<string, string, string, string, string> action)
        {
            var a = _doc.DocumentElement[AUTH];
            foreach (XmlNode node1 in a.ChildNodes)
            {
                if (node1.NodeType == XmlNodeType.Element)
                {
                    var element1 = node1 as XmlElement;
                    if (element1.Name == CRED && element1.GetAttribute(URL) == url)
                    {
                        DateTime dt;
                        try
                        {
                            dt = DateTime.ParseExact(element1.GetAttribute(TS), TS_FORMAT, System.Globalization.DateTimeFormatInfo.InvariantInfo);
                        }
                        catch (Exception)
                        {
                            dt = new DateTime(0);
                        }
                        action(url, element1.GetAttribute(TN), element1.GetAttribute(UN), DecodePassword(element1.GetAttribute(PW)), DateTime.Now.AddHours(-1) < dt ? element1.InnerText : null);
                        return;
                    }
                }
            }
        }

        public void AddCredentials(string url, string tenancyName, string userName, string password, string token)
        {
            token = string.IsNullOrEmpty(token) ? string.Empty : token;
            var a = _doc.DocumentElement[AUTH];
            foreach (XmlNode node1 in a.ChildNodes)
            {
                if (node1.NodeType == XmlNodeType.Element)
                {
                    var element1 = node1 as XmlElement;
                    if (element1.Name == CRED && element1.GetAttribute(URL) == url)
                    {
                        element1.SetAttribute(TN, tenancyName);
                        element1.SetAttribute(UN, userName);
                        element1.SetAttribute(PW, EncodePassword(password));
                        element1.SetAttribute(TS, DateTime.Now.ToString(TS_FORMAT));
                        element1.InnerText = token;
                        return;
                    }
                }
            }
            var element2 = _doc.CreateElement(CRED);
            element2.SetAttribute(URL, url);
            element2.SetAttribute(TN, tenancyName);
            element2.SetAttribute(UN, userName);
            element2.SetAttribute(PW, EncodePassword(password));
            element2.SetAttribute(TS, DateTime.Now.ToString(TS_FORMAT));
            element2.InnerText = token;
            a.AppendChild(element2);
        }

        private string EncodePassword(string password)
        {
            var bb = Encoding.UTF8.GetBytes(password);
            var s = SALT;
            for (int i = 0; i < bb.Length; i++)
            {
                bb[i] ^= s;
                s = bb[i];
            }
            return Convert.ToBase64String(bb);
        }

        private string DecodePassword(string password)
        {
            var bb = Convert.FromBase64String(password);
            var s = SALT;
            for (int i = 0; i < bb.Length; i++)
            {
                var t = bb[i];
                bb[i] ^= s;
                s = t;
            }
            return Encoding.UTF8.GetString(bb);
        }
    }
}
