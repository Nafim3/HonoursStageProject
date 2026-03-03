window.loadRevenueChart = function (labels, data) {

    setTimeout(() => {

        const canvas = document.getElementById('revenueChart');

        if (!canvas)
            return;

        const ctx = canvas.getContext('2d');

        new Chart(ctx, {
            type: 'line',

            data: {
                labels: labels,
                datasets: [{
                    label: '',
                    data: data,
                    borderWidth: 2,
                    fill: true
                }]
            },

            options: {
                responsive: true,

                scales: {
                    x: {
                        ticks: {
                            color: 'white'
                        },
                        grid: {
                            color: 'rgba(255,255,255,0.1)'
                        }
                    },

                    y: {
                        ticks: {
                            color: 'white'
                        },
                        grid: {
                            color: 'rgba(255,255,255,0.1)'
                        }
                    }
                },

                plugins: {
                    legend: {
                        display: false
                    }
                }
            }
        });

    }, 200);
};