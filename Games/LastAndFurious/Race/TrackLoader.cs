using System.Collections.Generic;
using System.Threading.Tasks;
using AGS.API;
using IniParser;
using IniParser.Model;

namespace LastAndFurious
{
    public static class TrackLoader
    {
        /// <summary>
        /// Loads a Track definition file and constructs a Track object.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static async Task<Track> LoadAsync(string defpath, string assetpath, IGraphicsFactory f)
        {
            FileIniDataParser file = new FileIniDataParser();
            file.Parser.Configuration.CommentString = "//";
            IniData inidata = file.ReadFile(defpath);
            if (inidata == null)
                return null;

            IniGetter ini = new IniGetter(inidata);
            string bkgpath = ini.GetString("track", "background");
            string maskpath = ini.GetString("track", "mask");
            Dictionary<Color, int> regionColors = new Dictionary<Color, int>();
            int regions = ini.GetInt("track", "regions");
            for (int i = 0; i < regions; ++i)
            {
                string secname = string.Format("region{0}", i);
                if (!inidata.Sections.ContainsSection(secname))
                    continue;
                Color col = Color.FromArgb(255,
                    (byte)ini.GetInt(secname, "color_r"),
                    (byte)ini.GetInt(secname, "color_g"),
                    (byte)ini.GetInt(secname, "color_b"));
                regionColors[col] = i;
            }

            IImage background = await f.LoadImageAsync(assetpath + bkgpath);
            IBitmap mask = await f.LoadBitmapAsync(assetpath + maskpath);
            /*
            // TODO: find out whether engine provides faster solution
            int[,] regionMap = new int[mask.Width, mask.Height];
            for (int x = 0; x < mask.Width; ++x)
            {
                // NOTE: since MonoAGS has Y axis pointing up, we need to invert the lookup array's Y index
                for (int y = 0, mapy = mask.Height - 1; y < mask.Height; ++y, --mapy)
                {
                    Color col = mask.GetPixel(x, y);
                    int index = 0;
                    regionColors.TryGetValue(col, out index);
                    regionMap[x, mapy] = index;
                }
            }

            return new Track(background, regionColors.Count, regionMap);
            */
            TrackAIData aiData = await LoadAIData(assetpath, f);
            return new Track(background, regionColors.Count, mask, regionColors, aiData);
        }

        private static async Task<TrackAIData> LoadAIData(string assetpath, IGraphicsFactory f)
        {
            TrackAIData data = new TrackAIData();
            await LoadAIRegionsData(data, assetpath, f);
            return data;
        }

        private static async Task LoadAIRegionsData(TrackAIData data, string assetpath, IGraphicsFactory f)
        {
            IBitmap regionMask = await f.LoadBitmapAsync(assetpath + "airegions.bmp");
            if (regionMask == null)
                return;

            FileIniDataParser file = new FileIniDataParser();
            file.Parser.Configuration.CommentString = "//";
            IniData inidata = file.ReadFile(assetpath + "airegions.ini");
            if (inidata == null)
                return;

            IniGetter ini = new IniGetter(inidata);
            Dictionary<Color, float> regionAngles = new Dictionary<Color, float>();
            int regions = ini.GetInt("ai", "regions");
            for (int i = 0; i < regions; ++i)
            {
                string secname = string.Format("region{0}", i);
                if (!inidata.Sections.ContainsSection(secname))
                    continue;
                Color col = Color.FromArgb(255,
                    (byte)ini.GetInt(secname, "color_r"),
                    (byte)ini.GetInt(secname, "color_g"),
                    (byte)ini.GetInt(secname, "color_b"));
                regionAngles[col] = MathUtils.DegreesToRadians(ini.GetFloat(secname, "angle"));
            }

            data.AIRegionMask = regionMask;
            data.AIRegionAngles = regionAngles;
            return;
        }
    }
}
