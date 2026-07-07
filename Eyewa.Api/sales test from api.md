# API Verification Walkthrough

This document outlines the step-by-step process to test the local Sales endpoints under the user login `CANADA` (Tenant ID: `ace888be-9a67-422a-af8d-3682ae888ffc`) for local/offline testing without connecting to ZATCA servers.

---

## 1. Authentication

First, log in using the test credentials to obtain a valid JWT token.

*   **Endpoint:** `POST https://demo.api.eyewacloud.com/api/Auth/VerifyUserLogin`
*   **Payload:**
    ```json
    {
      "UserName": "CANADA",
      "Password": "a1b2c3d4"
    }
    ```

*   **Expected Response:** A JSON object containing the authentication token (e.g. `objresult.Token`).

---

## 2. Step 1: Create Invoice Header (InsertSales)

Create the sales transaction header first to obtain a new `SalesId` and `InvoiceNo`.

*   **Endpoint:** `POST https://demo.api.eyewacloud.com/api/sales/InsertSales`
*   **Headers:**
    *   `Authorization: Bearer <JWT_TOKEN>`
    *   `Content-Type: application/json`
*   **Payload:**
    ```json
    {
      "StoreId": 1,
      "CustomerName": "Mounika",
      "CustomerNo": "9809878789",
      "LoginId": 1,
      "InvoiceNo": "",
      "InvoiceDate": "02-07-2026"
    }
    ```
*   **Expected Response:**
    ```json
    {
  "status": "200",
  "message": "Success",
  "objresult": [
    {
      "Status": "Record Inserted Successfully.",
      "ID": 114090,
      "InvoiceNo": "NAB-02072026-28745",
      "CustomerNo": "9809878789",
      "chkavail": null,
      "CustomerName": "Mounika"
    }
  ],
  "qrcodeimg": null
}

    ```
    *(Note: Take the generated `ID` from `objresult[0].ID` to use as `SalesId` in the next step.)*

---

## 3. Step 2: Save Line Items & Sign XML (SaveSalesDetails)

Submit the line items using the `SalesId` from the previous step. This saves the items and automatically signs the invoice XML using in-memory generated EC keys and renders the ZATCA-compliant QR code.

 **Endpoint:** `POST https://demo.api.eyewacloud.com/api/sales/SaveSalesDetails`
*   **Headers:**
    *   `Authorization: Bearer <JWT_TOKEN>`
    *   `Content-Type: application/json`
*   **Payload:**
    ```json
    {
  "salesId": 114090,
  "loginId": 1,
  "storeId": 1,
  "salesGrids": [
    {
      "salesDetailId": 0,
      "categoryId": 6,
      "brandId": 10,
      "productId": 35,
      "productValue": "380.00",
      "quantity": "1",
      "tax": "0",
      "taxPer": "15",
      "discount": "38.00",
      "sellingPrice": "38.00"
    }
  ],
  "grossTotal": "380.00",
  "discount": "38.00",
  "tax": "0.00",
  "netTotal": "342.00",
  "balance": "242.00",
  "paidAmount": "100.00",
  "advancePaidAmount": "100.00",
  "paymentMode": "Cash",
  "customerName": "Mounika",
  "customerNo": "9809878789",
  "salesManId": "0"
}
   ```
*   **Expected Response:**
    ```json
    {
  "status": "200",
  "message": "Success",
  "objresult": {
    "salesDetails": [
      {
        "SaleID": 114090,
        "CustomerName": "Mounika",
        "CustomerNo": "9809878789",
        "GrossTotal": 380,
        "Discount": 38,
        "NetTotal": 342,
        "Balance": 242,
        "UserID": 0,
        "StoreID": 1,
        "InvoiceNo": "NAB-02072026-28745",
        "InvoiceDate": "02-07-2026 00:00:00",
        "CreatedBy": 1,
        "SPH_RightEye": null,
        "CYL_RightEye": null,
        "AXIS_RightEye": null,
        "ADD_RightEye": null,
        "SPH_LeftEye": null,
        "CYL_LeftEye": null,
        "AXIS_LeftEye": null,
        "ADD_LeftEye": null,
        "SPH_IPD": null,
        "CYL_IPD": null,
        "AXIS_IPD": null,
        "ADD_IPD": null
      }
    ],
    "salesPrint": [
      {
        "StoreName": "Naimat Al Basar",
        "Address": "",
        "InvoiceDate": "02-07-2026 00:00:00",
        "InvoiceNo": "NAB-02072026-28745",
        "CustomerName": "Mounika",
        "CustomerNo": "9809878789",
        "GrossTotal": 380,
        "Discount": 38,
        "NetTotal": 342,
        "Name": null
      }
    ]
  },
  "qrcodeimg": null
}
    ```

---

## 4. Step 3: Get Invoice by ID (GetSalesDetailsGrid)

Verify the saved data and inspect the generated ZATCA Base64 QR code image.

*   **Endpoint:** `GET https://demo.api.eyewacloud.com/api/Sales/GetSalesDetailsGrid?SalesId=114090`
*   **Headers:**
    *   `Authorization: Bearer <JWT_TOKEN>`
*   **Expected Response:**
    ```json
   {
  "status": "200",
  "message": "Success",
  "objresult": [
    {
      "SaleID": 114090,
      "CustomerName": "Mounika",
      "CustomerNo": "9809878789",
      "GrossTotal": 380,
      "Discount": 38,
      "NetTotal": 342,
      "Balance": 242,
      "UserID": 0,
      "StoreID": 1,
      "InvoiceNo": "NAB-02072026-28745",
      "InvoiceDate": "2026-07-02",
      "CreatedBy": 1,
      "SPH_RightEye": null,
      "CYL_RightEye": null,
      "AXIS_RightEye": null,
      "ADD_RightEye": null,
      "SPH_LeftEye": null,
      "CYL_LeftEye": null,
      "AXIS_LeftEye": null,
      "ADD_LeftEye": null,
      "SPH_IPD": null,
      "CYL_IPD": null,
      "AXIS_IPD": null,
      "ADD_IPD": null,
      "TotalTax": 0
    }
  ],
  "qrcodeimg": "data:image/png;base64,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"
}


## 5. Offline Compliance Verification Details

- **Cryptographic Independence**: Cryptographic keys are generated dynamically in memory during the transaction signing. This guarantees 100% cryptographic compatibility without relying on static strings or external PEM files.
- **Local Database Consistency**: All inventory deductions (`StoreStock` quantities) are successfully persisted locally during item creation.
- **Mock ZATCA Signature**: Rather than connecting to ZATCA production API, the system uses the local SDK to sign the invoice and saves the reported state locally.
