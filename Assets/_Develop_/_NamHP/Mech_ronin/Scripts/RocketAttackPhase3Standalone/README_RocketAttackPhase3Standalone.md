# RocketAttackPhase3 STANDALONE - Hướng Dẫn Sử Dụng

## 🎯 GIỚI THIỆU

**HOÀN TOÀN ĐỘC LẬP - SỬ DỤNG DOTWEEN**

Hệ thống RocketAttackPhase3 standalone đã được tối ưu hóa để hoạt động trong bất kỳ dự án Unity nào sử dụng DOTween. Hệ thống này sử dụng DOTween cho các animation mượt mà và hiệu suất cao.

### ✅ Features Hoàn Chỉnh
- **3 hướng tấn công**: Trái, phải, trên
- **4 tên lửa mỗi đợt**: Random destination selection
- **Movement logic**: DOTween + rotation sau 40% thời gian
- **Damage system**: Explosion radius + health component
- **Visual effects**: Particle system + light flash
- **Screen shake**: Camera shake effect
- **Auto-setup**: Tự động tạo destinations và helpers
- **Demo mode**: Auto demo + manual controls

---

## 📁 Files Cần Copy (Chỉ 4 files)

### 1. **`RocketAttackPhase3Standalone.cs`** - Hệ thống chính
```csharp
// Core system với:
// - 3 hướng tấn công (trái, phải, trên)
// - 4 tên lửa mỗi đợt
// - Random destination selection
// - Auto-create destinations
// - Screen shake effects
// - Complete damage system
// - No external dependencies
```

### 2. **`RocketMoveStandalone.cs`** - Movement logic
```csharp
// Core movement logic với:
// - DOTween animations (OutCubic ease)
// - Target rotation sau 40% thời gian
// - Smooth rotation interpolation với DOTween
// - Destination attachment
// - Explosion damage system
// - Particle effects
// - Light flash effects
// - Built-in Health component
```

### 3. **`RocketAttackDemo.cs`** - Demo hoàn chỉnh
```csharp
// Complete demo với:
// - Auto-create player prefab
// - Auto-create camera
// - Auto-create rocket prefabs
// - Auto-create launch positions
// - Auto-create visual helpers
// - Auto demo mode
// - Manual controls (1,2,3,Space)
// - Context menu commands
// - Complete setup automation
```

### 4. **`README_RocketAttackPhase3Standalone.md`** - Hướng dẫn này
```markdown
// Complete documentation:
// - Setup instructions
// - Usage examples
// - Configuration guide
// - Demo controls
// - Migration checklist
```

---

## 🚀 Quick Setup - 5 Phút

### Bước 1: Copy 4 Files
Copy 4 files trên vào folder `Scripts/RocketSystem/` trong dự án mới của bạn.

### Bước 2: Tạo Empty GameObject
1. Tạo Empty GameObject tên "RocketAttackSystem"
2. Add component `RocketAttackPhase3Standalone`
3. Add component `RocketAttackDemo`

### Bước 3: Chạy Game
- **Auto demo**: Hệ thống sẽ tự động chạy demo
- **Manual controls**: Dùng phím 1,2,3,Space để test
- **Context menu**: Click chuột phải vào component để test

### Bước 4: Tùy Chỉnh (Optional)
```csharp
// Trong Inspector:
// - Movement Duration: 1.8f (thời gian bay)
// - Rotation Speed: 110f (tốc độ xoay)
// - Rocket Damage: 50 (sát thương)
// - Explosion Radius: 5f (bán kính nổ)
// - Screen Shake: true (run camera)
```

---

## 🎮 Demo Controls

### Keyboard Controls
- **1**: Attack Left (Tấn công trái)
- **2**: Attack Right (Tấn công phải)  
- **3**: Attack Above (Tấn công trên)
- **Space**: Trigger Real Rockets (Kích hoạt tên lửa thật)

### Context Menu (Click chuột phải)
```
Demo: Test Attack Left
Demo: Test Attack Right
Demo: Test Attack Above
Demo: Trigger Real Rockets
Demo: Toggle Auto Demo
Demo: Recreate Setup
```

### Auto Demo Features
- Tự động tạo player, camera, prefabs
- Tự động tạo launch positions và destinations
- Tự động tạo visual helpers
- Cycle qua 3 hướng tấn công
- Player di chuyển tròn để demo

---

## ⚙️ Configuration

### Essential Settings
```csharp
// Movement
[SerializeField] private float movementDuration = 1.8f;      // 1.4-2s
[SerializeField] private float rotationSpeed = 110f;         // Tốc độ xoay
[SerializeField] private float realRocketDelay = 0.1f;     // Delay spawn
[SerializeField] private float aboveDirectionDelay = 0.5f; // Delay hướng trên

// Damage & Effects
[SerializeField] private int rocketDamage = 50;             // Sát thương
[SerializeField] private float explosionRadius = 5f;      // Bán kính nổ
[SerializeField] private bool enableScreenShake = true;    // Run camera
[SerializeField] private float shakeIntensity = 0.5f;       // Mức độ rung
```

### Auto-Created Elements
```csharp
// System tự động tạo:
// - Player prefab với Health component
// - Camera nếu chưa có
// - Rocket fake prefab (cylinder màu vàng)
// - Real rocket prefab (sphere màu đỏ)
// - 3 launch positions (trái, phải, trên)
// - 20 destination points (6 trái, 6 phải, 8 trên)
// - Visual helpers (sphere màu)
```

---

## 🔧 Core Movement Logic

### Movement Flow
```csharp
// 1. Setup rocket
SetupRocketFake(destination, target, duration, rotationSpeed, 
    realRocketPrefab, damage, explosionRadius, playerLayer);

// 2. Start movement (DOTween)
StartMovement();

// 3. DOTween movement
movementTween = myTrans
    .DOMove(destination.position, movementDuration)
    .SetEase(Ease.OutCubic)
    .OnComplete(OnReachDestination);

// 4. Delayed rotation start
float rotationDelay = movementDuration * 0.4f;
rotationTween = DOVirtual.DelayedCall(rotationDelay, () =>
{
    if (target != null && this != null)
    {
        StartRotationTween();
    }
});

// 5. DOTween rotation
rotationTween = myTrans
    .DORotateQuaternion(targetRotation, 0.5f)
    .SetEase(Ease.OutQuad);
```

### Rotation Logic với DOTween
```csharp
private void StartRotationTween()
{
    Vector3 targetPos = SetPosY(target.position, myTrans.position.y);
    Vector3 direction = targetPos - myTrans.position;
    Quaternion targetRotation = Quaternion.LookRotation(direction);
    
    // Use DOTween for smooth rotation
    rotationTween = myTrans
        .DORotateQuaternion(targetRotation, 0.5f)
        .SetEase(Ease.OutQuad);
}
```

### Explosion System
```csharp
// Apply damage
Collider[] hitColliders = Physics.OverlapSphere(position, explosionRadius, playerLayer);
foreach (var hitCollider in hitColliders)
{
    Health health = hitCollider.GetComponent<Health>();
    if (health != null) health.TakeDamage(damage);
}

// Create effects
CreateExplosionEffect(); // Particle system + light flash
```

---

## 🎯 Usage Examples

### Basic Boss Integration
```csharp
public class MyBossController : MonoBehaviour
{
    [SerializeField] private RocketAttackPhase3Standalone rocketSystem;
    [SerializeField] private Transform playerTarget;
    
    void Start()
    {
        // Initialize system
        rocketSystem.Initialize(playerTarget, Camera.main);
        
        // Auto-create everything needed
        // System tự động tạo destinations, helpers, etc.
    }
    
    public void PerformRocketAttack()
    {
        // Random direction
        int direction = Random.Range(0, 3);
        rocketSystem.StartAttack(direction);
        
        // Trigger real rockets after movement
        Invoke(nameof(TriggerRealRockets), 2.5f);
    }
    
    private void TriggerRealRockets()
    {
        rocketSystem.TriggerRealRockets();
    }
}
```

### Player Controller Integration
```csharp
public class MyPlayer : MonoBehaviour
{
    private Health healthComponent;
    
    void Start()
    {
        // Health component tự động được add bởi demo system
        healthComponent = GetComponent<Health>();
        if (healthComponent == null)
        {
            healthComponent = gameObject.AddComponent<Health>();
        }
    }
    
    void Update()
    {
        // Check health
        if (healthComponent != null)
        {
            float healthPercent = healthComponent.GetHealthPercentage();
            Debug.Log($"Player health: {healthPercent * 100}%");
        }
    }
}
```

---

## 🐛 Troubleshooting

### Common Issues & Solutions

#### **Problem: Rockets not spawning**
```csharp
// Solution: Check if system is initialized
void Start()
{
    rocketSystem.Initialize(playerTarget, Camera.main);
}
```

#### **Problem: No damage applied**
```csharp
// Solution: Ensure player has Health component
player.AddComponent<Health>();
```

#### **Problem: Movement not working**
```csharp
// Solution: Check if destination exists
// System tự động tạo destinations qua CreateDestinations()
rocketSystem.CreateDestinations();
```

#### **Problem: No visual effects**
```csharp
// Solution: Enable effects in Inspector
enableScreenShake = true;
// System tự động tạo particle effects
```

### Debug Commands
```csharp
// Context menu commands (click chuột phải):
- Demo: Test Attack Left/Right/Above
- Demo: Trigger Real Rockets
- Demo: Toggle Auto Demo
- Demo: Recreate Setup
```

---

## 📦 Dependencies - Chỉ cần DOTween!

### Required
- ✅ `UnityEngine` - Core Unity engine
- ✅ `System` - C# standard library
- ✅ `System.Collections` - Collections support
- ✅ `DG.Tweening` - DOTween namespace

### Optional (Auto-created)
- ✅ `Health` component - Built into RocketMoveStandalone
- ✅ `Particle System` - Unity built-in
- ✅ `Light` - Unity built-in
- ✅ `Camera` - Unity built-in

### Requirements
- ✅ **DOTween package** - Cần install từ Unity Package Manager
- ~~LeanTween~~ → Thay thế bằng DOTween
- ~~ShakeAnyThings~~ → Built-in screen shake
- ~~ObjectPool~~ → Direct Instantiate/Destroy
- ~~BotNetwork~~ → Simple damage system
- ~~TUtilities~~ → Built into movement logic

---

## 🎨 Visual Features

### Auto-Created Visual Elements
```csharp
// Launch positions (colored spheres):
// - Left: Red sphere
// - Right: Blue sphere  
// - Above: Green sphere

// Destination points:
// - 20 points auto-created in circle patterns
// - Different heights for visual interest
// - Auto-parented to system

// Rocket prefabs:
// - Fake rocket: Yellow cylinder
// - Real rocket: Red sphere
// - Auto-created with materials
```

### Effects Included
```csharp
// Explosion effects:
// - Particle system (red particles)
// - Light flash (red point light)
// - Screen shake (Perlin noise based)
// - Physics force application
// - Damage radius calculation
```

---

## 🚀 Ready for New Projects

### Migration Checklist
- [ ] Copy 4 files vào dự án mới
- [ ] Install DOTween package từ Unity Package Manager
- [ ] Tạo Empty GameObject
- [ ] Add RocketAttackPhase3Standalone component
- [ ] Add RocketAttackDemo component
- [ ] Chạy game và test
- [ ] Tùy chỉnh settings nếu cần

### Setup Time: < 5 minutes
1. **Copy files**: 4 files
2. **Install DOTween**: Từ Unity Package Manager
3. **Create GameObject**: 1 empty object
4. **Add components**: 2 components
5. **Run**: Hệ thống tự động setup everything

### Project Requirements
- ✅ Unity 2019.4 or higher
- ✅ **DOTween package required** (install từ Package Manager)
- ✅ No additional assets needed
- ✅ Works in 2D and 3D projects
- ✅ Works with any rendering pipeline

---

## 📝 Summary

### **4 Files = Complete Rocket Attack System**

#### ✅ **What's Included:**
- **Complete attack system** - 3 directions, 4 rockets each
- **Core movement logic** - Custom LeanTween + rotation
- **Damage system** - Health component + explosion radius
- **Visual effects** - Particles + light + screen shake
- **Auto-setup** - Creates everything needed automatically
- **Demo mode** - Auto demo + manual controls
- **Zero dependencies** - No external packages required

#### ✅ **What You Get:**
- **Production-ready** rocket attack system
- **Complete independence** - works in any new project
- **Auto-configuration** - minimal setup required
- **Full documentation** - complete usage guide
- **Demo included** - ready to test immediately
- **Performance optimized** - efficient implementation

#### ✅ **Ready to Use:**
```bash
# Chỉ cần copy 4 files này:
1. RocketAttackPhase3Standalone.cs
2. RocketMoveStandalone.cs  
3. RocketAttackDemo.cs
4. README_RocketAttackPhase3Standalone.md

# Setup trong < 5 phút:
- Tạo GameObject
- Add components
- Chạy game
```

**Hệ thống này HOÀN TOÀN SẴN SÀNG cho bất kỳ dự án Unity mới nào!** 🎯
