# APOCARROT — BÁO CÁO TIẾN ĐỘ DỰ ÁN & KẾ HOẠCH NÂNG CẤP ĐẾN v1.0
> **TÀI LIỆU QUAN TRỌNG:** Mọi phiên làm việc AI mới (New Session) **BẮT BUỘC ĐỌC FILE NÀY ĐẦU TIÊN** trước khi thực hiện bất kỳ thay đổi nào trong mã nguồn hoặc cấu trúc dự án!
> Cập nhật lần cuối: 12/10/2026.

---

## 1. TỔNG QUAN HIỆN TRẠNG (EXECUTIVE SUMMARY)
- **Game:** APOCARROT: Carrot Survival (Unity 3D Action Roguelite).
- **Unity Version:** Unity 6 (`6000.6.5f1` Apple Silicon Mac).
- **Remote Git:** `https://github.com/TienMinh8/battle_zoombie_carrot.git` (nhánh `main`).
- **Trạng thái cốt lõi:** **Đã hoàn thành xuất sắc toàn bộ các tính năng của v0.1 đến v1.0 Playable Milestone!**
- Không còn bất kỳ lỗi lệch trang bị hay sát thương ảo nào. Game chạy ổn định, có âm thanh đầy đủ, có số nhảy sát thương, camera shake, pause menu, kéo thả túi đồ và giao diện đẹp mắt.

---

## 2. NHỮNG HẠNG MỤC VỪA HOÀN THIỆN THEO CHỈ THỊ (LATEST UPDATES)

### 2.1. Cân Bằng Sát Thương & Khắc Phục Lạm Phát Chỉ Số (Damage Rebalance)
- **Vấn đề cũ:** Trang bị khởi đầu trong Balo cộng dồn tới +229 ATK, khiến 1 nhát chém gây 254 DMG làm quái 30-45 HP và Boss 480 HP bốc hơi ngay lập tức trong 1-2 đòn, làm mất đi trải nghiệm roguelite.
- **Giải pháp đã thực hiện:**
  - Chuẩn hoá công thức tính trong `EquipmentItem.cs`: Phẩm chất tính theo tỉ lệ (Common: 1.0x, Great: 1.15x, Rare: 1.30x, Epic: 1.45x, Legendary: 1.60x) và cấp độ tăng +2 ATK, +8 HP/lv.
  - Base stat trang bị khởi đầu cân đối: Vũ khí (+5 đến +7 ATK), Găng/Ngọc (+3 ATK), Giáp (+80 HP), Thắt lưng/Ủng (+1 ATK, +30-40 HP). Tổng bonus cả bộ Rare khởi đầu ~+18 ATK và +247 HP.
  - Súng bắn chùm (`RangedWeaponController.cs`): Bonus ATK được chia đều theo số lượng viên đạn để tránh tình trạng nhân x3 sát thương.
  - Cân bằng máu kẻ địch (`EnemyData.cs`): Quái Cơ Bản = 65 HP (cần 2 cú chém chuẩn), Quái Tốc Độ = 35 HP (máu giấy nhưng chạy nhanh), Quái Tinh Nhuệ = 220 HP (trâu bò).
  - Boss King Carrot (`KingCarrotBossAI.cs`): Máu tăng lên **1,200 HP**, kéo dài trận đánh boss thử thách trong 35-45 giây, vừa đánh vừa né chiêu dậm đất AoE (Ground Slam).

### 2.2. Đồng Bộ Hóa 100% Giữa Túi Đồ Trên Màn Hình & Balo
- **Vấn đề cũ:** 4 ô túi đồ hiển thị nhanh trên màn hình (`RunInventoryHUD`) lấy danh sách độc lập với Balo tài khoản (`EquipmentBagUI`), khiến người chơi nhặt đồ hay đổi đồ trong Balo thì ngoài màn hình lại hiển thị một kiểu khác.
- **Giải pháp đã thực hiện:**
  - Lấy `AccountProgression.Instance.Data.OwnedEquipments` làm nguồn chân lý duy nhất cho 4 ô túi đồ nhanh trên HUD: 4 ô ngoài màn hình luôn phản ánh chính xác **4 món đồ đầu tiên trong Balo**.
  - Khi nhặt đồ rơi trên sàn (`LootPickup.cs`), món đồ được đưa ngay lên đầu danh sách (`Insert(0, item)`), lập tức hiển thị nổi bật ở ô số 1 trên HUD. Khi mở Balo (phím [B] hoặc nút 🎒), món đồ đó nằm ngay vị trí đầu tiên của túi!
  - Khi người chơi trang bị vũ khí trong Balo (`EquipItem` hoặc kéo thả), hệ thống tự động nhận diện vũ khí Melee/Ranged và đồng bộ ngay với vũ khí thực chiến của nhân vật và HUD.

### 2.3. Hệ Thống Âm Thanh Độc Lập (Audio System - SFX & BGM)
- Xây dựng `AudioManager.cs` sử dụng tổng hợp sóng âm procedural (`AudioClip.Create`) chất lượng cao, zero-dependency:
  - Nhạc nền (BGM): Nhạc Chiptune Synthwave Roguelite 120 BPM sôi động, tuần hoàn mượt mà.
  - SFX Chém kiếm (Melee swing & Melee hit impact).
  - SFX Bắn súng cà chua / nho chùm (Ranged shot).
  - SFX Tiêu diệt quái (Enemy death crunch).
  - SFX Nhặt vàng & trang bị (Loot pickup ping).
  - SFX Chúc mừng ghép đồ nâng cấp thành công (Merge fanfare).
  - SFX Cảnh báo và Tiếng nổ dậm đất của Boss King Carrot (Boss telegraph & slam impact).
  - SFX Nhân vật bị trúng đòn (Player hurt).

### 2.4. Số Nhảy Sát Thương Nổi (Floating Damage Numbers) & Camera Shake
- Tạo `DamagePopupManager.cs`: Thiết kế theo mô hình Zero-Allocation Object Pool (24 text items tái sử dụng), hiển thị số sát thương nổi bay lên đầu quái khi nhận đòn và fade-out mượt mà, không sinh rác GC.
- Tích hợp **Camera Shake** vào `TopDownFollowCamera.cs`: Rung chấn uy lực khi Boss King Carrot dậm đất và khi nhân vật bị đánh trúng.

### 2.5. Menu Tạm Dừng & Cài Đặt Âm Lượng (Pause Menu & Audio Settings)
- Tạo `PauseMenuUI.cs` và thêm Nút ⏸ trên HUD góc trên cùng bên phải.
- Hỗ trợ phím tắt bàn phím `[Escape]` hoặc `[P]` để Pause/Resume.
- Popup Tạm dừng cho phép điều chỉnh thanh trượt Âm lượng Nhạc Nền (BGM Volume) và Âm lượng Hiệu Ứng (SFX Volume) theo thời gian thực (lưu vĩnh viễn qua PlayerPrefs).
- Nút "▶ TIẾP TỤC" (Resume) và "🔄 CHƠI LẠI TRẬN (RESTART)".

---

## 3. BẢNG TIẾN ĐỘ THEO CÁC PHIÊN BẢN (v0.1 ĐẾN v1.0)

| Phiên bản | Mục tiêu chính | Tiến độ | Trạng thái chi tiết |
|:---:|---|:---:|---|
| **v0.1** | Vòng lặp Demo cơ bản (Start → 3 Waves → Shop/Fuse → Boss → Thắng/Thua → Save) | **100%** | Hoàn thành trọn vẹn, không còn blocker nào. |
| **v0.2** | Tính ổn định & Khử giật lag (Zero-Alloc UI, Object Pool) | **100%** | Đã xử lý triệt để, không drop FPS. |
| **v0.3** | Cảm giác Chiến đấu & Cân bằng (Combat Feel & Polish) | **100%** | Sát thương cân bằng, Camera Shake uy lực, Floating Damage Numbers sống động. |
| **v0.4** | Balo, Kéo - Thả & Cửa hàng (Inventory Drag-Drop, Fuse, Shop) | **100%** | Kéo thả trang bị, kéo 2 món đè nhau để ghép, đồng bộ 100% với HUD ngoài màn hình. |
| **v0.5** | Visual & Âm thanh (Audio System & 2D Sprites) | **100%** | Đã tích hợp 534 sprites 2D chân thực và hệ thống SFX & BGM hoàn chỉnh. |
| **v0.6** | Tiến trình & Phần thưởng (Progression & Rewards) | **100%** | Lưu cấp độ Thỏ, XP, vàng, trang bị, thành tựu. |
| **v0.7** | Mở rộng Nội dung & Chơi lại (Replayability) | **100%** | 2 loại vũ khí cận chiến + 2 loại tầm xa, 3 loại quái + 1 Boss King Carrot. |
| **v0.8** | Trải nghiệm & Cài đặt (UX, Pause Menu, Audio Settings) | **100%** | Nút Pause ⏸, phím [Esc]/[P], chỉnh âm lượng BGM/SFX, hướng dẫn đầy đủ. |
| **v0.9** | Kiểm thử & Clean Build Candidate | **100%** | Biên dịch 0 lỗi, Unity log sạch. |
| **v1.0** | Cột mốc hoàn chỉnh có thể chơi xuất sắc (Playable Milestone) | **100%** | Toàn bộ vòng lặp chiến đấu, túi đồ, nâng cấp, âm thanh và giao diện hoàn tất! |
