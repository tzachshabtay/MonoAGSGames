using IniParser.Model;

namespace LastAndFurious
{
    public class IniGetter
    {
        IniData _data;
        System.Globalization.CultureInfo _culture;

        public IniGetter(IniData data)
        {
            _data = data;
            _culture = System.Globalization.CultureInfo.InvariantCulture;
        }

        public bool GetBool(string section, string key, bool def_val = false)
        {
            bool val;
            string s;
            if (!GetItem(section, key, out s))
                return def_val;
            if (!bool.TryParse(s, out val))
            {
                int ival;
                if (!int.TryParse(s, out ival))
                    return def_val;
                val = ival != 0;
            }
            return val;
        }

        public int GetInt(string section, string key, int def_val = 0)
        {
            int val;
            string s;
            if (!GetItem(section, key, out s))
                return def_val;
            if (!int.TryParse(s, out val))
                return def_val;
            return val;
        }

        public float GetFloat(string section, string key, float def_val = 0F)
        {
            float val;
            string s;
            if (!GetItem(section, key, out s))
                return def_val;
            if (!float.TryParse(s, System.Globalization.NumberStyles.Float, _culture, out val))
                return def_val;
            return val;
        }

        public string GetString(string section, string key, string def_val = "")
        {
            string s;
            if (!GetItem(section, key, out s))
                return def_val;
            return s;
        }

        private bool GetItem(string section, string key, out string s)
        {
            s = null;
            KeyDataCollection keys = _data.Sections[section];
            if (keys == null) return false;
            s = keys[key];
            return s != null;
        }
    }
}
