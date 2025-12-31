// MediCare Healthcare System - Main JavaScript

// Document Ready
$(document).ready(function() {
    // Initialize tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Initialize date pickers
    initializeDatePickers();

    // Form validation
    initializeFormValidation();

    // Auto-dismiss alerts
    autoDismissAlerts();
});

// Date Picker Initialization
function initializeDatePickers() {
    if ($('.date-picker').length) {
        $('.date-picker').each(function() {
            $(this).attr('type', 'date');
            var minDate = $(this).data('min-date') || new Date().toISOString().split('T')[0];
            $(this).attr('min', minDate);
        });
    }
}

// Form Validation
function initializeFormValidation() {
    $('form').on('submit', function(e) {
        var form = $(this);
        if (form.hasClass('no-validate')) {
            return true;
        }

        var isValid = true;
        form.find('[required]').each(function() {
            if (!$(this).val()) {
                $(this).addClass('is-invalid');
                isValid = false;
            } else {
                $(this).removeClass('is-invalid');
            }
        });

        if (!isValid) {
            e.preventDefault();
            showAlert('Please fill in all required fields.', 'warning');
            return false;
        }
    });

    // Remove invalid class on input
    $('input, select, textarea').on('input change', function() {
        $(this).removeClass('is-invalid');
    });
}

// Loading Spinner
function showLoading() {
    if ($('#loadingSpinner').length === 0) {
        $('body').append(`
            <div id="loadingSpinner" class="loading-spinner">
                <div class="spinner-border text-primary" role="status">
                    <span class="visually-hidden">Loading...</span>
                </div>
            </div>
        `);
    }
    $('#loadingSpinner').fadeIn();
}

function hideLoading() {
    $('#loadingSpinner').fadeOut();
}

// Alert Messages
function showAlert(message, type = 'info') {
    var alertHtml = `
        <div class="alert alert-${type} alert-dismissible fade show position-fixed top-0 start-50 translate-middle-x mt-3" 
             role="alert" style="z-index: 9999; min-width: 300px;">
            <i class="fas fa-${getAlertIcon(type)}"></i> ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `;
    $('body').append(alertHtml);
    setTimeout(function() {
        $('.alert').fadeOut(function() {
            $(this).remove();
        });
    }, 5000);
}

function getAlertIcon(type) {
    switch(type) {
        case 'success': return 'check-circle';
        case 'danger': return 'exclamation-circle';
        case 'warning': return 'exclamation-triangle';
        case 'info': return 'info-circle';
        default: return 'info-circle';
    }
}

function autoDismissAlerts() {
    setTimeout(function() {
        $('.alert').not('.alert-permanent').fadeOut('slow', function() {
            $(this).remove();
        });
    }, 5000);
}

// AJAX Helper Functions
function ajaxPost(url, data, successCallback, errorCallback) {
    showLoading();
    $.ajax({
        url: url,
        type: 'POST',
        data: data,
        success: function(response) {
            hideLoading();
            if (successCallback) successCallback(response);
        },
        error: function(xhr, status, error) {
            hideLoading();
            if (errorCallback) {
                errorCallback(xhr, status, error);
            } else {
                showAlert('An error occurred. Please try again.', 'danger');
            }
        }
    });
}

function ajaxGet(url, successCallback, errorCallback) {
    showLoading();
    $.ajax({
        url: url,
        type: 'GET',
        success: function(response) {
            hideLoading();
            if (successCallback) successCallback(response);
        },
        error: function(xhr, status, error) {
            hideLoading();
            if (errorCallback) {
                errorCallback(xhr, status, error);
            } else {
                showAlert('An error occurred. Please try again.', 'danger');
            }
        }
    });
}

// Appointment Time Slot Picker
function initializeTimeSlotPicker(doctorId, selectedDate) {
    if (!doctorId || !selectedDate) return;

    showLoading();
    $.ajax({
        url: '/Appointment/GetAvailableSlots',
        type: 'GET',
        data: { doctorId: doctorId, date: selectedDate },
        success: function(slots) {
            hideLoading();
            var timeSlotsContainer = $('#timeSlots');
            if (timeSlotsContainer.length) {
                timeSlotsContainer.empty();
                if (slots && slots.length > 0) {
                    slots.forEach(function(slot) {
                        timeSlotsContainer.append(`
                            <div class="col-md-3 col-sm-4 col-6 mb-2">
                                <button type="button" class="btn btn-outline-primary w-100 time-slot-btn" data-time="${slot}">
                                    ${formatTime(slot)}
                                </button>
                            </div>
                        `);
                    });

                    $('.time-slot-btn').click(function() {
                        $('.time-slot-btn').removeClass('active');
                        $(this).addClass('active');
                        var selectedTime = $(this).data('time');
                        $('#AppointmentTime').val(selectedTime);
                    });
                } else {
                    timeSlotsContainer.append('<p class="text-muted">No available time slots for this date.</p>');
                }
            }
        },
        error: function() {
            hideLoading();
            showAlert('Error loading time slots.', 'danger');
        }
    });
}

function formatTime(time) {
    var parts = time.split(':');
    var hours = parseInt(parts[0]);
    var minutes = parts[1];
    var ampm = hours >= 12 ? 'PM' : 'AM';
    hours = hours % 12;
    hours = hours ? hours : 12;
    return hours + ':' + minutes + ' ' + ampm;
}

// Confirmation Dialogs
function confirmDelete(message = 'Are you sure you want to delete this item?') {
    return confirm(message);
}

function confirmAction(message) {
    return confirm(message);
}

// Data Tables Initialization
function initializeDataTable(tableId) {
    if ($.fn.DataTable && $(tableId).length) {
        $(tableId).DataTable({
            responsive: true,
            pageLength: 10,
            order: [[0, 'desc']],
            language: {
                search: "Search:",
                lengthMenu: "Show _MENU_ entries",
                info: "Showing _START_ to _END_ of _TOTAL_ entries"
            }
        });
    }
}

// Number Formatting
function formatCurrency(amount) {
    return 'LKR ' + parseFloat(amount).toFixed(2).replace(/\d(?=(\d{3})+\.)/g, '$&,');
}

// Date Formatting
function formatDate(dateString) {
    var date = new Date(dateString);
    var options = { year: 'numeric', month: 'short', day: 'numeric' };
    return date.toLocaleDateString('en-US', options);
}

// Smooth Scroll
$(document).on('click', 'a[href^="#"]', function (event) {
    if (this.hash !== "") {
        event.preventDefault();
        var hash = this.hash;
        $('html, body').animate({
            scrollTop: $(hash).offset().top - 70
        }, 800);
    }
});

// Back to Top Button
$(window).scroll(function() {
    if ($(this).scrollTop() > 100) {
        $('#backToTop').fadeIn();
    } else {
        $('#backToTop').fadeOut();
    }
});

$('#backToTop').click(function() {
    $('html, body').animate({scrollTop: 0}, 800);
    return false;
});

// Export to CSV
function exportTableToCSV(tableId, filename) {
    var csv = [];
    var rows = document.querySelectorAll(tableId + " tr");

    for (var i = 0; i < rows.length; i++) {
        var row = [], cols = rows[i].querySelectorAll("td, th");
        for (var j = 0; j < cols.length; j++) {
            row.push(cols[j].innerText);
        }
        csv.push(row.join(","));
    }

    downloadCSV(csv.join("\n"), filename);
}

function downloadCSV(csv, filename) {
    var csvFile;
    var downloadLink;

    csvFile = new Blob([csv], {type: "text/csv"});
    downloadLink = document.createElement("a");
    downloadLink.download = filename;
    downloadLink.href = window.URL.createObjectURL(csvFile);
    downloadLink.style.display = "none";
    document.body.appendChild(downloadLink);
    downloadLink.click();
}

// Print Function
function printPage() {
    window.print();
}

// CSS for Loading Spinner
$(document).ready(function() {
    if (!$('#customStyles').length) {
        $('head').append(`
            <style id="customStyles">
                .loading-spinner {
                    display: none;
                    position: fixed;
                    top: 0;
                    left: 0;
                    width: 100%;
                    height: 100%;
                    background: rgba(0, 0, 0, 0.5);
                    z-index: 9999;
                    justify-content: center;
                    align-items: center;
                }
                .loading-spinner .spinner-border {
                    width: 3rem;
                    height: 3rem;
                }
                .time-slot-btn.active {
                    background-color: #0d6efd;
                    color: white;
                }
            </style>
        `);
    }
});

