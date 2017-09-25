using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ZZJ_Module
{
  public static class YNCardToQYCard
  {
    public static bool ChangeCard()
    {
      #region 初始化
      string Msg = "";
      ZZJCore.FormSkin.UseRetCard = false;
      ZZJCore.Public_Var.ModuleName = "换区域卡";
      ZZJCore.BackForm.ShowForm("正在准备,请稍候...");
      Application.DoEvents();
      ZZJCore.Initial.Read();
      if (XMLCore.GetUserInfo(out Msg) != 0)
      {
        if (Msg.Length > 0) ZZJCore.BackForm.ShowForm(Msg, true);
        ZZJCore.SuanFa.Proc.Log("查询卡信息失败!" + ZZJCore.Public_Var.cardInfo.CardNo);
        ZZJCore.BackForm.CloseForm();
        return true;
      }
      #endregion
      int CreateNewCard = FormStyle.LoginStyle.HKYesNo();
      if (CreateNewCard != 0)
      {
        ZZJCore.BackForm.CloseForm();
        return true;
      }
      ZZJCore.BackForm.ShowForm("正在为您换卡,请稍候...");
      int iRet = QYKBK.ChangeCard();
      ZZJCore.SuanFa.Proc.MsgSend(0xB5, 0, true);//退卡
      ZZJCore.BackForm.CloseForm();
      return true;
    }

  }//EndCalss
}