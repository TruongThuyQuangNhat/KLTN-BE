using System.Collections.Generic;

namespace ManageUser.Model
{
    public class GridModel
    {
        public int page { set; get; }
        public bool pageLoading { set; get; }
        public int pageSize { set; get; }
        public string predicate { set; get; }
        public string searchText { set; get; }
        public string dataValue { set; get; }
        public string srtColumns { set; get; }
        public string srtDirections { set; get; }
        public List<FilterModel> listFilter { set; get; }
    }

    public class FilterModel
    {
        public string filterColumns { set; get; }
        public string filterDirections { set; get; }
        public string filterData { set; get; }
    }
}
