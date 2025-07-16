(function () {
    function buildConfig(cfg, data) {
        const palette = cfg.colors ? cfg.colors.split(',') : [];
        const datasets = data.datasets.map((d, i) => ({
            label: d.label,
            data: d.data,
            backgroundColor: palette[i % palette.length] || undefined,
            borderColor: palette[i % palette.length] || undefined,
            fill: false
        }));
        return {
            type: cfg.type,
            data: { labels: data.labels, datasets: datasets },
            options: Object.assign({
                indexAxis: cfg.horizontal ? 'y' : 'x',
                responsive: true,
                maintainAspectRatio: false
            }, cfg.options || {})
        };
    }

    async function initChart(cfg) {
        const canvas = document.getElementById(cfg.id);
        const mask = document.getElementById(cfg.id + '-mask');
        try {
            const res = await fetch(cfg.url);
            const json = await res.json();
            if (!json.success) throw new Error('Data error');
            new Chart(canvas, buildConfig(cfg, json.data));
        } catch (e) {
            canvas.parentElement.insertAdjacentHTML('beforeend', '<span class=\"text-danger\">' + e.message + '</span>');
        } finally {
            if (mask) mask.style.display = 'none';
        }
    }

    function init() {
        if (!window.chartRegistry) return;
        window.chartRegistry.forEach(initChart);
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
