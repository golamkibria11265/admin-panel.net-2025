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
                    res.messages.forEach(element => {
                        toastr.success(element);
                    });
                    $('#form-modal .modal-body').html('');
                    $('#form-modal .modal-title').html('');
                    $('#form-modal').hide();
                }
                else {
                    if (res.hasOwnProperty('session_expired') && res.session_expired) {
                        window.location.href = res.redirect_url;
                    }
                    else if (res.hasOwnProperty('code') && res.code != 200) {
                        res.messages.forEach(element => {
                            toastr.error(element);
                        });
                    }
                    else {
                        toastr.error('Failed');
                        $('#form-modal .modal-body').html(res.html);
                    }
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