using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZZJ_Module
{
    class YYJS
    {
        public static bool YYJSMain()
        {
					System.Diagnostics.Process.Start("iexplore.exe", "-k http:\\\\10.17.133.1:3000\\h5\\index.html");
						//System.Diagnostics.Process.Start("chrome.exe", "http:\\\\10.17.133.1:3000\\h5\\index.html ");
					//YYJSForm yyjs = new YYJSForm();
					//yyjs.ShowDialog();
					return true;
        
        }



       
    }
}
