# Hướng Dẫn Sử Dụng Object Pooling Trong Unity

## Mục Lục
1. [Object Pooling Là Gì?](#object-pooling-là-gì)
2. [Cách Hoạt Động](#cách-hoạt-động)
3. [Implement Object Pooling Trong Project](#implement-object-pooling-trong-project)
    - [3.1. Tổng Quan Kiến Trúc](#31-tổng-quan-kiến-trúc)
    - [3.2. `GameUnit<TEnum>`: Đối Tượng Có Thể Pool](#32-gameunittenum-đối-tượng-có-thể-pool)
    - [3.3. `SimplePool<TEnum>`: Trình Quản Lý Pool Tĩnh](#33-simplepooltenum-trình-quản-lý-pool-tĩnh)
    - [3.4. `PoolControl`: Thiết Lập Ban Đầu](#34-poolcontrol-thiết-lập-ban-đầu)
4. [Cách Sử Dụng](#cách-sử-dụng)
5. [Best Practices](#best-practices)

---

## Object Pooling Là Gì?

**Object Pooling** là một design pattern giúp tái sử dụng các object thay vì liên tục tạo mới (Instantiate) và hủy (Destroy) chúng.

### So Sánh: Không Dùng Pool vs Có Dùng Pool

```csharp
// ❌ KHÔNG DÙNG POOL - Tốn hiệu năng
void SpawnEnemy()
{
    GameObject enemy = Instantiate(enemyPrefab, position, rotation);
    Destroy(enemy, 5f); // Tạo rác cho Garbage Collector!
}

// ✅ DÙNG POOL - Tối ưu (Theo cách của project)
void SpawnEnemy()
{
    // Lấy enemy từ pool, kiểu EnemyEnum là enum đã định nghĩa
    Enemy enemy = SimplePool<EnemyEnum>.Spawn<Enemy>(EnemyEnum.Zombie, position, rotation);
    // enemy đã được tự động active và đặt vị trí
}
```

---

## Cách Hoạt Động

### Sơ Đồ Luồng Hoạt Động Của Hệ Thống Hiện Tại

```
[Game Start (Awake)]
    ↓
[PoolControl Đọc Danh Sách Prefab]
    ↓
[Gọi GameUnit.Preload() cho mỗi prefab]
    ↓
[GameUnit.Preload() gọi SimplePool<TEnum>.Preload()]
    ↓
[SimplePool<TEnum>.Preload() tạo Pool<TEnum> mới]
    ↓
[Pool<TEnum>.Preload() Instantiate N object, Deactive và đưa vào Queue]
    ↓
[Pool Sẵn Sàng]

[Cần Spawn Object (Ví dụ: Bullet)]
    ↓
[Gọi SimplePool<BulletEnum>.Spawn<Bullet>(BulletEnum.Fire, pos, rot)]
    ↓
[SimplePool tìm Pool<BulletEnum> tương ứng]
    ↓
[Pool<BulletEnum>.Spawn() lấy từ Queue hoặc Instantiate mới nếu hết]
    ↓
[Đặt vị trí, active object, thêm vào danh sách actives]
    ↓
[Trả về Bullet đã được khởi tạo]

[Object Cần Despawn (Ví dụ: Bullet hết thời gian sống)]
    ↓
[Gọi SimplePool<BulletEnum>.Despawn(bulletInstance)]
    ↓
[SimplePool tìm Pool<BulletEnum> tương ứng]
    ↓
[Pool<BulletEnum>.Despawn() inactive object, bỏ khỏi danh sách actives, đưa vào Queue]
    ↓
[Object Sẵn Sàng Dùng Lại]
```

---

## Implement Object Pooling Trong Project

Project hiện tại đã có một hệ thống pooling được xây dựng sẵn, bao gồm 3 thành phần chính: `GameUnit<TEnum>`, `SimplePool<TEnum>`, và `PoolControl`.

### 3.1. Tổng Quan Kiến Trúc

1.  **`GameUnit<TEnum>`**: Là lớp cơ sở (`abstract`) cho bất kỳ GameObject nào có thể được đưa vào pool. Nó giữ một `enum` (`_poolType`) để định danh loại của nó, giúp hệ thống pool quản lý nhiều loại object khác nhau.
2.  **`SimplePool<TEnum>`**: Là một lớp tĩnh (`static class`) đóng vai trò là điểm vào chính để tương tác với hệ thống pool. Nó quản lý một từ điển các `Pool<TEnum>`, mỗi `Pool` tương ứng với một loại `TEnum`.
3.  **`Pool<TEnum>`**: Là lớp logic cốt lõi, quản lý vòng đời của một loại object cụ thể. Nó chứa một `Queue` cho các object không hoạt động (`inactives`) và một `List` cho các object đang hoạt động (`actives`).
4.  **`PoolControl`**: Là một `MonoBehaviour` dùng để khởi tạo (preload) các pool ngay khi game bắt đầu (trong `Awake`). Nó chứa một danh sách các prefab cần được preload cùng với số lượng.

### 3.2. `GameUnit<TEnum>`: Đối Tượng Có Thể Pool

Đây là lớp cơ sở (`abstract`) mà bất kỳ GameObject nào có thể được đưa vào pool sẽ kế thừa.

*   **`GameUnit<TEnum>`**: [Xem code tại `Assets/_Game_/Scripts/Extensions/Pooling/GameUnit.cs`](Assets/_Game_/Scripts/Extensions/Pooling/GameUnit.cs)
    *   **`_poolType` (TEnum)**: Giữ một enum để định danh loại của object, giúp hệ thống pool phân biệt các loại object khác nhau (ví dụ: `BulletEnum.Fire`, `EnemyEnum.Soldier`).
    *   **`TF` (Transform)**: Property truy cập Transform của object, được cached để tối ưu hiệu năng.
    *   **`Preload(int amount, Transform parent)`**: Phương thức được ghi đè từ `GameUnitBase`. `PoolControl` sẽ gọi phương thức này để yêu cầu `SimplePool` tạo trước (preload) một số lượng object của loại này.
*   **`GameUnitBase`**: Lớp cơ sở không generic, giúp `PoolControl` có thể tham chiếu đến nhiều loại `GameUnit` khác nhau trong Inspector.
*   **`IPoolable`**: Interface định nghĩa phương thức `Preload`, đảm bảo bất kỳ `GameUnit` nào cũng có thể được preload.

**Cách sử dụng:**
1.  Tạo một script mới cho đối tượng của bạn (ví dụ: `Bullet.cs`).
2.  Cho script này kế thừa `GameUnit<BulletEnum>`, trong đó `BulletEnum` là enum bạn định nghĩa cho loại object đó.
3.  Trong script con, bạn có thể override `OnEnable()` để reset state khi object được lấy ra từ pool, và implement logic để gọi `SimplePool<TEnum>.Despawn(this)` khi object cần được trả về pool.

### 3.3. `SimplePool<TEnum>`: Trình Quản Lý Pool Tĩnh

Đây là trái tim của hệ thống, là lớp tĩnh (`static class`) đóng vai trò là điểm vào chính để tương tác với hệ thống pool. Nó quản lý một từ điển các `Pool<TEnum>` và cung cấp các phương thức `Spawn`, `Despawn`, `Collect`, `Release`.

*   **`SimplePool<TEnum>`**: [Xem code tại `Assets/_Game_/Scripts/Extensions/Pooling/SimplePool.cs`](Assets/_Game_/Scripts/Extensions/Pooling/SimplePool.cs)
    *   **`poolInstances` (Dictionary<TEnum, Pool<TEnum>>)**: Lưu trữ các instance của `Pool<TEnum>`, với key là enum của pool đó. Mỗi loại object (ví dụ: `BulletEnum.Fire`) sẽ có một `Pool` riêng.
    *   **`Preload(GameUnit<TEnum> prefab, int amount, Transform parent)`**: Tạo một `Pool<TEnum>` mới cho một loại prefab (nếu chưa có) và yêu cầu `Pool` đó tạo trước (preload) một số lượng object cụ thể.
    *   **`Spawn<T>(TEnum poolType, Vector3 pos, Quaternion rot, Transform parent = null)`**: Lấy một object từ pool tương ứng với `poolType`. Nếu pool không có object nào sẵn sàng, nó sẽ tự động `Instantiate` một cái mới. Object sau đó được đặt vị trí, kích hoạt và trả về.
    *   **`Despawn(GameUnit<TEnum> unit)`**: Tìm `Pool` tương ứng với loại của `unit` và yêu cầu `Pool` đó trả object về trạng thái inactive.
    *   **`Despawn(GameUnit<TEnum> unit, float delay)`**: Despawn object sau một khoảng trễ.
    *   **`Collect(TEnum poolType)` / `CollectAll()`**: Thu hồi tất cả các object đang hoạt động của một loại hoặc tất cả các loại về trạng thái inactive, hữu ích khi kết thúc một màn chơi.
    *   **`Release(TEnum poolType) / ReleaseAll()`**: Giải phóng hoàn toàn một hoặc tất cả các pool, hủy tất cả các object đã được tạo để giải phóng bộ nhớ, hữu ích khi chuyển scene.
*   **`Pool<TEnum>`**: Lớp logic cốt lõi, quản lý thực tế vòng đời của một loại object.
    *   **`inactives` (Queue<GameUnit<TEnum>>)**: Hàng đợi (FIFO) chứa các object không hoạt động, sẵn sàng được sử dụng lại.
    *   **`actives` (List<GameUnit<TEnum>>)**: Danh sách chứa các object đang hoạt động trong game.
    *   **`Preload(...)`**: Instantiate một số lượng object và ngay lập tức gọi `Despawn` để đưa chúng vào hàng đợi `inactives`.
    *   **`Spawn(...)`**: Lấy object từ `inactives` (hoặc Instantiate mới nếu hết), thiết lập transform, kích hoạt object và thêm vào `actives`.
    *   **`Despawn(...)`**: Vô hiệu hóa object, bỏ khỏi `actives` và đưa vào `inactives`.
    *   **`Collect()` / `Release()`**: Quản lý việc thu hồi và giải phóng bộ nhớ cho pool.

### 3.4. `PoolControl`: Thiết Lập Ban Đầu

Component này được đặt trên một GameObject trong Scene (thường là một GameManager) để tự động khởi tạo (preload) các pool cần thiết ngay khi game bắt đầu (trong `Awake`).

*   **`PoolControl`**: [Xem code tại `Assets/_Game_/Scripts/Extensions/Pooling/PoolControl.cs`](Assets/_Game_/Scripts/Extensions/Pooling/PoolControl.cs)
    *   **`prefabsToPreload` (List<PoolAmount>)**: Danh sách chứa thông tin về các prefab cần được preload. Bạn có thể kéo thả các prefab vào đây trong Inspector.
    *   **`Awake()`**: Khi game bắt đầu, phương thức này sẽ duyệt qua danh sách `prefabsToPreload` và gọi phương thức `Preload()` trên mỗi prefab, từ đó khởi tạo các pool tương ứng.
*   **`PoolAmount`**: Một lớp nhỏ giúp Inspector hiển thị rõ ràng hơn.
    *   **`gameUnitBase` (GameUnitBase)**: Prefab cần được pool. Vì nó là kiểu `GameUnitBase`, bạn có thể kéo bất kỳ prefab nào kế thừa từ `GameUnit<TEnum>` vào đây.
    *   **`parent` (Transform)**: Transform cha của các object được tạo, giúp tổ chức các object trong Hierarchy.
    *   **`amount` (int)**: Số lượng object cần preload ban đầu.

**Cách thiết lập trong Editor:**
1.  Tạo một GameObject trống, đặt tên là "PoolManager" hoặc "GameManager".
2.  Add component `PoolControl` vào GameObject này.
3.  Trong Inspector của `PoolControl`, bạn sẽ thấy danh sách "Prefabs To Preload".
4.  Nhập `Size` của danh sách bằng số lượng loại object bạn muốn preload.
5.  Ở mỗi phần tử trong danh sách:
    *   Kéo prefab của bạn (ví dụ: prefab Bullet) vào ô `Game Unit Base`.
    *   Kéo một GameObject (ví dụ: một Object tên "PooledItems") vào ô `Parent`. Các object được preload sẽ nằm dưới GameObject này.
    *   Nhập số lượng muốn preload vào ô `Amount`.

---

## Cách Sử Dụng

### 1. Tạo Enum cho loại object của bạn

```csharp
// GameEnum.cs (hoặc một file riêng)
public enum BulletEnum { Fire, Ice }
public enum EnemyEnum { Soldier, Tank }
```

### 2. Tạo Script cho object và kế thừa `GameUnit<TEnum>`

```csharp
// Bullet.cs
using UnityEngine;

public class Bullet : GameUnit<BulletEnum>
{
    // ... (code xử lý logic cho bullet như ở ví dụ trên)
}
```

### 3. Tạo Prefab

1.  Tạo một GameObject trong scene (ví dụ: một hình cầu cho đạn).
2.  Add script `Bullet` vào.
3.  Trong component `Bullet`, ở trường `_poolType`, chọn enum tương ứng (ví dụ: `BulletEnum.Fire`).
4.  Kéo thả GameObject này vào folder Prefabs để tạo prefab.

### 4. Cấu hình `PoolControl`

Như đã hướng dẫn ở mục 3.4, kéo prefab vừa tạo vào `PoolControl` để preload.

### 5. Spawn và Despawn trong Game

```csharp
// Gun.cs
using UnityEngine;

public class Gun : MonoBehaviour
{
    public Transform firePoint;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        // Spawn viên đạn loại Fire tại vị trí firePoint
        Bullet bullet = SimplePool<BulletEnum>.Spawn<Bullet>(
            BulletEnum.Fire, 
            firePoint.position, 
            firePoint.rotation
        );
    }
}

// Trong Bullet.cs (như đã ví dụ ở trên)
public class Bullet : GameUnit<BulletEnum>
{
    // ...
    private void DespawnSelf()
    {
        CancelInvoke();
        // Quan trọng: Gọi Despawn để trả về pool
        SimplePool<BulletEnum>.Despawn(this);
    }
    // ...
}
```

---
## Hướng Dẫn Sử Dụng Pool Setup Tool
### 1. Mở Công Cụ 
- Trong Unity Editor, hãy chọn Window -> Pool Setup Tool từ thanh menu.
- Một cửa sổ Editor sẽ hiện ra. Đây là giao diện chính của công cụ.
### 2. Thêm Mới Loại Pool
- Trong cửa sổ Pool Setup Tool, tìm và nhấn nút "Add New Pool Type".
- Một mục nhập mới sẽ xuất hiện ở danh sách bên dưới.
- Nhập tên cho loại pool của bạn vào ô "Pool Name" (ví dụ: Bullet, Enemy, VFX_Explosion). Tên này nên là một chuỗi đơn giản, không chứa ký tự đặc biệt.
Kéo prefab tương ứng với loại pool này từ Project View vào ô "Prefab".
- Nhấn "Save" để lưu lại loại pool vừa tạo.
### 3. Xóa Loại Pool
- Trong danh sách các loại pool, tìm đến loại bạn muốn xóa.
- Nhấn nút "Delete" (biểu tượng thùng rác) ở cuối dòng của loại pool đó.
- Một hộp thoại xác nhận sẽ hiện ra. Nhấn "Delete" để xác nhận xóa.
### 4. Thay Đổi Số Lượng Preload
- Trong danh sách, tìm đến loại pool bạn muốn thay đổi số lượng preload.
- Sửa giá trị trong ô "Preload Amount". Đây là số lượng object sẽ được tạo sẵn khi game bắt đầu.
_ Nhấn "Save" để áp dụng thay đổi.
  
## Best Practices

### 1. ✅ Luôn Reset State Khi Spawn

Khi một object được lấy từ pool (`OnEnable` hoặc ngay sau `Spawn`), hãy đảm bảo nó được reset về trạng thái ban đầu. Object có thể giữ lại các giá trị từ lần sử dụng trước.

```csharp
// Bullet.cs
void OnEnable()
{
    // Reset các giá trị quan trọng
    speed = 10f;
    timeToLive = 3f;
    hasHitTarget = false;

    // Bắt đầu coroutine để tự despawn
    StartCoroutine(LifetimeCoroutine());
}

IEnumerator LifetimeCoroutine()
{
    yield return new WaitForSeconds(timeToLive);
    DespawnSelf();
}

private void DespawnSelf()
{
    StopAllCoroutines(); // Dừng tất cả coroutine để tránh lỗi
    SimplePool<BulletEnum>.Despawn(this);
}
```

### 2. ✅ Tránh Despawn Một Object Nhiều Lần

Logic `Despawn` trong `Pool.cs` đã có kiểm tra `unit.gameObject.activeSelf`. Tuy nhiên, tốt nhất bạn cũng nên kiểm tra trong logic của mình, đặc biệt khi có nhiều sự kiện có thể trigger despawn (va chạm, hết thời gian sống, ra khỏi vùng chơi...).

```csharp
// Bullet.cs
private bool isDespawned = false;

void OnTriggerEnter(Collider other)
{
    if (isDespawned) return; // Không làm gì nếu đã despawn

    // Logic va chạm...
    DespawnSelf();
}

private void DespawnSelf()
{
    if (isDespawned) return; // Không làm gì nếu đã despawn

    isDespawned = true;
    StopAllCoroutines();
    SimplePool<BulletEnum>.Despawn(this);
}

void OnDisable()
{
    isDespawned = false; // Reset cờ khi object được đưa lại vào pool để dùng lần sau
}
```

### 3. ✅ Chọn `Pool Size` Phù Hợp

Sử dụng công thức để ước tính số lượng preload, tránh việc Instantiate thêm tại runtime gây lag.

```csharp
// Công thức tính Pool Size
Pool Size = (Spawn Rate × Lifetime) + Buffer

// Ví dụ: Đạn
// - Bắn 10 viên/giây
// - Mỗi viên sống 3 giây
// - Buffer: 20%
Pool Size = (10 × 3) + (30 × 0.2) = 36 viên

// → Nên set amount = 40 trong PoolControl để an toàn.
```

### 4. ✅ Sử Dụng `Collect` và `Release` Một Cách Thông Minh

*   **`Collect()`**: Dùng khi kết thúc một màn chơi, một đợt enemy... để thu tất cả object về pool thay vì despawn từng cái một.
    ```csharp
    // LevelManager.cs
    void OnLevelComplete()
    {
        // Thu tất cả enemy về pool
        SimplePool<EnemyEnum>.Collect(EnemyEnum.Soldier);
        SimplePool<EnemyEnum>.Collect(EnemyEnum.Tank);
    }
    ```
*   **`Release()`**: Dùng khi chuyển scene và bạn chắc chắn sẽ không dùng các object của pool đó nữa, để giải phóng hoàn toàn bộ nhớ.
    ```csharp
    // SceneManager.cs
    void LoadMainMenu()
    {
        // Giải phóng tất cả pool của game play
        SimplePool<BulletEnum>.ReleaseAll();
        SimplePool<EnemyEnum>.ReleaseAll();
        // ... Load main menu scene
    }
    ```

### 5. ✅ Tổ Chức Prefabs Gọn Gàng

Sử dụng một `Parent` transform trong `PoolControl` để chứa tất cả các object được preload. Điều này giúp giữ Hierarchy gọn gàng.

```
- PoolManager (GameObject)
  - PoolControl (Component)
- PooledItems (GameObject, rỗng, làm parent)
  - Bullet_Pool (GameObject, parent của các viên đạn được preload)
    - Bullet (inactive)
    - Bullet (inactive)
    - ...
  - Enemy_Pool (GameObject, parent của các enemy được preload)
    - Soldier (inactive)
    - Soldier (inactive)
    - ...
```

---

## Tổng Kết

Hệ thống Object Pooling hiện tại của project được xây dựng một cách rõ ràng và hiệu quả với 3 thành phần chính: `GameUnit<TEnum>`, `SimplePool<TEnum>`, và `PoolControl`.

### Key Takeaways

1.  **`GameUnit<TEnum>`**: Là nền tảng cho bất kỳ đối tượng nào bạn muốn pool. Nó kết hợp đối tượng với một `enum` định danh.
2.  **`SimplePool<TEnum>`**: Cung cấp giao diện tĩnh, dễ dàng để `Spawn`, `Despawn`, và quản lý vòng đời của các pool một cách toàn cục.
3.  **`PoolControl`:** Đơn giản hóa việc khởi tạo (preload) các loại object khác nhau ngay khi game bắt đầu thông qua Inspector.
4.  **Luôn reset state** của object khi nó được lấy ra từ pool (`OnEnable`) để tránh lỗi logic.
5.  **Quản lý vòng đời** của các pool một cách cẩn thận, sử dụng `Collect` để tái sử dụng và `Release` để giải phóng bộ nhớ khi cần.

Bằng cách tuân thủ cấu trúc này, bạn có thể tối ưu hóa hiệu năng game một cách đáng kể, giảm thiểu tác động của Garbage Collector và mang lại trải nghiệm mượt mà hơn cho người chơi.

**Chúc bạn lập trình hiệu quả! 🚀**
