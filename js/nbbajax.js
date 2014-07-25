
var nbxrtn = 'input[id*="nbxrtn"]';
var nbxaction = 'input[id*="nbxaction"]';
var nbxprocessing = 'input[id*="nbxprocessing"]';

function nbxonclick(selector,cmd,selformdiv,action)
{
	$(selector).click(function () {
		nbxget(cmd,selformdiv,action);
	});
}
function nbxonchange(selector,cmd,selformdiv,action)
{		 
	$(selector).change(function () {
		nbxget(cmd,selformdiv,action);
	});
}

function nbxget(cmd,selformdiv,action)
{		 
    $.ajaxSetup({ cache: false });

	$(nbxprocessing).val("BEFORE").trigger('change');

	// set the nbxaction field to action, so we know which ajax action is processing.
	$('input[id*="nbxaction"]').val(action)
	
	var cmdupdate = '/DesktopModules/NBright/NBrightBuy/XmlConnector.ashx?cmd=' + cmd;
	var values = $.fn.genxmlajax(selformdiv);
	var request = $.ajax({ type: "POST",
		url: cmdupdate,
		cache: false,
		data: { inputxml: encodeURI(values) }		
	});
	
	request.done(function (data) {
		if (data != 'noaction') $(nbxrtn).val(data).trigger('change');
		$(nbxprocessing).val("AFTER").trigger('change');
});

	request.fail(function (jqXHR, textStatus) {
		alert("Request failed: " + textStatus);
	});
}

	function nbxcheckifnull(selector,searchtoken)
	{ // check if the value is empty, if so do NOT build the SQL.  Add it to disabledlist token.
		var disabledlist = $("input[id*='disabledsearchtokens']").val();
		if ($(selector).val() == '')
			disabledlist = disabledlist.replace(';' + searchtoken,'') + ';' + searchtoken;
		else
			disabledlist = disabledlist.replace(';' + searchtoken,'');
		$("input[id*='disabledsearchtokens']").val(disabledlist);
	}

	function nbxcheckifselected(selector,searchtoken)
	{ // check if checboxlist is selected. If not, do not build the SQL.  Add it to disabledlist token.
		var disabledlist = $("input[id*='disabledsearchtokens']").val();
		var sel = false;
		$(selector).each(function() { 
			if (this.checked)
			{	
				sel = true; 
				return false;
			}
        });	
		if (sel)
			disabledlist = disabledlist.replace(';' + searchtoken,'');
		else
			disabledlist = disabledlist.replace(';' + searchtoken,'') + ';' + searchtoken;
		$("input[id*='disabledsearchtokens']").val(disabledlist);
	}

