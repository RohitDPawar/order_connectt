var table;

var selectedRowIds = []; // Array to store the selected row IDs

$(document).ready(function () {

  table = $('.table-pagination').DataTable({
    "paging": true,        // Enable pagination
    "searching": true,     // Enable default search functionality
    "ordering": true,     // Enable sorting
    "info": true,          // Show information about the table
    "pageLength": 10,       // Number of rows per page
    "stateSave": false,     // Save state (pagination, search, etc.)
    "select": {
      "style": "multi", // Multi-row selection (can select multiple rows)
      "selector": 'td:first-child' // Assuming you are selecting by checkbox in the first column
    }, "columnDefs": [
      {
        "targets": [0], // Disable sorting for the first and second columns (index-based)
        "orderable": false // Disable ordering
      }]
  });

  // Event listener for changing page length
  $('#pageLength').on('change', function () {
    var length = $(this).val(); // Get selected value
    table.page.len(length).draw(); // Update DataTable page length and redraw the table
  });

  // Set the initial value of the dropdown to match the current page length
  $('#pageLength').val(table.page.len());

  $('.table-pagination').show();
  // Update pagination icons
  updatePaginationIcons();

  function updatePaginationIcons() {
    // Add Font Awesome icons to Previous and Next buttons using class selectors
    $('.previous').html('<i class="fas fa-chevron-left"></i>'); // Left arrow for Previous button
    $('.next').html('<i class="fas fa-chevron-right"></i>'); // Right arrow for Next button
    $(".Checkbox_SelectAllHeader").on("change", function () {
      var isChecked = $(this).prop('checked'); // Get the checked state of the header checkbox

      // Select or deselect all row checkboxes based on the header checkbox state
      table.rows({ page: 'current' }).nodes().to$().find(".RowCheckboxCategory").prop('checked', isChecked);

      // Update the selectedRowIds array based on the "Select All" checkbox
      if (isChecked) {
        // Add all row IDs to the selectedRowIds array
        table.rows({ page: 'current' }).every(function () {
          var rowId = this.id();  // Get the row ID (assuming each row has a unique ID set)
          if (rowId && !selectedRowIds.includes(rowId)) {
            selectedRowIds.push(rowId);  // Add row ID to the array
          }
        });
      } else {
        // Remove all row IDs from the selectedRowIds array when "Select All" is deselected
        table.rows({ page: 'current' }).every(function () {
          var rowId = this.id();  // Get the row ID (assuming each row has a unique ID set)
          var index = selectedRowIds.indexOf(rowId);
          if (index !== -1) {
            selectedRowIds.splice(index, 1);  // Remove row ID from the array
          }
        });
      }

    });

    // When an individual row checkbox is clicked
    $(".RowCheckbox").on("change", function () {
      var rowId = $(this).closest('tr').attr('id'); // Get the ID of the row (assuming row ID is set)
      var isChecked = $(this).prop('checked'); // Get the checked state of the checkbox
      console.log('y');
      if (isChecked) {
        // If checked, add the row ID to the array
        if (!selectedRowIds.includes(rowId)) {
          selectedRowIds.push(rowId); // Add row ID to the array
        }
      } else {
        // If unchecked, remove the row ID from the array
        var index = selectedRowIds.indexOf(rowId);
        if (index !== -1) {
          selectedRowIds.splice(index, 1); // Remove row ID from the array
        }
      }

      // Check if all checkboxes are selected on the current page
      var allChecked = true;
      table.rows({ page: 'current' }).nodes().to$().find(".RowCheckbox").each(function () {
        if (!$(this).prop('checked')) {
          allChecked = false; // If any checkbox is unchecked, set allChecked to false
        }
      });

      // Update the "Select All" checkbox state based on whether all checkboxes on the page are checked
      $(".Checkbox_SelectAllHeader").prop('checked', allChecked);

    });
  }

  // Apply styling for current page button
  table.on('draw', function () {
    updatePaginationIcons();
    updateSelectAllCheckbox(); // Check the state of the "Select All" checkbox after every draw


  });

  // Reset Search functionality
  $('.resetpage').on('click', function () {
    $('.searchpage').val(''); // Clear the search input
    table.search('').draw(); // Reset the table search
    location.reload();
  });

  // Update the "Select All" checkbox state based on the checkboxes on the current page
  function updateSelectAllCheckbox() {
    // Get all checkboxes on the current page
    var allChecked = true;
    table.rows({ page: 'current' }).nodes().to$().find(".RowCheckbox").each(function () {
      if (!$(this).prop('checked')) {
        allChecked = false; // If any checkbox is unchecked, set allChecked to false
      }
    });

    // Update the "Select All" checkbox state based on allChecked
    $(".Checkbox_SelectAllHeader").prop('checked', allChecked);
  }

  // When the "Select All" checkbox in the header is clicked
  $(".Checkbox_SelectAllHeader").change(function () {
    var isChecked = $(this).prop('checked'); // Get the checked state of the header checkbox

    // Select or deselect checkboxes on the current page
    table.rows({ page: 'current' }).nodes().to$().find(".RowCheckbox").prop('checked', isChecked);
  });

});
