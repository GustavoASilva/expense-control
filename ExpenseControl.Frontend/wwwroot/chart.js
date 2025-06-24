window.initMonthlyChart = (canvasId, labels, incomeData, expenseData) => {
    if (!window.Chart) {
        console.error('Chart.js is not loaded!');
        return;
    }
    const ctx = document.getElementById(canvasId).getContext('2d');
    if (!ctx) {
        console.error('Cannot acquire context from the given item:', canvasId);
        return;
    }
    if (window._monthlyChartInstance) {
        window._monthlyChartInstance.destroy();
    }
    window._monthlyChartInstance = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [
                {
                    label: 'Income',
                    backgroundColor: 'rgba(40, 167, 69, 0.7)',
                    borderColor: 'rgba(40, 167, 69, 1)',
                    borderWidth: 1,
                    data: incomeData
                },
                {
                    label: 'Expenses',
                    backgroundColor: 'rgba(220, 53, 69, 0.7)',
                    borderColor: 'rgba(220, 53, 69, 1)',
                    borderWidth: 1,
                    data: expenseData
                }
            ]
        },
        options: {
            responsive: true,
            plugins: {
                legend: { position: 'top' },
                title: { display: true, text: 'Monthly Income vs Expenses' }
            },
            scales: {
                y: { beginAtZero: true }
            }
        }
    });
};
