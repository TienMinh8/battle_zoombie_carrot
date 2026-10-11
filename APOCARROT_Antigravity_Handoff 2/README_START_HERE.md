# Bắt đầu từ số 0 — APOCARROT + Antigravity

Đây là bộ tài liệu bàn giao cho AI lập trình, **không phải Unity project đã tạo sẵn**. Nếu bạn chưa cài Unity hoặc chưa tạo project, hãy làm theo `SETUP_FROM_ZERO.md` trước. Không cần có kiến thức Unity sâu để bắt đầu.

## Cấu trúc file

- `SETUP_FROM_ZERO.md` — quy trình dành cho máy chưa có Unity project.
- `FIRST_PROMPT_TO_ANTIGRAVITY.md` — prompt đầu tiên để AI kiểm tra môi trường và hướng dẫn setup đúng trạng thái thực tế.
- `ANTIGRAVITY_MASTER_PROMPT.md` — chỉ dẫn đầy đủ cho AI lập trình gameplay.
- `AGENTS.md` — quy tắc lâu dài của workspace; copy vào thư mục gốc Unity project sau khi tạo xong.
- `docs/APOCARROT_GDD_Demo_Plan_v0_1.md` và `.docx` — thiết kế game và phạm vi demo.
- `docs/DECISION_LOG.md` — quyết định đã chốt, giá trị tạm và vấn đề còn mở.
- `docs/IMPLEMENTATION_ROADMAP.md` — thứ tự triển khai và tiêu chí nghiệm thu.
- `docs/DEMO_ACCEPTANCE_CHECKLIST.md` — checklist xác nhận demo chơi được.
- `docs/BACKLOG.md` — ý tưởng để sau MVP, không tự ý đưa vào bản đầu.
- `art_reference/` — ảnh tham chiếu nhân vật, logo/boss và góc nhìn game; chưa phải model 3D có rig/animation.

## Quy trình ngắn nhất nếu bắt đầu mới

1. Cài **Unity Hub** từ trang chính thức: https://unity.com/download.
2. Trong Unity Hub, cài một bản **Unity 6.3 LTS** hoặc bản LTS đang được nhóm chọn; ưu tiên ổn định thay vì bản thử nghiệm/Tech Stream. Tại thời điểm tài liệu này được tạo, Unity giới thiệu Unity 6.3 LTS là nhánh LTS hiện hành và hỗ trợ đến tháng 12/2027: https://unity.com/releases/unity-6.
3. Trong Unity Hub chọn **New project** → chọn template **Universal 3D (URP)** nếu có (nếu không, chọn template 3D Core và ghi lại lựa chọn) → tên project `APOCARROT` → lưu ở thư mục riêng, dễ tìm, không nằm bên trong folder của bộ handoff → Create. Hướng dẫn chính thức: https://docs.unity.com/en-us/hub/project-create.
4. Chờ Unity Editor mở project mới; để Editor hoàn tất import và xử lý package. Nếu có lỗi đỏ trong Console, chụp lại/ghi lại trước khi tiếp tục.
5. Mở đúng thư mục gốc `APOCARROT` trong Antigravity. Thư mục gốc đúng là nơi có `Assets/`, `Packages/`, `ProjectSettings/`.
6. Sao chép `AGENTS.md`, `ANTIGRAVITY_MASTER_PROMPT.md`, `docs/` và `art_reference/` từ bộ handoff vào thư mục gốc project. Nếu file trùng tên đã tồn tại, so sánh trước khi ghi đè. **Không đặt toàn bộ tài liệu trong `Assets/`**.
7. Trong Antigravity mở file `FIRST_PROMPT_TO_ANTIGRAVITY.md` và gửi prompt trong đó. Yêu cầu AI kiểm tra Unity project mới rồi thực hiện Gate 1; đừng yêu cầu AI code toàn bộ game trong một lượt.

## Thiết lập ban đầu tối thiểu

- Tạo project 3D trước, chưa cần tải asset store hay model nhân vật.
- Chỉ cần làm demo trong Unity Editor trước. Chưa cần cài Android/iOS build modules cho đến khi chọn nền tảng đích và cần build ra thiết bị.
- Chưa cần dùng ảnh PNG concept như model điều khiển được. Gate đầu tiên dùng primitive/greybox; art sẽ thay sau.
- Nên bật Git hoặc tạo bản sao lưu trước khi agent thay đổi nhiều file.
- Đừng yêu cầu Antigravity tự đoán đường dẫn Unity, phiên bản Editor hay package đang có. Nó phải kiểm tra thực tế và báo lại.

## Nếu bạn chưa cài Unity Hub/Editor

Mở Antigravity ở một thư mục làm việc trống hoặc thư mục chứa bộ handoff, gửi `FIRST_PROMPT_TO_ANTIGRAVITY.md`. Agent phải kiểm tra trạng thái trước; nếu thiếu Unity, nó sẽ hướng dẫn cài đặt và dừng trước khi ghi script Unity vào một thư mục không phải project. Sau khi bạn tạo project qua Unity Hub, mở đúng thư mục project và tiếp tục.

## Quan trọng

Bộ tài liệu này không chứa game đã build hoặc model 3D hoàn chỉnh. Mục tiêu là hướng AI xây một vertical slice nhỏ nhưng liền mạch: di chuyển → cận chiến → vũ khí xa tự động → quái → loot → ghép đồ → intermission/shop → wave/boss → win/lose → lưu tài khoản.
