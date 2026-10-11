---
title: "APOCARROT: CARROT SURVIVAL"
subtitle: "Game Design Document & Playable Demo Plan"
author: "Bản thiết kế sản phẩm"
date: "10/10/2026"
---

**Phiên bản:** 0.1  
**Trạng thái:** Bản nền tảng để khóa phạm vi demo đầu tiên  
**Mục tiêu tài liệu:** Thống nhất game là gì, người chơi làm gì, demo đầu tiên phải có những hệ thống nào, cách chúng kết nối và điều kiện được xem là hoàn thành.

> **Nguyên tắc làm việc:** Không tự thêm hệ thống mới giữa chừng. Mọi ý tưởng mới đưa vào danh sách “Ngoài phạm vi / Backlog”; chỉ thay đổi phạm vi hiện tại khi có quyết định chủ động. Các thông số đánh dấu **TẠM THỬ NGHIỆM** là giá trị để chơi thử và cân bằng, không phải cam kết cuối cùng.

<!-- PAGEBREAK -->

**Nội dung tài liệu**

- Tóm tắt sản phẩm
- Phạm vi demo đầu tiên
- Trải nghiệm người chơi và vòng lặp game
- Điều khiển, camera và chiến đấu
- Nhân vật và kẻ địch
- Vũ khí, balo và ghép đồ
- Loot và kinh tế trong trận
- Wave, cửa hàng và điều kiện thắng/thua
- Tiến trình tài khoản, thành tựu và phần thưởng
- Năng lượng và xử lý thất bại
- Các màn hình và UI bắt buộc
- Dữ liệu và kiến trúc triển khai Unity
- Kế hoạch triển khai theo cổng nghiệm thu
- Tiêu chí nghiệm thu tổng thể của demo
- Danh sách quyết định và quy tắc chống phình phạm vi
- Phụ lục A-B. Tóm tắt và các yếu tố không được tự ý thêm

<!-- PAGEBREAK -->

# 1. Tóm tắt sản phẩm

## 1.1. Game là gì?

**APOCARROT: Carrot Survival** là game **3D Action Roguelite dạng arena survival theo wave**, kết hợp chiến đấu cận chiến trực tiếp, vũ khí tầm xa tự động, nhặt vật phẩm, ghép vũ khí và tiến trình tài khoản lâu dài.

Người chơi điều khiển một chú thỏ chiến binh trong đấu trường, đánh bại các đợt Zombie Carrot, nhặt vàng và vũ khí, ghép hai vũ khí cùng loại/cùng cấp để nâng cấp, mua hoặc đổi trang bị giữa các wave và kết thúc màn bằng trận boss.

## 1.2. Lời hứa trải nghiệm

- Vào trận nhanh, hiểu cách chơi trong vài chục giây.
- Người chơi trực tiếp điều khiển vị trí và đòn đánh cận chiến.
- Vũ khí tầm xa tự động tấn công theo tầm đánh và thời gian hồi chiêu, không cần thêm nút bắn.
- Mỗi wave có nhịp riêng, phần thưởng và quyết định chuẩn bị rõ ràng.
- Nhặt vũ khí và ghép đồ tạo ra cảm giác “build mạnh dần” ngay trong trận.
- Thua trận không xóa tài khoản, vũ khí sở hữu từ trước hoặc tiến trình đã ghi nhận; đồ nhặt được trong lượt chơi thất bại sẽ mất.
- Mỗi màn ngắn, có mục tiêu kết thúc cụ thể và một trận boss làm cao trào.

## 1.3. Thể loại và góc nhìn

| Thành phần | Định hướng hiện tại |
|---|---|
| Thể loại chính | 3D Action Roguelite / Arena Survival / Wave-based Combat |
| Lớp chiến thuật | Loot, quản lý balo, ghép vũ khí |
| Góc nhìn | 3D top-down nghiêng nhẹ, gần 3/4; camera theo nhân vật |
| Chiến trường | Một đấu trường mở cho demo đầu tiên |
| Nhịp trận | Mục tiêu 3-5 phút cho một màn demo, chưa tính thời gian ở menu |
| Nền tảng/khung hình | Unity 3D; định hướng điều khiển cảm ứng. Hướng màn hình cần xác nhận trước khi hoàn thiện UI |

**Không gọi game là auto-battler.** Người chơi chủ động điều khiển thỏ và đánh cận chiến. Chỉ các vũ khí tầm xa là tự động.

## 1.4. Trụ cột thiết kế

1. **Action dễ hiểu:** di chuyển, đánh cận chiến, né vị trí nguy hiểm.
2. **Loot có ý nghĩa:** quái thường cũng có thể rơi vũ khí; vật phẩm tạo cơ hội thay đổi build.
3. **Ghép đồ tạo quyết định:** hai món cùng loại và cùng cấp ghép thành một món cấp cao hơn.
4. **Wave ngắn, có cao trào:** chiến đấu, nghỉ ngắn, chuẩn bị, chiến đấu tiếp, boss.
5. **Tiến trình lâu dài nhưng không phá cân bằng:** cấp tài khoản, thành tựu và mở khóa được lưu; sức mạnh trong trận vẫn phải phụ thuộc lựa chọn của người chơi.

---

# 2. Phạm vi demo đầu tiên

## 2.1. Mục tiêu của demo

Demo đầu tiên không cần có nhiều màn hoặc đầy đủ nội dung của game thương mại. Nó phải chứng minh được rằng vòng lặp cốt lõi hoạt động trọn vẹn:

**Vào trận → điều khiển thỏ → đánh quái → nhận loot → nhặt/ghép vũ khí → qua wave → mua hoặc đổi trang bị → đánh boss → thắng/thua → nhận kết quả → tiến trình tài khoản được lưu.**

Demo phải chơi được từ đầu đến cuối mà không cần người phát triển điều khiển bằng công cụ debug hay sửa dữ liệu thủ công giữa trận.

## 2.2. Danh sách phải có trong demo (P0)

| Nhóm | Phạm vi bắt buộc |
|---|---|
| Nhân vật | Một thỏ người chơi; di chuyển, quay hướng, đánh cận chiến, nhận sát thương, chết và kết thúc trận |
| Camera | Camera top-down nghiêng, theo thỏ và giữ nhân vật/chiến trường dễ quan sát |
| Chiến trường | Một arena có ranh giới rõ, mặt đất và vật cản đơn giản |
| Kẻ địch | Quái thường, quái nhanh, quái tinh nhuệ và King Carrot làm boss |
| Combat | Hit detection, HP, sát thương, phản hồi khi trúng đòn, quái đuổi/tấn công người chơi |
| Vũ khí | Tối thiểu 2 vũ khí cận chiến và 2 vũ khí tầm xa tự động |
| Loot | Vàng, vũ khí, hồi phục hoặc vật liệu hiếm; có xác suất, không khóa một loại quái vào đúng một món |
| Balo trận | Nhìn được trang bị đang dùng và đồ dự trữ; nhặt, thay, ghép vũ khí giữa wave |
| Ghép | Hai vũ khí cùng loại/cùng cấp ghép thành một vũ khí cấp kế tiếp |
| Wave | Ba wave thường và một wave boss; có điều kiện kết thúc wave rõ ràng |
| Nghỉ giữa wave | Cửa hàng tối giản, ghép đồ, đổi vũ khí và bắt đầu wave kế tiếp |
| Kết quả | Màn thắng/thua, thống kê cơ bản, phần thưởng được ghi nhận theo quy tắc đã định |
| Tài khoản | Cấp thỏ, XP tài khoản và ít nhất một cột mốc chiến tích được lưu giữa các lần chạy |
| Lưu dữ liệu | Lưu cục bộ tiến trình tài khoản và các vật phẩm sở hữu từ trước |
| Năng lượng | Trong demo có công tắc kiểm thử để bỏ qua năng lượng; chỉ bật cơ chế thật sau khi chốt quy tắc trừ năng lượng |

## 2.3. Những thứ chưa làm trong demo (khóa ngoài phạm vi)

- Nhiều nhân vật có thể chơi; đổi nhân vật; cây kỹ năng riêng từng nhân vật.
- Nhiều thế giới, bản đồ lớn, bản đồ thế giới hoặc hàng chục màn.
- Multiplayer, PvP, co-op, bảng xếp hạng online, tài khoản máy chủ.
- Battle pass, gacha, quảng cáo, mua hàng trong ứng dụng hoặc cửa hàng tiền thật.
- Daily quest, guild, social, leaderboard, cloud save.
- Hàng chục vũ khí, nhiều nhánh tiến hóa và hệ thống chế tạo phức tạp.
- Cốt truyện cinematic, hội thoại dài, nhiệm vụ phụ, cutscene.
- Tối ưu nội dung live-ops.

Các ý tưởng này có thể được ghi vào backlog nhưng không được tự ý đưa vào demo.

---

# 3. Trải nghiệm người chơi và vòng lặp game

## 3.1. Mục đích chơi

**Mục tiêu trong một màn:** sống sót qua các wave, phát triển bộ vũ khí trong lượt chơi và đánh bại boss để hoàn thành màn.

**Mục tiêu dài hạn:** tăng cấp tài khoản thỏ, hoàn thành cột mốc chiến tích, mở khóa các lựa chọn trang bị và tiến tới những màn khó hơn.

**Động lực quay lại:** thử cách kết hợp vũ khí khác, đánh bại boss, hoàn thành thành tựu và nhận phần thưởng cấp tài khoản. Không yêu cầu người chơi phải cày một trận cực dài mới thấy tiến triển.

## 3.2. Vòng lặp một lượt chơi

1. **Chuẩn bị:** chọn bộ trang bị sở hữu trước trận; demo bắt đầu với một vũ khí cận chiến cơ bản và một vũ khí tầm xa cơ bản.
2. **Chiến đấu:** điều khiển di chuyển và đòn cận chiến; vũ khí tầm xa tự động tấn công khi hết cooldown và có mục tiêu hợp lệ.
3. **Nhận loot:** nhặt vàng, vũ khí hoặc vật phẩm hỗ trợ; vũ khí trong trận được đưa vào balo tạm thời.
4. **Kết thúc wave:** khi hoàn tất mục tiêu wave, dừng giao chiến và mở màn hình nghỉ ngắn.
5. **Chuẩn bị wave kế tiếp:** ghép vũ khí, đổi món đang trang bị, mua món từ cửa hàng bằng vàng của lượt chơi.
6. **Boss:** wave cuối của demo là King Carrot.
7. **Kết quả:** hiển thị thắng/thua và thống kê. Cấp/XP tài khoản được tính theo hiệu suất trận và ghi nhận ngoài trận. Đồ nhặt trong lượt chơi thất bại bị mất; tài sản có từ trước và tiến trình tài khoản đã ghi nhận được giữ.

## 3.3. Nhịp thời gian mục tiêu

Thông số bên dưới là **TẠM THỬ NGHIỆM**, để tránh một màn quá dài và có thể điều chỉnh sau khi chơi thử.

| Phân đoạn | Thời lượng mục tiêu | Nội dung |
|---|---:|---|
| Wave 1 | 30-40 giây | Dạy cách đánh và nhặt loot; xuất hiện quái cơ bản |
| Nghỉ 1 | 8-12 giây | Hướng dẫn ghép hoặc đổi món lần đầu |
| Wave 2 | 35-45 giây | Tăng số lượng/đưa quái nhanh vào |
| Nghỉ 2 | 8-12 giây | Cửa hàng và ghép vũ khí |
| Wave 3 | 35-45 giây | Có quái tinh nhuệ hoặc đội hình phối hợp |
| Chuẩn bị boss | 8-12 giây | Cho người chơi hoàn tất trang bị |
| Wave boss | 45-60 giây | King Carrot, đòn đánh có dấu hiệu báo trước |

Mục tiêu tổng thể: khoảng **3-5 phút**. Wave kết thúc khi đạt điều kiện hoàn thành, không bắt người chơi chờ bộ đếm giờ nếu quái đã bị tiêu diệt hết. Nếu người chơi chết, trận kết thúc ngay.

---

# 4. Điều khiển, camera và chiến đấu

## 4.1. Điều khiển trên màn hình

| Điều khiển | Hành vi |
|---|---|
| Joystick ảo | Di chuyển thỏ tự do trong arena |
| Nút đánh | Người chơi chủ động đánh bằng vũ khí cận chiến đang trang bị; cho phép nhấn hoặc giữ để lặp đòn theo tốc độ vũ khí |
| Vũ khí tầm xa | Không có nút bắn riêng; tự chọn mục tiêu hợp lệ và khai hỏa khi cooldown kết thúc |
| Tương tác đồ/balo | Dùng trong màn hình nghỉ giữa wave, không bắt buộc thao tác balo phức tạp giữa đàn quái |

Không thêm nút đánh xa vào HUD. Không thêm nút dash, block, ultimate hoặc joystick ngắm bắn vào demo nếu chưa có quyết định mới.

## 4.2. Quy tắc đánh cận chiến

- Nhân vật có một vũ khí cận chiến đang hoạt động.
- Đòn đánh dùng hoạt ảnh và vùng gây sát thương phù hợp loại vũ khí.
- Kiếm: nhịp nhanh, tầm vừa; rìu: chậm hơn, sát thương cao hơn; thương: tầm đánh dài hơn. Đây là hướng khác biệt hóa ban đầu, sẽ cân bằng bằng dữ liệu.
- Hướng đánh theo hướng nhân vật đang quay. Khi đang di chuyển, thỏ quay theo hướng di chuyển; khi đứng yên, mục tiêu gần trong tầm đánh có thể được ưu tiên để giảm cảm giác đánh hụt. Quy tắc chọn mục tiêu cần được kiểm thử trong prototype.
- Không để một lần nhấn gây nhiều hit ngoài ý muốn; hitbox phải đồng bộ với thời điểm vũ khí chạm mục tiêu.

## 4.3. Quy tắc vũ khí tầm xa tự động

- Chỉ vũ khí tầm xa đang được trang bị vào vị trí hoạt động mới bắn.
- Mỗi vũ khí có cooldown, tầm đánh, quy tắc chọn mục tiêu và hiệu ứng riêng.
- Không có mục tiêu hợp lệ: vũ khí chờ, không bắt đầu một chu kỳ bắn vô ích.
- Khi cooldown kết thúc và có mục tiêu: tự động tấn công.
- Nâng cấp có thể tăng sát thương, tốc độ bắn, số projectile, bán kính nổ hoặc hiệu ứng phụ; không tăng mọi thuộc tính cùng lúc một cách mặc định.

## 4.4. Camera và chiến trường

- Dùng góc nhìn **3D top-down nghiêng nhẹ / 3/4**, không phải góc nhìn sau lưng nhân vật.
- Camera theo nhân vật với độ trễ nhẹ, không rung quá mức và không làm thỏ bị che bởi công trình lớn.
- Nhân vật phải đủ lớn để đọc được hướng đánh và hoạt ảnh; đồng thời phải thấy đủ khu vực để nhận biết quái tiến tới từ nhiều hướng.
- Arena demo có ranh giới dễ hiểu, ít vật cản cao và không có khu vực khiến camera che khuất người chơi.
- Tách logic chiến đấu khỏi camera để có thể điều chỉnh góc, khoảng cách và trường nhìn mà không sửa AI hay hitbox.

---

# 5. Nhân vật và kẻ địch

## 5.1. Nhân vật người chơi: Rabbit Warrior

### Vai trò
Nhân vật trung tâm do người chơi trực tiếp điều khiển. Thiết kế cần giữ nhận diện thỏ trắng, tai dài, khăn/lá xanh và trang bị chiến binh rau củ theo hình mẫu hiện tại.

### Tài sản/hoạt ảnh tối thiểu
- Idle.
- Run/move.
- Melee attack.
- Hurt/hit reaction.
- Death.
- Các điểm gắn vũ khí cận chiến và hiệu ứng va chạm.

### Trạng thái gameplay
- HP hiện tại/tối đa.
- Hướng quay.
- Vũ khí cận chiến đang dùng.
- Vũ khí tầm xa đang dùng và cooldown của nó.
- Trạng thái bị đánh, chết hoặc bất động trong màn kết quả.

Không cần hệ thống trang phục, chỉ số chỉ số nhân vật phức tạp hoặc nhiều bộ animation khác nhau trong demo.

## 5.2. Kẻ địch cho demo

| Loại | Vai trò trong trận | Hành vi tối thiểu | Phần thưởng định hướng |
|---|---|---|---|
| Zombie Carrot cơ bản | Tạo số lượng và áp lực chính | Tiến về phía thỏ, tấn công cận chiến | Có thể rơi vàng, vũ khí hoặc vật phẩm |
| Zombie Carrot nhanh | Buộc người chơi đổi vị trí | Tốc độ di chuyển cao, HP thấp | Cơ hội rơi đồ như quái thường |
| Zombie Carrot tinh nhuệ | Tạo mục tiêu ưu tiên | HP/sát thương cao hơn hoặc có một kỹ năng đơn giản | Cơ hội rơi đồ tốt hơn; tính XP tài khoản cao hơn khi hạ |
| King Carrot (boss) | Cao trào và điều kiện thắng màn | Ít nhất 2 đòn có dấu hiệu báo trước, vùng gây sát thương rõ | Rương boss sau khi bị đánh bại |

### Quy tắc AI
- Kẻ địch phải có thể tìm đến người chơi mà không bị kẹt thường xuyên ở địa hình mẫu.
- Tấn công phải có phạm vi và thời điểm rõ; tránh gây sát thương liên tục không phản hồi.
- Spawn theo từng wave và số lượng đã định, nhưng tốc độ kết thúc wave phụ thuộc tình trạng quái còn lại, không chỉ phụ thuộc đồng hồ.
- Khi boss bị đánh bại, dừng spawn, phát trạng thái thắng và chuyển qua kết quả.

## 5.3. Thiết kế boss tối thiểu

King Carrot cần tối thiểu:

1. Đòn đánh cận thân hoặc quét vùng quanh boss.
2. Một đòn đánh diện rộng có cảnh báo trước trên mặt đất.
3. Chuyển pha hoặc tăng áp lực khi HP thấp; có thể bắt đầu bằng tăng nhịp tấn công thay vì thêm bộ chiêu mới.
4. HP đủ dài để người chơi thấy sự khác biệt với quái thường nhưng không kéo trận vượt xa mục tiêu 3-5 phút.

Không cần nhiều boss trong demo đầu tiên.

---

# 6. Vũ khí, balo và ghép đồ

## 6.1. Quy tắc trang bị

Đề xuất cho demo:

- Một vị trí vũ khí cận chiến hoạt động.
- Một vị trí vũ khí tầm xa tự động hoạt động.
- Các vũ khí còn lại nằm trong balo tạm thời, dùng để thay thế, ghép hoặc dự trữ.
- Quản lý và thay đổi trang bị chủ yếu ở màn hình nghỉ giữa wave.
- Vật phẩm nhặt trong trận thuộc lượt chơi hiện tại, không biến thành tài sản vĩnh viễn chỉ vì đã được đưa vào balo.

Đề xuất thử nghiệm balo có **6 ô tổng cộng**, trong đó hai ô đại diện cho hai vũ khí đang trang bị; các ô còn lại dành cho đồ dự trữ. Số ô này là **TẠM THỬ NGHIỆM**, cần xác nhận sau khi thử UI và tốc độ ghép.

## 6.2. Bộ vũ khí tối thiểu

| Vũ khí | Nhóm | Hành vi cấp cơ bản | Khác biệt khi nâng cấp |
|---|---|---|---|
| Carrot Blade | Cận chiến | Chém nhanh, tầm vừa | Tăng sát thương/tốc độ hoặc mở rộng cung chém |
| Carrot Axe | Cận chiến | Chém chậm hơn, sát thương cao | Tăng sức mạnh đòn nặng hoặc vùng đánh |
| Tomato Blaster | Tầm xa tự động | Ném cà chua vào mục tiêu, gây sát thương vùng nhỏ | Tăng bán kính nổ hoặc số projectile |
| Grape Splitter | Tầm xa tự động | Bắn chùm nho hoặc projectile phân tán | Tăng số viên hoặc khả năng chạm nhiều mục tiêu |

Bốn vũ khí này đủ để kiểm chứng hệ thống. Thương, chanh độc, dưa hấu, cherry, coconut và các nhánh tiến hóa khác nằm trong backlog, không phải điều kiện để hoàn tất demo.

## 6.3. Ghép vũ khí

**Quy tắc lõi:** hai vũ khí cùng loại và cùng cấp ghép thành một vũ khí cùng loại ở cấp tiếp theo.

Ví dụ: Tomato Blaster Lv.1 + Tomato Blaster Lv.1 = Tomato Blaster Lv.2.

- Bắt đầu với tối đa ba cấp để demo chứng minh ghép đồ.
- Ghép trong màn hình nghỉ giữa wave, không mở giao diện ghép bắt buộc giữa lúc đang bị tấn công.
- Hai món được tiêu thụ và thay bằng một món cấp cao hơn.
- Nếu ghép món đang trang bị, món kết quả tiếp tục giữ vai trò trang bị nếu còn hợp lệ.
- Nếu không có hai món phù hợp, không cho phép thao tác ghép.
- Cấp cao hơn phải có phản hồi trực quan và ít nhất một thay đổi cơ chế hoặc thông số rõ ràng.

## 6.4. Mua và đổi trang bị

- Cửa hàng giữa wave dùng vàng kiếm được trong lượt chơi.
- Danh sách hàng hóa ngắn, tránh tạo thêm menu sâu.
- Cho phép mua vũ khí cơ bản, một vật phẩm hồi phục hoặc một món hỗ trợ đơn giản.
- Cho phép đổi vũ khí cận chiến/tầm xa đang hoạt động bằng món trong balo.
- Nếu balo đầy khi nhặt vũ khí, lưu vào lựa chọn xử lý giữa wave: thay món, bỏ món hoặc từ chối nhặt. Demo cần triển khai một quy tắc rõ ràng, không để vật phẩm biến mất âm thầm.

---

# 7. Loot và kinh tế trong trận

## 7.1. Nguyên tắc rơi đồ

- Không khóa mỗi loại quái vào một phần thưởng cố định.
- Quái thường ngay từ đầu có thể rơi vũ khí, để người chơi sớm khám phá cơ chế loot và ghép.
- Mỗi phần thưởng dùng một lượt kiểm tra xác suất độc lập; một quái không nhất thiết phải rơi mọi loại vật phẩm.
- Quái tinh nhuệ/boss có cơ hội phần thưởng tốt hơn theo cấp độ và vai trò.
- Tutorial có thể đảm bảo một lần rơi vũ khí sớm để người chơi học ghép; sau phần hướng dẫn, dùng xác suất thông thường.

## 7.2. Bảng rơi đồ khởi điểm (TẠM THỬ NGHIỆM)

Tỷ lệ sau chỉ là thông số bắt đầu để có thể chơi thử, không phải cân bằng cuối cùng. Lưu thành dữ liệu cấu hình để chỉnh không cần sửa code.

| Phần thưởng | Quái thường | Quái nhanh | Tinh nhuệ | King Carrot |
|---|---:|---:|---:|---:|
| Vàng rơi | 20% | 20% | 100% | Thưởng bảo đảm |
| Vũ khí thường | 8% | 8% | 30% | Qua rương boss |
| Hồi phục | 3% | 3% | 10% | Có thể nằm trong rương |
| Vật liệu hiếm | 0% ở bản đầu | 0% ở bản đầu | 10% | Có cơ hội qua rương |

Các tỷ lệ rơi độc lập nên tổng phần trăm có thể vượt 100%. Bản demo **không có XP rơi thành vật phẩm** vì cấp thỏ là cấp tài khoản chung, không phải cấp trong trận. XP tài khoản được tính khi kết thúc lượt dựa trên số quái hạ, wave hoàn thành và cột mốc đạt được; khi thất bại, phần XP được ghi nhận theo hiệu suất vẫn được giữ.

## 7.3. Phân biệt tài nguyên

| Tài nguyên | Tồn tại ở đâu | Khi thua |
|---|---|---|
| Vũ khí nhặt/ghép trong trận | Balo lượt chơi | Mất |
| Vàng thu được trong trận | Ví tạm của lượt chơi, dùng để mua giữa wave | Phần còn lại chưa được xác nhận sẽ mất theo quy tắc thất bại |
| XP/cấp tài khoản | Tiến trình lâu dài; tính theo hiệu suất trận và ghi nhận ở màn kết quả | Giữ phần đã được ghi nhận |
| Thành tựu/cột mốc đã đạt | Tiến trình lâu dài | Giữ |
| Trang bị sở hữu trước trận | Kho tài khoản | Giữ |
| Ngọc/vật liệu tài khoản | Chỉ thêm khi đã xác định rõ nguồn và mục đích | Nếu đã ghi nhận vào tài khoản thì giữ |

Để tránh tạo hai loại vàng khó hiểu, bản demo có thể dùng vàng rơi trong trận cho cửa hàng giữa wave, sau đó chỉ chuyển phần thưởng kết quả sang tài khoản khi màn hoàn tất theo quy tắc đã chốt. Nếu cần vàng vĩnh viễn riêng, đó là quyết định sau kiểm thử, không tự thêm vào demo.

## 7.4. Tự nhặt hay nhặt thủ công

- Vàng tự hút khi thỏ đi vào phạm vi gần. XP tài khoản được tính tại màn kết quả, không phải vật phẩm rơi trong trận.
- Vũ khí hiển thị vật thể rơi trên mặt đất và được nhặt khi thỏ tiếp cận.
- Phải có hiệu ứng/âm thanh ngắn để xác nhận đã nhặt.
- Không yêu cầu người chơi bấm nút riêng cho từng đồng vàng.

---

# 8. Wave, cửa hàng và điều kiện thắng/thua

## 8.1. Cấu trúc wave demo

| Wave | Nội dung | Điều kiện kết thúc |
|---|---|---|
| 1 | Quái cơ bản, tutorial loot/ghép | Đợt quái bị tiêu diệt hết |
| 2 | Thêm quái nhanh, tăng áp lực di chuyển | Đợt quái bị tiêu diệt hết |
| 3 | Tinh nhuệ và phối hợp quái | Đợt quái bị tiêu diệt hết hoặc tinh nhuệ mục tiêu bị hạ theo thiết kế |
| 4 | King Carrot | Boss bị đánh bại |

## 8.2. Nghỉ giữa wave

Khi một wave kết thúc:

1. Dừng spawn và dừng sát thương từ kẻ địch còn lại (không để quái bị sót đánh người chơi trong shop).
2. Hiển thị thông báo wave hoàn thành và phần thưởng tóm tắt.
3. Mở màn hình nghỉ: balo, ghép, cửa hàng, đổi trang bị.
4. Cho người chơi bắt đầu wave tiếp theo khi đã sẵn sàng; nếu muốn giữ nhịp nhanh có thể có nút “Tiếp tục”.

Thời gian nghỉ mục tiêu 8-12 giây là gợi ý. Nếu người chơi cần thao tác ghép, không nên tự động bắt đầu wave sau một bộ đếm quá ngắn. Với demo, ưu tiên nhịp dễ hiểu hơn là ép tốc độ.

## 8.3. Thắng/thua

**Thắng:** hạ King Carrot. Dừng toàn bộ spawn, phát hiệu ứng thắng, trao rương/ phần thưởng, cập nhật XP tài khoản và cột mốc, lưu tiến trình.

**Thua:** HP thỏ về 0. Dừng combat, hiện kết quả thất bại, xóa dữ liệu loot tạm của lượt chơi, giữ tài sản từ trước và các tiến trình tài khoản đã được ghi nhận. Sau đó cho phép thử lại nếu đủ năng lượng hoặc đang bật chế độ test không giới hạn.

**Không có kết thúc bằng hết giờ trong wave thường.** Thời gian chỉ dùng để kiểm soát nhịp hoặc hiển thị thống kê; điều kiện wave phải dựa vào mục tiêu chiến đấu.

---

# 9. Tiến trình tài khoản, thành tựu và phần thưởng

## 9.1. Cấp thỏ là cấp tài khoản chung

Cấp thỏ không reset khi vào trận. XP tài khoản được tích lũy qua trận, hiệu quả chiến đấu và các cột mốc. Khi lên cấp, người chơi nhận phần thưởng ngoài trận.

Phải phân biệt:
- **Tiến trình tài khoản:** được lưu lâu dài.
- **Build trong một lượt chơi:** vũ khí nhặt được, cấp ghép và hiệu ứng tạm thời; mất khi thua.

Bản demo không bắt buộc có level-up chiến đấu trong wave. Nếu có bổ sung về sau, nó phải tách biệt với cấp tài khoản.

## 9.2. Thành tựu/cột mốc demo

Tối thiểu ba cột mốc để kiểm tra toàn bộ đường đi dữ liệu:

- Hạ 20 Zombie Carrot.
- Hoàn thành wave đầu tiên.
- Hạ King Carrot lần đầu.

Khi đạt điều kiện, tiến trình được lưu và phần thưởng hiển thị trên màn hình kết quả hoặc màn hình tài khoản. Tránh để một thành tựu nhận thưởng lặp không giới hạn do tải lại màn.

## 9.3. Phần thưởng cấp tài khoản

- Tạo một bảng mốc XP đơn giản, dễ chỉnh.
- Phần thưởng demo nên giới hạn ở vàng/ngọc nếu đã triển khai ví tài khoản, hoặc mở khóa một lựa chọn trang bị.
- Tránh nâng sát thương vĩnh viễn quá mạnh theo cấp, vì sẽ làm hỏng cân bằng của các wave sau.
- Thất bại vẫn có thể ghi nhận XP tài khoản theo hiệu suất, nhưng loot nhặt trong trận không được tự động chuyển thành tài sản vĩnh viễn.

---

# 10. Năng lượng và xử lý thất bại

## 10.1. Nguyên tắc đã thống nhất

- Game có năng lượng để giới hạn lượt chơi.
- Khi thua, người chơi bị trừ năng lượng.
- Khi hết năng lượng, người chơi chờ hồi theo thời gian hoặc có lựa chọn mua thêm.
- Trang bị sở hữu từ trước không bị xóa khi thua; vật phẩm nhặt trong lượt chơi bị mất.

## 10.2. Điểm chưa khóa

**Chưa xác định rõ trận thắng có tiêu hao năng lượng hay chỉ trận thua mới bị trừ.** Không tự suy đoán quy tắc kinh tế tiền thật.

Để không chặn phát triển combat, demo phải có một cờ cấu hình `UnlimitedEnergyForTesting`. Trong quá trình kiểm thử gameplay, cờ này được bật. Quy tắc năng lượng thật chỉ triển khai sau khi người sở hữu sản phẩm chốt một trong các cách áp dụng.

## 10.3. Yêu cầu khi bật năng lượng thật

- HUD thể hiện số năng lượng hiện có và thời điểm hồi kế tiếp.
- Nêu rõ vì sao không thể bắt đầu màn.
- Không trừ hai lần cho một thất bại hoặc khi tải lại màn kết quả.
- Không để mất năng lượng do crash, lỗi tải màn hoặc lỗi mạng (demo lưu cục bộ nên xử lý qua trạng thái giao dịch an toàn).
- Mua năng lượng bằng tiền thật không thuộc demo đầu tiên.

---

# 11. Các màn hình và UI bắt buộc

## 11.1. Home / Lobby tối giản

- Nút Play.
- Hiển thị cấp thỏ và thanh XP tài khoản.
- Hiển thị trang bị bắt đầu game.
- Hiển thị năng lượng ở dạng test/placeholder cho tới khi chốt luật.
- Nút mở chiến tích hoặc phần thưởng cấp tài khoản ở mức tối giản.

## 11.2. Battle HUD

- Thanh HP của thỏ.
- Joystick di chuyển.
- Nút đánh cận chiến.
- Chỉ báo vũ khí tầm xa và cooldown (không phải nút bắn).
- Chỉ báo wave hiện tại, tiến độ wave và số quái bị hạ.
- Hiển thị vàng trong trận.
- Ký hiệu nhặt đồ có thể xuất hiện gần thỏ nhưng không che khuất chiến trường.

## 11.3. Intermission / Shop

- Danh sách vũ khí trong balo và hai vũ khí đang hoạt động.
- Thao tác ghép hai món phù hợp.
- Danh sách cửa hàng ngắn, giá rõ ràng.
- Nút thay vũ khí/đóng giao diện/bắt đầu wave tiếp theo.
- Khi không đủ vàng hoặc ghép không hợp lệ, hiển thị lý do.

## 11.4. Kết quả

- Trạng thái Victory/Defeat.
- Wave cao nhất, số quái bị hạ, thời gian chơi và boss đã hạ/chưa hạ.
- Phần thưởng tài khoản được ghi nhận.
- Thông báo đồ trong trận nào sẽ mất nếu thua.
- Nút thử lại và nút về Home.

Không cần UI polished cuối cùng, nhưng mọi màn hình phải đủ rõ để người thử nghiệm tự hiểu mà không cần người phát triển giải thích.

---

# 12. Dữ liệu và kiến trúc triển khai Unity

Mục tiêu kiến trúc là thay đổi cân bằng bằng dữ liệu thay vì sửa code từng loại vũ khí/quái. Không cần tạo backend trong demo đầu tiên.

## 12.1. Các thành phần logic chính

| Thành phần | Trách nhiệm |
|---|---|
| `PlayerController` | Nhận input, di chuyển, quay hướng, áp dụng giới hạn arena |
| `PlayerHealth` | HP, nhận sát thương, chết |
| `MeleeCombat` | Đọc nút đánh, cooldown/nhịp đánh, kiểm tra mục tiêu trong vùng đánh |
| `RangedWeaponController` | Chọn mục tiêu, cooldown và sinh projectile/hiệu ứng tầm xa |
| `EnemyAI` | Di chuyển đến người chơi, quyết định tấn công và nhận sát thương |
| `EnemyHealth` | HP, chết, phát sự kiện loot |
| `WaveDirector` | Cấu hình wave, spawn, điều kiện hoàn thành, chuyển wave |
| `LootDropper` | Roll loot theo cấu hình và tạo vật phẩm trong thế giới |
| `RunInventory` | Theo dõi vũ khí/tài nguyên tạm của lượt chơi |
| `WeaponMergeSystem` | Kiểm tra điều kiện và xử lý hợp nhất hai vũ khí |
| `IntermissionShop` | Hàng hóa, giá, mua, đổi trang bị giữa wave |
| `RunManager` | Trạng thái lượt chơi: chuẩn bị, combat, nghỉ, thắng, thua |
| `AccountProgression` | XP tài khoản, cấp thỏ, chiến tích, phần thưởng |
| `SaveService` | Lưu/tải dữ liệu tài khoản cục bộ an toàn |
| `HUD/UI Controllers` | Chỉ hiển thị và gửi lệnh đến hệ thống logic, không tự chứa luật gameplay |

Tên class có thể đổi theo codebase, nhưng trách nhiệm phải tách tương đương. Tránh để toàn bộ logic nằm trong một script `GameManager` lớn.

## 12.2. Dữ liệu cấu hình

Nên dùng `ScriptableObject` hoặc cấu hình dữ liệu tương đương cho:

- `WeaponDefinition`: ID, tên, nhóm cận chiến/tầm xa, cấp tối đa demo, damage, cooldown, range, prefab/effect và giá.
- `EnemyDefinition`: ID, HP, tốc độ, damage, tầm đánh, thời gian hồi đòn và loot table.
- `WaveDefinition`: đội hình, nhóm/thời điểm spawn, điều kiện kết thúc và boss.
- `LootTable`: loại phần thưởng, xác suất và số lượng.
- `AccountRewardTable`: ngưỡng XP, mốc chiến tích và phần thưởng.

Dữ liệu cần có ID ổn định để không mất tiến trình khi đổi tên hiển thị.

## 12.3. Lưu dữ liệu

Lưu cục bộ cho demo:

- Cấp và XP tài khoản.
- Thành tựu/cột mốc đã đạt.
- Danh sách loại vũ khí đã mở khóa và trang bị bắt đầu.
- Tùy chọn âm thanh/điều khiển nếu đã có.

Không lưu như tài sản lâu dài:
- Balo của một lượt chơi chưa hoàn thành.
- Vàng tạm trong trận.
- Cấp ghép/tác dụng tạm thời chỉ thuộc lượt chơi.

Ghi save theo sự kiện quan trọng (nhận thưởng, mở khóa, hoàn thành màn), tránh ghi liên tục mỗi frame.

---

# 13. Kế hoạch triển khai theo cổng nghiệm thu

Không khóa số ngày khi chưa chốt số người và asset có sẵn. Tiến độ được quản lý bằng **cổng nghiệm thu**: chỉ chuyển bước khi tiêu chí hiện tại đã đạt.

## Gate 0: Khóa thiết kế và kiểm tra asset

**Làm:** chốt tài liệu này, xác nhận quy tắc năng lượng còn mở, kiểm kê mô hình thỏ/quái/boss, animation và môi trường hiện có.

**Đạt khi:** có danh sách asset thật, asset thiếu, các quyết định tạm thử nghiệm được đánh dấu; không còn việc “để sau nghĩ”.

## Gate 1: Movement + camera + arena

**Làm:** thỏ di chuyển được, camera đúng góc, ranh giới arena hoạt động.

**Đạt khi:** thỏ luôn đọc được trên màn; camera không giật/đâm xuyên; không đi ra khỏi khu vực; điều khiển ổn định.

## Gate 2: Combat căn bản

**Làm:** nút đánh cận chiến, một loại quái, HP/damage/death, một vũ khí tầm xa tự động có cooldown.

**Đạt khi:** người chơi chủ động đánh được; projectile không bắn liên tục vượt cooldown; quái nhận sát thương một lần đúng thời điểm; thỏ chết kết thúc lượt.

## Gate 3: Loot + inventory + merge

**Làm:** quái rơi vàng/vũ khí/vật phẩm hỗ trợ theo cấu hình; nhặt đồ; balo; hai món cùng loại/cấp ghép thành một món cấp kế.

**Đạt khi:** có thể tái hiện được trường hợp loot vũ khí thường, nhặt vào balo, ghép, thay trang bị và thấy tác dụng trong combat.

## Gate 4: Wave + nghỉ giữa wave + cửa hàng

**Làm:** ba wave thường, cửa hàng nghỉ, thay đổi trang bị, wave boss.

**Đạt khi:** wave kết thúc đúng điều kiện; không phải chờ hết giờ khi hết quái; cửa hàng không bị tấn công; wave tiếp theo bắt đầu đúng trạng thái.

## Gate 5: Boss + kết quả + thất bại

**Làm:** King Carrot, cảnh báo đòn đánh, Victory/Defeat, xóa loot tạm khi thua.

**Đạt khi:** có thể thắng và thua; boss không tiếp tục spawn sau khi chết; đồ tài khoản vẫn giữ và đồ nhặt trong run bị xóa đúng quy tắc.

## Gate 6: Account progression + save

**Làm:** XP tài khoản, cấp thỏ, thành tựu, phần thưởng và save/load.

**Đạt khi:** đóng game/mở lại không mất cấp tài khoản và thành tựu; không nhận thưởng trùng khi tải lại; kết quả trận ghi nhận nhất quán.

## Gate 7: Chơi thử và cân bằng

**Làm:** chạy nhiều lượt, ghi nhận thời lượng, tỉ lệ thắng, số lần dùng shop, tần suất loot vũ khí, trường hợp chết và lỗi.

**Đạt khi:** một người mới có thể hoàn thành hoặc thất bại trận mà tự hiểu lý do; trận demo nằm gần mục tiêu thời lượng; không có lỗi blocker trong vòng lặp chính.

---

# 14. Tiêu chí nghiệm thu tổng thể của demo

Demo được coi là hoàn thành khi tất cả các câu dưới đây đều trả lời **Có**:

☐ Người chơi vào game và bắt đầu trận mà không cần thao tác debug.  
☐ Thỏ di chuyển và camera hoạt động đúng góc nhìn top-down nghiêng.  
☐ Nút đánh cận chiến hoạt động với ít nhất hai vũ khí có cảm giác khác nhau.  
☐ Vũ khí tầm xa tự động chọn mục tiêu và tuân thủ cooldown; không có nút bắn riêng.  
☐ Ít nhất ba loại quái và King Carrot có hành vi nhận biết được.  
☐ Quái thường có thể rơi vũ khí; phần thưởng không hoàn toàn cố định theo loại quái.  
☐ Người chơi có thể nhặt hai vũ khí cùng loại/cấp và ghép thành cấp cao hơn.  
☐ Có ba wave thường và một wave boss; wave không chờ timer khi mục tiêu đã hoàn thành.  
☐ Có màn nghỉ giữa wave để mua, ghép và đổi trang bị.  
☐ Có thể thắng hoặc thua một lượt chơi.  
☐ Khi thua, loot tạm bị xóa; trang bị từ trước và tiến trình tài khoản được giữ.  
☐ Cấp tài khoản/ít nhất một cột mốc chiến tích được cập nhật và vẫn tồn tại sau khi khởi động lại game.  
☐ Năng lượng có thể bị vô hiệu hóa bằng cờ test trong quá trình phát triển.  
☐ Một lượt chơi bình thường có thể hoàn thành trong mục tiêu 3-5 phút, hoặc các lý do lệch thời lượng đã được ghi lại.  

---

# 15. Danh sách quyết định và quy tắc chống phình phạm vi

## 15.1. Đã thống nhất trong định hướng hiện tại

1. Game là Carrot Survival, hành động sinh tồn theo wave, kết hợp loot và ghép vũ khí.
2. Thỏ là nhân vật người chơi trực tiếp điều khiển.
3. Góc nhìn định hướng là 3D top-down nghiêng nhẹ/3/4, theo nhân vật.
4. Có joystick di chuyển và nút đánh cận chiến.
5. Vũ khí tầm xa tự động tấn công theo cooldown; không có nút đánh xa.
6. Một màn gồm nhiều wave; mỗi wave có thể kết thúc sớm khi mục tiêu bị hạ hết.
7. Sau wave có giai đoạn ngắn để mua, ghép hoặc đổi vũ khí.
8. Quái thường cũng có thể rơi vũ khí; loot là ngẫu nhiên có cân bằng.
9. Hai vũ khí cùng loại/cùng cấp ghép thành một vũ khí cấp cao hơn.
10. Cấp thỏ là tiến trình tài khoản lâu dài; phần thưởng cấp và chiến tích hiển thị ngoài trận.
11. Thua làm mất đồ nhặt trong lượt chơi; trang bị sở hữu từ trước vẫn còn.
12. Có hệ thống năng lượng; thất bại làm trừ năng lượng, hết năng lượng thì chờ hồi hoặc mua thêm theo thiết kế tương lai.

## 15.2. Cần xác nhận trước khi coi là thông số cuối cùng

- Quy tắc năng lượng khi thắng: có trừ hay chỉ thất bại mới trừ.
- Hướng màn hình cuối cùng và kích thước HUD.
- Số ô balo (đang đề xuất 6 ô) và xử lý khi balo đầy.
- Tỷ lệ loot, thời gian wave, HP/damage/cooldown và giá shop.
- Cách chuyển vàng còn lại từ run sang tài khoản sau chiến thắng.
- Mức XP tài khoản được nhận khi thất bại.

Các mục này không được dùng làm lý do để trì hoãn prototype: dùng giá trị thử nghiệm và cấu hình hóa chúng. Nhưng không được tự biến chúng thành hệ thống mới.

## 15.3. Quy tắc quản lý ý tưởng mới

Mỗi ý tưởng mới phải được xếp vào một trong ba nhóm:

- **Blocker:** cần thiết để một tính năng P0 hoạt động; giải quyết ngay.
- **Backlog:** hữu ích nhưng không cần để demo chơi được; ghi lại, không làm ngay.
- **Out of scope:** không thuộc mục tiêu demo; chỉ xem xét sau khi demo đạt nghiệm thu.

Không triển khai một ý tưởng mới nếu chưa trả lời được: “Nó phục vụ mục tiêu demo nào, tiêu chí nghiệm thu nào sẽ thay đổi, và thứ gì bị loại bỏ để đổi lấy nó?”

---

# Phụ lục A. Nội dung demo trong một câu

**Một chú thỏ chiến binh bước vào đấu trường, chủ động đánh cận chiến và né đòn, trong khi vũ khí tầm xa tự động hỗ trợ. Quái có thể rơi vàng, vũ khí và vật phẩm hỗ trợ; người chơi nhặt, ghép, mua và đổi trang bị giữa các wave ngắn, sau đó đối đầu King Carrot. Thắng/thua cập nhật kết quả và XP tài khoản, còn đồ nhặt trong lượt chơi thất bại sẽ mất.**

# Phụ lục B. Những thứ không thay đổi khi demo đang được triển khai

Trừ khi người sở hữu sản phẩm chủ động duyệt thay đổi, không tự thêm:

- Nút đánh xa.
- Nhiều kỹ năng chủ động trên HUD.
- Nhiều nhân vật hoặc nhiều thế giới.
- Nhiều loại tiền tệ không có công dụng rõ ràng.
- Hệ thống nhiệm vụ/online/monetization.
- Các wave dài hơn chỉ để kéo thời gian.
- Loot cố định hoàn toàn theo từng loại quái.
- Mất trang bị tài khoản khi thua.

**Kết thúc tài liệu - GDD & Playable Demo Plan v0.1**
