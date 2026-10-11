# SETUP FROM ZERO — Cài đặt và tạo Unity project cho APOCARROT

Tài liệu này dành cho trường hợp **chưa tạo Unity project nào**. Làm theo đúng thứ tự, chưa cần viết code.

## Giai đoạn 1 — Cài môi trường

1. Tải Unity Hub ở trang chính thức: https://unity.com/download.
2. Cài và mở Unity Hub, đăng nhập tài khoản Unity nếu được yêu cầu và hoàn thành quy trình license cá nhân phù hợp với tài khoản của bạn.
3. Trong Unity Hub mở mục cài đặt **Installs** → **Install Editor**. Chọn nhánh LTS ổn định. Khuyến nghị bắt đầu bằng **Unity 6.3 LTS** (không chọn Alpha/Beta/Tech Stream cho prototype này nếu không có lý do đặc biệt). Xem trang Unity 6: https://unity.com/releases/unity-6.
4. Nếu mục tiêu hiện tại chỉ là tạo và chạy demo trong Editor trên Mac, cài Editor và module nền tảng máy tính hiện tại. Chưa cần cài Android/iOS Build Support ngay; có thể thêm module qua Unity Hub sau khi chốt nền tảng phát hành.

## Giai đoạn 2 — Tạo project mới

1. Mở **Projects** trong Unity Hub và nhấn **New project**. Hướng dẫn của Unity: https://docs.unity.com/en-us/hub/project-create.
2. Chọn bản Editor LTS vừa cài.
3. Chọn template **Universal 3D (URP)** nếu được cung cấp. Nếu không có, chọn **3D Core** và ghi lựa chọn vào `docs/PROJECT_AUDIT.md`; không cài/chuyển render pipeline chỉ vì agent thích template khác.
4. Project name: `APOCARROT`.
5. Location: dùng một thư mục riêng, ví dụ `~/UnityProjects/APOCARROT` hoặc một nơi bạn dễ tìm. Không lưu bên trong thư mục `APOCARROT_Antigravity_Handoff`.
6. Nhấn **Create project** và chờ Editor mở xong.
7. Trong Unity, chờ asset import và package compile hoàn tất. Mở **Window → General → Console** (nếu Console chưa mở). Nếu có lỗi đỏ, chụp ảnh hoặc copy log để AI hỗ trợ trước khi thêm gameplay code.

## Giai đoạn 3 — Mở project đúng trong Antigravity

1. Trong Antigravity chọn **Open Folder** và chọn thư mục `APOCARROT` vừa tạo.
2. Xác minh thư mục mở đúng có các mục `Assets`, `Packages`, `ProjectSettings` và file `ProjectSettings/ProjectVersion.txt`. Nếu thiếu, có thể đã mở nhầm folder cha/con.
3. Giải nén bộ handoff. Sao chép các file/thư mục sau vào thư mục gốc Unity project:
   - `AGENTS.md`
   - `ANTIGRAVITY_MASTER_PROMPT.md`
   - `docs/`
   - `art_reference/`
   - `SETUP_FROM_ZERO.md` (tùy chọn, giữ như tài liệu tham khảo)
4. Không chép tài liệu `.md` vào `Assets/` trừ khi có lý do rõ ràng; code và assets Unity nằm trong `Assets/`, còn tài liệu quy trình nằm ở root/`docs/`.
5. Nếu project đã tự có `AGENTS.md`, `README` hoặc thư mục `docs`, so sánh trước khi ghi đè.

## Giai đoạn 4 — Cho AI bắt đầu đúng cách

Mở `FIRST_PROMPT_TO_ANTIGRAVITY.md` và gửi prompt trong đó. AI cần:

- đọc GDD và quy tắc;
- kiểm tra phiên bản Unity, template/render pipeline, Input System hiện có, scene và console;
- tạo `docs/PROJECT_AUDIT.md` báo cáo trạng thái thực tế;
- chỉ triển khai Gate 1 sau khi project hợp lệ: arena greybox, thỏ placeholder, camera top-down nghiêng và di chuyển;
- xác minh bằng Play Mode khi có thể, không tự tuyên bố đã test nếu không chạy được.

## Chưa làm ở giai đoạn này

- Chưa tải model 3D trả phí hay asset store.
- Chưa triển khai tất cả tính năng game cùng một lúc.
- Chưa setup quảng cáo, mua hàng, tài khoản online, multiplayer hoặc cloud save.
- Chưa cần build iOS/Android cho đến khi bản Editor prototype đã chạy ổn và quyết định target platform.

## Mốc hoàn thành bước setup

Setup chỉ hoàn thành khi Unity Editor mở project APOCARROT không có lỗi compile/blocking, Antigravity mở đúng thư mục project, và AI đã tạo báo cáo `docs/PROJECT_AUDIT.md`. Đây chưa phải demo chơi được; đó là nền móng để bắt đầu Gate 1.
