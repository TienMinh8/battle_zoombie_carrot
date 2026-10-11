# APOCARROT — Project Audit Report

- **Thời gian thực hiện:** 10/10/2026
- **Workspace:** `/Users/appdexter/Documents/lemonqueen/Apocarrot`
- **Unity Project Directory:** `/Users/appdexter/Documents/lemonqueen/Apocarrot/My project`
- **Người thực hiện:** Antigravity AI Unity Lead Programmer

---

## 1. Môi trường & Phiên bản Unity

- **Hệ điều hành:** macOS (arm64)
- **Unity Editor Version:** `6000.6.5f1` (Unity 6 / 6000 series LTS)
- **Unity Editor Status:** Đang mở trực tiếp và hoạt động (PID 50274) trỏ vào thư mục `My project`
- **Render Pipeline:** Universal Render Pipeline (URP 17.6.0) với profile URP Mobile & PC đã thiết lập sẵn trong `Assets/Settings/`
- **Input System:** Unity Input System package 1.20.0 (`com.unity.inputsystem`) với asset `InputSystem_Actions.inputactions` đã có trong `Assets/`
- **UI Framework:** Unity UGUI 2.6.0 (`com.unity.ugui`)
- **AI Navigation:** `com.unity.ai.navigation` 2.0.15
- **Git status:** Chưa khởi tạo Git trong thư mục gốc.

---

## 2. Hiện trạng Assets & Scenes

- **Scenes:**
  - `Assets/Scenes/SampleScene.unity` (Scene 3D mặc định với Directional Light, Main Camera, Global Volume)
- **Cấu trúc Assets hiện tại:**
  - `Assets/Scenes/`
  - `Assets/Settings/` (URP Pipeline Assets, Renderers, Volume Profiles)
  - `Assets/TutorialInfo/`
  - `Assets/InputSystem_Actions.inputactions`
- **Scripts hiện có:** Chưa có gameplay script nào của người dùng. Không có nguy cơ ghi đè code cũ.
- **Tài liệu dự án:** Đã đồng bộ đầy đủ các tài liệu chuẩn vào thư mục gốc `Apocarrot`:
  - `AGENTS.md` (Workspace Rules)
  - `APOCARROT_ANTIGRAVITY_MASTER_PROMPT.md`
  - `docs/APOCARROT_GDD_Demo_Plan_v0_1.md` (Game Design Document)
  - `docs/DECISION_LOG.md`
  - `docs/IMPLEMENTATION_ROADMAP.md`
  - `docs/DEMO_ACCEPTANCE_CHECKLIST.md`
  - `docs/BACKLOG.md`
  - `art_reference/` (Ảnh concept mẫu Thỏ, King Carrot, góc nhìn camera)

---

## 3. Các quy tắc cốt lõi đã chốt (Locked)

1. **Thể loại:** 3D Action Roguelite / Arena Survival theo wave.
2. **Camera:** 3D top-down góc nghiêng 3/4 đi theo nhân vật Thỏ (không dùng camera sau lưng hay 2D side-scroller).
3. **Điều khiển:** Joystick di chuyển + 1 nút đánh cận chiến. **Không có nút bắn xa** (vũ khí tầm xa tự ngắm và tự bắn theo cooldown).
4. **Vòng lặp Run:** 3 wave thường + 1 wave Boss King Carrot (mục tiêu 3–5 phút). Nghỉ giữa wave để ghép vũ khí và mua đồ.
5. **Ghép đồ:** 2 vũ khí cùng loại + cùng cấp → 1 vũ khí cấp kế tiếp.
6. **Loot:** Quái thường có tỉ lệ rơi vũ khí, vàng và máu.
7. **Xử lý thất bại:** Thua chỉ mất đồ nhặt trong run, giữ nguyên cấp tài khoản và trang bị cố định.
8. **Đồ họa giai đoạn này:** Dùng Greybox / Primitive 3D có màu sắc phân biệt rõ ràng để kiểm chứng toàn bộ gameplay logic trước khi lắp 3D model/animation.

---

## 4. Kế hoạch triển khai theo Gate

- **Gate 0:** ✅ Môi trường & Project Audit (Hoàn tất)
- **Gate 1:** ✅ Playable Greybox Arena + Angled 3D Top-Down Camera + Player Movement (WASD/Phím mũi tên + Joystick ảo)
- **Gate 2:** ✅ Máu, Combat Cận Chiến (Carrot Blade, Carrot Axe) & Enemy cơ bản (Zombie Carrot, Hit flash, Billboard HP)
- **Gate 3:** ✅ Vũ khí tầm xa tự động (Tomato Blaster, Grape Splitter) tự khóa mục tiêu và bắn theo Cooldown
- **Gate 4:** ✅ Các biến thể quái (Cơ bản, Tốc độ, Tinh nhuệ), 3D Loot Pickups (Vàng, Máu, Rương Vũ Khí, Nam châm hút), Pity Guarantee Drop, và Run Inventory (Vàng + 6 ô đồ)
- **Gate 5 (Kế tiếp):** Hệ thống Ghép vũ khí (Weapon Merge Lv.1 + Lv.1 = Lv.2) & Cửa hàng nghỉ giữa wave (Intermission Shop)
- **Gate 6:** Wave Director (3 wave thường) & Boss King Carrot (Đòn telegraph AoE, chuyển pha)
- **Gate 7:** Tiến trình tài khoản, kết quả thắng/thua & Lưu file cục bộ (Save/Load)
- **Gate 8:** Tích hợp toàn diện & Kiểm thử nghiệm thu Demo P0
