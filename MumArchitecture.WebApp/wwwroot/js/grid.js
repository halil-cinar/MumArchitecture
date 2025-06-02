var Grid = (function () {
    // Her grid için ayrı config objeleri saklanacak
    var instances = {};

    // Grid başlatma
    function init(selector, url) {
        var id = selector.startsWith('#') ? selector.slice(1) : selector;
        var wrapper = document.querySelector('#' + id).closest('.table-responsive');
        instances[id] = {
            id: id,
            url: url,
            currentPage: 0,
            $wrapper: wrapper,
            $tableBody: wrapper.querySelector('table#' + id + ' tbody'),
            $pagination: document.getElementById(id + '_pagination'),
            colfors: document.querySelector("#" + id + '_colfors')?.innerHTML,
            rowbuttons : document.querySelector("#" + id + '_rowbuttons')?.innerHTML
        };
        loadData(id);
    }

    // Veri çekme
    function loadData(id) {
        var cfg = instances[id];
        hideError(cfg);
        showLoading(cfg);

        var form = document.getElementById(cfg.id + 'FilterForm');
        var params = new URLSearchParams(new FormData(form));
        params.set('page', cfg.currentPage);

        fetch(cfg.url + '?' + params)
            .then(res => {
                if (!res.ok) {
                    return res.json()
                        .then(errData => { throw new Error(errData.error || errData.message || 'Sunucu hatası: ' + res.status); })
                        .catch(() => { throw new Error('Sunucu hatası: ' + res.status); });
                }
                return res.json();
            })
            .then(data => {
                if (!data.success) {
                    throw new Error(data.Messages?.map(m => m.Message).join(', ') || 'Bir hata oluştu');
                }
                
                // Check if it's a paginated result
                if (data.allPageCount !== undefined) {
                    renderTable(cfg, data.data);
                    renderPagination(cfg, data.allPageCount);
                } else {
                    renderTable(cfg, data.data);
                }
            })
            .catch(err => {
                console.error('Grid load error:', err);
                showError(cfg, err.message);
            })
            .finally(() => {
                hideLoading(cfg);
            });
    }

    function camelCase(str) {
        str = str.trim();
        return (str[0].toLowerCase() + str.substring(1))
    }
    function getByPath(obj, path) {
        return path
            .split(".")
            .reduce((current, key) => current?.[camelCase(key)], obj);
    }

    // Tabloyu doldurma
    function renderTable(cfg, items) {
        console.log("jefcwo")
        // tbody'yi boşalt
        cfg.$tableBody.innerHTML = '';

        // "Id;OrderNo;ProductName;..." formatındaki listeyi ayrıştır
        var cols = cfg.colfors?.split(';') || [];

        items.forEach(item => {
            var row = '<tr>';

            // sıralı kolonlar üzerinden dön
            cols.forEach(col => {
                var key = camelCase(col);

                // id kolonunda özel action butonları
                if (key == 'id') {
                    let html = cfg.rowbuttons
                        .replaceAll('[id]', item[key])
                        .replace(/%5Bid%5D/g, item[key]);
                    row += `<td>
                  <div class="dropdown">
                    <button class="action-menu dropdown-toggle" type="button" data-bs-toggle="dropdown" aria-expanded="false">
                      <i class="bi bi-three-dots-vertical"></i>
                    </button>
                    <ul class="dropdown-menu">
                      ${html}
                    </ul>
                  </div>
                </td>`;
                }
                else {
                    // diğer kolonlar için getByPath ile derin property erişimi
                    row += `<td>${getByPath(item, key) ?? ''}</td>`;
                }
            });

            row += '</tr>';
            cfg.$tableBody.insertAdjacentHTML('beforeend', row);
        });
    }


    // Sayfalamayı çizme
    function renderPagination(cfg, totalPages) {
        var $pag = cfg.$pagination;
        $pag.innerHTML = '';
        for (let i = 1; i <= totalPages; i++) {
            var li = document.createElement('li');
            li.className = 'page-item' + (i === cfg.currentPage ? ' active' : '');
            var a = document.createElement('a');
            a.className = 'page-link';
            a.href = '#';
            a.textContent = i;
            a.addEventListener('click', function (e) {
                e.preventDefault();
                goToPage(cfg.id, i-1);
            });
            li.appendChild(a);
            $pag.appendChild(li);
        }
    }

    // Sayfaya git
    function goToPage(id, page) {
        var cfg = instances[id];
        cfg.currentPage = page;
        loadData(id);
    }

    // Filtre uygula
    function applyFilters(id) {
        var cfg = instances[id];
        cfg.currentPage = 0;
        loadData(id);
    }

    // Loading mask
    function showLoading(cfg) {
        var w = cfg.$wrapper;
        if (!w || w.querySelector('.loading-mask')) return;
        w.style.position = 'relative';
        var mask = document.createElement('div');
        mask.className = 'loading-mask';
        mask.style.cssText = 'position:absolute;top:0;left:0;width:100%;height:100%;background:rgba(255,255,255,0.7);display:flex;justify-content:center;align-items:center;z-index:10';
        mask.innerHTML = '<div class="spinner-border" role="status"><span class="visually-hidden">Loading...</span></div>';
        w.appendChild(mask);
    }
    function hideLoading(cfg) {
        var w = cfg.$wrapper;
        if (!w) return;
        var m = w.querySelector('.loading-mask');
        if (m) w.removeChild(m);
    }

    // Hata göster
    function showError(cfg, msg) {
        var w = cfg.$wrapper;
        if (!w) return;
        hideError(cfg);
        var err = document.createElement('div');
        err.className = 'grid-error';
        err.style.cssText = 'position:absolute;top:0;left:0;width:100%;background:rgba(248,215,218,0.9);color:#721c24;padding:0.75rem;text-align:center;z-index:11';
        err.textContent = msg;
        w.appendChild(err);
    }
    function hideError(cfg) {
        var w = cfg.$wrapper;
        if (!w) return;
        var e = w.querySelector('.grid-error');
        if (e) w.removeChild(e);
    }

    // Public API
    return {
        init: init,
        goToPage: goToPage,
        applyFilters: applyFilters
    };
})();
