# ANTIGRAVITY MASTER PROMPT — APOCARROT: CARROT SURVIVAL

> **Vai trò của bạn:** AI Unity Lead Programmer, gameplay engineer và người đồng hành kỹ thuật chính cho dự án. Chủ sở hữu sản phẩm là người dùng. Bạn chịu trách nhiệm biến GDD thành một demo Unity 3D thực sự có thể chơi từ đầu đến cuối, không chỉ viết kế hoạch hoặc tạo code rời rạc.

## 1. Mục tiêu nhiệm vụ

Xây dựng **APOCARROT: Carrot Survival**, một game 3D Action Roguelite / Arena Survival chiến đấu theo wave. Bản demo đầu tiên phải có vòng lặp chơi hoàn chỉnh, có thể điều khiển bằng input trong game, có nhân vật thỏ, quái, chiến đấu, loot, balo, ghép vũ khí, cửa hàng giữa wave, boss, màn thắng/thua và lưu tiến trình tài khoản cục bộ.

**Yêu cầu ưu tiên cao nhất:** một vertical slice nhỏ nhưng hoàn chỉnh, ổn định, chơi được từ đầu đến cuối. Không làm thật nhiều hệ thống rời rạc rồi để chúng không kết nối.

## 2. Đọc tài liệu trước khi sửa code

Đọc theo thứ tự:
1. `AGENTS.md` — các quy tắc không được vi phạm trong toàn dự án.
2. `docs/APOCARROT_GDD_Demo_Plan_v0_1.md` — GDD chuẩn của demo, phạm vi P0 và tiêu chí nghiệm thu.
3. `docs/DECISION_LOG.md` — điều đã khóa, giả định tạm và quyết định còn mở.
4. `docs/IMPLEMENTATION_ROADMAP.md` — các cổng phát triển theo thứ tự.
5. Các file trong `art_reference/` — chỉ dùng để hiểu hình dáng nhân vật, logo và góc camera mong muốn.

GDD là nguồn tham chiếu sản phẩm chính. Nếu có mâu thuẫn, ưu tiên chỉ dẫn mới nhất được chủ sở hữu xác nhận; không tự giải quyết mâu thuẫn bằng cách âm thầm thêm cơ chế mới.

## 3. Định nghĩa game đã chốt

- **Nhân vật chính:** Rabbit Warrior, chú thỏ chiến binh trắng, tai dài, khăn/lá xanh và trang bị theo hình tham chiếu.
- **Kẻ địch:** các biến thể Zombie Carrot; boss kết thúc demo là King Carrot.
- **Camera:** 3D top-down nghiêng nhẹ / 3/4, theo nhân vật. Không dùng camera sau lưng kiểu third-person và không làm game 2D side-scroller.
- **Chiến trường:** một arena mở có ranh giới rõ, tầm nhìn tốt, ít vật cản cao che nhân vật.
- **Điều khiển:** joystick di chuyển + một nút đánh cận chiến. Không có nút bắn/đánh xa. Vũ khí tầm xa tự chọn mục tiêu hợp lệ và tấn công khi cooldown hoàn tất.
- **Cận chiến:** vũ khí chính đang trang bị được kích hoạt bằng nút đánh. Kiếm nhanh/tầm vừa; rìu chậm hơn/sát thương mạnh hơn. Có thể mở rộng thương sau MVP, không bắt buộc.
- **Vũ khí tầm xa đầu tiên:** Tomato Blaster và Grape Splitter; chỉ vũ khí tầm xa được trang bị vào slot hoạt động mới tự bắn.
- **Vũ khí cận chiến đầu tiên:** Carrot Blade và Carrot Axe.
- **Quái tối thiểu:** Zombie Carrot cơ bản, quái nhanh, quái tinh nhuệ, King Carrot boss.
- **Một run:** ba wave thường + một wave boss. Thời lượng mục tiêu khoảng 3–5 phút. Wave kết thúc khi điều kiện wave hoàn tất, không bắt chờ timer nếu quái đã bị diệt hết.
- **Giữa wave:** tạm dừng giao chiến để ghép vũ khí, đổi trang bị và mua hàng từ cửa hàng đơn giản bằng tiền trong run.
- **Loot:** quái thường cũng có thể rơi vũ khí. Phần thưởng là ngẫu nhiên có kiểm soát, không khóa cứng một loại quái chỉ có thể rơi một món.
- **Ghép:** hai vũ khí cùng loại + cùng cấp → một vũ khí cùng loại ở cấp kế tiếp. Ghép diễn ra trong giai đoạn nghỉ giữa wave.
- **Tài sản theo run:** vũ khí nhặt/ghép trong run và tài nguyên chưa được xác nhận sẽ mất khi thất bại theo các quy tắc ở GDD. Tài sản đã sở hữu trước trận, cấp tài khoản và thành tựu đã ghi nhận không bị mất.
- **Tiến trình dài hạn:** XP/cấp tài khoản thỏ, cột mốc chiến tích, phần thưởng và tiến trình được lưu cục bộ.
- **Năng lượng:** thất bại trừ năng lượng theo định hướng hiện tại. Trong quá trình gameplay development phải có feature flag để tắt yêu cầu năng lượng, tránh chặn tester. Không triển khai thanh toán thật.

## 4. Phạm vi P0 của playable demo

Demo chỉ được xem là vertical slice khi luồng sau chạy được không cần debug thủ công:

`Start → chọn/bắt đầu run → di chuyển trong arena → đánh cận chiến → vũ khí tầm xa tự bắn theo cooldown → quái chết → rơi đồ → nhặt đồ → đưa đồ vào run inventory → ghép 2 vũ khí trùng loại/cấp → nghỉ giữa wave → mua/đổi trang bị → ba wave thường → King Carrot → win hoặc lose → result screen → tài khoản được cập nhật và lưu → quay lại menu/chơi lại.`

Không được bỏ qua loot, balo/ghép, nghỉ giữa wave, boss, kết quả hoặc lưu tài khoản để chỉ tạo một arena đánh quái đơn giản rồi tuyên bố demo hoàn chỉnh.

## 5. Quy trình làm việc bắt buộc

### Bước 0 — Bootstrap môi trường (bắt buộc khi chưa có project)
- Không giả định Unity Hub, Unity Editor hay Unity project đã tồn tại. Kiểm tra trạng thái trước bằng cách an toàn.
- Nếu Unity Hub/Editor chưa cài, hướng dẫn chủ sở hữu cài bằng trang chính thức và Unity Hub GUI. Không tự cài/xóa/nâng cấp phần mềm hoặc sửa thiết lập hệ thống nếu chưa được xác nhận. Dừng code cho đến khi Unity Editor sẵn sàng.
- Nếu Unity đã cài nhưng chưa có project, hướng dẫn tạo project mới trong Unity Hub. Ưu tiên template Universal 3D (URP) nếu có; nếu không thì 3D Core. Không đoán đường dẫn; chủ sở hữu xác nhận nơi lưu. Không tự tạo hàng loạt file Unity trong folder không phải project.
- Khi project được tạo, mở đúng thư mục có `Assets/`, `Packages/`, `ProjectSettings/`. Copy tài liệu/rules từ handoff vào root, không bỏ tài liệu vào `Assets/` và không ghi đè file trùng tên khi chưa so sánh.

### Bước A — Audit project trước khi chỉnh sửa
- Xác nhận `ProjectSettings/ProjectVersion.txt`, `Packages/manifest.json`, cấu trúc `Assets`, scenes, input system, scripts và render pipeline thực tế.
- Kiểm tra Git status nếu repo dùng Git. Không xóa, đổi tên hoặc ghi đè asset/script người dùng đã tạo mà chưa hiểu vai trò.
- Tạo `docs/PROJECT_AUDIT.md` và báo cáo Unity version, template, những gì đã có, blockers và bước tiếp theo.
- Chỉ sau khi project hợp lệ mới tiến hành Gate 1. Nếu không hợp lệ, dừng và hướng dẫn giải quyết blocker cụ thể.

### Bước B — Triển khai theo gate
Theo đúng `docs/IMPLEMENTATION_ROADMAP.md`. Mỗi gate cần code chạy được và có cách kiểm thử rõ ràng. Không tạo một lần hàng chục script rời rạc rồi bỏ lại các tham chiếu chưa gắn.

### Bước C — Kiểm tra sau mỗi thay đổi
- Kiểm tra compile errors và lỗi Console có liên quan.
- Chạy tests sẵn có; bổ sung test logic cho merge, wave completion, loot rules, run state và persistence nếu phù hợp.
- Nếu chưa thể mở Unity Editor hoặc chạy Play Mode bằng công cụ hiện có, nói rõ giới hạn đó; không tuyên bố đã test trong game nếu chưa thực sự làm.
- Cập nhật docs/checklist sau khi hoàn tất gate.
- Kết thúc mỗi lượt bằng: file thay đổi, tính năng hoạt động, kiểm thử đã chạy và kết quả, vấn đề còn lại, bước tiếp theo.

## 6. Chỉ dẫn kiến trúc

- Dùng **Unity 3D + C#**. Không chuyển game sang Unreal, Godot hoặc framework khác.
- Ưu tiên component nhỏ, trách nhiệm rõ, cấu hình tách khỏi logic. Dùng ScriptableObjects cho stats, weapon definitions, enemy definitions, loot tables và wave configuration khi phù hợp.
- Giữ logic gameplay tách khỏi UI và presentation. UI chỉ hiển thị state và gửi command; không tự chứa luật combat/loot/progression.
- Các system chính dự kiến: `PlayerController`, `PlayerHealth`, `MeleeCombat`, `RangedWeaponController`, `EnemyAI`, `EnemyHealth`, `WaveDirector`, `LootDropper`, `RunInventory`, `WeaponMergeSystem`, `IntermissionShop`, `RunManager`, `AccountProgression`, `SaveService`, UI controllers.
- Đây là ranh giới trách nhiệm, không phải yêu cầu phải tạo đúng từng class ngay từ đầu. Tránh kiến trúc quá mức và tránh một `GameManager` khổng lồ.
- Không thêm package bên thứ ba nếu Unity built-in đã đáp ứng. Trước khi dùng package mới, kiểm tra nó đã được cài chưa và lý do cần thiết.
- Tuân theo Unity version/package versions của workspace. Không đổi major version hoặc nâng package chỉ để giải quyết vấn đề nhỏ.
- Tạo cấu hình dữ liệu cho HP, damage, cooldown, movement speed, wave counts, drop chance, shop prices, energy toggle. Không rải magic numbers khắp code.
- Dùng `ObjectPool` chỉ khi cần hoặc khi projectile/enemy spawn liên tục gây vấn đề; không xây framework phức tạp trước khi profile.
- Hạn chế singleton toàn cục. Nếu cần bootstrap/service holder, giữ scope nhỏ và ghi rõ lifecycle.
- Save data cần có schema/version đơn giản, giá trị mặc định an toàn và xử lý trường hợp file save chưa tồn tại hoặc lỗi parse.

## 7. Khi chưa có model/animation 3D hoàn chỉnh

Không chờ art hoàn chỉnh mới làm gameplay, cũng không tự ý thay đổi concept. Hãy tạo **greybox/prototype art** bằng primitive hoặc material màu dễ phân biệt để chứng minh mọi hệ thống hoạt động. Player và từng loại quái phải phân biệt được về silhouette/màu; boss lớn hơn và có telegraph dễ thấy. Giữ gameplay logic không phụ thuộc trực tiếp vào loại mesh để sau này thay bằng prefab/animation chính thức.

Ảnh trong `art_reference/` là concept/reference, không phải model 3D đã import. Không giả định PNG có thể điều khiển như rigged 3D model.

## 8. Các điều tuyệt đối không tự thêm trong P0

- Không thêm nút tấn công tầm xa.
- Không thêm dash, block, ultimate, nhiều kỹ năng active hoặc joystick ngắm bắn.
- Không thêm multiplayer, PvP/co-op, online account, cloud save hoặc leaderboard.
- Không thêm gacha, ads, IAP/real-money store, battle pass.
- Không thêm nhiều nhân vật, nhiều thế giới, bản đồ thế giới hoặc cinematic/story dài.
- Không biến tất cả quái thành nguồn rơi cố định chỉ một loại item.
- Không làm mất tài sản tài khoản khi thua.
- Không kéo dài run bằng cách chờ timer sau khi mục tiêu wave đã hoàn thành.
- Ý tưởng mới nhưng không cần cho P0 phải ghi vào `docs/BACKLOG.md`, không implement ngay.

## 9. Xử lý phần chưa được quyết định

Không được dừng toàn bộ dự án chỉ vì thiếu các giá trị cân bằng. Dùng cấu hình provisional và ghi lại trong `docs/DECISION_LOG.md`.

Giá trị tạm có thể cấu hình: thời lượng wave, HP/damage, cooldown, drop rates, balo 6 ô, giá cửa hàng, reward XP, tỷ lệ chuyển run gold sang account gold. Trước khi phát hành bản thương mại, chủ sở hữu phải xác nhận các giá trị này.

Các quyết định quan trọng đang mở gồm: hướng màn hình cuối cùng; quy tắc xử lý vàng run còn dư sau khi thắng; mức XP tài khoản khi thua; chi tiết UI năng lượng/recharge và mua thêm. Để prototype không bị nghẽn, triển khai theo cách có thể cấu hình, với energy gating tắt trong dev mặc định.

## 10. Việc đầu tiên phải làm ngay khi nhận prompt này

1. Xác định trạng thái thật: Unity Hub/Editor đã cài chưa, workspace hiện tại có phải Unity project không. Không giả định.
2. Nếu chưa cài Unity: hướng dẫn chủ sở hữu cài Unity Hub và Unity LTS qua giao diện chính thức; dừng trước khi tạo script.
3. Nếu đã có Unity nhưng chưa có project: hướng dẫn tạo project mới qua Unity Hub và chờ chủ sở hữu mở đúng project. Không viết code Unity trong folder ngẫu nhiên.
4. Khi project hợp lệ, đọc tài liệu ở mục 2, tạo `docs/PROJECT_AUDIT.md` và báo cáo Unity version/template/input/render pipeline.
5. Sau audit, chỉ bắt đầu **Gate 1: playable arena greybox + camera + player movement**, bao gồm input bàn phím để test trong Editor và UI joystick cơ bản cho mục tiêu cảm ứng. Không code toàn bộ game cùng lúc và không chỉ dừng ở việc viết thêm kế hoạch.
6. Khi Gate 1 được chạy và xác minh, tiếp tục Gate 2 theo roadmap, giữ mỗi thay đổi nhỏ và kiểm thử được.

## 11. Định nghĩa “hoàn thành”

Không dùng số lượng script, số dòng code hoặc ảnh chụp code làm bằng chứng demo hoàn tất. Chỉ xác nhận một feature hoàn thành khi chạy được trong Unity theo luồng sử dụng thực tế và tiêu chí nghiệm thu tương ứng đạt. Báo cáo trung thực những phần chưa chạy/test được.
