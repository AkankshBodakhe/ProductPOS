using POSApp.Helpers;
using POSLibrary.Entities;
using POSLibrary.Helpers;
using POSLibrary.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace POSApp
{
    public partial class Frm_Pos : Form
    {
        private readonly AuthService _authService = new AuthService();
        private readonly ProductService _productService = new ProductService();
        private readonly SaleService _saleService = new SaleService();

        private bool _isEditMode = false; // To track Add/Edit state
        private BindingSource _productBindingSource = new BindingSource();
        private bool isEditing = false;
        private int editingRowIndex = -1;
        private List<object> _allSalesHistory; // keep all sales in memory




        public Frm_Pos()
        {
            InitializeComponent();
            tabControl2.Visible = true;
            tabControl1.Visible = false;// Hide main tab initially

        }


        private void Frm_Pos_Load(object sender, EventArgs e)
        {
            timer1.Interval = 1000;
            timer1.Start();
            RefreshAllGrids();
            ConfigureGrid();
            SetupCartGrid();
            LoadProductsIntoCombo();
            LoadSalesHistory();

            PrinterHelper.LoadConfig(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "printer.config"));

        }


        private void ConfigureSaleHistoryGrid()
        {
            if (!dgvSaleHistory.Columns.Contains("PrintInvoice"))
            {
                DataGridViewButtonColumn btnCol = new DataGridViewButtonColumn
                {
                    Name = "PrintInvoice",
                    HeaderText = "Action",
                    Text = "Print Invoice",
                    UseColumnTextForButtonValue = true
                };
                dgvSaleHistory.Columns.Add(btnCol);
            }
        }


        private void LoadSalesHistory()
        {
            var todaysTotal = _saleService.GetTodaysSalesTotal();
            lblTodaysTotalSales.Text = todaysTotal.ToString("C");

            // get projected sales (with all needed columns)
            var allSales = _saleService.GetAllSales(forceRefresh: true);

            dgvSaleHistory.AutoGenerateColumns = false;
            dgvSaleHistory.Columns.Clear();   // clear old columns
            dgvSaleHistory.DataSource = null;
            dgvSaleHistory.DataSource = allSales;

            // Add columns dynamically
            dgvSaleHistory.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "InvoiceNo", Name = "InvoiceNo", HeaderText = "Invoice No" });
            dgvSaleHistory.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "SaleDate", Name = "SaleDate", HeaderText = "Date" });
            dgvSaleHistory.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Customer", Name = "Customer", HeaderText = "Customer" });
            dgvSaleHistory.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Salesman", Name = "Salesman", HeaderText = "Salesman" });
            dgvSaleHistory.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "PaymentMode", Name = "PaymentMode", HeaderText = "Payment Mode" });
            dgvSaleHistory.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "GrandTotal", Name = "GrandTotal", HeaderText = "Total" });
            dgvSaleHistory.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Products", Name = "Products", HeaderText = "Products" });

            ConfigureSaleHistoryGrid(); // adds PrintInvoice button
        }


        private void btnRegister_Click(object sender, EventArgs e)
        {
            string result = _authService.Register(
                txtRegName.Text,
                txtRegUsername.Text,
                txtRegPassword.Text,
                txtRegConfPassword.Text,
                txtRegContact.Text
            );



            MessageBox.Show(result);

            if (result == "Registration successful!")
            {
                // Clear registration fields
                txtRegName.Text = "";
                txtRegUsername.Text = "";
                txtRegPassword.Text = "";
                txtRegConfPassword.Text = "";
                txtRegContact.Text = "";

                // Switch to Login tab (assuming Login tab is at index 0)
                tabControl1.SelectedIndex = 0;
            }
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            var user = _authService.Login(txtLoginUsername.Text, txtLoginPassword.Text);

            if (user != null)
            {
                // Save user in session
                SessionManager.SetUser(user);

                tabControl1.Visible = false;
                tabControl2.Visible = true;

                label10.Text = $"Welcome {user.Name}";
            }
            else
            {
                MessageBox.Show("Invalid username or password.");
            }
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            // Clear session
            SessionManager.Clear();

            // Switch back to login tab
            tabControl2.Visible = false;
            tabControl1.Visible = true;

            // Clear login fields
            txtLoginUsername.Text = string.Empty;
            txtLoginPassword.Text = string.Empty;

            MessageBox.Show("You have been logged out successfully.");
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label11.Text = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
        }

        private void btnSearchProd_Click(object sender, EventArgs e)
        {
            if (int.TryParse(txtSearchProdID.Text, out int searchId))
            {
                foreach (DataGridViewRow row in dgvManageProd.Rows)
                {
                    if (row.Cells["Id"].Value != null && (int)row.Cells["Id"].Value == searchId)
                    {
                        row.Selected = true;
                        dgvManageProd.FirstDisplayedScrollingRowIndex = row.Index;
                        return;
                    }
                }
                MessageBox.Show("Product not found!");
            }
            else
            {
                MessageBox.Show("Enter a valid Product ID!");
            }
        }

        private void btnAddProd_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_isEditMode) // Add Mode
                {
                    var product = new Product
                    {
                        Id = int.Parse(txtProdID.Text),
                        Name = txtProdName.Text,
                        Rate = decimal.Parse(txtProdPrice.Text),
                        DiscountPercent = numericDefDisc.Value,
                        AvailableStock = Convert.ToInt32(numericProdQty.Value)
                    };

                    _productService.AddProduct(product);
                    MessageBox.Show("Product added successfully!");
                }
                else // Edit Mode
                {
                    var product = new Product
                    {
                        Id = int.Parse(txtProdID.Text),
                        Name = txtProdName.Text,
                        Rate = decimal.Parse(txtProdPrice.Text),
                        DiscountPercent = numericDefDisc.Value,
                        AvailableStock = Convert.ToInt32(numericProdQty.Value)
                    };

                    _productService.UpdateProduct(product);
                    MessageBox.Show("Product updated successfully!");

                    // Reset to Add mode
                    _isEditMode = false;
                    btnAddProd.Text = "Add Product";
                    label32.Text = "Add Product";
                    txtProdID.Enabled = true;
                }

                ClearInputs();
                RefreshAllGrids();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void RefreshAllGrids()
        {
            var products = _productService.GetAllProducts();

            dgvManageProd.DataSource = null;
            dgvManageProd.DataSource = products;

            dgvDashBoardAll.DataSource = null;
            dgvDashBoardAll.DataSource = products;

            dgvDashBoardLow.DataSource = null;
            dgvDashBoardLow.DataSource = products
                                            .Where(p => p.AvailableStock < 10)
                                            .ToList();
        }


        private void ConfigureGrid()  //grid for manage products
        {
            // Prevent duplicate column creation
            if (dgvManageProd.Columns["Edit"] == null)
            {
                DataGridViewButtonColumn editCol = new DataGridViewButtonColumn();
                editCol.Name = "Edit";
                editCol.HeaderText = ""; // no column name
                editCol.Text = "Edit";
                editCol.UseColumnTextForButtonValue = true;
                dgvManageProd.Columns.Add(editCol);
            }

            if (dgvManageProd.Columns["Delete"] == null)
            {
                DataGridViewButtonColumn deleteCol = new DataGridViewButtonColumn();
                deleteCol.Name = "Delete";
                deleteCol.HeaderText = ""; // no column name
                deleteCol.Text = "Delete";
                deleteCol.UseColumnTextForButtonValue = true;
                dgvManageProd.Columns.Add(deleteCol);
            }

            dgvManageProd.Columns["Edit"].DisplayIndex = dgvManageProd.Columns.Count - 2;
            dgvManageProd.Columns["Delete"].DisplayIndex = dgvManageProd.Columns.Count - 1;
        }

        private void ClearInputs()
        {
            txtProdID.Clear();
            txtProdName.Clear();
            txtProdPrice.Clear();
            numericDefDisc.Value = 0;
        }

        private void dgvManageProd_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var selectedId = (int)dgvManageProd.Rows[e.RowIndex].Cells["Id"].Value;

                if (dgvManageProd.Columns[e.ColumnIndex].Name == "Edit")
                {
                    // Load product for editing
                    var product = _productService.GetProductById(selectedId);
                    if (product != null)
                    {
                        txtProdID.Text = product.Id.ToString();
                        txtProdName.Text = product.Name;
                        txtProdPrice.Text = product.Rate.ToString();
                        numericDefDisc.Value = product.DiscountPercent;

                        txtProdID.Enabled = false;
                        btnAddProd.Text = "Confirm Edit";
                        label32.Text = "Edit Product";
                        _isEditMode = true;
                    }
                }
                else if (dgvManageProd.Columns[e.ColumnIndex].Name == "Delete")
                {
                    // Confirm delete
                    var confirm = MessageBox.Show("Are you sure to delete this product?", "Confirm Delete",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    if (confirm == DialogResult.Yes)
                    {
                        _productService.DeleteProduct(selectedId);
                        RefreshAllGrids();
                    }
                }
            }
        }

        private void LoadProductsIntoCombo()
        {
            var products = _productService.GetAllProducts();

            // Temporarily clear handlers to avoid accidental triggers
            comboBoxProducts.SelectionChangeCommitted -= comboBoxProducts_SelectionChangeCommitted;

            comboBoxProducts.DataSource = products;
            comboBoxProducts.DisplayMember = "Name";
            comboBoxProducts.ValueMember = "Id";

            // Ensure nothing is pre-selected
            comboBoxProducts.SelectedIndex = -1;
            comboBoxProducts.Text = string.Empty;

            // Reattach handler
            comboBoxProducts.SelectionChangeCommitted += comboBoxProducts_SelectionChangeCommitted;
        }



        private void numericQty_ValueChanged(object sender, EventArgs e)
        {
            UpdateTotalPrice();
        }

        private void numericBillDisc_ValueChanged(object sender, EventArgs e)
        {
            UpdateTotalPrice();
        }

        private void UpdateTotalPrice()
        {
            if (comboBoxProducts.SelectedItem is Product product)
            {
                decimal rate = product.Rate;
                int qty = (int)numericQty.Value;
                decimal discount = numericBillDisc.Value; // percentage
                decimal gross = rate * qty;
                decimal net = gross - (gross * discount / 100);

                txtTotalPrice.Text = net.ToString("0.00");
            }
        }

        private void comboBoxProducts_KeyUp(object sender, KeyEventArgs e)
        {
            // Ignore non-input keys
            if (e.KeyCode == Keys.Enter ||
        e.KeyCode == Keys.Tab ||
        e.KeyCode == Keys.ShiftKey ||
        e.KeyCode == Keys.ControlKey ||
        e.KeyCode == Keys.Alt ||
        e.KeyCode == Keys.Up ||
        e.KeyCode == Keys.Down ||
        e.KeyCode == Keys.Left ||
        e.KeyCode == Keys.Right ||
        e.KeyCode == Keys.PageUp ||
        e.KeyCode == Keys.PageDown)
            {
                return;
            }

            string searchText = comboBoxProducts.Text;

            var products = _productService.GetAllProducts();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                comboBoxProducts.DataSource = products;
            }
            else
            {
                var filtered = products
                    .Where(p => p.Name.Contains(searchText))
                    .ToList();

                comboBoxProducts.DataSource = filtered;
            }

            comboBoxProducts.DisplayMember = "Name";
            comboBoxProducts.ValueMember = "Id";

            comboBoxProducts.DroppedDown = true;
            Cursor.Current = Cursors.Default;

            // Preserve typed text
            comboBoxProducts.Text = searchText;
            comboBoxProducts.SelectionStart = comboBoxProducts.Text.Length;
            comboBoxProducts.SelectionLength = 0;
        }

        private void comboBoxProducts_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (comboBoxProducts.SelectedItem is Product product)
            {
                txtRate.Text = product.Rate.ToString("0.00");
                numericBillDisc.Value = product.DiscountPercent;
                numericQty.Value = 1;
                txtAvailStk.Text = product.AvailableStock.ToString();

                UpdateTotalPrice();

                // Move to next control in tab order
                this.SelectNextControl(comboBoxProducts, true, true, true, true);
            }
        }

        private void BtnAddToCart_Click(object sender, EventArgs e)
        {
            if (!(comboBoxProducts.SelectedItem is Product product))
            {
                MessageBox.Show("Please select a product.");
                return;
            }

            int qty = (int)numericQty.Value;
            decimal rate = product.Rate;
            decimal discountPercent = numericBillDisc.Value; // %
            decimal gross = rate * qty;
            decimal net = gross - (gross * discountPercent / 100m);

            if (qty <= 0)
            {
                MessageBox.Show("Quantity must be greater than zero.");
                return;
            }

            if (isEditing && editingRowIndex >= 0)
            {
                // Update existing row
                DataGridViewRow row = dgvCart.Rows[editingRowIndex];
                row.Cells["ProductId"].Value = product.Id; // hidden column
                row.Cells["SrNo"].Value = editingRowIndex + 1;
                row.Cells["Product"].Value = product.Name;
                row.Cells["Qty"].Value = qty;
                row.Cells["Rate"].Value = rate.ToString("0.00");
                row.Cells["Discount"].Value = discountPercent.ToString("0.##");
                row.Cells["Price"].Value = net.ToString("0.00");

                // Reset editing state
                isEditing = false;
                editingRowIndex = -1;
                lbladdedit.Text = "Add Products to Cart";
                BtnAddToCart.Text = "Add to Cart";
                comboBoxProducts.Enabled = true;
                txtCustName.Enabled = true;
                txtCustContact.Enabled = true;
            }
            else
            {
                // Add new row
                int srNo = dgvCart.Rows.Count + 1;
                dgvCart.Rows.Add(
                    product.Id,                          // hidden ProductId
                    srNo,
                    product.Name,
                    qty,
                    rate.ToString("0.00"),
                    discountPercent.ToString("0.##"),
                    net.ToString("0.00")
                );
            }
            UpdateBillTotals();
        }

        private void SetupCartGrid()
        {
            dgvCart.Columns.Clear();
            dgvCart.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; // 🔑 Make all columns adjust

            var colPid = new DataGridViewTextBoxColumn { Name = "ProductId", HeaderText = "ProductId", Visible = false };
            dgvCart.Columns.Add(colPid);

            dgvCart.Columns.Add("SrNo", "Sr. No");
            dgvCart.Columns.Add("Product", "Product");
            dgvCart.Columns.Add("Qty", "Quantity");
            dgvCart.Columns.Add("Rate", "Rate");
            dgvCart.Columns.Add("Discount", "Discount %");
            dgvCart.Columns.Add("Price", "Price");

            // Add Edit and Delete buttons
            DataGridViewButtonColumn btnEdit = new DataGridViewButtonColumn();
            btnEdit.HeaderText = "";
            btnEdit.Text = "Edit";
            btnEdit.UseColumnTextForButtonValue = true;
            btnEdit.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells; // 🔑 Button only takes needed space
            dgvCart.Columns.Add(btnEdit);

            DataGridViewButtonColumn btnDelete = new DataGridViewButtonColumn();
            btnDelete.HeaderText = "";
            btnDelete.Text = "Delete";
            btnDelete.UseColumnTextForButtonValue = true;
            btnDelete.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells; // 🔑 Button only takes needed space
            dgvCart.Columns.Add(btnDelete);
        }

        private void dgvCart_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            // Ensure it's a button column
            if (dgvCart.Columns[e.ColumnIndex] is DataGridViewButtonColumn)
            {
                string columnName = dgvCart.Columns[e.ColumnIndex].HeaderText;

                if (e.ColumnIndex == dgvCart.Columns.Count - 2) // second last column = Edit
                {
                    // ---- Edit Logic ----
                    editingRowIndex = e.RowIndex;
                    isEditing = true;

                    DataGridViewRow row = dgvCart.Rows[e.RowIndex];
                    comboBoxProducts.Text = row.Cells["Product"].Value.ToString();
                    numericQty.Value = Convert.ToInt32(row.Cells["Qty"].Value);
                    txtRate.Text = row.Cells["Rate"].Value.ToString();
                    numericBillDisc.Value = Convert.ToDecimal(row.Cells["Discount"].Value);
                    txtTotalPrice.Text = row.Cells["Price"].Value.ToString();

                    lbladdedit.Text = "Edit Cart Item";
                    BtnAddToCart.Text = "Edit Item";

                    comboBoxProducts.Enabled = false;
                    txtCustName.Enabled = false;
                    txtCustContact.Enabled = false;
                }
                else if (e.ColumnIndex == dgvCart.Columns.Count - 1) // last column = Delete
                {
                    // ---- Delete Logic ----
                    dgvCart.Rows.RemoveAt(e.RowIndex);

                    // Reorder Sr. No
                    for (int i = 0; i < dgvCart.Rows.Count; i++)
                    {
                        dgvCart.Rows[i].Cells["SrNo"].Value = i + 1;
                    }

                    UpdateBillTotals();
                }
            }
        }



        private void UpdateBillTotals()
        {
            decimal grossTotal = 0;
            decimal discountTotal = 0;
            decimal netTotal = 0;

            foreach (DataGridViewRow row in dgvCart.Rows)
            {
                if (row.IsNewRow) continue;

                decimal qty = Convert.ToDecimal(row.Cells["Qty"].Value);
                decimal rate = Convert.ToDecimal(row.Cells["Rate"].Value);
                decimal discountPercent = Convert.ToDecimal(row.Cells["Discount"].Value);

                decimal lineGross = qty * rate;
                decimal lineDiscount = lineGross * (discountPercent / 100m);
                decimal lineNet = lineGross - lineDiscount;

                grossTotal += lineGross;
                discountTotal += lineDiscount;
                netTotal += lineNet;
            }

            // update text fields
            txtAmtBDisc.Text = grossTotal.ToString("0.00");
            txtDiscAmount.Text = discountTotal.ToString("0.00");
            txtNetTotal.Text = netTotal.ToString("0.00");
        }

        
        private void btnGenerateInvoice_Click(object sender, EventArgs e)
        {
            try
            {
                // 0) Basic validations
                if (dgvCart.Rows.Count == 0)
                {
                    MessageBox.Show("Cart is empty.", "Generate Invoice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var currentUser = SessionManager.GetUser();
                if (currentUser == null)
                {
                    MessageBox.Show("Please login again.", "Session Expired", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 1) Build sale items from cart grid
                var saleItems = new List<SaleItem>();
                decimal grossTotal = 0m, netTotal = 0m, discountTotal = 0m;

                foreach (DataGridViewRow row in dgvCart.Rows)
                {
                    if (row.IsNewRow) continue;

                    int productId = Convert.ToInt32(row.Cells["ProductId"].Value);
                    string productName = row.Cells["Product"].Value?.ToString() ?? "";
                    int qty = Convert.ToInt32(row.Cells["Qty"].Value);
                    decimal rate = Convert.ToDecimal(row.Cells["Rate"].Value);
                    decimal disc = Convert.ToDecimal(row.Cells["Discount"].Value);
                    decimal price = Convert.ToDecimal(row.Cells["Price"].Value);

                    grossTotal += rate * qty;
                    netTotal += price;
                    discountTotal += (rate * qty) - price;

                    saleItems.Add(new SaleItem
                    {
                        ProductId = productId,
                        ProductName = productName,
                        Quantity = qty,
                        Rate = rate,
                        DiscountPercent = disc,
                        FinalPrice = price
                    });
                }

                // 2) Payment mode (change this to however you collect it)
                // Example: a ComboBox named comboPaymentMode with "Cash/Card/UPI"
                PaymentMode mode;
                try
                {
                    mode = GetSelectedPaymentMode();
                }
                catch (InvalidOperationException ex)
                {
                    MessageBox.Show(ex.Message, "Payment Mode", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 3) Persist sale + items and update product stock (atomic)
                int invoiceNo = _saleService.CreateSale(
                    salesmanId: currentUser.Id,
                    salesmanName: currentUser.Name,
                    customerName: txtCustName.Text,
                    customerContact: txtCustContact.Text,
                    mode: mode,
                    items: saleItems
                );

                // 4) Update totals UI (final)
                txtAmtBDisc.Text = grossTotal.ToString("0.00");
                txtDiscAmount.Text = discountTotal.ToString("0.00");
                txtNetTotal.Text = netTotal.ToString("0.00");

                // 5) Generate PDF
                // 5) Generate PDF
                string invoicesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Invoices");
                string filePath = Path.Combine(invoicesDir, $"Invoice_{invoiceNo}.pdf");

                var saleForPdf = new Sale
                {
                    Id = invoiceNo,
                    SaleDate = DateTime.Now,
                    SalesmanName = currentUser.Name,
                    CustomerName = txtCustName.Text,
                    CustomerContact = txtCustContact.Text,
                    PaymentMode = mode,
                    GrandTotal = netTotal
                };

                // Generate PDF invoice
                InvoicePdfGenerator.Generate(filePath, saleForPdf, saleItems);

                PrinterHelper.PrintInvoiceAsync(saleForPdf, saleItems);

                // 7) Clear cart and reset UI
                dgvCart.Rows.Clear();
                txtAmtBDisc.Text = "0.00";
                txtDiscAmount.Text = "0.00";
                txtNetTotal.Text = "0.00";
                numericQty.Value = 1;
                numericBillDisc.Value = 0;
                comboBoxProducts.SelectedIndex = -1;
                txtAvailStk.Clear();
                txtRate.Clear();
                txtTotalPrice.Clear();

                RefreshAllGrids();
                LoadSalesHistory();

                MessageBox.Show($"Invoice #{invoiceNo} generated successfully.", "Success",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to generate invoice.\n" + ex.Message,
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private PaymentMode GetSelectedPaymentMode()
        {
            switch (true)
            {
                case bool _ when radioCash.Checked:
                    return PaymentMode.Cash;

                case bool _ when radioCard.Checked:
                    return PaymentMode.Card;

                case bool _ when radioUPI.Checked:
                    return PaymentMode.UPI;

                default:
                    throw new InvalidOperationException("No payment mode selected.");
            }
        }

        private void btnHisSearch_Click(object sender, EventArgs e)
        {
            int? invoiceNo = null;
            if (int.TryParse(txtHisInvNo.Text, out int parsedInvoice))
                invoiceNo = parsedInvoice;

            string customer = string.IsNullOrWhiteSpace(txtHisCust.Text)
                ? null
                : txtHisCust.Text.Trim();

            DateTime? from = dtFrom.Checked ? dtFrom.Value.Date : (DateTime?)null;
            DateTime? to = dtTo.Checked ? dtTo.Value.Date : (DateTime?)null;

            var filtered = _saleService.SearchSales(customer, invoiceNo, from, to);

            dgvSaleHistory.DataSource = null;
            dgvSaleHistory.DataSource = filtered;
        }

        private void dgvSaleHistory_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return; // ignore header clicks

            if (dgvSaleHistory.Columns[e.ColumnIndex].Name == "PrintInvoice")
            {
                var row = dgvSaleHistory.Rows[e.RowIndex];
                int invoiceNo = Convert.ToInt32(row.Cells["InvoiceNo"].Value);

                // get sale items for this invoice
                var saleItems = _saleService.GetSaleItemsBySaleId(invoiceNo);

                if (saleItems == null || saleItems.Count == 0)
                {
                    MessageBox.Show("No sale items found for this invoice.");
                    return;
                }

                // Call print function
                PrinterHelper(invoiceNo, saleItems);
            }
        }



    }
}


