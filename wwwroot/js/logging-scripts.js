document.addEventListener('DOMContentLoaded', function() {
    // Header button functionality
    const pauseBtn = document.getElementById('pauseBtn');
    if (pauseBtn) {
        let isPaused = false;

        pauseBtn.addEventListener('click', function() {
            isPaused = !isPaused;
            this.innerHTML = isPaused ?
                '<i class="fa-solid fa-play"></i>' :
                '<i class="fa-solid fa-pause"></i>';
            this.title = isPaused ? 'Resume recording' : 'Pause recording';
        });
    }

    const clearBtn = document.getElementById('clearBtn');
    if (clearBtn) {
        clearBtn.addEventListener('click', function() {
            if (confirm('Are you sure you want to clear all entries?')) {
                const tableBody = document.getElementById('requestsTableBody');
                if (tableBody) {
                    tableBody.innerHTML = '';
                }
            }
        });
    }

    const refreshBtn = document.getElementById('refreshBtn');
    if (refreshBtn) {
        refreshBtn.addEventListener('click', function() {
            // Simulate refresh with a spinning animation
            this.querySelector('i').classList.add('fa-spin');
            setTimeout(() => {
                this.querySelector('i').classList.remove('fa-spin');
                // In a real app, you would reload the data here
                window.location.reload();
            }, 1000);
        });
    }

    const themeBtn = document.getElementById('themeBtn');
    if (themeBtn) {
        let isDarkMode = false;

        themeBtn.addEventListener('click', function() {
            isDarkMode = !isDarkMode;
            document.body.classList.toggle('dark-mode', isDarkMode);
        });
    }
});