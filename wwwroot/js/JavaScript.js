// Main JavaScript file for Jobs and Bid Management
$(document).ready(function () {
    initializeJobSearch();
    initializeBidModal();
    initializeAdminModal();

    // Initialize settings preview if on settings page
    if ($('#settingsForm').length) {
        initializeSettingsPreview();
    }
});

// Constants for default values
const DEFAULT_SETTINGS = {
    laborRate: 45.00,
    materialMargin: 15.00,
    travelCost: 50.00,
    profitMargin: 20.00
};

const SAMPLE_JOB = {
    laborHours: 10,
    materialCost: 500
};

// Job Search and Filter Functionality
function initializeJobSearch() {
    let searchTimeout;

    // Search functionality
    $('#jobSearch').on('input', function () {
        const searchTerm = $(this).val();
        $('#clearSearch').toggle(searchTerm.length > 0);

        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(() => {
            performSearch(searchTerm);
        }, 300); // Reduced for better responsiveness

        // Show suggestions
        if (searchTerm.length > 2) {
            getSearchSuggestions(searchTerm);
        } else {
            $('#searchSuggestions').hide();
        }
    });

    $('#searchButton').click(function () {
        performSearch($('#jobSearch').val());
    });

    $('#clearSearch').click(function () {
        $('#jobSearch').val('');
        $(this).hide();
        performSearch('');
        $('#searchSuggestions').hide();
    });

    // Filter functionality
    $('#locationFilter, #jobTypeFilter, #budgetFilter').change(function () {
        performSearch($('#jobSearch').val());
    });

    // Clear timeouts on page unload
    $(window).on('beforeunload', function () {
        if (searchTimeout) {
            clearTimeout(searchTimeout);
        }
    });
}

// Search suggestions
function getSearchSuggestions(term) {
    $.get('/Jobs/SearchSuggestions', { term: term })
        .done(function (suggestions) {
            if (suggestions && suggestions.length > 0) {
                const suggestionsHtml = suggestions.map(s =>
                    `<div class="suggestion-item" onclick="selectSuggestion('${s.replace(/'/g, "\\'")}')">${s}</div>`
                ).join('');
                $('#searchSuggestions').html(suggestionsHtml).show();
            } else {
                $('#searchSuggestions').hide();
            }
        })
        .fail(function () {
            $('#searchSuggestions').hide();
        });
}

// Perform search
function performSearch(searchTerm) {
    $('#loadingSpinner').show();
    $('#jobsContainer').hide();

    const filters = {
        location: $('#locationFilter').val(),
        jobType: $('#jobTypeFilter').val(),
        budget: $('#budgetFilter').val()
    };

    $.get('/Jobs/Index', {
        search: searchTerm,
        ...filters
    })
        .done(function (data) {
            if (data && $(data).find('#jobsContainer').length) {
                $('#jobsContainer').html($(data).find('#jobsContainer').html());
                attachBidHandlers();
            } else {
                showToast('No results found', 'info');
            }
        })
        .fail(function (xhr, status, error) {
            console.error('Search failed:', error);
            showToast('Search failed. Please try again.', 'error');
        })
        .always(function () {
            $('#loadingSpinner').hide();
            $('#jobsContainer').show();
        });
}

// Bid Modal Functionality
function initializeBidModal() {
    // Attach bid handlers to initial content
    attachBidHandlers();

    // Recalculate when inputs change - with namespace to prevent duplicates
    $('#laborHours, #laborRate, #materialCost, #profitMargin, #materialMargin, #travelCost')
        .off('input.bidCalculation')
        .on('input.bidCalculation', calculateBid);

    // Submit bid handler
    $('#submitBid').click(function () {
        var formData = {
            jobId: $('#jobId').val(),
            bidAmount: $('#bidAmount').val(),
            notes: $('#notes').val()
        };

        $.post('/Jobs/SubmitBid', formData)
            .done(function (response) {
                if (response.success) {
                    showToast(response.message, 'success');
                    $('#bidModal').modal('hide');
                    // Optional: reload or update UI instead of full page reload
                    setTimeout(() => {
                        location.reload();
                    }, 1500);
                } else {
                    showToast(response.message, 'error');
                }
            })
            .fail(function (xhr, status, error) {
                console.error('Bid submission failed:', error);
                showToast('Error submitting bid. Please try again.', 'error');
            });
    });

    // Load settings when modal opens
    $('#bidModal').on('show.bs.modal', function (event) {
        const button = $(event.relatedTarget);
        const jobId = button.data('job-id');

        // Set job ID
        $('#jobId').val(jobId);

        // Load job details
        $.get('/Jobs/GetJobDetails', { id: jobId })
            .done(function (response) {
                if (response.success) {
                    $('#jobTitle').text(response.job.title);

                    let details = [];
                    if (response.job.budgetRange) details.push(`Budget: ${response.job.budgetRange}`);
                    if (response.job.estimatedHours) details.push(`Est. Hours: ${response.job.estimatedHours}`);
                    if (response.job.requiredMaterials) details.push(`Materials: ${response.job.requiredMaterials}`);

                    $('#jobDetails').text(details.join(' | '));

                    // Pre-fill labor hours if available
                    if (response.job.estimatedHours) {
                        $('#laborHours').val(response.job.estimatedHours);
                    }

                    // Pre-fill material cost if available
                    if (response.job.materialCost) {
                        $('#materialCost').val(response.job.materialCost);
                    }
                }
            })
            .fail(function (xhr, status, error) {
                console.error('Error loading job details:', error);
                showToast('Error loading job details', 'error');
            });

        // Load contractor settings
        loadContractorSettings();
    });

    // Clean up event listeners when modal closes
    $('#bidModal').on('hidden.bs.modal', function () {
        // Reset form
        $('#bidForm')[0]?.reset();
        // Remove specific event handlers if needed
        $('#laborHours, #laborRate, #materialCost, #profitMargin, #materialMargin, #travelCost')
            .off('input.bidCalculation');
    });
}

// Load contractor settings for bid calculation
function loadContractorSettings() {
    $.get('/Dashboard/GetSettings')
        .done(function (response) {
            if (response.success) {
                $('#laborRate').val(response.settings.laborRate || DEFAULT_SETTINGS.laborRate);
                $('#materialMargin').val(response.settings.materialMargin || DEFAULT_SETTINGS.materialMargin);
                $('#travelCost').val(response.settings.travelCost || DEFAULT_SETTINGS.travelCost);
                $('#profitMargin').val(response.settings.profitMargin || DEFAULT_SETTINGS.profitMargin);
                calculateBid();
            } else {
                useDefaultBidSettings();
            }
        })
        .fail(function (xhr, status, error) {
            console.log('Using default bid calculation values');
            useDefaultBidSettings();
        });
}

function useDefaultBidSettings() {
    $('#laborRate').val(DEFAULT_SETTINGS.laborRate);
    $('#materialMargin').val(DEFAULT_SETTINGS.materialMargin);
    $('#travelCost').val(DEFAULT_SETTINGS.travelCost);
    $('#profitMargin').val(DEFAULT_SETTINGS.profitMargin);
    calculateBid();
}

// Attach bid handlers to dynamically loaded content
function attachBidHandlers() {
    $('.calculate-bid').off('click.bidHandler').on('click.bidHandler', function () {
        var jobId = $(this).data('job-id');
        $('#bidModal').modal('show');
    });
}

// Input sanitization function
function sanitizeInput(value) {
    const num = parseFloat(value);
    return isNaN(num) ? 0 : Math.max(0, num);
}

// Enhanced Calculate bid function with all cost components
function calculateBid() {
    const laborHours = sanitizeInput($('#laborHours').val());
    const materialCost = sanitizeInput($('#materialCost').val());
    const laborRate = sanitizeInput($('#laborRate').val());
    const profitMargin = Math.min(100, sanitizeInput($('#profitMargin').val())); // Cap at 100%
    const materialMargin = Math.min(100, sanitizeInput($('#materialMargin').val())); // Cap at 100%
    const travelCost = sanitizeInput($('#travelCost').val());

    // Calculate costs
    const laborCost = laborHours * laborRate;
    const materialMarginAmount = materialCost * (materialMargin / 100);
    const materialTotal = materialCost + materialMarginAmount;
    const subtotal = laborCost + materialTotal + travelCost;
    const profitAmount = subtotal * (profitMargin / 100);
    const totalBid = subtotal + profitAmount;

    // Update display
    $('#laborCost').text(laborCost.toFixed(2));
    $('#materialCostDisplay').text(materialCost.toFixed(2));
    $('#materialMarginCost').text(materialMarginAmount.toFixed(2));
    $('#travelCostDisplay').text(travelCost.toFixed(2));
    $('#subtotal').text(subtotal.toFixed(2));
    $('#profitMarginCost').text(profitAmount.toFixed(2));
    $('#totalBid').text(totalBid.toFixed(2));

    // Update bid amount input
    $('#bidAmount').val(totalBid.toFixed(2));
}

// Admin Modal Functionality
function initializeAdminModal() {
    $('#bidDetailsModal').on('show.bs.modal', function (event) {
        var button = $(event.relatedTarget);
        $('#modal-contractor').text(button.data('contractor'));
        $('#modal-job').text(button.data('job'));
        $('#modal-amount').text('$' + parseFloat(button.data('amount')).toFixed(2));
        $('#modal-status').text(button.data('status'));
        $('#modal-notes').text(button.data('notes') || 'No additional notes provided.');
        $('#modal-date').text(button.data('date'));
    });
}

// Settings Page Real-time Preview
function initializeSettingsPreview() {
    const laborRateInput = $('#LaborRate');
    const materialMarginInput = $('#MaterialMargin');
    const travelCostInput = $('#TravelCost');
    const profitMarginInput = $('#ProfitMargin');

    // Add event listeners for real-time updates
    const settingsInputs = [laborRateInput, materialMarginInput, travelCostInput, profitMarginInput];

    settingsInputs.forEach(input => {
        input.off('input.settingsPreview').on('input.settingsPreview', updateSettingsPreview);
    });

    // Reset to defaults button
    $('#resetBtn').click(function () {
        if (confirm('Are you sure you want to reset all settings to default values?')) {
            laborRateInput.val(DEFAULT_SETTINGS.laborRate.toFixed(2));
            materialMarginInput.val(DEFAULT_SETTINGS.materialMargin.toFixed(2));
            travelCostInput.val(DEFAULT_SETTINGS.travelCost.toFixed(2));
            profitMarginInput.val(DEFAULT_SETTINGS.profitMargin.toFixed(2));
            $('#PreferredLocations').val('');
            $('#AutoCalculateBids').prop('checked', true);

            updateSettingsPreview();
            showToast('Settings reset to defaults', 'success');
        }
    });

    // Form validation
    $('#settingsForm').submit(function (e) {
        let isValid = true;
        const inputs = $(this).find('input[required]');

        inputs.each(function () {
            const value = $(this).val();
            const numValue = parseFloat(value);

            if (!value || isNaN(numValue) || numValue < 0) {
                isValid = false;
                $(this).addClass('is-invalid');
                $(this).next('.invalid-feedback').remove();
                $(this).after('<div class="invalid-feedback">Please enter a valid positive number</div>');
            } else {
                $(this).removeClass('is-invalid');
                $(this).next('.invalid-feedback').remove();
            }
        });

        // Validate percentage fields
        const percentageFields = ['MaterialMargin', 'ProfitMargin'];
        percentageFields.forEach(field => {
            const fieldElement = $(`#${field}`);
            const value = parseFloat(fieldElement.val());
            if (value > 100) {
                isValid = false;
                fieldElement.addClass('is-invalid');
                fieldElement.next('.invalid-feedback').remove();
                fieldElement.after('<div class="invalid-feedback">Percentage cannot exceed 100%</div>');
            }
        });

        if (!isValid) {
            e.preventDefault();
            showToast('Please fix all validation errors before saving', 'error');
        }
    });

    // Initialize preview on page load
    updateSettingsPreview();
}

function updateSettingsPreview() {
    const laborRate = sanitizeInput($('#LaborRate').val());
    const materialMargin = Math.min(100, sanitizeInput($('#MaterialMargin').val()));
    const travelCost = sanitizeInput($('#TravelCost').val());
    const profitMargin = Math.min(100, sanitizeInput($('#ProfitMargin').val()));

    // Calculations using sample job parameters
    const laborCost = SAMPLE_JOB.laborHours * laborRate;
    const materialTotal = SAMPLE_JOB.materialCost + (SAMPLE_JOB.materialCost * materialMargin / 100);
    const subtotal = laborCost + materialTotal + travelCost;
    const profitAmount = subtotal * profitMargin / 100;
    const totalBid = subtotal + profitAmount;

    // Update preview displays
    $('#previewLaborRate').text(laborRate.toFixed(2));
    $('#previewLaborCost').text(laborCost.toFixed(2));
    $('#previewMaterialMargin').text(materialMargin.toFixed(2));
    $('#previewMaterialCost').text(materialTotal.toFixed(2));
    $('#previewTravelCost').text(travelCost.toFixed(2));
    $('#previewSubtotal').text(subtotal.toFixed(2));
    $('#previewProfitMargin').text(profitMargin.toFixed(2));
    $('#previewProfitAmount').text(profitAmount.toFixed(2));
    $('#previewTotalBid').text(totalBid.toFixed(2));

    // Update overview
    $('#overviewLaborRate').text(laborRate.toFixed(2));
    $('#overviewMaterialMargin').text(materialMargin.toFixed(2));
    $('#overviewTravelCost').text(travelCost.toFixed(2));
    $('#overviewProfitMargin').text(profitMargin.toFixed(2));
}

// Toast notification function
function showToast(message, type) {
    // Create toast element with accessibility
    const toast = $(`
        <div class="toast align-items-center text-white bg-${type === 'success' ? 'success' : type === 'error' ? 'danger' : type === 'info' ? 'info' : 'primary'} border-0" 
             role="alert" aria-live="assertive" aria-atomic="true">
            <div class="d-flex">
                <div class="toast-body">
                    <i class="fas ${type === 'success' ? 'fa-check-circle' : type === 'error' ? 'fa-exclamation-circle' : 'fa-info-circle'} me-2"></i>
                    ${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" 
                        data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        </div>
    `);

    // Add to page
    let toastContainer = $('#toastContainer');
    if (toastContainer.length === 0) {
        toastContainer = $('<div id="toastContainer" class="toast-container position-fixed top-0 end-0 p-3" style="z-index: 9999"></div>');
        $('body').append(toastContainer);
    }
    toastContainer.append(toast);

    // Initialize and show toast
    const bsToast = new bootstrap.Toast(toast[0]);
    bsToast.show();

    // Remove toast after hide
    toast.on('hidden.bs.toast', function () {
        toast.remove();
    });
}

// Global functions for search suggestions
window.selectSuggestion = function (suggestion) {
    $('#jobSearch').val(suggestion);
    $('#searchSuggestions').hide();
    performSearch(suggestion);
};

window.clearSearch = function () {
    $('#jobSearch').val('');
    $('#clearSearch').hide();
    $('#locationFilter').val('');
    $('#jobTypeFilter').val('');
    $('#budgetFilter').val('');
    performSearch('');
};

// Utility function for formatting currency
window.formatCurrency = function (amount) {
    return '$' + parseFloat(amount).toFixed(2);
};

// Utility function for formatting percentage
window.formatPercentage = function (percentage) {
    return parseFloat(percentage).toFixed(2) + '%';
};

// Debounce function for performance optimization
window.debounce = function (func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
};


// Bid Details Modal
// Get AntiForgery Token - FIXED VERSION
function getToken() {
    const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
    if (tokenElement) {
        return tokenElement.value;
    }
    console.error('AntiForgery token not found');
    return '';
}

// Show Alert Function
function showAlert(message, type) {
    const alertClass = type === 'success' ? 'alert-success' : 'alert-danger';
    // Remove existing alerts
    const existingAlerts = document.querySelectorAll('.alert');
    existingAlerts.forEach(alert => alert.remove());

    const alertDiv = document.createElement('div');
    alertDiv.className = `alert ${alertClass} alert-dismissible fade show`;
    alertDiv.innerHTML = `
                <i class="fas fa-${type === 'success' ? 'check' : 'exclamation'}-circle me-2"></i>
                ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            `;

    const mainElement = document.querySelector('main');
    if (mainElement) {
        mainElement.insertBefore(alertDiv, mainElement.firstChild);
    }

    setTimeout(() => {
        if (alertDiv.parentElement) {
            alertDiv.remove();
        }
    }, 5000);
}

// Update UI after action
function updateBidUI(bidId, newStatus) {
    const badge = document.getElementById(`status-badge-${bidId}`);
    const row = document.getElementById(`bid-row-${bidId}`);

    if (!badge || !row) {
        console.error('Bid elements not found for ID:', bidId);
        return;
    }

    // Update badge
    const statusClass = newStatus === 'Submitted' ? 'bg-warning' :
        newStatus === 'UnderReview' ? 'bg-info' :
            newStatus === 'Accepted' ? 'bg-success' :
                newStatus === 'Rejected' ? 'bg-danger' : 'bg-secondary';

    badge.className = `badge ${statusClass}`;
    badge.textContent = newStatus;

    // Update row data attribute
    row.setAttribute('data-status', newStatus);

    // Update action buttons
    const actionCell = row.querySelector('td:last-child');
    if (newStatus === 'Accepted' || newStatus === 'Rejected') {
        actionCell.innerHTML = `
                    <div class="btn-group btn-group-sm">
                        <button class="btn btn-outline-primary btn-view-details"
                                data-bs-toggle="modal"
                                data-bs-target="#bidDetailsModal"
                                data-bid-id="${bidId}">
                            <i class="fas fa-eye"></i>
                        </button>
                        <span class="btn btn-outline-secondary disabled">
                            ${newStatus === 'Accepted' ? '<i class="fas fa-check text-success"></i>' : '<i class="fas fa-times text-danger"></i>'}
                        </span>
                    </div>
                `;
    }
}

// AJAX Functions - SIMPLIFIED VERSION
async function makeRequest(url, method = 'POST') {
    try {
        const token = getToken();
        if (!token) {
            showAlert('Security token missing. Please refresh the page.', 'error');
            return null;
        }

        const response = await fetch(url, {
            method: method,
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            }
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        return await response.json();
    } catch (error) {
        console.error('Request failed:', error);
        showAlert('Request failed: ' + error.message, 'error');
        return null;
    }
}

async function acceptBid(bidId) {
    const result = await makeRequest(`/Admin/AcceptBid/${bidId}`);
    if (result && result.success) {
        showAlert(result.message, 'success');
        updateBidUI(bidId, 'Accepted');
        // Close modal if open
        const modal = bootstrap.Modal.getInstance(document.getElementById('bidDetailsModal'));
        if (modal) modal.hide();
    } else if (result) {
        showAlert(result.message, 'error');
    }
}

async function rejectBid(bidId) {
    const result = await makeRequest(`/Admin/RejectBid/${bidId}`);
    if (result && result.success) {
        showAlert(result.message, 'success');
        updateBidUI(bidId, 'Rejected');
        // Close modal if open
        const modal = bootstrap.Modal.getInstance(document.getElementById('bidDetailsModal'));
        if (modal) modal.hide();
    } else if (result) {
        showAlert(result.message, 'error');
    }
}

async function updateBidStatus(bidId, status) {
    const result = await makeRequest(`/Admin/UpdateBidStatus?id=${bidId}&status=${status}`);
    if (result && result.success) {
        showAlert(result.message, 'success');
        updateBidUI(bidId, status);
    } else if (result) {
        showAlert(result.message, 'error');
    }
}

async function deleteBid(bidId) {
    const result = await makeRequest(`/Admin/DeleteBid/${bidId}`);
    if (result && result.success) {
        showAlert(result.message, 'success');
        // Remove row from table
        const row = document.getElementById(`bid-row-${bidId}`);
        if (row) row.remove();
    } else if (result) {
        showAlert(result.message, 'error');
    }
}

function filterBids(status) {
    const rows = document.querySelectorAll('.bid-row');
    rows.forEach(row => {
        if (status === 'all' || row.getAttribute('data-status') === status) {
            row.style.display = '';
        } else {
            row.style.display = 'none';
        }
    });
}

// Event Listeners
document.addEventListener('DOMContentLoaded', function () {
    // View bid details
    document.addEventListener('click', function (e) {
        if (e.target.closest('.btn-view-details')) {
            const button = e.target.closest('.btn-view-details');
            const bidId = button.getAttribute('data-bid-id');
            const contractor = button.getAttribute('data-contractor');
            const contractorEmail = button.getAttribute('data-contractor-email');
            const job = button.getAttribute('data-job');
            const jobLocation = button.getAttribute('data-job-location');
            const jobBudget = button.getAttribute('data-job-budget');
            const amount = button.getAttribute('data-amount');
            const status = button.getAttribute('data-status');
            const notes = button.getAttribute('data-notes') || 'No additional notes provided.';
            const date = button.getAttribute('data-date');

            document.getElementById('modal-contractor').textContent = contractor;
            document.getElementById('modal-contractor-email').textContent = contractorEmail;
            document.getElementById('modal-job').textContent = job;
            document.getElementById('modal-job-location').textContent = jobLocation;
            document.getElementById('modal-job-budget').textContent = jobBudget;
            document.getElementById('modal-amount').textContent = amount;
            document.getElementById('modal-status').textContent = status;
            document.getElementById('modal-notes').textContent = notes;
            document.getElementById('modal-date').textContent = date;

            // Update modal actions
            const modalActions = document.getElementById('modal-actions');
            modalActions.innerHTML = '';

            if (status === 'Submitted' || status === 'UnderReview') {
                modalActions.innerHTML = `
                            <button class="btn btn-success btn-sm me-2 btn-accept-bid-modal" 
                                    data-bid-id="${bidId}"
                                    data-contractor="${contractor}"
                                    data-job="${job}">
                                <i class="fas fa-check me-1"></i>Accept Bid
                            </button>
                            <button class="btn btn-danger btn-sm me-2 btn-reject-bid-modal" 
                                    data-bid-id="${bidId}">
                                <i class="fas fa-times me-1"></i>Reject Bid
                            </button>
                        `;
            }
        }
    });

    // Accept Bid
    document.addEventListener('click', function (e) {
        if (e.target.closest('.btn-accept-bid') || e.target.closest('.btn-accept-bid-modal')) {
            const button = e.target.closest('.btn-accept-bid, .btn-accept-bid-modal');
            const bidId = button.getAttribute('data-bid-id');
            const contractor = button.getAttribute('data-contractor');
            const job = button.getAttribute('data-job');

            if (confirm(`Are you sure you want to accept ${contractor}'s bid for "${job}"? This will reject all other bids for this job.`)) {
                acceptBid(bidId);
            }
        }
    });

    // Reject Bid
    document.addEventListener('click', function (e) {
        if (e.target.closest('.btn-reject-bid') || e.target.closest('.btn-reject-bid-modal')) {
            const button = e.target.closest('.btn-reject-bid, .btn-reject-bid-modal');
            const bidId = button.getAttribute('data-bid-id');
            if (confirm('Are you sure you want to reject this bid?')) {
                rejectBid(bidId);
            }
        }
    });

    // Update Status
    document.addEventListener('click', function (e) {
        if (e.target.closest('.update-status')) {
            const button = e.target.closest('.update-status');
            const bidId = button.getAttribute('data-bid-id');
            const status = button.getAttribute('data-status');
            updateBidStatus(bidId, status);
        }
    });

    // Delete Bid
    document.addEventListener('click', function (e) {
        if (e.target.closest('.btn-delete-bid')) {
            const button = e.target.closest('.btn-delete-bid');
            const bidId = button.getAttribute('data-bid-id');
            if (confirm('Are you sure you want to delete this bid? This action cannot be undone.')) {
                deleteBid(bidId);
            }
        }
    });

    // Filter Bids
    document.addEventListener('click', function (e) {
        if (e.target.closest('.filter-status')) {
            const button = e.target.closest('.filter-status');
            const status = button.getAttribute('data-status');
            filterBids(status);
        }
    });
});
