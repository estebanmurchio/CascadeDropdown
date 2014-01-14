using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

namespace Depofis.Infrastructure.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class CascadeDropdownAttribute : Attribute, IMetadataAware
    {
        public string GetChildsByParentApiUrl { get; private set; }
        public string ParentDropdownSelector { get; private set; }
        public string ChildDropdownSelector { get; private set; }

        public string ChildIdPropertyName { get; private set; }
        public string ChildNamePropertyName { get; private set; }

        public string UiHint { get; private set; }

        public string SelectText { get; private set; }
        public string TriggerJQueryChosenEvent { get; private set; }

        public CascadeDropdownAttribute(string getChildsByParentApiUrl, string parentDropdownSelector, string childDropdownSelector, string childIdPropertyName, string childNamePropertyName, string uiHint)
        {
            GetChildsByParentApiUrl = getChildsByParentApiUrl;
            ParentDropdownSelector = parentDropdownSelector;
            ChildDropdownSelector = childDropdownSelector;

            ChildIdPropertyName = childIdPropertyName;
            ChildNamePropertyName = childNamePropertyName;

            UiHint = uiHint;

            SelectText = "Seleccione...";
            TriggerJQueryChosenEvent = "liszt:updated";
        }

        #region Script
        private const string ScriptText =
            "<script data-eval='true' type='text/javascript'>" +
                "jQuery(document).ready(function () {" +

                    "jQuery('{0}').change(function() {" +
                        "var self = this;" +
                        "var selectedValue = jQuery(self).val();" +

                        "jQuery('{1}').attr('disabled', 'disabled');" +
                        "jQuery('{1}').empty();" +
                        "jQuery('{1}').append(jQuery('<option/>', { text: '{2}' }));" +
                        "jQuery('{1}').trigger('{3}');" +

                        "if(selectedValue != null && selectedValue != '') {" +
                            "jQuery.getJSON('{4}', { id: selectedValue }, function(collection) {" +
                                "jQuery.each(collection, function(index, item) {" +
                                    "jQuery('{1}').append(jQuery('<option/>', {" +
                                        "value: item.{5}," +
                                        "text: item.{6}" +
                                    "}));" +
                                "});" +

                                "jQuery('{1}').removeAttr('disabled');" +
                                "jQuery('{1}').val(parseInt(sessionStorage['{1}']));" +
                                "sessionStorage.removeItem('{1}');" +
                                "jQuery('{1}').change();" +
                                "jQuery('{1}').trigger('{3}');" +
                            "});" +
                        "}" +
                    "});" +
                    "sessionStorage['{1}'] = jQuery('{1}').val();" +
                    "jQuery('{0}').change();" +
                "});" +
            "</script>";
        #endregion

        //public string TemplateHint = UiHint;

        internal HttpContextBase Context
        {
            get { return new HttpContextWrapper(HttpContext.Current); }
        }

        public void OnMetadataCreated(ModelMetadata metadata)
        {
            var list = Context.Items["Scripts"] as IList<string> ?? new List<string>();

            metadata.TemplateHint = UiHint;
            metadata.AdditionalValues[UiHint] = string.Format("{0}-{1}", ParentDropdownSelector, ChildDropdownSelector);

            var s = ScriptText
                .Replace("{0}", ParentDropdownSelector)
                .Replace("{1}", ChildDropdownSelector)
                .Replace("{2}", SelectText)
                .Replace("{3}", TriggerJQueryChosenEvent)
                .Replace("{4}", GetChildsByParentApiUrl)
                .Replace("{5}", ChildIdPropertyName)
                .Replace("{6}", ChildNamePropertyName);

            if (!list.Contains(s))
                list.Add(s);

            Context.Items["Scripts"] = list;
        }
    }
}
