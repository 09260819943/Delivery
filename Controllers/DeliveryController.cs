using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Mvc;
using System.Web.Script.Serialization;  // Import for JavaScriptSerializer
using Siasystem.Models;
using System.IO;

namespace Siasystem.Controllers
{
    public class DeliveryController : Controller
    {
        private string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\WINDOWS 11 PRO\Desktop\SIA\Siasystem\Siasystem\App_Data\Database1.mdf;Integrated Security=True";

        // GET: Delivery/Index (Display Pending Deliveries)
        public ActionResult DeliveryPage()
        {
            List<Delivery> deliveries = new List<Delivery>();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT * FROM Delivery WHERE Status = 'Pending'", connection);
                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    deliveries.Add(new Delivery
                    {
                        DeliveredID = reader.GetInt32(reader.GetOrdinal("DeliveredID")),
                        ProductImage = reader.GetString(reader.GetOrdinal("ProductImage")),
                        ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
                        OrderQuantity = reader.GetInt32(reader.GetOrdinal("OrderQuantity")),
                        Supplier = reader.GetString(reader.GetOrdinal("Supplier")),
                        Status = reader.GetString(reader.GetOrdinal("Status"))
                    });
                }
            }

            return View(deliveries);  // Return the view with the list of pending deliveries
        }

        // POST: Delivery/ConfirmMove (Move Item to Inventory and History)
        [HttpPost]
        public ActionResult ConfirmMove()
        {
            // Read the raw JSON from the request body
            string jsonString;
            using (var reader = new StreamReader(Request.InputStream))
            {
                jsonString = reader.ReadToEnd();
            }

            // Deserialize the JSON into the DeliveryConfirmationRequest object using JavaScriptSerializer
            var serializer = new JavaScriptSerializer();
            var request = serializer.Deserialize<DeliveryConfirmationRequest>(jsonString);

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var moveToDeliveryHistoryCmd = new SqlCommand(
    "INSERT INTO DeliveryHistory (ProductImage, ProductName, Quantity, Supplier) " +
    "SELECT ProductImage, ProductName, OrderQuantity, Supplier " +
    "FROM Delivery WHERE DeliveredID = @DeliveredID", connection);

                moveToDeliveryHistoryCmd.Parameters.AddWithValue("@DeliveredID", request.DeliveredID);
                moveToDeliveryHistoryCmd.ExecuteNonQuery();

                // Move the item to Inventory
                var moveToInventoryCmd = new SqlCommand(
                    "INSERT INTO Inventory (ProductImage, ProductName, Quantity, Supplier) " +
                    "SELECT ProductImage, ProductName, OrderQuantity, Supplier " +
                    "FROM Delivery WHERE DeliveredID = @DeliveredID", connection);
                moveToInventoryCmd.Parameters.AddWithValue("@DeliveredID", request.DeliveredID);

                // Execute the command to insert into Inventory
                moveToInventoryCmd.ExecuteNonQuery();

                // Delete the item from the Delivery table after moving
                var deleteFromDeliveryCmd = new SqlCommand("DELETE FROM Delivery WHERE DeliveredID = @DeliveredID", connection);
                deleteFromDeliveryCmd.Parameters.AddWithValue("@DeliveredID", request.DeliveredID);

                // Execute the delete command
                deleteFromDeliveryCmd.ExecuteNonQuery();
            }

            return Json(new { success = true });  // Return a success response
        }

        // GET: Delivery/History (Display Delivery History)
        public ActionResult DeliveryHistory()
        {
            List<DeliveryHistory> history = new List<DeliveryHistory>();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT * FROM DeliveryHistory", connection);
                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    history.Add(new DeliveryHistory
                    {
                        HistoryID = reader.GetInt32(reader.GetOrdinal("HistoryID")),
                        ProductImage = reader.GetString(reader.GetOrdinal("ProductImage")),
                        ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
                        Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                        Supplier = reader.GetString(reader.GetOrdinal("Supplier")),
                        DateDelivered = reader.IsDBNull(reader.GetOrdinal("DateDelivered"))
                            ? (DateTime?)null
                            : reader.GetDateTime(reader.GetOrdinal("DateDelivered"))

                    });
                }
            }

            return View(history);  // Return the view with the list of delivery history
        }

        // GET: Delivery/Inventory (Display Inventory)
        public ActionResult Inventory()
        {
            List<Inventory> inventory = new List<Inventory>();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT * FROM Inventory", connection);
                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    inventory.Add(new Inventory
                    {
                        InventoryID = reader.GetInt32(reader.GetOrdinal("InventoryID")),
                        ProductImage = reader.GetString(reader.GetOrdinal("ProductImage")),
                        ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
                        Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                        Supplier = reader.GetString(reader.GetOrdinal("Supplier")),
                        DateAdded = reader.IsDBNull(reader.GetOrdinal("DateAdded"))
                            ? (DateTime?)null
                            : reader.GetDateTime(reader.GetOrdinal("DateAdded"))
                    });
                }
            }

            return View(inventory);  // Return the view with the list of inventory
        }
    }
}
