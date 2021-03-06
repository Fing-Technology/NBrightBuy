// --- Copyright (c) notice NevoWeb ---
//  Copyright (c) 2014 SARL NevoWeb.  www.nevoweb.com. The MIT License (MIT).
// Author: D.C.Lee
// ------------------------------------------------------------------------
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// ------------------------------------------------------------------------
// This copyright notice may NOT be removed, obscured or modified without written consent from the author.
// --- End copyright notice --- 

using System;
using System.Collections.Generic;
using System.IO;
using System.Web.UI.WebControls;
using System.Windows.Forms.VisualStyles;
using System.Xml;
using DotNetNuke.Common;
using DotNetNuke.UI.Skins;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Base;
using Nevoweb.DNN.NBrightBuy.Components;

namespace Nevoweb.DNN.NBrightBuy.Admin
{

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ViewNBrightGen class displays the content
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class Orders : NBrightBuyAdminBase
    {

        private GenXmlTemplate _templSearch; 
        private String _entryid = "";
        private Boolean _displayentrypage = false;
        private String _uid = "";
        private String _page = "";
        private String _print = "";
        private String _printtemplate = "";
        
        private const string NotifyRef = "orderaction";

        #region Event Handlers


        override protected void OnInit(EventArgs e)
        {
            _entryid = Utils.RequestQueryStringParam(Context, "eid");
            _uid = Utils.RequestParam(Context, "uid");
            _print = Utils.RequestParam(Context, "print");
            _printtemplate = Utils.RequestParam(Context, "template");
            _page = Utils.RequestParam(Context, "page");
            EnablePaging = true;

            base.OnInit(e);

            // if we want to print a order we need to open the browser with a startup script, this points to a Printview.aspx. (Must go after the ModSettings has been init.)
            if (_print != "") Page.ClientScript.RegisterStartupScript(this.GetType(), "printorder", "window.open('" + StoreSettings.NBrightBuyPath() + "/PrintView.aspx?itemid=" + _entryid + "&printcode=" + _print + "&template=" + _printtemplate + "&theme=" + ModSettings.Get("theme") + "','_blank');", true);

            CtrlPaging.Visible = true;
            CtrlPaging.UseListDisplay = true;
            try
            {
                if (_entryid != "") _displayentrypage = true;

                #region "load templates"

                // Get Search
                var rpSearchTempl = ModCtrl.GetTemplateData(ModSettings, "orderssearch.html", Utils.GetCurrentCulture(), DebugMode);
                _templSearch = NBrightBuyUtils.GetGenXmlTemplate(rpSearchTempl, ModSettings.Settings(), PortalSettings.HomeDirectory);
                rpSearch.ItemTemplate = _templSearch;

                var t1 = "ordersheader.html";
                var t2 = "ordersbody.html";
                var t3 = "ordersfooter.html";

                if (Utils.IsNumeric(_entryid))
                {
                    t1 = "ordersdetailheader.html";
                    t2 = "ordersdetail.html";
                    t3 = "ordersdetailfooter.html";
                }

                // Get Display Header
                var rpDataHTempl = ModCtrl.GetTemplateData(ModSettings, t1, Utils.GetCurrentCulture(), DebugMode);
                rpDataH.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(rpDataHTempl, ModSettings.Settings(), PortalSettings.HomeDirectory);
                // Get Display Body
                var rpDataTempl = ModCtrl.GetTemplateData(ModSettings, t2, Utils.GetCurrentCulture(), DebugMode);
                rpData.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(rpDataTempl, ModSettings.Settings(), PortalSettings.HomeDirectory);
                // Get Display Footer
                var rpDataFTempl = ModCtrl.GetTemplateData(ModSettings, t3, Utils.GetCurrentCulture(), DebugMode);
                rpDataF.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(rpDataFTempl, ModSettings.Settings(), PortalSettings.HomeDirectory);

                if (Utils.IsNumeric(_entryid))
                {
                    var rpItemHTempl = ModCtrl.GetTemplateData(ModSettings, "ordersdetailitemheader.html", Utils.GetCurrentCulture(), DebugMode);
                    rpItemH.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(rpItemHTempl, ModSettings.Settings(), PortalSettings.HomeDirectory);
                    // Get Display Body
                    var rpItemTempl = ModCtrl.GetTemplateData(ModSettings, "ordersdetailitem.html", Utils.GetCurrentCulture(), DebugMode);
                    rpItem.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(rpItemTempl, ModSettings.Settings(), PortalSettings.HomeDirectory);
                    // Get Display Footer
                    var rpItemFTempl = ModCtrl.GetTemplateData(ModSettings, "ordersdetailitemfooter.html", Utils.GetCurrentCulture(), DebugMode);
                    rpItemF.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(rpItemFTempl, ModSettings.Settings(), PortalSettings.HomeDirectory);
                }
                else
                {
                    rpItemH.Visible = false;
                    rpItem.Visible = false;
                    rpItemF.Visible = false;
                }
                #endregion


            }
            catch (Exception exc)
            {
                //display the error on the template (don;t want to log it here, prefer to deal with errors directly.)
                var l = new Literal();
                l.Text = exc.ToString();
                phData.Controls.Add(l);
            }

        }

        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);
                if (Page.IsPostBack == false)
                {
                    PageLoad();
                }
            }
            catch (Exception exc) //Module failed to load
            {
                //remove the navigation data, it could be causing the error.
                var navigationData = new NavigationData(PortalId, "OrderAdmin");
                navigationData.Delete();
                //display the error on the template (don;t want to log it here, prefer to deal with errors directly.)
                var l = new Literal();
                l.Text = exc.ToString();
                phData.Controls.Add(l);
            }
        }

       
        private void PageLoad()
        {

            #region "Data Repeater"
            if (UserId > 0) // only logged in users can see data on this module.
            {

                #region "display list and detail"

                var navigationData = new NavigationData(PortalId, "AdminOrders");
                // get search data
                var sInfo = new NBrightInfo();
                sInfo.XMLData = navigationData.XmlData;
                // display search
                base.DoDetail(rpSearch, sInfo);

                if (_displayentrypage)
                {
                    DisplayDataEntryRepeater(_entryid);
                }
                else
                {

                    //setup paging
                    var pagesize = StoreSettings.Current.GetInt("pagesize");
                    var pagenumber = 1;
                    var strpagenumber = Utils.RequestParam(Context, "page");
                    if (Utils.IsNumeric(strpagenumber)) pagenumber = Convert.ToInt32(strpagenumber);
                    var recordcount = 0;


                    var strFilter = navigationData.Criteria;

                    // if we have a uid, then we want to display only that clients orders.
                    if (Utils.IsNumeric(_uid)) strFilter += " and UserId = " + _uid + " ";

                    if (Utils.IsNumeric(navigationData.RecordCount))
                    {
                        recordcount = Convert.ToInt32(navigationData.RecordCount);
                    }
                    else
                    {
                        recordcount = ModCtrl.GetListCount(PortalId, -1, "ORDER", strFilter); 
                        navigationData.RecordCount = recordcount.ToString("");
                    }

                    //Default orderby if not set
                    const string strOrder = "   order by [XMLData].value('(genxml/createddate)[1]','nvarchar(20)') DESC, ModifiedDate DESC  ";
                    rpData.DataSource = ModCtrl.GetList(PortalId, -1, "ORDER", strFilter, strOrder, 0, pagenumber, pagesize, recordcount);
                    rpData.DataBind();
                    
                    if (pagesize > 0)
                    {
                        CtrlPaging.PageSize = pagesize;
                        CtrlPaging.CurrentPage = pagenumber;
                        CtrlPaging.TotalRecords = recordcount;
                        CtrlPaging.BindPageLinks();
                    }

                    // display header (Do header after the data return so the productcount works)
                    base.DoDetail(rpDataH);

                    // display footer
                    base.DoDetail(rpDataF);

                }

                #endregion
            }

            #endregion

        }

        #endregion

        #region  "Events "

        protected void CtrlItemCommand(object source, RepeaterCommandEventArgs e)
        {
            var cArg = e.CommandArgument.ToString();
            var tabId = TabId;
            var param = new string[4];
            if (_uid != "") param[0] = "uid=" + _uid;
            var navigationData = new NavigationData(PortalId, "AdminOrders");
            var cmd = e.CommandName.ToLower();
            var resxpath = StoreSettings.NBrightBuyPath() + "/App_LocalResources/Notification.ascx.resx";
            var emailoption = "";

            switch (cmd)
            {
                case "entrydetail":
                    param[0] = "eid=" + cArg;
                    if (_page != "") param[1] = "page=" + _page;
                    Response.Redirect(NBrightBuyUtils.AdminUrl(TabId, param), true);
                    break;
                case "reorder":
                    param[0] = "";
                    if (Utils.IsNumeric(cArg))
                    {
                        var orderData = new OrderData(PortalId, Convert.ToInt32(cArg));
                        orderData.CopyToCart(DebugMode);
                        tabId = StoreSettings.Current.GetInt("carttab");
                        if (tabId == 0) tabId = TabId;
                    }
                    Response.Redirect(NBrightBuyUtils.AdminUrl(tabId, param), true);
                    break;
                case "editorder":
                    param[0] = "";
                    if (Utils.IsNumeric(cArg))
                    {
                        var orderData = new OrderData(PortalId, Convert.ToInt32(cArg));
                        orderData.ConvertToCart(DebugMode);
                        tabId = StoreSettings.Current.GetInt("carttab");
                        if (tabId == 0) tabId = TabId;
                    }
                    Response.Redirect(NBrightBuyUtils.AdminUrl(tabId, param), true);
                    break;
                case "return":
                    param[0] = "";
                    if (_page != "") param[1] = "page=" + _page;
                    Response.Redirect(NBrightBuyUtils.AdminUrl(TabId, param), true);
                    break;
                case "search":
                    var strXml = GenXmlFunctions.GetGenXml(rpSearch, "", "");
                    navigationData.Build(strXml, _templSearch);
                    navigationData.OrderBy = GenXmlFunctions.GetSqlOrderBy(rpSearch);
                    navigationData.XmlData = GenXmlFunctions.GetGenXml(rpSearch);
                    navigationData.Save();
                    if (StoreSettings.Current.DebugModeFileOut)
                    {
                        strXml = "<root><sql><![CDATA[" + navigationData.Criteria + "]]></sql>" + strXml + "</root>";
                        var xmlDoc = new System.Xml.XmlDocument();
                        xmlDoc.LoadXml(strXml);
                        xmlDoc.Save(PortalSettings.HomeDirectoryMapPath + "debug_search.xml");
                    }
                    Response.Redirect(NBrightBuyUtils.AdminUrl(TabId, param), true);
                    break;
                case "resetsearch":
                    // clear cookie info
                    navigationData.Delete();
                    Response.Redirect(NBrightBuyUtils.AdminUrl(TabId, param), true);
                    break;
                case "orderby":
                    navigationData.OrderBy = GenXmlFunctions.GetSqlOrderBy(rpData);
                    navigationData.Save();
                    break;
                case "viewclient":
                    param[1] = "ctrl=clients";
                    if (Utils.IsNumeric(cArg))
                    {
                        var orderData = new OrderData(PortalId, Convert.ToInt32(cArg));
                        param[0] = "uid=" + orderData.UserId.ToString("");
                    }
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "save":
                    param[0] = "eid=" + _entryid;
                    var result = Update();
                    NBrightBuyUtils.SetNotfiyMessage(ModuleId, NotifyRef + cmd, result);
                    Response.Redirect(NBrightBuyUtils.AdminUrl(TabId, param), true);
                    break;
                case "printorder":
                        param[0] = "eid=" + _entryid;
                        param[1] = "print=printorder";
                        param[2] = "template=printorder.html";
                        Response.Redirect(NBrightBuyUtils.AdminUrl(TabId, param), true);                        
                    break;
                case "printreceipt":
                        param[0] = "eid=" + _entryid;
                        param[1] = "print=printorder";
                        param[2] = "template=printreceipt.html";
                        Response.Redirect(NBrightBuyUtils.AdminUrl(TabId, param), true);
                    break;
                case "printdeliverylabel":
                    param[0] = "eid=" + _entryid;
                    param[1] = "print=printorder";
                    param[2] = "template=printdeliverylabel.html";
                    Response.Redirect(NBrightBuyUtils.AdminUrl(TabId, param), true);
                    break;
                case "emailamended":
                    param[0] = "eid=" + _entryid;
                    emailoption = DnnUtils.GetLocalizedString("orderamended_emailsubject.Text", resxpath, Utils.GetCurrentCulture());
                    Update(emailoption);
                    SendOrderEmail(Convert.ToInt32(_entryid), "orderamendedemail.html", "orderamended_emailsubject.Text");
                    NBrightBuyUtils.SetNotfiyMessage(ModuleId, NotifyRef + cmd, NotifyCode.ok);
                    Response.Redirect(NBrightBuyUtils.AdminUrl(TabId, param), true);
                    break;
                case "emailreceipt":
                    param[0] = "eid=" + _entryid;
                    emailoption = DnnUtils.GetLocalizedString("orderreceipt_emailsubject.Text", resxpath, Utils.GetCurrentCulture());
                    Update(emailoption);
                    SendOrderEmail(Convert.ToInt32(_entryid), "orderreceiptemail.html", "orderreceipt_emailsubject.Text");
                    NBrightBuyUtils.SetNotfiyMessage(ModuleId, NotifyRef + cmd, NotifyCode.ok);
                    Response.Redirect(NBrightBuyUtils.AdminUrl(TabId, param), true);
                    break;
                case "emailshipped":
                    param[0] = "eid=" + _entryid;
                    emailoption = DnnUtils.GetLocalizedString("ordershipped_emailsubject.Text", resxpath, Utils.GetCurrentCulture());
                    Update(emailoption);
                    SendOrderEmail(Convert.ToInt32(_entryid), "ordershippedemail.html", "ordershipped_emailsubject.Text");
                    NBrightBuyUtils.SetNotfiyMessage(ModuleId, NotifyRef + cmd, NotifyCode.ok);
                    Response.Redirect(NBrightBuyUtils.AdminUrl(TabId, param), true);
                    break;
                case "emailvalidated":
                    param[0] = "eid=" + _entryid;
                    emailoption = DnnUtils.GetLocalizedString("ordervalidated_emailsubject.Text", resxpath, Utils.GetCurrentCulture());
                    Update(emailoption);
                    SendOrderEmail(Convert.ToInt32(_entryid), "ordervalidatedemail.html", "ordervalidated_emailsubject.Text");
                    NBrightBuyUtils.SetNotfiyMessage(ModuleId, NotifyRef + cmd, NotifyCode.ok);
                    Response.Redirect(NBrightBuyUtils.AdminUrl(TabId, param), true);
                    break;
                case "downloadinvoice":
                    DownloadInvoice(Convert.ToInt32(cArg));
                    break;
                case "deleteinvoice":
                    DeleteInvoice(Convert.ToInt32(cArg));
                    param[0] = "eid=" + _entryid;
                    Response.Redirect(NBrightBuyUtils.AdminUrl(TabId, param), true);
                    break;
                case "export":
                    DoExport();
                    Response.Redirect(NBrightBuyUtils.AdminUrl(TabId, param), true);
                    break;
            }

        }

        #endregion

        private NotifyCode Update(String emailoption = "")
        {
            // we don;t have the full field set on this form, so only update the fields we know are there.
            var trackingcode = GenXmlFunctions.GetField(rpDataF, "trackingcode");
            var shippingdate = GenXmlFunctions.GetField(rpDataF, "shippingdate");
            var orderstatus = GenXmlFunctions.GetField(rpDataF, "orderstatus");
            var showtouser = GenXmlFunctions.GetField(rpDataF, "showtouser");
            var notes = GenXmlFunctions.GetField(rpDataF, "notes");
            var emailmsg = GenXmlFunctions.GetField(rpDataF, "emailmsg");
            
            var strUpd = GenXmlFunctions.GetGenXml(rpDataF, "", StoreSettings.Current.FolderUploadsMapPath);
            var nbi = new NBrightInfo(true);
            nbi.XMLData = strUpd;


            if (!Utils.IsNumeric(_entryid)) return NotifyCode.error;
            var ordData = new OrderData(PortalId, Convert.ToInt32(_entryid));
            if (ordData.PurchaseInfo.ItemID == -1) return NotifyCode.fail;
            ordData.ShippedDate = shippingdate;
            ordData.OrderStatus = orderstatus;
            ordData.TrackingCode = trackingcode;
            ordData.InvoiceFileName = nbi.GetXmlProperty("genxml/hidden/hidinvoicedoc");
            ordData.InvoiceFileExt = Path.GetExtension(ordData.InvoiceFileName);
            ordData.InvoiceFilePath = StoreSettings.Current.FolderUploadsMapPath + "\\" + ordData.InvoiceFileName;
            ordData.AddAuditMessage(notes,"msg",UserInfo.Username,showtouser);
            if (emailoption != "")
            {
                ordData.AddAuditMessage(emailmsg, "email", UserInfo.Username, showtouser, emailoption); 
            }

            if (ordData.OrderNumber == "") ordData.OrderNumber = StoreSettings.Current.Get("orderprefix") + ordData.PurchaseInfo.ModifiedDate.Year.ToString("").Substring(2, 2) + ordData.PurchaseInfo.ModifiedDate.Month.ToString("00") + ordData.PurchaseInfo.ModifiedDate.Day.ToString("00") + _entryid;

            ordData.InvoiceDownloadName = ordData.OrderNumber + ordData.InvoiceFileExt;

            ordData.Save();
            return NotifyCode.ok;
        }

        private void DownloadInvoice(int orderid)
        {
            var orderData = new OrderData(PortalId, orderid);
            Utils.ForceDocDownload(StoreSettings.Current.FolderUploadsMapPath + "\\" + orderData.InvoiceFileName, orderData.InvoiceDownloadName, Response);
        }

        private void DeleteInvoice(int orderid)
        {
            var orderData = new OrderData(PortalId, orderid);
            GenXmlFunctions.DeleteFile(orderData.InvoiceFileName, StoreSettings.Current.FolderUploadsMapPath,"");
            orderData.InvoiceDownloadName = "";
            orderData.InvoiceFileExt = "";
            orderData.InvoiceFileName = "";
            orderData.InvoiceFilePath = "";
            orderData.SavePurchaseData();
        }

        private void DisplayDataEntryRepeater(String entryId)
        {
            if (Utils.IsNumeric(entryId) && entryId != "0")
            {
                var orderData = new OrderData(PortalId, Convert.ToInt32(entryId));

                //render the detail page
                base.DoDetail(rpData, orderData.GetInfo());

                base.DoDetail(rpItemH, orderData.GetInfo());
                rpItem.DataSource = orderData.GetCartItemList(StoreSettings.Current.Get("chkgroupresults") == "True");
                rpItem.DataBind();
                base.DoDetail(rpItemF, orderData.GetInfo());

                // display header (Do header so we pickup the special invoice document field in the header)
                base.DoDetail(rpDataH, orderData.GetInfo());

                // display footer (Do here so we pickup the itemid of the order for the action buttons.)
                base.DoDetail(rpDataF, orderData.GetInfo());

            }
        }

        private void SendOrderEmail(int orderid, String emailtemplate,String emailsubjectresxkey)
        {
            var emailmsg = GenXmlFunctions.GetField(rpDataF, "emailmsg");
            NBrightBuyUtils.SendEmailOrderToClient(emailtemplate, orderid, emailsubjectresxkey,"", emailmsg);
        }

        private void DoExport()
        {
            var navigationData = new NavigationData(PortalId, "AdminOrders");
            var strFilter = navigationData.Criteria;
            var l2 = new List<NBrightInfo>();

            const string strOrder = "   order by [XMLData].value('(genxml/createddate)[1]','nvarchar(20)') DESC, ModifiedDate DESC  ";
            var l1 = ModCtrl.GetList(PortalId, -1, "ORDER", strFilter, strOrder, 1000);
            foreach (var i in l1)
            {
                var nodList = i.XMLDoc.SelectNodes("genxml/items/*");
                foreach (XmlNode nod in nodList)
                {
                    var itemline = (NBrightInfo)i.Clone();
                    itemline.RemoveXmlNode("genxml/items");
                    itemline.AddXmlNode("<item>" + nod.OuterXml + "</item>", "item", "genxml");
                    l2.Add(itemline);
                }
            }

            var rp = new Repeater();
            var rpSearchTempl = ModCtrl.GetTemplateData(ModSettings, "ordersexport.html", Utils.GetCurrentCulture(), DebugMode);
            rp.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(rpSearchTempl, ModSettings.Settings(), PortalSettings.HomeDirectory);
            rp.DataSource = l2;
            var strOut = GenXmlFunctions.RenderRepeater(rp);
            Utils.ForceStringDownload(Response,"ordersexport.csv",strOut);
        }



    }

}
