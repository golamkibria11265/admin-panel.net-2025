showInPopup = (url, title) => {
    $('#form-modal .modal-body').html('');
    $.ajax({
        type: 'GET',
        url: url,
        success: function (res) {
            // JSON instead of HTML means "session expired" or "access denied".
            if (res && typeof res === 'object') {
                if (res.session_expired) { window.location.href = res.redirect_url; return; }
                (res.messages || ['Failed']).forEach(m => toastr.error(m));
                return;
            }
            $('#form-modal .modal-body').html(res);
            $('#form-modal .modal-title').html(title);
            $('#form-modal').show();
        },
        error: function (err) {
            errorResponse(err);
        }
    })
}
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
                    if ($.fn.DataTable && $.fn.DataTable.isDataTable('#menu_table')) { $('#menu_table').DataTable().ajax.reload(); }
                }
                else
                {
                    if (res.hasOwnProperty('session_expired') && res.session_expired) {
                        window.location.href = res.redirect_url;
                    }
                    else if (res.hasOwnProperty('code') && res.code != 200)
                    {
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
jQueryAjaxDelete = (url, id) => {
    if (confirm('Are you sure to delete this record ?')) {
        try {
            $.ajax({
                type: 'POST',
                url: url,
                data: { id: id },
                contentType: false,
                processData: false,
                success: function (res) {
                    if (res.hasOwnProperty('code') && res.code == 200) {
                        res.messages.forEach(element => {
                            toastr.error(element);
                        });
                        $('#form-modal .modal-body').html('');
                        $('#form-modal .modal-title').html('');
                        $('#form-modal').modal('hide');
                        if ($.fn.DataTable && $.fn.DataTable.isDataTable('#menu_table')) { $('#menu_table').DataTable().ajax.reload(); }
                    }
                    else
                    {
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
                    console.log(err);
                }
            })
        } catch (ex) {
            console.log(ex)
        }
    }
    return false;
}

