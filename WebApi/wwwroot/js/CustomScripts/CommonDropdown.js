function GetDropdown(url, selector, type, select = null) {
    $.ajax({
        type: 'GET',
        url: url,
        async: false,
        success: function (result) {
            var options = $(selector);
            if (type != "") {
                options.append($("<option />").val(0).text('--- Select ' + type + ' ---'));
            }
            $.each(result, function (index, item) {
                var selected = "";
                if (item.value == select) {
                    selected = "selected"
                }
                options.append($("<option " + selected + "/>").val(item.value).text(item.text));
            });
            var settings = {};
/*            new TomSelect(options, settings);*/
        }
    });
}
function GetDropdownWithEmptySelector(url, selector, type) {
    console.log('Url: ', url);
    $.ajax({
        type: 'GET',
        url: url,
        async: false,
        success: function (result) {
            var options = $(selector);
            if (type != "") {
                options.append($("<option />").val('').text('--- Select ' + type + ' ---'));
            }
            $.each(result, function (index, item) {
                options.append($("<option />").val(item.value).text(item.text));
            });
            var settings = {};
/*            new TomSelect(options, settings);*/
        }
    });
}
