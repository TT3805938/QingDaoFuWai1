using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZZJ_Module
{
	public static class PingTaiCreateCard
	{
		public static int CreateCard(int Mode=0)
		{
			if (ZZJCore.SuanFa.Proc.ReadPublicINI("LocalInfo", "0") == "1")return 0;
			Public_Var.PF = new PlatformInFo();
			return RegionalPlatform.CreateCardAddPosit(Mode);
		}

		public static int WriterCard()
		{
			return EasyHiTi.WriterCardAndOutCard();
		}
	}//End Class
}