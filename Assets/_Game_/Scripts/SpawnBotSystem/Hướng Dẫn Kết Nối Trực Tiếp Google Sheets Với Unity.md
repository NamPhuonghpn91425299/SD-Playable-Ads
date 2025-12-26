### **README: Hướng Dẫn Kết Nối Trực Tiếp Google Sheets Với Unity**

Tài liệu này sẽ hướng dẫn bạn cách cài đặt một lần duy nhất để hệ thống có thể tự động đồng bộ hóa dữ liệu kịch bản từ Google Sheets vào Unity Editor.

#### **Mục Lục**
1.  **Phần 1: Chuẩn Bị File Google Sheets**
2.  **Phần 2: Cài Đặt Phía Google Cloud (Lấy "Chìa Khóa")**
3.  **Phần 3: Cài Đặt Phía Unity (Kết Nối)**
4.  **Phần 4: Workflow Sử Dụng Hàng Ngày**
5.  **Phần 5: Xử Lý Lỗi Thường Gặp**

---

### **Phần 1: Chuẩn Bị File Google Sheets**

Trước tiên, bạn cần có một file Google Sheets được cấu trúc đúng cách.

1.  **Tạo File**: Mở [sheets.google.com](https://sheets.google.com) và tạo một bảng tính mới. Đặt tên cho nó, ví dụ: `MyGame - Level Design`.

2.  **Tạo Sheet Kịch Bản**: Tạo một sheet (tab) và đặt tên cho nó, ví dụ: `Level1_Desert`.

3.  **Thiết Lập Cấu Trúc Cột**:
    *   **Dòng 1**: Là **tên cột dễ đọc**.
    *   **Dòng 2**: Là **dòng mô tả, hướng dẫn** (sẽ được tool bỏ qua).
    *   **Dòng 3 trở đi**: Là **dữ liệu kịch bản** thực tế.

**Ví dụ về cấu trúc sheet `Level1_Desert`:**

| (Dòng 1) | **Round** | **Bot To Spawn** | **Quantity** | **Delay (Bot)** | **Move Type** | **Conditions** | **Delay (Round)** |
|---|---|---|---|---|---|---|---|
| (Dòng 2) | *(Số thứ tự)* | *(Chọn từ enum)* | *(Số lượng)* | *(Giây)* | *(Chọn từ enum)* | *(Timer(s); KillCount(n,type))* | *(Giây, sau round)* |
| (Dòng 3) | 1 | Grunt | 5 | 0.5 | Ground_Main_Path | | 5 |
| (Dòng 4) | 1 | Grunt | 3 | 0.3 | Ground_Flank_Left | Timer(5) | |
| (Dòng 5) | 2 | Sniper | 2 | 1 | Air_Attack_Route | KillCount(5,Bot) | 8 |

4.  **Lấy Spreadsheet ID**:
    *   Nhìn vào thanh địa chỉ URL của trình duyệt.
    *   Sao chép chuỗi ký tự dài nằm giữa `.../spreadsheets/d/` và `/edit`.
    *   Ví dụ: `https://docs.google.com/spreadsheets/d/`**`1aBcDeFgHiJkLmNoPqRsTuVwXyZaBcdEfGhIjKlMnOpQ`**`/edit`
    *   **Lưu lại chuỗi ID này, chúng ta sẽ cần nó ở Phần 3.**

---

### **Phần 2: Cài Đặt Phía Google Cloud (Lấy "Chìa Khóa")**

Chúng ta sẽ tạo một "tài khoản robot" để Unity có thể đọc file sheet một cách an toàn.

1.  **Truy cập Google Cloud Console**: Mở [https://console.cloud.google.com/](https://console.cloud.google.com/).
2.  **Tạo Project Mới**: Ở góc trên bên trái, bấm vào menu chọn project -> **NEW PROJECT**. Đặt tên (`MyGame Data`) và bấm **CREATE**.
3.  **Kích hoạt API**:
    *   Dùng thanh tìm kiếm ở trên, gõ **"Google Sheets API"** và chọn nó.
    *   Bấm nút **ENABLE**.
4.  **Tạo Service Account ("Tài khoản Robot")**:
    *   Từ menu bên trái, vào `IAM & Admin -> Service Accounts`.
    *   Bấm **`+ CREATE SERVICE ACCOUNT`**.
    *   **Mục 1**: Đặt tên cho tài khoản (ví dụ: `unity-sheet-reader`) -> Bấm **CREATE AND CONTINUE**.
    *   **Mục 2**: Trong ô "Select a role", tìm và chọn `Project -> Viewer`. -> Bấm **CONTINUE**.
    *   **Mục 3**: Bỏ qua và bấm **DONE**.
5.  **Tải "Chìa Khóa Bí Mật"**:
    *   Trong danh sách, bấm vào email của tài khoản bạn vừa tạo.
    *   Chuyển sang tab **KEYS**.
    *   Bấm **ADD KEY -> Create new key**.
    *   Chọn **JSON** và bấm **CREATE**.
    *   Một file `.json` sẽ tự động được tải về. **Đây là "chìa khóa" của bạn.**
6.  **Chia Sẻ Google Sheets Cho "Robot"**:
    *   Sao chép địa chỉ email của Service Account (có dạng `...@...iam.gserviceaccount.com`).
    *   Quay lại file Google Sheets của bạn, bấm nút **Share** (Chia sẻ).
    *   Dán địa chỉ email của robot vào, đảm bảo nó có quyền **Viewer**, bỏ tick "Notify people" và bấm **Share**.

---

### **Phần 3: Cài Đặt Phía Unity (Kết Nối)**

1.  **Cài Đặt Thư Viện (Nếu chưa có)**:
    *   Dùng **NuGet for Unity**, tìm và cài đặt package `Google.Apis.Sheets.v4`.

2.  **Import "Chìa Khóa" vào Unity**:
    *   Tìm file `.json` bạn đã tải về ở bước trước.
    *   Đổi tên nó thành **`google_credentials`**.
    *   Trong Unity, tạo một thư mục tại `Assets/Resources`.
    *   Kéo file `google_credentials.json` vào thư mục `Assets/Resources`.

3.  **Cấu Hình Tool Importer**:
    *   Mở project Unity của bạn.
    *   Từ menu trên cùng, vào `Tools -> SpawnSystem -> Google Sheets Level Importer`.
    *   Cửa sổ tool sẽ hiện ra.
    *   **Dán Spreadsheet ID** (bạn đã lấy ở Bước 1.4) vào ô "Spreadsheet ID".

---

### **Phần 4: Workflow Sử Dụng Hàng Ngày**

Bây giờ, quy trình cập nhật kịch bản của bạn sẽ siêu nhanh:

1.  **Thiết Kế**: Mở file Google Sheets trên trình duyệt và chỉnh sửa kịch bản (thêm/xóa hàng, thay đổi số lượng, v.v.).
2.  **Đồng Bộ Hóa**:
    *   Mở cửa sổ "Google Sheets Level Importer" trong Unity.
    *   Trong ô "Sheet Name", gõ **tên chính xác** của sheet bạn vừa chỉnh sửa (ví dụ: `Level1_Desert`).
    *   Bấm nút **"Sync Level From Sheet"**.
3.  **Kiểm Tra**: Tool sẽ chạy và hiển thị log. Các file `.asset` tương ứng trong `Assets/Rounds` và `Assets/Levels` sẽ được tự động tạo mới hoặc cập nhật.
4.  **Chơi Thử**: Kéo file `LevelConfigSO` đã được cập nhật vào `GameManager` (nếu cần) và bấm Play để xem ngay thay đổi.

---

### **Phần 5: Xử Lý Lỗi Thường Gặp**

*   **Lỗi `Authentication Failed` hoặc `'google_credentials.json' not found`**:
    *   **Kiểm tra**: File `google_credentials.json` có nằm chính xác trong `Assets/Resources` không? Tên file có đúng là `google_credentials` không? Bạn đã chia sẻ Google Sheet cho email của robot chưa?

*   **Lỗi `Error fetching sheet 'SheetName'`**:
    *   **Kiểm tra**: `Spreadsheet ID` bạn dán vào tool có đúng không? `Sheet Name` bạn gõ vào tool có khớp 100% với tên tab trong Google Sheets không (kể cả chữ hoa/thường và dấu cách)?

*   **Lỗi `Failed to parse row`**:
    *   **Kiểm tra**: Dữ liệu trong các ô của Google Sheets có đúng định dạng không? Các giá trị `enum` (`BotToSpawn`, `botMoveType`) có gõ chính xác như trong code không? Cú pháp `Conditions` có đúng là `Timer(5)` hay `KillCount(10,Bot)` không?