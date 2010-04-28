using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace ConsoleWrapper
{
    class WrapperConfig
    {
        private XmlDocument _xmldoc;

        public WrapperConfig(string configFile)
        {
            XmlTextReader reader = new XmlTextReader(configFile);
            _xmldoc = new XmlDocument();
            _xmldoc.Load(reader);
        }

        public WrapperConfig()
        {
            XmlTextReader reader = new XmlTextReader(System.Windows.Forms.Application.StartupPath + "\\Config.xml");
            _xmldoc = new XmlDocument();
            _xmldoc.Load(reader);
        }

        public string GetSettingValue(string setting)
        {
            string value = "";

            foreach (XmlNode tag in _xmldoc.GetElementsByTagName(setting))
            {
                foreach (XmlNode child in tag.ChildNodes)
                {
                    if (child.Name.Equals("value"))
                    {
                        value = child.FirstChild.Value;
                    }
                }
            }

            return value;
        }
    }
}
