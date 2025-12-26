# Game Pikachu Hoa Quả - Documentation

## **Tổng Quan Dự Án**

**Tên Game:** Pikachu Fruit Match
**Platform:** PC (Windows)
**Engine:** Unity 2022.3+
**Genre:** Puzzle, Match-3
**Target Audience:** Casual gamers, all ages

## **Game Concept**

Game match-3 theo phong cách Pikachu kinh điển, kết nối các loại hoa quả giống nhau qua đường thẳng không quá 3 đoạn. Game bao gồm nhiều level với độ khó tăng dần và các game mode khác nhau.

## **Core Features**

### **1. Gameplay Mechanics**
- **Grid System:** Grid 8x8 (có thể mở rộng lên 12x12)
- **Matching Rule:** Kết nối 2 fruit giống nhau qua đường thắn ≤ 3 đoạn
- **Path Finding:** Sử dụng algorithm BFS để tìm đường kết nối
- **Validation:** Kiểm tra obstacles và path validity

### **2. Game Modes**
- **Classic Mode:** Xóa tất cả fruits để hoàn thành level
- **Time Challenge:** Đạt score cao nhất trong thời gian giới hạn
- **Move Limit (Future):** Hoàn thành mục tiêu với số lần di chuyển giới hạn

### **3. Level System**
- **Level 1-5:** Grid 6x6, 4-6 loại fruits, Easy difficulty
- **Level 6-10:** Grid 8x8, 6-8 loại fruits, thêm obstacles
- **Level 11+:** Grid 10x10, 8+ loại fruits, Hard/Expert difficulty

### **4. Scoring System**
- **Base Score:** 100 điểm/match
- **Combo System:** Multiplier 1.5x - 3x
- **Time Bonus:** +50 điểm/giây còn lại
- **Level Bonus:** 500-2000 điểm tùy difficulty

## **Technical Architecture**

### **Core Components**
```
GameManager.cs        - Quản lý game state, score, timer
GridManager.cs        - Quản lý grid layout, spawn/destroy fruits
PathFinder.cs         - Algorithm tìm đường kết nối
InputManager.cs       - Xử lý mouse/touch input
LevelManager.cs       - Quản lý level progression
UIManager.cs          - Quản lý HUD, menus, screens
AudioManager.cs       - Quản lý sounds và music
EffectManager.cs      - Particle effects và animations
```

### **Data Architecture**
```
FruitData.cs          - ScriptableObject cho fruit information
LevelData.cs          - ScriptableObject cho level configuration
GameSettings.cs       - ScriptableObject cho global settings
```

### **Folder Structure**
```
Assets/
├── Scripts/
│   ├── Core/                 # Core game systems
│   │   ├── GameManager.cs
│   │   ├── GridManager.cs
│   │   └── PathFinder.cs
│   ├── Managers/             # System managers
│   │   ├── UIManager.cs
│   │   ├── AudioManager.cs
│   │   └── EffectManager.cs
│   ├── Game/                 # Game-specific logic
│   │   ├── Fruit.cs
│   │   ├── LevelManager.cs
│   │   └── InputManager.cs
│   └── Data/                 # ScriptableObjects
│       ├── FruitData.cs
│       ├── LevelData.cs
│       └── GameSettings.cs
├── Prefabs/
│   ├── Fruits/              # Fruit prefabs
│   ├── UI/                  # UI prefabs
│   └── Effects/             # Effect prefabs
├── Sprites/
│   └── Fruits/              # Fruit sprites
├── Audio/
│   ├── Music/               # Background music
│   └── SFX/                 # Sound effects
├── Scenes/
│   ├── MainMenu.unity
│   ├── Gameplay.unity
│   └── LevelSelect.unity
└── Resources/               # ScriptableObject instances
```

## **Development Plan**

### **Phase 1: Foundation (Days 1-2)**
**Mục tiêu:** Xây dựng core systems và basic setup

**Tasks:**
- [x] Create project folder structure
- [x] Design ScriptableObject architecture
- [ ] Create basic scenes (MainMenu, Gameplay)
- [ ] Implement GameManager with basic state management
- [ ] Create GridManager with 2D array system
- [ ] Setup basic UI canvas and camera

**Deliverables:**
- Basic project structure
- Core manager classes
- Empty scenes with basic UI

### **Phase 2: Core Mechanics (Days 3-4)**
**Mục tiêu:** Implement game mechanics cơ bản

**Tasks:**
- [ ] Create Fruit prefab and Fruit.cs component
- [ ] Implement InputManager for mouse handling
- [ ] Develop PathFinder algorithm (BFS)
- [ ] Create matching validation system
- [ ] Add visual feedback for selected fruits
- [ ] Implement fruit spawn and destroy logic

**Deliverables:**
- Working fruit matching system
- Visual feedback systems
- Basic animations

### **Phase 3: Game Logic (Days 5-6)**
**Mục tiêu:** Hoàn thiện game logic và scoring

**Tasks:**
- [ ] Implement scoring system with combos
- [ ] Add timer functionality
- [ ] Create LevelManager with level progression
- [ ] Design level data system
- [ ] Add win/lose conditions
- [ ] Implement basic AI hints system

**Deliverables:**
- Complete game loop
- Scoring and progression systems
- Basic level designs

### **Phase 4: UI/UX Polish (Days 7-8)**
**Mục tiêu:** Hoàn thiện UI và user experience

**Tasks:**
- [ ] Design main menu interface
- [ ] Create gameplay HUD
- [ ] Implement level selection screen
- [ ] Add game over and victory screens
- [ ] Create settings menu
- [ ] Add smooth transitions and animations

**Deliverables:**
- Complete UI system
- Smooth user experience
- Settings and options

### **Phase 5: Content & Polish (Days 9-11)**
**Mục tiêu:** Thêm content, effects và optimization

**Tasks:**
- [ ] Create multiple fruit sprites
- [ ] Add sound effects and background music
- [ ] Implement particle effects
- [ ] Add screen shake and visual feedback
- [ ] Create 20+ levels with increasing difficulty
- [ ] Performance optimization
- [ ] Bug testing and fixes

**Deliverables:**
- Complete game with content
- Audio and visual effects
- Performance optimization

## **Game Assets Required**

### **Visual Assets**
- **Fruit Sprites:** Apple, Orange, Banana, Grape, Strawberry, Watermelon, Cherry, Lemon, Mango, Pineapple
- **UI Elements:** Buttons, panels, icons, backgrounds
- **Effects:** Particles, animations, screen transitions
- **Backgrounds:** Main menu, gameplay, level select

### **Audio Assets**
- **Background Music:** 2-3 tracks (menu, gameplay, victory)
- **Sound Effects:** Match sound, select sound, invalid move, victory, game over
- **UI Sounds:** Button clicks, navigation sounds

### **Technical Requirements**
- **Unity Version:** 2022.3 LTS
- **Target Platform:** Windows PC
- **Resolution:** 1920x1080 (scalable)
- **Input:** Mouse only

## **Quality Assurance**

### **Testing Checklist**
- [ ] Core gameplay mechanics work correctly
- [ ] Path finding algorithm accuracy
- [ ] Level progression functions properly
- [ ] UI/UX is intuitive and responsive
- [ ] Audio integration works seamlessly
- [ ] Performance maintains 60 FPS
- [ ] No memory leaks or crashes
- [ ] Save/load functionality works

### **Performance Targets**
- **Frame Rate:** 60 FPS minimum
- **Load Time:** < 3 seconds for initial load
- **Memory Usage:** < 500MB RAM
- **File Size:** < 200MB final build

## **Future Enhancements**

### **Post-Release Features**
- **Multiplayer Mode:** Local competitive gameplay
- **Power-ups:** Shuffle, hint, time bonus, bomb
- **Achievement System:** Steam integration
- **Leaderboards:** Global and friends leaderboards
- **Custom Levels:** Level editor functionality
- **Additional Themes:** Seasonal fruit packs, different themes

### **Technical Improvements**
- **Mobile Port:** iOS/Android compatibility
- **Cloud Save:** Cross-platform save synchronization
- **Analytics:** Player behavior tracking
- **Localization:** Multiple language support

## **Risk Assessment**

### **Technical Risks**
- **Path Finding Performance:** BFS algorithm on large grids
  - **Mitigation:** Optimize with early exit conditions, use A* for larger grids
- **Memory Management:** Sprite loading and pooling
  - **Mitigation:** Implement object pooling, proper resource management

### **Design Risks**
- **Level Difficulty:** Balancing progression curve
  - **Mitigation:** Play testing, analytics-driven adjustments
- **Player Engagement:** Maintaining interest over time
  - **Mitigation:** Regular content updates, achievement systems

## **Success Metrics**

### **Launch Targets**
- **Downloads:** 10,000+ in first month
- **Retention:** 40% Day 1, 15% Day 7
- **Rating:** 4.0+ stars on distribution platform
- **Session Length:** 15-30 minutes average

### **Long-term Goals**
- **Community:** Active player base and feedback
- **Content Updates:** Monthly new levels and features
- **Platform Expansion:** Mobile port within 6 months
- **Monetization:** Consider DLC or cosmetic items

---

**Document Version:** 1.0
**Last Updated:** October 2025
**Next Review:** End of Phase 1 development