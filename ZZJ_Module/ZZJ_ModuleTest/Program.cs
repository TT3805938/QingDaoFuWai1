using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
//using Mw_Voice;
namespace ZZJ_ModuleTest
{
	static class Program
	{
		/// <summary>
		/// 应用程序的主入口点。
		/// </summary>
		[STAThread]
		static void Main(string[] argv)
		{
			if (argv.Length == 0)
			{
				Application.Run(new ModuleDBG());
				return;
			}

			string Command = argv[0];
			if (Command.Substring(0, 1) == "/")
			{
				CommandRun(argv);
				return;
			}

			ZZJ_Module.MEF M = new ZZJ_Module.MEF();
			M.srunmode = true;
			string S = "";
			M.InitSubModule("", null);
			M.StartSubModule(Command, new object[] { }, out S);
			return;
		}

		static void CommandRun(string[] argv)
		{
			if (argv[0] == "/PrintImage")
			{
				Bitmap image = (Bitmap)Bitmap.FromFile(argv[2]);
				ZZJCore.SuanFa.PrintCall.Print(argv[1], image, false);
			}
		}

	}//End Class
}
