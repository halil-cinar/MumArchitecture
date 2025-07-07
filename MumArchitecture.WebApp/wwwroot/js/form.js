(function () {
    function initSelects(scope) { if (window.jQuery && $.fn.select2) $(scope).find('.select2').select2({ width: '100%' }); }
    function initSummernote(scope) { if (window.jQuery && $.fn.summernote) $(scope).find('.summernote').summernote({ height: 200 }); }
    function initDatetime(scope) {
        if (window.flatpickr) {
            flatpickr('.datetime-picker', { enableTime: true, dateFormat: 'Y-m-d H:i:S' });
            flatpickr('.date-picker', { enableTime: false, dateFormat: 'Y-m-d' });
        }
    }
    function initFilePond(scope) {
        if (!window.FilePond) return;
        Array.from(scope.querySelectorAll('input.filepond')).forEach(function (input) {
            var files =
                (input.dataset.existing || '')
                    .split('|')
                    .filter(Boolean)
                    .map(function (u) { return { source: u, options: { type: 'local' } }; });
            const formEl = input.closest('form');
            var pond = FilePond.create(input, {
                allowMultiple: input.hasAttribute('multiple'),
                allowReorder: input.hasAttribute('multiple'),
                allowRevert: true,
                allowVideoPreview: true,
                allowAudioPreview: true,
                allowPdfPreview: true,
                pdfPreviewHeight: 240, 
                files: files,
                credits: false,
                labelIdle: 'Dosyaları sürükleyin veya <span class="filepond--label-action">tıklayın</span>'
            });

            pond.on('addfile', function () { markChanged(formEl, input.name); });
            pond.on('removefile', function () { markChanged(formEl, input.name); });
        });
    }

    function showToast(message, success) {
        var toastEl = document.createElement('div');
        toastEl.className = 'toast align-items-center text-white ' + (success ? 'bg-success' : 'bg-danger') + ' border-0';
        toastEl.role = 'alert';
        toastEl.innerHTML = '<div class="d-flex"><div class="toast-body">' + message + '</div><button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button></div>';
        document.body.appendChild(toastEl);
        new bootstrap.Toast(toastEl, { delay: 5000 }).show();
        toastEl.addEventListener('hidden.bs.toast', function () { toastEl.remove(); });
    }
    function populateData(form, data) {
        for (const key in data) {
            const fieldName = key.charAt(0).toUpperCase() + key.slice(1);
            const el = form.querySelector('[name="' + fieldName + '"]');
            if (!el) continue;
            let value = data[key];

            if (window.jQuery && $(el).hasClass('select2')) {
                if (el.multiple && !Array.isArray(value))
                    value = typeof value === 'string' ? value.split(',').map(v => v.trim()) : [];
                $(el).val(value).trigger('change.select2');
                continue;
            }

            if (el.type === 'checkbox' || el.type === 'radio') {
                el.checked = value === true || value === 'true' || value === 1 || value === '1';
                continue;
            }

            if (el.classList.contains('summernote') && window.jQuery) {
                $(el).summernote('code', value || '');
                continue;
            }

            el.value = value ?? '';
        }
    }

    function hookFileInputs(form) {
        form.querySelectorAll('.file-drop-zone').forEach(function (zone) {
            var input = zone.querySelector('.file-input');
            var inner = zone.querySelector('.file-drop-zone-inner');
            var name = zone.dataset.name;
            function renderFiles(files) { inner.innerHTML = ''; Array.from(files).forEach(function (f) { var card = document.createElement('div'); card.className = 'file-card'; card.textContent = f.name; inner.appendChild(card); }); }
            zone.addEventListener('click', function () { input.click(); });
            zone.addEventListener('dragover', function (e) { e.preventDefault(); zone.classList.add('drag'); });
            zone.addEventListener('dragleave', function () { zone.classList.remove('drag'); });
            zone.addEventListener('drop', function (e) { e.preventDefault(); zone.classList.remove('drag'); input.files = e.dataTransfer.files; renderFiles(input.files); markChanged(form, name); });
            input.addEventListener('change', function () { renderFiles(input.files); markChanged(form, name); });
        });
    }
    function markChanged(form, name) {
        var hidden = form.querySelector('input[name="MediaChanged"]');
        if (!hidden) { hidden = document.createElement('input'); hidden.type = 'hidden'; hidden.name = 'MediaChanged'; hidden.value = ''; form.appendChild(hidden); }
        var list = hidden.value ? hidden.value.split(',') : [];
        if (!list.includes(name)) list.push(name);
        hidden.value = list.join(',');
    }
    function clearEnhanced(f) {
        if (window.jQuery) {
            $(f).find('.select2').val(null).trigger('change');
            $(f).find('.summernote').each(function () { $(this).summernote('code', ''); });
        }
        if (window.FilePond) {
            f.querySelectorAll('input.filepond').forEach(function (inp) {

                let pond =
                    (typeof FilePond.find === 'function' && FilePond.find(inp)) ||   // v4+
                    inp.filepond ||                                                 // bazı paketlerde
                    inp._pond;                                                      // eski versiyonlar

                if (!pond || typeof pond.removeFiles !== 'function') return;

                pond.removeFiles();

                //const existing = (inp.dataset.existing || '')
                //    .split('|')
                //    .filter(Boolean)
                //    .map(u => ({ source: u, options: { type: 'local' } }));
                //if (existing.length) pond.setOptions({ files: existing });
                
            });
        }
    }
    function submitHandler(e) {
        e.preventDefault();
        const form = e.target;
        const data = new FormData(form);

        form.querySelectorAll('input[type="checkbox"]').forEach(el => {
            if (!data.has(el.name)) data.append(el.name, el.checked ? 'true' : 'false');
        });

        fetch(form.getAttribute('action'), {
            method: form.getAttribute('method') || 'POST',
            body: data
        })
            .then(r => r.json())
            .then(j => {
                if (j.success) showToast(j.message || 'Başarılı', true);
                else showToast(j.message || 'Hata', false);
                if (j.redirectUrl) window.location.href = j.redirectUrl;
            })
            .catch(() => showToast('İşlem başarısız', false));

        const onsubmit = form.dataset.onsubmit;
        if (onsubmit && typeof window[onsubmit] === 'function') window[onsubmit](form);
    }

    function initAjaxForms(root) {
        root = root || document;
        root.querySelectorAll('form.ajax-form').forEach(function (f) {
            f.addEventListener('submit', submitHandler, false);
            f.addEventListener('reset', function () { setTimeout(function () { clearEnhanced(f); }, 0); }, false);

            if (f.dataset.getUrl) { fetch(f.dataset.getUrl).then(r => r.json()).then(d => populateData(f, d.data)); }
            initSelects(f); initSummernote(f); initDatetime(f); hookFileInputs(f); initFilePond(f);
        });
    }
    document.addEventListener('DOMContentLoaded', function () { initAjaxForms(document); });
    window.ajaxFormHelper = { initAjaxForms: initAjaxForms };
})();
