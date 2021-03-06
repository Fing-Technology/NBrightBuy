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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using DotNetNuke.Common;
using DotNetNuke.Entities.Content.Common;
using DotNetNuke.Entities.Portals;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;

using Nevoweb.DNN.NBrightBuy.Base;
using Nevoweb.DNN.NBrightBuy.Components;
using Nevoweb.DNN.NBrightBuy.Components.Interfaces;
using DataProvider = DotNetNuke.Data.DataProvider;

namespace Nevoweb.DNN.NBrightBuy
{

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ViewNBrightGen class displays the content
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class CartView : NBrightBuyFrontOfficeBase
    {

        public String DisplayHeader { get; set; }
        public String DisplayBody { get; set; }
        public String DisplayFooter { get; set; }
        public String PaymentTab { get; set; }
        public String Themefolder { get; set; }


        private String _catid = "";
        private String _catname = "";
        private GenXmlTemplate _templateHeader;//this is used to pickup the meta data on page load.
        private String _templH = "";
        private String _templD = "";
        private String _templDfoot = "";
        private String _templF = "";
        private String _entryid = "";
        private String _tabid = "";
        private CartData _cartInfo;
        private AddressData _addressData;
        private String carttype = "";

        #region Event Handlers


        override protected void OnInit(EventArgs e)
        {

            base.OnInit(e);

            // get setting via control params
            if (DisplayHeader != null && DisplayHeader == "") DisplayHeader = "minicartheader.html";
            if (DisplayBody != null && DisplayHeader == "") DisplayBody = "minicartbody.html";
            if (DisplayFooter != null && DisplayHeader == "") DisplayFooter = "minicartfooter.html";
            if (!String.IsNullOrEmpty(DisplayHeader) && !ModSettings.Settings().ContainsKey("txtdisplayheader")) ModSettings.Settings().Add("txtdisplayheader", DisplayHeader);
            if (!String.IsNullOrEmpty(DisplayBody) && !ModSettings.Settings().ContainsKey("txtdisplaybody")) ModSettings.Settings().Add("txtdisplaybody", DisplayBody);
            if (!String.IsNullOrEmpty(DisplayFooter) && !ModSettings.Settings().ContainsKey("txtdisplayfooter")) ModSettings.Settings().Add("txtdisplayfooter", DisplayFooter);
            if (!String.IsNullOrEmpty(PaymentTab) && !ModSettings.Settings().ContainsKey("PaymentTab")) ModSettings.Settings().Add("PaymentTab",PaymentTab );
            if (!String.IsNullOrEmpty(Themefolder) && !ModSettings.Settings().ContainsKey("themefolder")) ModSettings.Settings().Add("themefolder", Themefolder);

            _cartInfo = new CartData(PortalId);

            _addressData = new AddressData(_cartInfo.UserId.ToString(""));

            if (ModSettings.Get("themefolder") == "")  // if we don't have module setting jump out
            {
                rpDataH.ItemTemplate = new GenXmlTemplate("NO MODULE SETTINGS");
                return;
            }

            try
            {
                _templH = ModSettings.Get("txtdisplayheader");
                _templD = ModSettings.Get("txtdisplaybody");
                _templDfoot = ModSettings.Get("txtdisplaybodyfoot");
                _templF = ModSettings.Get("txtdisplayfooter");

                const string templAB = "cartbillingaddress.html";
                const string templAS = "cartshippingaddress.html";
                const string templS = "cartshipment.html";
                const string templE = "cartextra.html";
                const string templD = "cartdetails.html";

                carttype = ModSettings.Get("ddlcarttype");  // This is left for backward compatiblity with NBS_Cart module (now removed from install).

                if (carttype == "")
                {
                    // cart type is not a setting, so use the controlanme
                    if (ModuleConfiguration.DesktopModule.ModuleName == "NBS_MiniCart") carttype = "1";
                    if (ModuleConfiguration.DesktopModule.ModuleName == "NBS_FullCart") carttype = "3";
                    if (ModuleConfiguration.DesktopModule.ModuleName == "NBS_Checkout") carttype = "2";
                }

                if (carttype == "3" || carttype == "2") // check if we need to add cookie items
                {
                    _cartInfo.AddCookieToCart();
                    _cartInfo.Save();
                }
                if (!_cartInfo.GetCartItemList().Any() && (carttype == "3" || carttype == "2"))
                    _templH = "cartempty.html"; // check for empty cart 
                else
                {
                    // Get Display Body
                    var rpDataTempl = ModCtrl.GetTemplateData(ModSettings, _templD, Utils.GetCurrentCulture(), DebugMode);
                    rpData.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(rpDataTempl, ModSettings.Settings(), PortalSettings.HomeDirectory);

                    // Get Display Footer
                    var rpDataFTempl = ModCtrl.GetTemplateData(ModSettings, _templF, Utils.GetCurrentCulture(), DebugMode);
                    rpDataF.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(rpDataFTempl, ModSettings.Settings(), PortalSettings.HomeDirectory);

                    // Get CartLayout
                    var layouttemplate = "checkoutlayout.html";
                    if (carttype == "3") layouttemplate = "fullcartlayout.html";
                    var checkoutlayoutTempl = ModCtrl.GetTemplateData(ModSettings, layouttemplate, Utils.GetCurrentCulture(), DebugMode);
                    checkoutlayout.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(checkoutlayoutTempl, ModSettings.Settings(), PortalSettings.HomeDirectory);
                    _templateHeader = (GenXmlTemplate)checkoutlayout.ItemTemplate;
                    
                }
                // Get Display Header
                var rpDataHTempl = ModCtrl.GetTemplateData(ModSettings, _templH, Utils.GetCurrentCulture(), DebugMode);

                rpDataH.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(rpDataHTempl, ModSettings.Settings(), PortalSettings.HomeDirectory);

                // insert page header text
                NBrightBuyUtils.IncludePageHeaders(ModCtrl, ModuleId, Page, _templateHeader, ModSettings.Settings(), null, DebugMode);


                if (carttype == "2")
                {
                    // add any shiiping provider templates to the cart layout, so we can process any data required by them
                    rpShip.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(ModCtrl.GetTemplateData(ModSettings, templS, Utils.GetCurrentCulture(), DebugMode), ModSettings.Settings(), PortalSettings.HomeDirectory);
                    rpAddrB.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(ModCtrl.GetTemplateData(ModSettings, templAB, Utils.GetCurrentCulture(), DebugMode), ModSettings.Settings(), PortalSettings.HomeDirectory);
                    rpAddrS.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(ModCtrl.GetTemplateData(ModSettings, templAS, Utils.GetCurrentCulture(), DebugMode), ModSettings.Settings(), PortalSettings.HomeDirectory);

                    var checkoutextraTempl = ModCtrl.GetTemplateData(ModSettings, templE, Utils.GetCurrentCulture(), DebugMode);
                    checkoutextraTempl += GetShippingProviderTemplates();
                    rpExtra.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(checkoutextraTempl, ModSettings.Settings(), PortalSettings.HomeDirectory);
                    
                }


            }
            catch (Exception exc)
            {
                rpDataF.ItemTemplate = new GenXmlTemplate(exc.Message, ModSettings.Settings());
                // catch any error and allow processing to continue, output error as footer template.
            }

        }

        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);
                if (Page.IsPostBack == false)
                {
                    // check for empty cart 
                    if (!_cartInfo.GetCartItemList().Any() && (carttype == "2" || carttype == "3"))
                    {
                        var cartL = new List<NBrightInfo>();
                        cartL.Add(_cartInfo.GetInfo());

                        // display header for empty cart
                        rpDataH.DataSource = cartL;
                        rpDataH.DataBind();
                    }
                    else
                    {
                        PageLoad();                        
                    }
                }
            }
            catch (Exception exc) //Module failed to load
            {
                //display the error on the template (don;t want to log it here, prefer to deal with errors directly.)
                var l = new Literal();
                l.Text = exc.ToString();
                checkoutlayout.Controls.Add(l);
            }
        }

        private void PageLoad()
        {

            #region " Cart List Data Repeater"


            if (_templD.Trim() != "") // if we don;t have a template, don't do anything
            {
                var groupresults = ModSettings.Get("chkgroupresults") == "True";
                var l = _cartInfo.GetCartItemList(groupresults);
                rpData.DataSource = l;
                rpData.DataBind();
            }

            var cartL = new List<NBrightInfo>();
            cartL.Add(_cartInfo.GetInfo());

            // display header
            rpDataH.DataSource = cartL;
            rpDataH.DataBind();

            // display footer
            rpDataF.DataSource = cartL;
            rpDataF.DataBind();

            #endregion

            if (carttype == "3") // full cart list
            {
                // display footer
                cartL[0].SetXmlProperty("genxml/hidden/currentcartstage", cartL[0].GetXmlProperty("genxml/currentcartstage")); // set the cart stage so we appear on correct stage.
                checkoutlayout.DataSource = cartL;
                checkoutlayout.DataBind();
            }
            if (carttype == "2") // full checkout
            {

                // display footer
                cartL[0].SetXmlProperty("genxml/hidden/currentcartstage", cartL[0].GetXmlProperty("genxml/currentcartstage")); // set the cart stage so we appear on correct stage.
                checkoutlayout.DataSource = cartL;
                checkoutlayout.DataBind();

                var objl = new List<NBrightInfo>();
                var billaddr = _cartInfo.GetBillingAddress();
                if (billaddr.XMLData == null)
                {
                    var defAddr = _addressData.GetDefaultAddress();
                    if (defAddr == null)
                    {
                        var cookieaddr = Cookie.GetCookieValue(PortalId,"cartaddress","billingaddress","cartaddress");
                        billaddr.XMLData = cookieaddr;
                    }
                    else
                        billaddr.XMLData = defAddr.XMLData;

                }
                objl.Add(billaddr);
                rpAddrB.DataSource = objl;
                rpAddrB.DataBind();

                objl = new List<NBrightInfo>();
                objl.Add(_cartInfo.GetShippingAddress());
                rpAddrS.DataSource = objl;
                rpAddrS.DataBind();

                // display shipping input form
                objl = new List<NBrightInfo>();
                objl.Add(_cartInfo.GetShipData());
                rpShip.DataSource = objl;
                rpShip.DataBind();

                // display extra input form
                objl = new List<NBrightInfo>();
                objl.Add(_cartInfo.GetExtraInfo());
                rpExtra.DataSource = objl;
                rpExtra.DataBind();

            }

        }

        #endregion

        #region  "Events "

        protected void CtrlItemCommand(object source, RepeaterCommandEventArgs e)
        {
            var cArg = e.CommandArgument.ToString();
            var param = new string[3];
            if (Utils.RequestParam(Context, "eid") != "") param[0] = "eid=" + Utils.RequestParam(Context, "eid"); 

            switch (e.CommandName.ToLower())
            {
                case "additems":
                    // save before add
                    UpdateCartAddresses();
                    UpdateCartInfo();
                    SaveCart();
                    // add item
                    _cartInfo.AddItem(rpData, StoreSettings.Current.SettingsInfo, e.Item.ItemIndex, DebugMode);
                    _cartInfo.Save();
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "addqty":
                    if (!Utils.IsNumeric(cArg)) cArg = "1";
                    if (Utils.IsNumeric(cArg))
                    {
                        _cartInfo.UpdateItemQty(e.Item.ItemIndex,Convert.ToInt32(cArg));
                        _cartInfo.Save();
                    }
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "removeqty":
                    if (!Utils.IsNumeric(cArg)) cArg = "-1";
                    if (Utils.IsNumeric(cArg))
                    {
                        _cartInfo.UpdateItemQty(e.Item.ItemIndex, Convert.ToInt32(cArg));
                        _cartInfo.Save();
                    }
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "deletecartitem":
                    UpdateCartAddresses();
                    UpdateCartInfo();
                    SaveCart();
                    if (cArg == "")
                        _cartInfo.RemoveItem(e.Item.ItemIndex);
                    else
                        _cartInfo.RemoveItem(cArg);
                    _cartInfo.Save();
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "deletecart":
                    _cartInfo.DeleteCart();
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "updatecart":
                    UpdateCartAddresses();
                    UpdateCartInfo();
                    SaveCart();
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "confirm":
                    UpdateCartAddresses();
                    UpdateCartInfo();
                    SaveCart();
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "gotocart":
                    if (_cartInfo.GetCartItemList().Count > 0)
                    {
                        UpdateCartInfo();
                        SaveCart();
                        var paytabid = ModSettings.Get("paymenttab");
                        if (!Utils.IsNumeric(paytabid)) paytabid = TabId.ToString("");
                        Response.Redirect(Globals.NavigateURL(Convert.ToInt32(paytabid), "", param), true);
                    }
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "order":
                    if (_cartInfo.GetCartItemList().Count > 0)
                    {
                        _cartInfo.SetValidated(true);
                        _cartInfo.Lang = Utils.GetCurrentCulture();  // set lang so we can send emails in same language the order was made in.
                        UpdateCartAddresses();
                        UpdateCartInfo();
                        SaveCart(true); // remove zero qty items on order, so they don;t appear in BO or on order.
                        _addressData.AddAddress(rpAddrS);
                        _addressData.AddAddress(rpAddrB);
                        var paytabid = ModSettings.Get("paymenttab");
                        if (!Utils.IsNumeric(paytabid)) paytabid = TabId.ToString("");
                        Response.Redirect(Globals.NavigateURL(Convert.ToInt32(paytabid), "", param), true);
                    }
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "saveshipaddress":
                    UpdateCartAddresses();
                    _cartInfo.Save();
                    _addressData.AddAddress(rpAddrS);
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
                case "savebilladdress":
                    UpdateCartAddresses();
                    _cartInfo.Save();
                    _addressData.AddAddress(rpAddrB);
                    Response.Redirect(Globals.NavigateURL(TabId, "", param), true);
                    break;
            }

        }

        #endregion

        #region "Methods"

        private void SaveCart(Boolean removeZeroQtyItems = false)
        {
            var activecartstage = _cartInfo.PurchaseInfo.GetXmlProperty("genxml/currentcartstage");
            var currentcartstage = GenXmlFunctions.GetField(checkoutlayout, "currentcartstage");
            if (_cartInfo.EditMode == "E" && activecartstage == "cartsummary" && currentcartstage == "cartsummary") 
            {
                // is order being edited, so return to order status after edit.
                // ONLY if the cartsummry is being displayed to the manager.
                _cartInfo.ConvertToOrder();
                // redirect to back office
                var param = new string[2];
                param[0] = "ctrl=orders";
                param[1] = "eid=" + _cartInfo.PurchaseInfo.ItemID.ToString("");
                var strbackofficeTabId = StoreSettings.Current.Get("backofficetabid");
                var backofficeTabId = TabId;
                if (Utils.IsNumeric(strbackofficeTabId)) backofficeTabId = Convert.ToInt32(strbackofficeTabId);
                var href = Globals.NavigateURL(backofficeTabId, "", param);
                Response.Redirect(href, true);
            }
            else
            {
                _cartInfo.PurchaseInfo.SetXmlProperty("genxml/currentcartstage", currentcartstage);
                var pickuppointref = GenXmlFunctions.GetField(rpExtra, "pickuppointref");
                _cartInfo.PurchaseInfo.SetXmlProperty("genxml/pickuppointref", pickuppointref);
                var pickuppointaddr = GenXmlFunctions.GetField(rpExtra, "pickuppointaddr");
                _cartInfo.PurchaseInfo.SetXmlProperty("genxml/pickuppointaddr", pickuppointaddr);

                _cartInfo.Save(DebugMode, removeZeroQtyItems);
            }
            if (UserId == -1)
            {
                // no user registered so save address as cookie
                Cookie.SetCookieValue(PortalId, "cartaddress", "billingaddress", _cartInfo.GetBillingAddress().XMLData,  "cartaddress");
            }
        }

        private void UpdateCartAddresses()
        {
            //update address
            _cartInfo.AddShippingAddress(rpAddrS); //add shipping address to cart
            _cartInfo.AddBillingAddress(rpAddrB); //add billing address to cart
        }

        private void UpdateCartInfo()
        {
            //update items
            foreach (RepeaterItem i in rpData.Items)
            {
                var strXml = GenXmlFunctions.GetGenXml(i);
                var cInfo = new NBrightInfo();
                cInfo.XMLData = strXml;
                _cartInfo.MergeCartInputData(cInfo.GetXmlProperty("genxml/hidden/itemcode"), cInfo);
            }
            //update data
            _cartInfo.AddExtraInfo(rpExtra);
            _cartInfo.AddShipData(rpShip);
        }


        private String GetShippingProviderTemplates()
        {
            var strRtn = "";
            var pluginData = new PluginData(PortalSettings.Current.PortalId);
            var provList = pluginData.GetShippingProviders();
            foreach (var d in provList)
            {
                var p = d.Value;
                var shippingkey = p.GetXmlProperty("genxml/textbox/ctrl");
                var shipprov = ShippingInterface.Instance(shippingkey);
                if (shipprov != null)
                {
                    var activeprovider = _cartInfo.GetInfo().GetXmlProperty("genxml/extrainfo/genxml/radiobuttonlist/shippingprovider");
                    // no shipping provider selected, so get the default one.
                    if (activeprovider == "") activeprovider = provList.First().Key;
                    if (activeprovider == d.Key) strRtn += shipprov.GetTemplate(_cartInfo.PurchaseInfo);
                }
            }
            return strRtn;
        }

        #endregion


    }

}
