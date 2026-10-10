// Shared client-side helpers used by every page:
//  - escapeHtml(): use it whenever a value from the server is put into HTML built as a string.
//  - Adds the anti-forgery token header to every non-GET jQuery AJAX request (the server validates it globally).
//  - errorResponse(): common handler for failed AJAX calls.
(function () {
    'use strict';

    window.escapeHtml = function (value) {
        if (value === null || value === undefined) {
            return '';
        }
        return String(value)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;')
            .replace(/`/g, '&#96;');
    };

    function antiForgeryToken() {
        var input = document.querySelector('input[name="__RequestVerificationToken"]');
        return input ? input.value : '';
    }

    window.errorResponse = function (err) {
        var status = err && err.status;
        var message = 'Something went wrong. Please try again.';
        if (status === 400) {
            message = 'The page has expired. Please reload the page and try again.';
        } else if (status === 401) {
            window.location.href = '/User/Login';
            return;
        } else if (status === 403) {
            message = 'You do not have permission to perform this action.';
        }
        if (window.toastr) {
            toastr.error(message);
        } else {
            console.error(message, err);
        }
    };

    if (window.jQuery) {
        jQuery.ajaxSetup({
            beforeSend: function (xhr, settings) {
                var method = (settings.type || 'GET').toUpperCase();
                if (method !== 'GET' && method !== 'HEAD' && method !== 'OPTIONS' && method !== 'TRACE') {
                    var token = antiForgeryToken();
                    if (token) {
                        xhr.setRequestHeader('RequestVerificationToken', token);
                    }
                }
            }
        });
    }
})();
