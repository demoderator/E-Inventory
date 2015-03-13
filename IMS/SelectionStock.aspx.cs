﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Configuration;
using IMSCommon.Util;

namespace IMS
{
    public partial class SelectionStock : System.Web.UI.Page
    {
        public static SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["IMSConnectionString"].ToString());
        public static DataSet ProductSet;
        
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                ProductSet = new DataSet();
                #region Populating Product List
                try
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand("Select TOP 200 tbl_ProductMaster.* From tbl_ProductMaster INNER JOIN tblStock_Detail ON tbl_ProductMaster.ProductID = tblStock_Detail.ProductID Where tbl_ProductMaster.Product_Id_Org LIKE '444%' AND tbl_ProductMaster.Status = 1", connection);
                    DataSet ds = new DataSet();
                    SqlDataAdapter sA = new SqlDataAdapter(command);
                    sA.Fill(ds);
                    ProductSet = ds;
                    SelectProduct.DataSource = ds.Tables[0];
                    SelectProduct.DataTextField = "Product_Name";
                    SelectProduct.DataValueField = "ProductID";
                    SelectProduct.DataBind();
                    if (SelectProduct != null)
                    {
                        SelectProduct.Items.Insert(0, "Select Product");
                        SelectProduct.SelectedIndex = 0;
                    }
                }
                catch (Exception ex)
                {

                }
                finally
                {
                    connection.Close();
                }
                #endregion
            }
        }

        private void BindGrid()
        {
            DataTable dt = new DataTable();
            DataSet ds = new DataSet();
            #region Getting Product Details
            try
            {
                DataView dv = new DataView();
                dv = ProductSet.Tables[0].DefaultView;
                dv.RowFilter = "ProductID = '" + SelectProduct.SelectedValue.ToString() + "'";
                dt = dv.ToTable();
                String Query = "Select tblStock_Detail.ProductID AS ProductID ,tbl_ProductMaster.Product_Name AS ProductName, tblStock_Detail.BarCode AS BarCode, tblStock_Detail.Quantity AS Qauntity, tblStock_Detail.ExpiryDate As Expiry, tblStock_Detail.UCostPrice AS CostPrice, tblStock_Detail.USalePrice AS SalePrice, tblStock_Detail.StoredAt AS Location From  tblStock_Detail INNER JOIN tbl_ProductMaster ON tblStock_Detail.ProductID = tbl_ProductMaster.ProductID Where tblStock_Detail.ProductID = '" + Int32.Parse(dt.Rows[0]["ProductID"].ToString()) + "'";

                connection.Open();
                SqlCommand command = new SqlCommand(Query, connection);
                SqlDataAdapter SA = new SqlDataAdapter(command);
                SA.Fill(ds);
                StockDisplayGrid.DataSource = ds;
                StockDisplayGrid.DataBind();

            }
            catch (Exception ex)
            {
            }
            finally
            {
                connection.Close();
            }
            #endregion
        }
        protected void SelectProduct_SelectedIndexChanged(object sender, EventArgs e)
        {

            BindGrid();
            
        }

        protected void StockDisplayGrid_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            StockDisplayGrid.PageIndex = e.NewPageIndex;
            BindGrid();
        }

        protected void StockDisplayGrid_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            StockDisplayGrid.EditIndex = -1;
            BindGrid();
        }

        protected void StockDisplayGrid_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            try
            {
                if (e.CommandName.Equals("UpdateStock"))
                {
                    Label Barcode = (Label)StockDisplayGrid.Rows[StockDisplayGrid.EditIndex].FindControl("BarCode");
                    TextBox Quantity = (TextBox)StockDisplayGrid.Rows[StockDisplayGrid.EditIndex].FindControl("txtQuantity");
                    TextBox UnitCostPrice = (TextBox)StockDisplayGrid.Rows[StockDisplayGrid.EditIndex].FindControl("txtUnitCostPrice");
                    TextBox UnitSalePrice = (TextBox)StockDisplayGrid.Rows[StockDisplayGrid.EditIndex].FindControl("txtUnitSalePrice");

                    DataView dv = ProductSet.Tables[0].DefaultView;
                    dv.RowFilter = "BarCode = '" + long.Parse(Barcode.Text.ToString()) + "'";
                    DataTable dt = dv.ToTable();
                    int ProductID = Int32.Parse(dt.Rows[0]["ProductID"].ToString());

                    if (Barcode.Text.Equals(""))
                    {
                        #region Barcode Generation

                        long BarCodeNumber = 0;
                        #endregion

                        String Query = "Update tblStock_Detail Set BarCode= '" + BarCodeNumber + "', Quantity = '" + Decimal.Parse(Quantity.Text.ToString()) + "', UCostPrice = '" + Decimal.Parse(UnitCostPrice.Text.ToString()) + "', USalePrice = '" + Decimal.Parse(UnitSalePrice.Text.ToString()) + "' Where ProductID = '" + ProductID + "'";
                        connection.Open();
                        SqlCommand command = new SqlCommand(Query, connection);
                        command.ExecuteNonQuery();
                    }
                    else
                    {
                        String Query = "Update tblStock_Detail Set Quantity = '" + Decimal.Parse(Quantity.Text.ToString()) + "', UCostPrice = '" + Decimal.Parse(UnitCostPrice.Text.ToString()) + "', USalePrice = '" + Decimal.Parse(UnitSalePrice.Text.ToString()) + "' Where ProductID = '" + ProductID + "'";
                        connection.Open();
                        SqlCommand command = new SqlCommand(Query, connection);
                        command.ExecuteNonQuery();
                    }
                    WebMessageBoxUtil.Show("Stock Successfully Updated ");
                }
            }
            catch (Exception exp)
            {
            }
            finally
            {
                StockDisplayGrid.EditIndex = -1;
                BindGrid();
            }
        }

        protected void StockDisplayGrid_RowDataBound(object sender, GridViewRowEventArgs e)
        {

        }

        protected void StockDisplayGrid_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            try
            {
                Label Barcode = (Label)StockDisplayGrid.Rows[StockDisplayGrid.EditIndex].FindControl("BarCode");
                DataView dv = ProductSet.Tables[0].DefaultView;
                dv.RowFilter = "BarCode = '" + long.Parse(Barcode.Text.ToString()) + "'";
                DataTable dt = dv.ToTable();
                int ProductID = Int32.Parse(dt.Rows[0]["ProductID"].ToString());

                String Query = "Delete From tblStock_Detail Where ProductID = '" + ProductID + "'";
                connection.Open();
                SqlCommand command = new SqlCommand(Query, connection);
                command.ExecuteNonQuery();
                WebMessageBoxUtil.Show("Stock Successfully Deleted ");
            }
            catch (Exception exp) 
            { 
            }
            finally
            {
                StockDisplayGrid.EditIndex = -1;
                BindGrid();
            }
        }

        protected void StockDisplayGrid_RowEditing(object sender, GridViewEditEventArgs e)
        {
            StockDisplayGrid.EditIndex = e.NewEditIndex;
            BindGrid();
        }
        protected void btnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect("ManageStocks.aspx", false);
        }
    }
}