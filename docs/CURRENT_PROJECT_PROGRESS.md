# APOCARROT — BÁO CÁO TIẾN ĐỘ DỰ ÁN & KẾ HOẠCH NÂNG CẤP ĐẾN v1.0
> **TÀI LIỆU QUAN TRỌNG:** Mọi phiên làm việc AI mới (New Session) **BẮT BUỘC ĐỌC FILE NÀY ĐẦU TIÊN** trước khi thực hiện bất kỳ thay đổi nào trong mã nguồn hoặc cấu trúc dự án!
> Cập nhật lần cuối: 12/10/2026.

---

## 1. TỔNG QUAN HIỆN TRẠNG (EXECUTIVE SUMMARY)
- **Game:** APOCARROT: Carrot Survival (Unity 3D Action Roguelite).
- **Unity Version:** Unity 6 (`6000.6.5f1` Apple Silicon Mac).
- **Remote Git:** `https://github.com/TienMinh8/battle_zoombie_carrot.git` (nhánh `main`).
- **Trạng thái cốt lõi:** **Phiên bản Demo v0.1 đã hoàn thành trọn vẹn 100%** theo tiêu chí GDD ([docs/APOCARROT_GDD_Demo_Plan_v0_1.md](file:///Users/appdexter/Documents/lemonqueen/Apocarrot/docs/APOCARROT_GDD_Demo_Plan_v0_1.md)) và đã hoàn thành trước nhiều hạng mục quan trọng của các phiên bản tiếp theo (**v0.2**, **v0.3**, **v0.4**, **v0.5**).

---

## 2. NHỮNG HẠNG MỤC ĐÃ HOÀN THÀNH VƯỢT TIẾN ĐỘ (AHEAD OF SCHEDULE)

Trước khi tiếp nhận chỉ thị phát triển mở rộng `APOCARROT_MORE.md`, đội ngũ phát triển đã chủ động hoàn thiện nhiều tính năng quan trọng:

| Hạng mục | Thuộc phiên bản | Chi tiết đã thực hiện | Tệp liên quan |
|---|:---:|---|---|
| **Khử giật lag (Zero-Alloc UI)** | **v0.2** | Loại bỏ hoàn toàn việc hủy/tạo lại GameObject trong Balo khi nhặt đồ; xây dựng **Object Pool** cho Toast thông báo nhặt đồ. Không còn hiện tượng drop FPS. | `LootToastManager.cs`, `EquipmentBagUI.cs` |
| **Cân bằng & Phân biệt Vũ khí** | **v0.3** | Cận chiến (Kiếm & Rìu) khác biệt rõ rệt về tầm quét và tốc độ; Tầm xa (Súng Cà Chua & Nỏ Cà Rốt) tự ngắm chuẩn; Boss King Carrot có đĩa cảnh báo AoE đỏ trước khi dậm đất; cơ chế Pity Drop bảo hiểm rớt đồ sớm. | `MeleeWeaponData.cs`, `RangedWeaponController.cs`, `KingCarrotBossAI.cs`, `WaveData.cs` |
| **Kéo - Thả (Drag & Drop) & Bố cục Balo** | **v0.4** | Người chơi có thể **kéo đồ lên 6 ô trang bị để mặc/thay thế**, **kéo 2 món đồ đè nhau để Ghép (Fuse) nâng cấp**, **kéo ra ngoài để tháo đồ**. Đã sửa triệt để lỗi danh sách đồ che khuất ô trang bị. | `UIDragDropItem.cs`, `EquipmentBagUI.cs` |
| **Tài nguyên Vũ khí 2D chân thực** | **v0.5** | Đã tích hợp **534 ảnh PNG vũ khí & trang bị** từ thư mục `weapons/` vào `Resources/Weapons/`. Hiển thị chân thực từ Lv.1 đến Lv.3 trên thanh HUD góc dưới, 6 ô trang bị, lưới Balo và Cửa hàng. | `WeaponSpriteService.cs`, `RunInventoryHUD.cs` |

---

## 3. ĐỐI CHIẾU TIẾN ĐỘ THEO CÁC PHIÊN BẢN (v0.1 ĐẾN v1.0)

| Phiên bản | Mục tiêu chính | Tiến độ | Việc đã hoàn thành | Việc còn lại cần làm |
|:---:|---|:---:|---|---|
| **v0.1** | Vòng lặp Demo cơ bản (Start → 3 Waves → Shop/Fuse → Boss → Thắng/Thua → Save) | **100%** | Toàn bộ 14 tiêu chí GDD Section 14 đã đạt chuẩn. | Không còn blocker nào. |
| **v0.2** | Tính ổn định & Sửa lỗi (Zero Lag, dọn dẹp Coroutine/Event) | **95%** | Khử lag UI, tái sử dụng Card, Object Pool Toast, hủy đăng ký sự kiện an toàn, wave quota kết thúc sạch. | Bổ sung logging chẩn đoán nếu phát hiện ngoại lệ mới. |
| **v0.3** | Cảm giác Chiến đấu & Cân bằng (Combat Feel & Polish) | **85%** | Tốc độ đánh, tầm ngắm, đĩa báo đòn của boss, 3 loại quái rõ ràng, dữ liệu cấu hình tập trung. | Bổ sung hiệu ứng rung màn hình nhẹ (Screen Shake) hoặc tia chớp va chạm khi đánh trúng (Hit Flash/Sparks). |
| **v0.4** | Balo, Ghép đồ & Cửa hàng (Inventory, Fuse, Shop) | **95%** | Kéo thả trang bị & ghép đồ mượt mà, kiểm tra điều kiện ghép, trừ tiền mua đồ trong run chính xác, bố cục màn hình không bị đè. | Kiểm tra mép an toàn trên màn hình dọc siêu dài (nếu có). |
| **v0.5** | Visual 3D & Âm thanh (Audio & 3D Vertical Slice) | **45%** | Đã có 534 sprite 2D vũ khí/trang bị thực tế. Model Thỏ, quái và boss có màu sắc và hình khối phân biệt rõ. | **CHƯA CÓ ÂM THANH (SFX & BGM)**: Cần bổ sung âm thanh chém, trúng đòn, nhặt đồ, ghép đồ và nhạc nền đấu trường. Đổi model 3D thỏ/quái khi có asset 3D rig. |
| **v0.6** | Tiến trình & Phần thưởng (Progression & Rewards) | **80%** | Lưu cấp độ, XP, vàng, gem, năng lượng, danh sách trang bị tài khoản, thành tựu chiến tích. | Giao diện nâng cấp chỉ số vĩnh viễn (Talent Tree / Stats Upgrade) ngoài sảnh (nếu yêu cầu). |
| **v0.7** | Mở rộng Nội dung & Chơi lại (Replayability) | **60%** | 2 cận chiến + 2 tầm xa + 6 loại trang bị trang phục, 3 loại quái + 1 Boss. | Thêm 1-2 biến thể quái mới hoặc hiệu ứng trang bị độc đáo. |
| **v0.8** | Trải nghiệm người dùng & Hướng dẫn (UX & Onboarding) | **75%** | Bảng phím điều khiển, dải hướng dẫn kéo thả trong Balo, banner thông báo nhặt đồ. | Bổ sung bảng Tạm dừng (Pause Menu) & Thanh điều chỉnh âm lượng (Settings). |
| **v0.9** | Kiểm thử & Bản Build Candidate | **80%** | Batchmode biên dịch 0 lỗi, Play Mode chạy mượt từ đầu đến cuối. | Xuất build standalone (.app hoặc .apk) để test môi trường thực tế. |
| **v1.0** | Cột mốc hoàn chỉnh có thể chơi xuất sắc (Playable Milestone) | **75%** | Khung game hoàn chỉnh, trải nghiệm chơi tốt. | Tích hợp Âm thanh (SFX) + Menu Tạm dừng là đủ chuẩn v1.0 Playable! |

---

## 4. KẾ HOẠCH HÀNH ĐỘNG ĐỂ VỀ ĐÍCH NHANH NHẤT (FAST-TRACK PLAN)

Để đưa dự án tiến thẳng từ v0.1 lên **v1.0 Playable Milestone** một cách nhanh và vững chắc nhất mà không làm phình phạm vi (Scope Creep):

### GIAI ĐOẠN 1: Bổ sung Hệ thống Âm thanh Cơ bản (Audio Manager & SFX) — *Ưu tiên số 1*
- Tạo `AudioManager.cs` dạng Singleton với AudioSource riêng cho SFX và BGM.
- Tích hợp âm thanh phản hồi:
  1. Âm chém gió / vung vũ khí cận chiến.
  2. Âm thanh đạn nổ / bắn tầm xa.
  3. Âm thanh trúng đòn quái (Hit) & Quái bị tiêu diệt.
  4. Âm thanh nhặt Vàng & nhặt Trang bị.
  5. Âm thanh Ghép đồ thành công (Merge Jingle).
  6. Âm thanh Thất bại & Chiến thắng.
  7. Nhạc nền Arena chiến đấu & Boss combat.
*(Có thể dùng âm thanh synthesized thủ công qua procedural AudioSynthesis hoặc nạp file .wav/.mp3 từ Resources/Audio).*

### GIAI ĐOẠN 2: Menu Tạm Dừng (Pause & Settings)
- Bổ sung nút **Tạm Dừng (Pause ⏸)** trên góc HUD.
- Bảng Pause gồm:
  - Tiếp tục (Resume).
  - Thanh chỉnh âm lượng (BGM / SFX Volume Slider).
  - Thoát ra màn hình bắt đầu / Bỏ cuộc (Surrender).

### GIAI ĐOẠN 3: Hiệu ứng Thị giác Chiến đấu (Combat Juice)
- Hiệu ứng giật rung camera nhẹ khi trúng đòn mạnh (Camera Shake).
- Số sát thương nhảy lên đầu quái khi bị đánh trúng (Damage Floating Numbers) với Object Pool 0-alloc.

### GIAI ĐOẠN 4: Đóng gói & Nghiệm thu v1.0
- Kiểm tra toàn bộ luồng Play Mode: Khởi động → Đánh quái → Kéo thả ghép đồ → Diệt Boss → Thắng → Nhận thưởng & Lưu dữ liệu.
- Xuất build và hoàn tất danh mục kiểm thử QA.

---

## 5. HƯỚNG DẪN CHO CÁC PHIÊN LÀM VIỆC TIẾP THEO (AGENT DIRECTIVE)
1. **Tuyệt đối không viết lại toàn bộ mã nguồn:** Dự án đang chạy rất ổn định và sạch sẽ. Chỉ bổ sung theo dạng module độc lập (Additive).
2. **Tuân thủ quy tắc 0-Alloc:** Giữ vững hiệu năng không lag giật; mọi hiệu ứng bay hoặc âm thanh đều phải tái sử dụng Object Pool hoặc AudioSource dùng chung.
3. **Cập nhật tiến độ vào file này:** Mỗi khi hoàn thành một tính năng mới, hãy cập nhật trạng thái trong tài liệu này và commit lên Git.
