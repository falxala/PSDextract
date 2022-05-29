using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSD
{
    internal class html
    {
        string Doctype = "<!DOCTYPE html>\r\n";
        string StartHead = "<head>";
        string EndHead = "</head>";
        string StartBody = "<body>";
        string EndBody = "</body>";
        string StartTr = "<tr>";
        string EndTr = "</tr>";
        string StartTd = "<td>";
        string EndTd = "</td>";

        public string _html(string str)
        {
            str = Doctype + StartHead + "\r\n" + str + "\r\n";
            str += EndHead;
            return str;
        }

        public string _Body(string str)
        {
            str = StartBody + "\r\n" + str + "\r\n";
            str += EndBody;
            return str;
        }
        public string _Tr(string str)
        {
            str = StartTr + "\r\n" + str + "\r\n";
            str += EndTr;
            return str;
        }
        public string _Td(string str)
        {
            str = StartTd + str;
            str += EndTd;
            return str;
        }

        public string _Font(string str,string color)
        {
            str = "<font color=\"" + color + "\">" + str + "</font>";
            return str;
        }

        public string _ColorTable(string str ,int border, string color)
        {
            string start = $"<table border = \"{border}\" bgcolor = \"{color}\">" + "\r\n";
            string end = "\r\n</table >";
            str = _Tr(_Td(str));
            str = start + str + end;
            
            return str;
        }

    }
}
