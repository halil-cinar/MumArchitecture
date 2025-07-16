const AjaxJsonForm = {
    init: function (selector) {
        const $f = $(selector);
        this.bindArrays($f);
        var dataText = $f.data("value");
        console.log(dataText)
        if (typeof (dataText) != "object") {
            dataText=JSON.parse(dataText)
        }
        if (dataText) this.populate($f, dataText);
        $f.on("submit", function (e) {
            e.preventDefault();
            AjaxJsonForm.collect($f).then(function (payload) {
                const entityId = $f.data("entity-id") || $f.find("[name=entityId]").val();
                console.log({ id: entityId, value: payload })
                mum.post($f.attr("action"), { id: entityId, value: JSON.stringify(payload) }, null)
            })
        });
    },
    getToastType: function (priority, success) {
        switch (priority) {
            case 0: return success ? "success" : "info";
            case 1: return "info";
            case 2: return "warning";
            case 3: return "error";
            case 4: return "error";
            default: return "info";
        }
    },
    showMessages: function (result) {
        if (!result || !result.messages) return;
        result.messages.forEach(function (m) {
            toastr[AjaxJsonForm.getToastType(m.priority, result.success)](m.message);
        });
    },
    bindArrays: function ($form) {
        $form.find(".array-add").off("click").on("click", function () {
            const targetId = $(this).data("target");
            const tplHtml = $("#tpl-" + targetId.replace("array-", "")).html();
            const index = $("#" + targetId).children().length;
            const html = tplHtml.replace(/__index__/g, index);
            const $item = $('<div class="array-item border p-2 mb-2 position-relative"></div>').append(html);
            $("#" + targetId).append($item);
            $item.find(".array-remove").on("click", function () { $(this).closest(".array-item").remove(); });
        });
    },
    populate: function ($form, data) {
        const setVal = (prefix, obj) => {
            for (const k in obj) {
                const v = obj[k];
                const path = prefix ? prefix + "." + k : k;
                if (Array.isArray(v)) {
                    v.forEach((el, i) => {
                        $form.find(`.array-add[data-name='${path}']`).click();
                        setVal(`${path}[${i}]`, el);
                    });
                } else if (typeof v === "object" && v !== null) {
                    setVal(path, v);
                } else {
                    const $el = $form.find(`[name='${path}']`);
                    if ($el.length && $el.attr("type") === "file") {
                        AjaxJsonForm.renderExistingFile(path, v, $el);
                    } else {
                        if ($el.attr("type") === "checkbox") $el.prop("checked", !!v);
                        else if ($el.attr("type") !== "file") $el.val(v);
                    }
                }
            }
        };
        setVal("", data);
    },
    fileToBase64: function (file) {
        return new Promise(function (resolve, reject) {
            const reader = new FileReader();
            reader.onload = function () { resolve(reader.result); };
            reader.onerror = reject;
            reader.readAsDataURL(file);
        });
    },
    renderExistingFile: function (path, val, $input) {
        const filename = val.split("/").pop();
        const $preview = $(`<div class="d-flex align-items-center gap-2 mb-2 file-preview" data-name="${path}"><a href="${val}" target="_blank">${filename.length > 50 ? "Dosya bulunmaktadır" : filename}</a><button type="button" class="btn btn-sm btn-outline-danger file-remove">×</button></div>`);
        $input.after($preview);
        const $hidden = $(`<input type="hidden" class="file-existing" name="${path}$existing" value="${val}">`);
        $input.after($hidden);
        $preview.find(".file-remove").on("click", function () {
            $preview.remove();
            $hidden.attr("data-removed", "1");
        });
    },
    collect: function ($form) {
        return new Promise(function (resolve) {
            const result = {};
            const setDeep = (o, parts, val) => {
                const part = parts.shift();
                const m = part.match(/(.*)\[(\d+)\]/);
                if (m) {
                    const key = m[1], idx = parseInt(m[2]);
                    if (!o[key]) o[key] = [];
                    if (parts.length === 0) o[key][idx] = val;
                    else {
                        if (!o[key][idx]) o[key][idx] = {};
                        setDeep(o[key][idx], parts, val);
                    }
                } else {
                    if (parts.length === 0) o[part] = val;
                    else {
                        if (!o[part]) o[part] = {};
                        setDeep(o[part], parts, val);
                    }
                }
            };
            $form.serializeArray().forEach(i => {
                if (i.name !== "entityId" && !i.name.endsWith("$existing")) setDeep(result, i.name.split("."), i.value);
            });
            $form.find("input[type=checkbox]").each(function () {
                setDeep(result, $(this).attr("name").split("."), $(this).is(":checked"));
            });
            const filePromises = [];
            $form.find("input[type=file]").each(function () {
                const path = $(this).attr("name");
                const files = this.files;
                if (files && files.length > 0) {
                    if (files.length === 1) {
                        filePromises.push(AjaxJsonForm.fileToBase64(files[0]).then(b64 => setDeep(result, path.split("."), b64)));
                    } else {
                        Array.from(files).forEach((f, idx) => {
                            filePromises.push(AjaxJsonForm.fileToBase64(f).then(b64 => setDeep(result, `${path}[${idx}]`.split("."), b64)));
                        });
                    }
                } else {
                    const $hidden = $form.find(`input[name='${path}$existing']`);
                    if ($hidden.length && $hidden.attr("data-removed") !== "1") {
                        setDeep(result, path.split("."), $hidden.val());
                    }
                }
            });
            Promise.all(filePromises).then(() => resolve(result));
        });
    },
};
