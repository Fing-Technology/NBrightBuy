﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Routing;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using DotNetNuke.Collections;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Services.Localization;
using NBrightCore.common;
using NBrightCore.providers;
using NBrightCore.render;
using NBrightDNN;
using NBrightDNN.render;
using Nevoweb.DNN.NBrightBuy.Components;
using RazorEngine.Templating;
using RazorEngine.Text;

namespace NBrightBuy.render
{
    public class NBrightBuyRazorTokens<T> : RazorEngineTokens<T>
    {

        #region "NBS Action tokens"

        public IEncodedString AddToBasketButton(NBrightInfo info, String xpath, String attributes = "", String defaultValue = "")
        {
            if (attributes.StartsWith("ResourceKey:")) attributes = ResourceKey(attributes.Replace("ResourceKey:", "")).ToString();
            if (defaultValue.StartsWith("ResourceKey:")) defaultValue = ResourceKey(defaultValue.Replace("ResourceKey:", "")).ToString();

            var id = xpath.Split('/').Last();
            var value = info.GetXmlProperty(xpath);
            if (value == "") value = defaultValue;
            var strOut = "<input value='" + value + "' id='" + id + "' " + attributes + "  type='text' />";

            return new RawString(strOut);
        }

        #endregion

        #region "NBS display tokens"

        #region "products"

        public IEncodedString ProductName(NBrightInfo info)
        {
            return new RawString(info.GetXmlProperty("genxml/lang/genxml/textbox/txtproductname"));
        }

        public IEncodedString ProductImage(NBrightInfo info, String width = "150", String height = "0", String idx = "1", String attributes = "")
        {
            var imagesrc = info.GetXmlProperty("genxml/imgs/genxml[" + idx + "]/hidden/imageurl");
            var url = StoreSettings.NBrightBuyPath() + "/NBrightThumb.ashx?src=" + imagesrc + "&w=" + width + "&h=" + height +   attributes;
            return new RawString(url);
        }

        public IEncodedString EditUrl(NBrightInfo info, NBrightRazor model)
        {
            var entryid = info.ItemID;
            var url = "Unable to find BackOffice Setting, go into Back Office settings and save.";
            if (entryid > 0 && StoreSettings.Current.GetInt("backofficetabid") > 0)
            {
                var param = new List<String>();

                param.Add("eid=" + entryid.ToString(""));
                param.Add("ctrl=products");
                param.Add("rtntab=" + PortalSettings.Current.ActiveTab.TabID.ToString());
                if (model.GetSetting("moduleid") != "") param.Add("rtnmid=" + model.GetSetting("moduleid").Trim());
                if (model.GetUrlParam("page") != "") param.Add("PageIndex=" + model.GetUrlParam("page").Trim());
                if (model.GetUrlParam("catid") != "") param.Add("catid=" + model.GetUrlParam("catid").Trim());

                var paramlist = new string[param.Count];
                for (int lp = 0; lp < param.Count; lp++)
                {
                    paramlist[lp] = param[lp];
                }
                
                url = Globals.NavigateURL(StoreSettings.Current.GetInt("backofficetabid"), "", paramlist);
            }
            return new RawString(url);
        }

        #endregion

        #region "categories"

        public IEncodedString Category(String fieldname)
        {
            var strOut = "";
            try
            {
                // if we have no catid in url, we're going to need a default category from module.
                var grpCatCtrl = new GrpCatController(Utils.GetCurrentCulture());
                var objCInfo = grpCatCtrl.GetCategory(StoreSettings.Current.ActiveCatId);
                if (objCInfo != null)
                {
                    GroupCategoryData objPcat;
                    switch (fieldname.ToLower())
                    {
                        case "categorydesc":
                            strOut = objCInfo.categorydesc;
                            break;
                        case "message":
                            strOut = System.Web.HttpUtility.HtmlDecode(objCInfo.message);
                            break;
                        case "archived":
                            strOut = objCInfo.archived.ToString(CultureInfo.InvariantCulture);
                            break;
                        case "breadcrumb":
                            strOut = objCInfo.breadcrumb;
                            break;
                        case "categoryid":
                            strOut = objCInfo.categoryid.ToString("");
                            break;
                        case "categoryname":
                            strOut = objCInfo.categoryname;
                            break;
                        case "categoryref":
                            strOut = objCInfo.categoryref;
                            break;
                        case "depth":
                            strOut = objCInfo.depth.ToString("");
                            break;
                        case "disabled":
                            strOut = objCInfo.disabled.ToString(CultureInfo.InvariantCulture);
                            break;
                        case "entrycount":
                            strOut = objCInfo.entrycount.ToString("");
                            break;
                        case "grouptyperef":
                            strOut = objCInfo.grouptyperef;
                            break;
                        case "imageurl":
                            strOut = objCInfo.imageurl;
                            break;
                        case "ishidden":
                            strOut = objCInfo.ishidden.ToString(CultureInfo.InvariantCulture);
                            break;
                        case "isvisible":
                            strOut = objCInfo.isvisible.ToString(CultureInfo.InvariantCulture);
                            break;
                        case "metadescription":
                            strOut = objCInfo.metadescription;
                            break;
                        case "metakeywords":
                            strOut = objCInfo.metakeywords;
                            break;
                        case "parentcatid":
                            strOut = objCInfo.parentcatid.ToString("");
                            break;
                        case "parentcategoryname":
                            objPcat = grpCatCtrl.GetCategory(objCInfo.parentcatid);
                            strOut = objPcat.categoryname;
                            break;
                        case "parentcategoryref":
                            objPcat = grpCatCtrl.GetCategory(objCInfo.parentcatid);
                            strOut = objPcat.categoryref;
                            break;
                        case "parentcategorydesc":
                            objPcat = grpCatCtrl.GetCategory(objCInfo.parentcatid);
                            strOut = objPcat.categorydesc;
                            break;
                        case "parentcategorybreadcrumb":
                            objPcat = grpCatCtrl.GetCategory(objCInfo.parentcatid);
                            strOut = objPcat.breadcrumb;
                            break;
                        case "parentcategoryguidkey":
                            objPcat = grpCatCtrl.GetCategory(objCInfo.parentcatid);
                            strOut = objPcat.categoryrefGUIDKey;
                            break;
                        case "recordsortorder":
                            strOut = objCInfo.recordsortorder.ToString("");
                            break;
                        case "seoname":
                            strOut = objCInfo.seoname;
                            if (strOut == "") strOut = objCInfo.categoryname;
                            break;
                        case "seopagetitle":
                            strOut = objCInfo.seopagetitle;
                            break;
                        case "url":
                            strOut = objCInfo.url;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                strOut = ex.ToString();
            }

            return new RawString(strOut);
        }

        public IEncodedString CategoryBreadCrumb(Boolean includelinks, Boolean aslist = true, int tabRedirect = -1, String separator = "", int wordlength = -1, int maxlength = 400)
        {
            var strOut = "";

            try
            {
                var grpCatCtrl = new GrpCatController(Utils.GetCurrentCulture());
                var objCInfo = grpCatCtrl.GetCategory(StoreSettings.Current.ActiveCatId);
                if (objCInfo != null)
                {

                    if (StoreSettings.Current.ActiveCatId > 0) // check we have a catid
                    {
                        if (includelinks)
                        {
                            if (tabRedirect == -1) tabRedirect = PortalSettings.Current.ActiveTab.TabID;
                            strOut = grpCatCtrl.GetBreadCrumbWithLinks(StoreSettings.Current.ActiveCatId, tabRedirect, wordlength, separator, aslist);
                        }
                        else
                        {
                            strOut = grpCatCtrl.GetBreadCrumb(StoreSettings.Current.ActiveCatId, wordlength, separator, aslist);
                        }

                        if ((strOut.Length > maxlength) && (!aslist))
                        {
                            strOut = strOut.Substring(0, (maxlength - 3)) + "...";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                strOut = ex.ToString();
            }


            return new RawString(strOut);
        }

        #endregion

        #region "Functional"

        public IEncodedString EntryUrl(NBrightInfo info, NBrightRazor model, Boolean relative = true, String categoryref = "")
        {
            var url = "";
            try
            {
                var urlname = info.GetXmlProperty("genxml/lang/genxml/textbox/txtseoname");
                if (urlname == "") urlname = info.GetXmlProperty("genxml/lang/genxml/textbox/txtproductname");

                    // see if we've injected a categoryid into the data class, this is done in the case of the categorymenu when displaying products.
                var categoryid = info.GetXmlProperty("genxml/categoryid");
                if (categoryid == "") categoryid = StoreSettings.Current.ActiveCatId.ToString();
                if (categoryid == "0") categoryid = ""; // no category active if zero

                url = NBrightBuyUtils.GetEntryUrl(PortalSettings.Current.PortalId, info.ItemID.ToString(), model.GetSetting("detailmodulekey"), urlname, model.GetSetting("ddldetailtabid"), categoryid, categoryref);
                if (relative) url = Utils.GetRelativeUrl(url);

            }
            catch (Exception ex)
            {
                url = ex.ToString();
            }

            return new RawString(url);
        }

        public IEncodedString EntryReturnUrl(NBrightRazor model)
        {
            var product = (ProductData)model.List.First();
            var entryid = product.Info.ItemID;
            var url = "";

            var param = new List<String>();
            if (model.GetUrlParam("page") != "") param.Add("PageIndex=" + model.GetUrlParam("page").Trim());
            if (model.GetUrlParam("catid") != "") param.Add("catid=" + model.GetUrlParam("catid").Trim());
            var listtab = model.GetUrlParam("rtntab");
            if (listtab == "") listtab = StoreSettings.Current.Get("productlisttab");
            var intlisttab = StoreSettings.Current.ActiveCatId;
            if (Utils.IsNumeric(listtab)) intlisttab = Convert.ToInt32(listtab);

            var paramlist = new string[param.Count];
            for (int lp = 0; lp < param.Count; lp++)
            {
                paramlist[lp] = param[lp];
            }

            url = Globals.NavigateURL(intlisttab, "", paramlist);
            return new RawString(url);
        }


        public IEncodedString CurrencyOf(Double x)
        {
            var strOut = NBrightBuyUtils.FormatToStoreCurrency(x);
            return new RawString(strOut);
        }


        #endregion

        #endregion

    }


}