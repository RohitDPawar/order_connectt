//THIS IS USED FOR JOB CARD DETAILS SHOW
function ShowJobCardDetails(ItemId) {
  $.ajax({
    type: "POST",
    url: "/Dashboard/GetJobcardDetails",
    data: {
      ItemId: ItemId
    },
    success: function (data) {
      var res = JSON.parse(data);
      var OrderData = res["OrderData"];
      var Image = res["ImageData"];
      var StoneData = res["StoneData"];

      if (OrderData.length > 0) {
        //console.log(OrderData);
        $("#OrderDetailsId").val(OrderData[0]["order_details_id"])
        $("#Lbl_OrderNo").text(OrderData[0]["order_series_no"])
        $("#Lbl_Date").text(OrderData[0]["order_date"].split('T')[0])
        $("#Lbl_DeliveryDate").text(OrderData[0]["order_delivery_date"].split('T')[0])
        $("#Lbl_ItemName").text(OrderData[0]["item_name"])
        $("#Lbl_Size").text(parseFloat(OrderData[0]["size"]).toFixed(2))
        $("#Lbl_Pieces").text(OrderData[0]["pcs"])
        $("#Lbl_Category").text(OrderData[0]["category_name"])
        $("#grossWt").text(parseFloat(OrderData[0]["gross_wt"]).toFixed(3))
        $("#netWt").text(parseFloat(OrderData[0]["net_wt"]).toFixed(3) )
        $("#productGroup").text(OrderData[0]["product_group_name"])
        $("#PurityId").text(OrderData[0]["purity"])
        $("#remarkid").text(OrderData[0]["remark"])
      }
      $("#StoneDetailsTableId").empty();
      if (StoneData.length > 0) {
        var StoneAppendData = `<table class="table table-bordered table-sm">
                          <thead>
                              <tr>
                                  <th class="p-1 text-left" style="padding-top: 5px; padding-bottom: 5px;"><strong class="small">Stone Name</strong></th>
                                  <th class="p-1 text-left" style="padding-top: 5px; padding-bottom: 5px;"><strong class="small">Stone Category</strong></th>
                                  <th class="p-1 text-left" style="padding-top: 5px; padding-bottom: 5px;"><strong class="small">Stone Color</strong></th>
                                  <th class="p-1 text-left" style="padding-top: 5px; padding-bottom: 5px;"><strong class="small">Stone Weight</strong></th>
                                  <th class="p-1 text-left" style="padding-top: 5px; padding-bottom: 5px;"><strong class="small">Stone Pcs</strong></th>
                              </tr>
                          </thead><tbody>`;

        for (var i = 0; i < StoneData.length; i++) {
          StoneAppendData += `<tr>
                                                   <td style="padding-top: 5px; padding-bottom: 5px;">` + StoneData[i]["item_name"] + `</td>
                                                   <td style="padding-top: 5px; padding-bottom: 5px;">` + StoneData[i]["stone_category_name"] + `</td>
                                                   <td style="padding-top: 5px; padding-bottom: 5px;">` + StoneData[i]["color_name"] + `</td>
                                                   <td style="padding-top: 5px; padding-bottom: 5px;">` + parseFloat(StoneData[i]["stone_wt"]).toFixed(3) + `</td>
                                                   <td style="padding-top: 5px; padding-bottom: 5px;">` + StoneData[i]["stone_pcs"] + `</td>
                                                </tr>`;
        }

        $("#StoneDetailsTableId").append(StoneAppendData + '</tbody></table>');

      }
      $("#ImageAppendDivId").empty();
      if (Image.length > 0) {
        console.log(Image)
        // Get the parent container
        var parentContainer = document.getElementById("ImageAppendDivId");

        // Create a row container
        var rowElement = null;

        // Loop through the images
        for (var i = 0; i < Image.length; i++) {
          // Start a new row for every group of 3 images
          if (i % 3 === 0) {
            rowElement = document.createElement('div');
            rowElement.className = "row mb-3"; // Add row class and margin
            parentContainer.appendChild(rowElement); // Append the row to the parent container
          }

          // Process the image
          var Path = Image[i]["path"].replace('~', ''); // Replace '~' in the path
          var imgElement = document.createElement('img');
          imgElement.alt = "Item Image";
          imgElement.id = "ItemImage_" + i; // Set dynamic id
          imgElement.src = Path; // Set the source
          imgElement.className = "img-equal-size"; // Add a custom class for size control

          // Create a div for the image
          var colElement = document.createElement('div');
          colElement.className = "col-md-4"; // Set column class
          colElement.appendChild(imgElement); // Append the image to the column

          // Append the column to the current row
          rowElement.appendChild(colElement);
        }
      }
    },
    error: function (response) {

    }
  });
  const myModal = new bootstrap.Modal(document.getElementById('JobCardModalId'));
  myModal.show();
}

//THIS IS USED FOR HISTORY SHOW
function ShowHistoryPage(ItemId)
{
  $.ajax({
    type: "POST",
    url: "/OrderMaster/GetHistoryData",
    data: { OrderDetailsId: ItemId },
    success: function (data) {
      var res = JSON.parse(data);
      var HistoryData = res["HistoryData"];
      //console.log(HistoryData)
      $("#GetHistory").empty();
      if (HistoryData.length > 0) {
        HistoryData.forEach(function (item) {
          var row = '<tr>';
          row += '<td>' + (item.action_name === 'A' ? 'Add' : item.action_name === 'U' ? 'Update' : 'Delete') + '</td>';
          row += '<td>' + (item.ip_address || '') + '</td>';
          row += '<td>' + (item.tenant_name || '') + '</td>';
          row += '<td>' + (item.item_name || '') + '</td>';
          row += '<td>' + (item.category_name || '') + '</td>';
          row += '<td>' + (item.purity_name || '') + '</td>';
          row += '<td>' + (parseFloat(item.gross_wt).toFixed(3) || '') + '</td>';
          row += '<td>' + (parseFloat(item.net_wt).toFixed(3) || '') + '</td>';
          row += '<td>' + (parseFloat(item.actual_item_gross_wt).toFixed(3) || '') + '</td>';
          row += '<td>' + (parseFloat(item.actual_item_net_wt).toFixed(3) || '') + '</td>';
          row += '<td>' + (item.vendor_name ? item.vendor_name : '-') + '</td>';
          row += '<td>' + (item.status_name || '') + '</td>';
          row += '<td>' + (item.remark || '') + '</td>';
          row += '<td>' + (item.rejected_remark || '') + '</td>';
          row += '<td>' + (item.updated_name || '') + '</td>';
          row += '<td>' + (item.updated_at.split('T')[0] + ' ' + item.updated_at.split('T')[1] || '') + '</td>';
          row += '</tr>';

          $("#GetHistory").append(row); // Append row to table
          $('#OrderHistoryData').modal('show');
        });
      } else {
        // If no data, show a message (optional)
        $("#GetHistory").append('<tr><td colspan="11" class="text-center">No history available</td></tr>');
        $('#OrderHistoryData').modal('show');
      }
    },
    error: function (response) {
      console.error("Error during AJAX request:", response);
    }
  });
}
// Function to open the preview modal with order details
function ShowStoneDetails(orderItemId) {
  // Make the AJAX call to fetch the order details
  $.ajax({
    type: "POST",
    url: "/Dashboard/GetStoneDetails",
    data: { itemId: orderItemId },
    success: function (response) {
      // Parse the response data
      var res = JSON.parse(response);
      var orderDetails = res["StoneDetails"];
      console.log(orderDetails);

      // Check if orderDetails are found
      if (orderDetails.length > 0) {
        var detailsHtml = ""; // Initialize as empty string

        for (var i = 0; i < orderDetails.length; i++) {
          // Construct the HTML to display the order details in a table row
          detailsHtml +=
            '<tr>' +
          '<td>' + orderDetails[i]["item_name"] + '</td>' +
          '<td>' + orderDetails[i]["stone_category_name"] + '</td>' +
          '<td>' + orderDetails[i]["color_name"] + '</td>' +
          '<td>' + parseFloat(orderDetails[i]["stone_wt"]).toFixed(3) + '</td>' +
            '<td>' + orderDetails[i]["stone_pcs"] + '</td>' +
            '</tr>';
        }

        // Set the constructed rows to the table body
        $('#stoneDetailsBody').html(detailsHtml);

        // Show the modal
        $('#stoneDetailsModalShow').modal('show');
      } else {
        alert('Order details not found!');
      }
    },
    error: function (error) {
      // Handle any errors from the AJAX request
      console.error('Error fetching order details:', error);
      alert('An error occurred while fetching order details.');
    }
  });
}


