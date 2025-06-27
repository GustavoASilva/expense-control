export function renderBudgetGauge(canvasId, usage, amount) {
    const ctx = document.getElementById(canvasId).getContext('2d');
    if (!ctx) return;
    if (window[canvasId + '_chart']) {
        window[canvasId + '_chart'].destroy();
    }
    window[canvasId + '_chart'] = new Chart(ctx, {
        type: 'doughnut',
        data: {
            datasets: [{
                data: [usage, Math.max(amount - usage, 0)],
                backgroundColor: [usage > amount ? '#dc3545' : '#198754', '#e9ecef'],
                borderWidth: 0
            }]
        },
        options: {
            cutout: '70%',
            plugins: {
                legend: { display: false },
                tooltip: { enabled: false },
            },
            responsive: false,
        }
    });
}
