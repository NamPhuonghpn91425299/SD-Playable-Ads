Chắc chắn rồi! Bạn đã đi đến bước cuối cùng và quan trọng nhất: tài liệu hóa kiến trúc và hướng dẫn sử dụng. Một hệ thống tốt cần có tài liệu tốt để người khác (hoặc chính bạn trong tương lai) có thể hiểu, sử dụng và mở rộng nó.

Dưới đây là tài liệu tổng hợp hoàn chỉnh cho hệ thống của bạn, được trình bày dưới dạng Markdown.

---

# Tài Liệu Hệ Thống Spawn Bot

## 1. Mục Tiêu và Triết Lý Thiết Kế

Hệ thống này được xây dựng để quản lý vòng đời của các "bot" trong một trò chơi, từ việc chúng được sinh ra theo một kịch bản định trước cho đến khi bị tiêu diệt.

Triết lý thiết kế cốt lõi bao gồm:

*   **Phân Tách Trách Nhiệm (Separation of Concerns):** Mỗi class có một vai trò duy nhất và rõ ràng. `GameManager` quản lý luồng chơi, `BotSpawnManager` quản lý việc tạo bot, `ConditionManager` quản lý các điều kiện.
*   **Hướng Dữ Liệu (Data-Driven):** Toàn bộ kịch bản của các vòng chơi được định nghĩa trong các file dữ liệu (ScriptableObjects), cho phép Game Designer thay đổi màn chơi mà không cần đụng đến code.
*   **Giảm Phụ Thuộc (Loose Coupling):** Các thành phần giao tiếp với nhau qua "hợp đồng" (interfaces) và "sự kiện" (events), thay vì các tham chiếu trực tiếp. Điều này giúp hệ thống dễ dàng mở rộng và bảo trì.
*   **Tối Ưu Hóa Hiệu Năng (Optimized for Performance):** Sử dụng các kỹ thuật như caching, object pooling và các cấu trúc dữ liệu hiệu năng cao (HashSet, Dictionary) để giảm thiểu rác và tăng tốc độ xử lý.

---

## 2. Sơ Đồ Kiến Trúc và Luồng Hoạt Động

Hệ thống hoạt động theo một luồng thông tin rõ ràng:

**Luồng "Ra Lệnh" (Từ trên xuống):**
1.  **`GameManager` (Bộ Não):** Quyết định bắt đầu một round mới.
2.  **`RoundSO` (Kịch Bản):** `GameManager` đọc dữ liệu từ file `RoundSO` tương ứng để biết round này có những bước spawn nào.
3.  **`GameManager` -> `BotSpawnManager` (Ủy Quyền):** `GameManager` tạo ra các `SpawnRequest` (Yêu cầu Spawn) dựa trên `RoundSO` và gửi chúng đến `BotSpawnManager`.
4.  **`BotSpawnManager` (Nhà Máy):** Nhận `SpawnRequest`, chờ các điều kiện (`ISpawnCondition`) được thỏa mãn, sau đó `Instantiate` Prefab từ `SpawnableDefinition` tương ứng.

**Luồng "Báo Cáo" (Từ dưới lên):**
1.  **Một Bot bị tiêu diệt:** Script `SimpleKillTest` (hoặc hệ thống vũ khí) gọi hàm `System_Destroy()` trên `SpawnableWrapper` của bot.
2.  **`SpawnableWrapper` (Tấm Căn Cước):** Bắn ra sự kiện `OnSystemDestroy`.
3.  **`BotSpawnManager` (Nhà Máy):** Lắng nghe sự kiện `OnSystemDestroy`, thực hiện việc dọn dẹp (xóa khỏi danh sách, hủy GameObject) và sau đó bắn ra sự kiện toàn cục `OnBotKilled`.
4.  **`GameManager` & `ConditionManager` (Các Quản Lý Cấp Cao):** Cả hai đều lắng nghe sự kiện `OnBotKilled`.
    *   `GameManager` cập nhật bộ đếm `killedBotsForRound`.
    *   `ConditionManager` cập nhật bộ đếm của nó và thông báo cho các điều kiện spawn đang chờ.

---

## 3. Các Thành Phần Chính và Vai Trò

| Tên Class | Vai Trò Chính | Ghi Chú |
| :--- | :--- | :--- |
| **`GameManager`** | **Bộ Não / Đạo Diễn:** Quản lý trạng thái game, bắt đầu/kết thúc round, đếm số bot tổng thể. | Đây là điểm khởi đầu của mọi logic gameplay. |
| **`BotSpawnManager`** | **Nhà Máy Spawn Bot:** Nhận yêu cầu và thực hiện việc tạo bot. Không biết gì về round hay màn chơi. | Chỉ làm một việc: spawn theo yêu cầu. |
| **`ConditionManager`** | **Trọng Tài:** Theo dõi các sự kiện toàn cục (như số kill) và thông báo cho các điều kiện. | Giúp các điều kiện không cần tự lắng nghe sự kiện. |
| **`RoundSO`** | **Kịch Bản Chi Tiết:** File dữ liệu chứa các bước spawn, tên round, thời gian chờ. | Nơi làm việc chính của Game Designer. |
| **`SpawnableDefinition`** | **Bản Thiết Kế Bot:** Liên kết một `BotType` (enum) với một `Prefab` cụ thể. | Giúp hệ thống biết "loại bot" này trông như thế nào. |
| **`SpawningUnitController`**| **Bot Mẹ Đẻ Bot Con:** Một component tùy chọn gắn lên bot để cho nó khả năng spawn ra các bot khác. | Tạo ra các kịch bản phức tạp hơn. |
| **`SpawnableWrapper`** | **Tấm Căn Cước:** Component bắt buộc trên mọi bot, cung cấp thông tin chung và sự kiện `OnSystemDestroy`. | Giúp các hệ thống khác giao tiếp với bot một cách thống nhất. |

---

## 4. Hướng Dẫn Sử Dụng Cho Game Designer

Để thiết kế một màn chơi mới, bạn chỉ cần làm việc với các file dữ liệu (ScriptableObjects) mà không cần viết code.

#### **Bước 1: Chuẩn bị Prefab cho Bot**

1.  Tạo một Prefab cho bot của bạn.
2.  **Bắt buộc:** Gắn component **`SpawnableWrapper`** vào Prefab.
3.  **Tùy chọn:** Nếu bạn muốn bot này có thể đẻ ra bot con, hãy gắn thêm component **`SpawningUnitController`** và cấu hình `SpawnContract` của nó.
4.  Đảm bảo bot có `Collider` để script test có thể click vào nó.

#### **Bước 2: Tạo "Bản Thiết Kế" cho Bot**

1.  Trong cửa sổ Project, click chuột phải -> **Create -> Spawning/System/1. Spawnable Definition**.
2.  Đặt tên cho file, ví dụ `Def_Grunt`.
3.  Chọn file vừa tạo. Trong Inspector:
    *   **Bot Type:** Chọn một `BotType` từ danh sách enum (ví dụ: `Grunt`).
    *   **Prefab:** Kéo Prefab của bot bạn đã tạo ở Bước 1 vào đây.

#### **Bước 3: Thiết Kế một Round (Vòng Chơi)**

Đây là bước quan trọng nhất.

1.  Trong cửa sổ Project, click chuột phải -> **Create -> Spawning/Gameplay/1. Round Kịch Bản**.
2.  Đặt tên cho file, ví dụ `Round_01_Wave_1`.
3.  **Cách dễ nhất:** Mở cửa sổ editor tùy chỉnh bằng cách vào **Tools -> SpawnSystem/Advanced Round Editor**.
4.  Chọn file `Round_01_Wave_1` của bạn trong Project. Cửa sổ editor sẽ hiển thị nội dung của nó.
5.  **Trong cửa sổ editor:**
    *   **Round Name:** Đặt tên cho round (ví dụ: "Đợt Tấn Công Đầu Tiên").
    *   **Delay After Complete:** Đặt thời gian chờ trước khi sang round tiếp theo.
    *   **Spawn Steps:** Bấm nút `+` (Add) để thêm một bước spawn mới.
    *   **Chọn một Step:**
        *   **Bot To Spawn:** Chọn loại bot bạn muốn spawn (ví dụ: `Grunt`).
        *   **Quantity:** Số lượng bot sẽ spawn trong bước này.
        *   **Delay Between Spawns:** Thời gian chờ giữa mỗi con bot nếu `Quantity > 1`.
        *   **Conditions:** Thêm các điều kiện để bước này được kích hoạt. Ví dụ:
            *   Thêm `Timer`, đặt `Wait Time = 10` -> Bước này sẽ bắt đầu sau 10 giây.
            *   Thêm `KillCount`, đặt `Target Kills = 5` -> Bước này sẽ bắt đầu sau khi người chơi giết được 5 bot.

#### **Bước 4: Thêm Round vào Màn Chơi**

1.  Tìm đến GameObject **`_GameManager`** trong Scene của bạn.
2.  Trong Inspector, tìm đến thuộc tính **`Level Rounds`**.
3.  Tăng kích thước của danh sách và kéo file `Round_01_Wave_1.asset` của bạn vào ô trống. Bạn có thể thêm nhiều round để tạo thành một màn chơi hoàn chỉnh.

#### **Bước 5: Chạy và Kiểm Tra**

1.  Đảm bảo bạn có một GameObject `_TestManager` trong Scene chứa script `SimpleKillTest`.
2.  Bấm **Play**.
3.  Sử dụng các nút **"Kill First Bot"** và **"Force Next Round"** để kiểm tra xem hệ thống có hoạt động như mong đợi không. Quan sát các log trong Console.

---

## 5. Hướng Dẫn Mở Rộng Hệ Thống (Dành cho Lập Trình Viên)

Hệ thống được thiết kế để dễ dàng mở rộng.

#### **Làm thế nào để thêm một loại Bot mới?**

1.  Mở file `BotType.cs` và thêm một giá trị mới vào enum `BotType` (ví dụ: `Grenadier`).
2.  Làm theo **Hướng Dẫn Sử Dụng Cho Game Designer** ở trên để tạo Prefab và `SpawnableDefinition` cho `Grenadier`.
3.  Bây giờ, Game Designer có thể chọn `Grenadier` trong cửa sổ editor.

#### **Làm thế nào để thêm một Điều Kiện Spawn mới?**

1.  Tạo một class mới, ví dụ `PlayerHealthCondition`, và cho nó implement interface **`ISpawnCondition`**.
2.  Triển khai 3 hàm bắt buộc: `IsMet()`, `Reset()`, và `Terminate()`.
    *   Trong `IsMet()`, bạn sẽ kiểm tra máu của người chơi.
    *   Trong `Reset()` và `Terminate()`, bạn có thể đăng ký/hủy đăng ký sự kiện thay đổi máu của người chơi.
3.  Mở file `ConditionDefinition.cs` và thêm một giá trị mới vào enum `EConditionType`, ví dụ: `PlayerHealthLow`.
4.  Trong hàm `CreateRuntimeCondition()` của `ConditionDefinition.cs`, thêm một `case` mới:
    ```csharp
    case EConditionType.PlayerHealthLow: return new PlayerHealthCondition(TargetHealth);
    ```
5.  Bây giờ, Game Designer có thể chọn điều kiện `PlayerHealthLow` trong cửa sổ editor.