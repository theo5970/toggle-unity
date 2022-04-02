using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toggle.LevelSearch
{

    public class LevelSearchOptions
    {
        public enum Filter
        {
            MostRecent, MostLiked, MostDownloaded, FewCleared, Keyword
        }

        public Filter filter;
        public string keyword;
        public int page = 1;
        public bool searchLastTime = false;
    }
}
