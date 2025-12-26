# Tổng Quan Về Hệ Thống Bot và Vũ Khí Trong Game

## 1. Cấu Trúc Hệ Thống Bot

### 1.1. Thư mục và tài nguyên
- **Thư mục dữ liệu**: `Assets/_Game_/_DataSO/Bot/` chứa các Scriptable Object định nghĩa thông số của từng loại bot.
- **Thư mục script**: `Assets/_Game_/Scripts/Enemy/` chứa mã nguồn xử lý logic của bot.
- **Thư mục định nghĩa bot**: `Assets/_Game_/_DataSO/BotDefinition/` chứa các file asset định nghĩa loại bot và prefab tương ứng.

### 1.2. Các thành phần chính

#### BotConfigSO
- Là Scriptable Object chứa các thông số cơ bản của bot:
  - `health`: Máu của bot
  - `damage`: Sát thương bot gây ra
  - `isImportant`: Đánh dấu bot quan trọng

#### CharacterNetwork
- Kế thừa từ `EnemyBase`, là class chính điều khiển logic của bot.
- Xử lý việc nhận sát thương và cái chết của bot.
- Giao tiếp với hệ thống spawn bot thông qua `BotIdentity`.

#### BotSpawnManager
- Là singleton quản lý việc tạo ra bot trong game.
- Nhận lệnh spawn từ `GameManager` hoặc `MinionSpawner`.
- Sử dụng `BotDefinition` để tạo ra các instance của bot.
- Theo dõi các bot đang hoạt động trong scene.

#### Default_Move
- Là một state trong hệ thống state machine của bot.
- Xử lý logic di chuyển của bot theo các điểm đường đi được chỉ định.
- Sử dụng `WaypointMovementUtility` để di chuyển và xoay bot.

## 2. Cấu Trúc Hệ Thống Vũ Khí

### 2.1. Thư mục và tài nguyên
- **Thư mục dữ liệu**: `Assets/_Game_/_DataSO/Weapon/` chứa các Scriptable Object định nghĩa thông số của từng loại vũ khí.
- **Thư mục script**: `Assets/_Game_/Scripts/WeaponBase/` chứa mã nguồn xử lý logic của vũ khí.

### 2.2. Các thành phần chính

#### Weapon26.asset
- Là Scriptable Object chứa các thông số của vũ khí:
  - `damage`: Sát thương mỗi viên đạn
  - `FireRate`: Tốc độ bắn
  - `bulletCount`: Số lượng đạn trong băng
  - `reloadTime`: Thời gian nạp đạn
  - Các hiệu ứng âm thanh và animation

#### Weapon26.cs
- Kế thừa từ `ReloadableWeapons`, là class xử lý logic cụ thể của vũ khí.
- Gọi hàm `Shoot()` để bắn đạn.
- Xử lý logic nạp đạn và hiệu ứng nòng súng.

#### ReloadableWeapons.cs
- Kế thừa từ `WeaponBase`, là class cơ sở cho các vũ khí có thể nạp đạn.
- Xử lý logic bắn đạn, nạp đạn và hiệu ứng.
- Quản lý số lượng đạn và nhiệt độ nòng súng.

## 3. Mối Quan Hệ Giữa Bot và Vũ Khí

### 3.1. Gán vũ khí cho bot
- Bot không trực tiếp chứa vũ khí trong mã nguồn hiện tại.
- Vũ khí được gán cho người chơi thông qua `GameController`.
- Bot sử dụng các hành vi được định nghĩa sẵn (AI) để tấn công, nhưng không có logic cụ thể về việc trang bị vũ khí.

### 3.2. Tấn công của bot
- Bot chuyển sang trạng thái `Attack` khi đến cuối đường đi.
- Logic tấn công của bot chưa được cung cấp trong các file đã đọc, nhưng có thể sử dụng các hệ thống vũ khí tương tự như người chơi với các tham số được định nghĩa trong `BotDefinition`.

## 4. Kết Luận

Hệ thống bot và vũ khí trong game được thiết kế riêng biệt:
- **Bot**: Được quản lý bởi `BotSpawnManager`, di chuyển theo đường đi và có các trạng thái được điều khiển bởi state machine.
- **Vũ khí**: Được định nghĩa bởi Scriptable Object và xử lý logic bắn đạn thông qua hệ thống class kế thừa từ `WeaponBase`.
- **Mối quan hệ**: Hiện tại chưa có sự liên kết trực tiếp giữa bot và vũ khí trong mã nguồn, người chơi sử dụng vũ khí thông qua `GameController`.