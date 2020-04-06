using AudioMark.Core.Measurements.Analysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioMark.ViewModels.Reports
{
    public class AnalysisResultViewModel
    {
        private AnalysisResultBase _model;

        public class Item
        {
            public string Key { get; set; }
            public string Value { get; set; }
        }

        public List<Item> Items { get; set; }

        public AnalysisResultViewModel(AnalysisResultBase model)
        {
            _model = model;
            Items = model.ToDictionary()
                .Select(item => new Item()
                {
                    Key = item.Key,
                    Value = item.Value
                })
                .ToList();
        }

        public string GetText()
        {
            return _model.ToString();
        }
    }
}
