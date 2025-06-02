var ModalFormManager = (function () {
    var config = {};

    // Varsayılan ayarlar; init ile üzerine yazılabilir
    var defaults = {
        modalSelector: '#deneme_modal',
        formSelector: 'form',
        triggerAttr: 'data-id',
        getUrlAttr: 'data-get-url',
        saveUrlAttr: 'data-save-url',
        saveButtonSelector: '.btn-primary'
    }

    function init(options) {

        config = $.extend({}, defaults, options);
        console.log(config)
        config.modal = $(config.modalSelector);
        config.form = config.modal.find(config.formSelector);
        //config.getUrl = config.modal.data(config.getUrlAttr.replace('data-', ''));
        //config.saveUrl = config.modal.data(config.saveUrlAttr.replace('data-', ''));
        console.log("dd")
        bindShowEvent();
        bindSaveEvent();
    }

    function bindShowEvent() {
        var myModalEl = document.querySelector(config.modalSelector)
        myModalEl.addEventListener('show.bs.modal', function (e) {
            console.log('Modal açılıyor:', config.modalSelector, 'tetikleyen:', e.relatedTarget);
            const id = $(e.relatedTarget).attr(config.triggerAttr);
            console.log(id)
            if (!id) return console.warn('Trigger\'da data-id yok');

            // her açılışta güncel modal ve form referansını al
            config.modal = $(config.modalSelector);
            config.form = config.modal.find(config.formSelector);
           console.log(config.form)

            loadData(id);
        })
    }

    function loadData(id) {
        console.log(config)
        $.getJSON(config.getUrl, { id })
            .done((data) => {
                if (!data.success) {
                    throw new Error(data.Messages?.map(m => m.Message).join(', ') || 'Bir hata oluştu');
                }
                populateFields(data.data);
            })
            .fail((_, status, err) => console.error('Veri çekme hatası:', status, err));
    }

    function populateFields(data) {
        // her [name] input/select/textarea alanına tek tek değer ata
        $.each(data, (key, val) => {
            key = key[0].toUpperCase() + key.substring(1)
            console.log(key)    
            const $field = config.form.find(`[name="${key}"]`);
            if (!$field.length) return;

            if ($field.is('select')) {
                $field.val(val).trigger('change');
            }
            else if ($field.is(':checkbox, :radio')) {
                $field.prop('checked', Boolean(val));
            }
            else {
                $field.val(val);
            }
        });
    }

    function bindSaveEvent() {
        config.modal.find(config.saveButtonSelector)
            .on('click', () => saveData());
    }

    function saveData() {
        const formData = new FormData(config.form[0]);
        $.ajax({
            url: config.saveUrl,
            method: 'POST',
            data: formData,
            processData: false,
            contentType: false
        })
            .done((res) => {
                if (!res.success) {
                    onSaveError({ responseJSON: res.messages });
                    return;
                }
                onSaveSuccess(res);
            })
            .fail((xhr, status, err) => {
                onSaveError(xhr, status, err);
            });
    }

    function onSaveSuccess(res) {
        // default: modal kapat ve sayfayı yenile
        config.modal.modal('hide');
        location.reload();
    }

    function onSaveError(xhr) {
        // örnek: validation hatalarını form altında göster
        let errors = xhr.responseJSON || {};
        console.error('Kaydetme hatası:', errors);
        displayValidationErrors(errors);
    }

    function displayValidationErrors(errors) {
        // önce eski mesajları temizle
        config.form.find('.is-invalid').removeClass('is-invalid');
        config.form.find('.invalid-feedback').remove();

        if (Array.isArray(errors)) {
            // Handle SystemResult messages
            errors.forEach(message => {
                const $feedback = $('<div>').addClass('alert alert-danger')
                    .text(message.Message);
                config.form.prepend($feedback);
            });
            return;
        }

        $.each(errors, (fieldName, messages) => {
            const $field = config.form.find(`[name="${fieldName}"]`);
            if (!$field.length) return;

            $field.addClass('is-invalid');
            const $feedback = $('<div>').addClass('invalid-feedback')
                .text(messages.join(' '));
            $field.closest('.col-md-6, .form-group').append($feedback);
        });
    }

    return { init };
})(jQuery);
