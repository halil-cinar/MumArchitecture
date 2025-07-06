(function (global, $) {
    var mum = {};

    var loaderClass = "mum-loader-overlay";

    mum.openModal = function (id) {
        var $modal = $("#" + id);
        if ($modal.length) {
            $modal.modal("show");
        }
    };

    mum.alert = function (options) {
        var o = $.extend({ title: "", message: "", okText: "OK" }, options);
        var id = "mumAlertModal" + Date.now();
        var html = '<div id="' + id + '" class="modal fade" tabindex="-1" role="dialog"><div class="modal-dialog" role="document"><div class="modal-content"><div class="modal-header"><h5 class="modal-title">' + o.title + '</h5><button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button></div><div class="modal-body"><p>' + o.message + '</p></div><div class="modal-footer"><button type="button" class="btn btn-primary" data-bs-dismiss="modal">' + o.okText + '</button></div></div></div></div>';
        $("body").append(html);
        var $modal = $("#" + id);
        $modal.on("hidden.bs.modal", function () { $(this).remove(); });
        $modal.modal("show");
    };

    mum.confirm = function (options) {
        var o = $.extend({ title: "", message: "", okText: "OK", cancelText: "Cancel", onConfirm: function () { }, onCancel: function () { } }, options);
        var id = "mumConfirmModal" + Date.now();
        var html = '<div id="' + id + '" class="modal fade" tabindex="-1" role="dialog"><div class="modal-dialog" role="document"><div class="modal-content"><div class="modal-header"><h5 class="modal-title">' + o.title + '</h5><button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button></div><div class="modal-body"><p>' + o.message + '</p></div><div class="modal-footer"><button type="button" class="btn btn-secondary" data-bs-dismiss="modal">' + o.cancelText + '</button><button type="button" class="btn btn-primary" id="' + id + 'Ok">' + o.okText + '</button></div></div></div></div>';
        $("body").append(html);
        var $modal = $("#" + id);
        $modal.on("click", "#" + id + "Ok", function () { o.onConfirm(); $modal.modal("hide"); });
        $modal.on("click", ".btn-secondary", function () { o.onCancel(); });
        $modal.on("hidden.bs.modal", function () { $(this).remove(); });
        $modal.modal("show");
    };

    mum.redirect = function (url, newTab) {
        if (newTab) {
            window.open(url, "_blank");
        } else {
            window.location.href = url;
        }
    };

    mum.isLoggedIn = function (selector) {
        var val = $(selector).val();
        return !!val;
    };

    var translations = {};
    mum.loadLocalization = function (basePath="") {
        var lang = navigator.language || navigator.userLanguage || "en";
        return $.getScript(basePath + "/" + lang + "/localization.js").done(function () {
            if (typeof localization !== "undefined") {
                translations = localization;
            }
        });
    };

    mum.t = function (key) {
        return translations[key] || key;
    };
    

    mum.notify = function (type, message, opts) {
        if (typeof toastr !== "undefined") {
            toastr.options = $.extend({ timeOut: 3000, positionClass: "toast-bottom-right" }, opts);
            toastr[type](message);
        } else {
            alert(message);
        }
    };

    function showLoader(target) {
        var $t = target ? $(target) : $("body");
        var $overlay = $('<div class="' + loaderClass + '"><div class="spinner-border" role="status"></div></div>');
        $t.append($overlay);
        if (!target) { $overlay.css({ position: "fixed", top: 0, left: 0, width: "100%", height: "100%", "z-index": 1055, background: "rgba(0,0,0,0.3)", display: "flex", "align-items": "center", "justify-content": "center" }); }
        else { $overlay.css({ position: "absolute", top: 0, left: 0, width: "100%", height: "100%", background: "rgba(255,255,255,0.6)", display: "flex", "align-items": "center", "justify-content": "center" }); $t.css("position", "relative"); }
    }

    function hideLoader(target) {
        var $t = target ? $(target) : $("body");
        $t.find("." + loaderClass).remove();
    }

    function handleResponse(data) {
        if (typeof data === "object" && data !== null) {
            if (data.hasOwnProperty("isSuccess") && data.isSuccess === false) {
                if (Array.isArray(data.messages)) {
                    data.messages.forEach(function (m) { mum.notify("error", m); });
                }
                return false;
            }
        }
        return true;
    }

    function ajax(method, url, data, options) {
        var opts = $.extend({ target: null, notify: true }, options);
        showLoader(opts.target);
        return $.ajax({ type: method, url: url, data: data, dataType: "json" }).done(function (resp) {
            hideLoader(opts.target);
            if (!handleResponse(resp) && opts.notify) { return $.Deferred().reject(resp).promise(); }
        }).fail(function (xhr) {
            hideLoader(opts.target);
            if (opts.notify) { mum.notify("error", xhr.statusText || "Error"); }
        });
    }

    mum.get = function (url, data, options) { return ajax("GET", url, data, options); };
    mum.post = function (url, data, options) { return ajax("POST", url, data, options); };

    mum.showLoader = showLoader;
    mum.hideLoader = hideLoader;

    mum.initSignalR = function (hubUrl, handlers) {
        var connection = new signalR.HubConnectionBuilder().withUrl(hubUrl).build();
        if (handlers) {
            Object.keys(handlers).forEach(function (k) { connection.on(k, handlers[k]); });
        }
        connection.start().catch(function (err) { mum.notify("error", err.toString()); });
        return connection;
    };

    mum.sendSignalR = function (connection, method, data) {
        if (connection && connection.invoke) {
            connection.invoke(method, data).catch(function (err) { mum.notify("error", err.toString()); });
        }
    };

    $(function () {
        $(document).ajaxError(function (event, jqxhr) {
            mum.notify("error", jqxhr.statusText || "Ajax Error");
        });
    });

    global.mum = mum;

})(window, jQuery);
// -------------------------------
// API Reference & Usage Examples
// -------------------------------
// openModal(id:string):void
//   Kullanım: mum.openModal('myModal');
// alert(options:object):void
//   Kullanım: mum.alert({ title: 'Bilgi', message: 'Kaydedildi', okText: 'Tamam' });
// confirm(options:object):void
//   Kullanım: mum.confirm({ title: 'Sil', message: 'Emin misiniz?', okText: 'Evet', cancelText: 'Hayır', onConfirm: function () { console.log('silindi'); } });
// redirect(url:string, newTab:boolean):void
//   Kullanım: mum.redirect('/home', false);
//            mum.redirect('https://example.com', true);
// isLoggedIn(selector:string):boolean
//   Kullanım: if (mum.isLoggedIn('#userIdInput')) { console.log('Giriş var'); }
// loadLocalization(basePath:string):jqXHR
//   Kullanım: mum.loadLocalization('/i18n').done(function () { console.log(mum.t('welcome')); });
// t(key:string):string
//   Kullanım: var txt = mum.t('welcome');
// notify(type:string, message:string, opts?:object):void
//   Kullanım: mum.notify('success', 'İşlem başarılı');
// showLoader(target?:string):void
//   Kullanım: mum.showLoader('#panel');
// hideLoader(target?:string):void
//   Kullanım: mum.hideLoader('#panel');
// get(url:string, data:any, options?:object):jqXHR
//   Kullanım: mum.get('/api/items', { id: 1 }, { target: '#panel' }).done(function (r) { console.log(r); });
// post(url:string, data:any, options?:object):jqXHR
//   Kullanım: mum.post('/api/save', { name: 'test' }, { target: null });
// initSignalR(hubUrl:string, handlers:object):HubConnection
//   Kullanım: var conn = mum.initSignalR('/chatHub', { ReceiveMessage: function (u, m) { mum.notify('info', u + ': ' + m); } });
// sendSignalR(connection:HubConnection, method:string, data:any):void
//   Kullanım: mum.sendSignalR(conn, 'SendMessage', { text: 'Merhaba' });
