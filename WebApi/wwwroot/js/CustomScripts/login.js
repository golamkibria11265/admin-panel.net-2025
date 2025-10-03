jQueryAjaxPost = form => {
    try {
        $.ajax({
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            contentType: false,
            processData: false,
            success: function (res) {
                if (res.hasOwnProperty('code') && res.code == 200) {
                    window.location.href = res.redirect_url;
                    res.messages.forEach(element => {
                        toastr.success(element);
                    });
                }
                else if (res.hasOwnProperty('code') && res.code != 200) {
                    res.messages.forEach(element => {
                        toastr.error(element);
                    });
                }
                else {
                    toastr.error('Failed');
                }
            },
            error: function (err) {
                errorResponse(err);
            }
        })
        return false;
    } catch (ex) {
        console.log(ex)
    }
}