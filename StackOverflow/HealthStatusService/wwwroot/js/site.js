// Site-wide JavaScript functionality

// Auto-refresh functionality
function enableAutoRefresh(intervalMinutes = 0.5) {
    setInterval(function() {
        location.reload();
    }, intervalMinutes * 60 * 1000);
}

// Initialize tooltips if using Bootstrap
document.addEventListener('DOMContentLoaded', function() {
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
});
