using IniParser.Model;

namespace LastAndFurious
{
    public class IniSetter
    {
        IniData _data;
        System.Globalization.CultureInfo _culture;

        public IniSetter(IniData data)
        {
            _data = data;
            _culture = System.Globalization.CultureInfo.InvariantCulture;
        }

        public void SetBool(string section, string key, bool value)
        {
            SetItem(section, key, value.ToString(_culture));
        }

        public void SetInt(string section, string key, int value)
        {
            SetItem(section, key, value.ToString(_culture));
        }

        public void SetFloat(string section, string key, float value)
        {
            SetItem(section, key, value.ToString(_culture));
        }

        public void SetString(string section, string key, string value)
        {
            SetItem(section, key, value);
        }

        private void SetItem(string section, string key, string s)
        {
            KeyDataCollection keys = _data.Sections[section];
            if (keys == null)
            {
                SectionData secData = new SectionData(section);
                secData.Keys[key] = s;
                _data.Sections.Add(secData);
            }
            else
            {
                keys[key] = s;
            }
        }
    }
}
