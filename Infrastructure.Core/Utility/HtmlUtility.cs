using FuzzySharp.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility
{
    public static class HtmlUtility
    {
        public static string ToHtmlTable<T>(this IEnumerable<T> list, List<string> headerList, List<CustomTableStyle> customTableStyles, params Func<T, object>[] columns)
        {
            if (customTableStyles == null)
                customTableStyles = new List<CustomTableStyle>();

            var tableCss = string.Join(" ", customTableStyles?.Where(w => w.CustomTableStylePosition == CustomTableStylePosition.Table).Where(w => w.ClassNameList != null).SelectMany(s => s.ClassNameList)) ?? "";
            var trCss = string.Join(" ", customTableStyles?.Where(w => w.CustomTableStylePosition == CustomTableStylePosition.Tr).Where(w => w.ClassNameList != null).SelectMany(s => s.ClassNameList)) ?? "";
            var thCss = string.Join(" ", customTableStyles?.Where(w => w.CustomTableStylePosition == CustomTableStylePosition.Th).Where(w => w.ClassNameList != null).SelectMany(s => s.ClassNameList)) ?? "";
            var tdCss = string.Join(" ", customTableStyles?.Where(w => w.CustomTableStylePosition == CustomTableStylePosition.Td).Where(w => w.ClassNameList != null).SelectMany(s => s.ClassNameList)) ?? "";

            var tableInlineCss = string.Join(";", customTableStyles?.Where(w => w.CustomTableStylePosition == CustomTableStylePosition.Table).Where(w => w.InlineStyleValueList != null).SelectMany(s => s.InlineStyleValueList?.Select(x => String.Format("{0}:{1}", x.Key, x.Value)))) ?? "";
            var trInlineCss = string.Join(";", customTableStyles?.Where(w => w.CustomTableStylePosition == CustomTableStylePosition.Tr).Where(w => w.InlineStyleValueList != null).SelectMany(s => s.InlineStyleValueList?.Select(x => String.Format("{0}:{1}", x.Key, x.Value)))) ?? "";
            var thInlineCss = string.Join(";", customTableStyles?.Where(w => w.CustomTableStylePosition == CustomTableStylePosition.Th).Where(w => w.InlineStyleValueList != null).SelectMany(s => s.InlineStyleValueList?.Select(x => String.Format("{0}:{1}", x.Key, x.Value)))) ?? "";
            var tdInlineCss = string.Join(";", customTableStyles?.Where(w => w.CustomTableStylePosition == CustomTableStylePosition.Td).Where(w => w.InlineStyleValueList != null).SelectMany(s => s.InlineStyleValueList?.Select(x => String.Format("{0}:{1}", x.Key, x.Value)))) ?? "";

            var sb = new StringBuilder();

            sb.Append($"<table{(string.IsNullOrEmpty(tableCss) ? "" : $" class=\"{tableCss}\"")}{(string.IsNullOrEmpty(tableInlineCss) ? "" : $" style=\"{tableInlineCss}\"")}>");
            if (headerList != null)
            {
                sb.Append($"<tr{(string.IsNullOrEmpty(trCss) ? "" : $" class=\"{trCss}\"")}{(string.IsNullOrEmpty(trInlineCss) ? "" : $" style=\"{trInlineCss}\"")}>");
                foreach (var header in headerList)
                {
                    sb.Append($"<th{(string.IsNullOrEmpty(thCss) ? "" : $" class=\"{thCss}\"")}{(string.IsNullOrEmpty(thInlineCss) ? "" : $" style=\"{thInlineCss}\"")}>{header}</th>");
                }
                sb.Append("</tr>");
            }
            foreach (var item in list)
            {
                sb.Append($"<tr{(string.IsNullOrEmpty(trCss) ? "" : $" class=\"{trCss}\"")}{(string.IsNullOrEmpty(trInlineCss) ? "" : $" style=\"{trInlineCss}\"")}>");
                foreach (var column in columns)
                    sb.Append($"<td{(string.IsNullOrEmpty(tdCss) ? "" : $" class=\"{tdCss}\"")}{(string.IsNullOrEmpty(tdInlineCss) ? "" : $" style=\"{tdInlineCss}\"")}>{column(item)}</td>");
                sb.Append("</tr>");
            }

            sb.Append("</table>");

            return sb.ToString();
        }

        public class CustomTableStyle
        {
            public CustomTableStylePosition CustomTableStylePosition { get; set; }

            public List<string> ClassNameList { get; set; }
            public Dictionary<string, string> InlineStyleValueList { get; set; }
        }

        public enum CustomTableStylePosition
        {
            Table,
            Tr,
            Th,
            Td
        }


        public static void Using()
        {
            var dataList = new List<TestDataClass>
            {
                new TestDataClass {Name = "A", Lastname = "B", Other = "ABO"},
                new TestDataClass {Name = "C", Lastname = "D", Other = "CDO"},
                new TestDataClass {Name = "E", Lastname = "F", Other = "EFO"},
                new TestDataClass {Name = "G", Lastname = "H", Other = "GHO"}
            };

            var headerList = new List<string> { "Name", "Surname", "Merge" };

            var customTableStyle = new List<CustomTableStyle>
                {
                    new CustomTableStyle{CustomTableStylePosition = CustomTableStylePosition.Table, InlineStyleValueList = new Dictionary<string, string>{{"font-family", "Comic Sans MS" },{"font-size","15px"}}},
                    new CustomTableStyle{CustomTableStylePosition = CustomTableStylePosition.Table, InlineStyleValueList = new Dictionary<string, string>{{"background-color", "yellow" }}},
                    new CustomTableStyle{CustomTableStylePosition = CustomTableStylePosition.Tr, InlineStyleValueList =new Dictionary<string, string>{{"color","Blue"},{"font-size","10px"}}},
                    new CustomTableStyle{CustomTableStylePosition = CustomTableStylePosition.Th,ClassNameList = new List<string>{"normal","underline"}},
                    new CustomTableStyle{CustomTableStylePosition = CustomTableStylePosition.Th,InlineStyleValueList =new Dictionary<string, string>{{ "background-color", "gray"}}},
                    new CustomTableStyle{CustomTableStylePosition = CustomTableStylePosition.Td, InlineStyleValueList  =new Dictionary<string, string>{{"color","Red"},{"font-size","15px"}}},
                };

            var htmlResult = dataList.ToHtmlTable(headerList, customTableStyle, x => x.Name, x => x.Lastname, x => $"{x.Name} {x.Lastname}");
        }

        private class TestDataClass
        {
            public string Name { get; set; }
            public string Lastname { get; set; }
            public string Other { get; set; }
        }

    }
}
