
# APOCARROT
## Gameplay Expansion & Progression Design

**Phiên bản:** 1.0  
**Ngày:** 12/10/2026  
**Trạng thái:** Tài liệu định hướng thiết kế, phục vụ phát triển prototype và MVP.

---

# 1. TỔNG QUAN DỰ ÁN

## 1.1. Định hướng gameplay

APOCARROT là game hành động 3D phong cách hoạt hình, nơi nhân vật thỏ chiến đấu với những sinh vật cà rốt đột biến.

Trải nghiệm cốt lõi bao gồm:
- Điều khiển nhân vật chiến đấu chủ động.
- Đánh bại quái vật qua nhiều wave.
- Thu thập tiền và vũ khí trong trận.
- Sắp xếp vũ khí trong balo dạng lưới.
- Fuse các vũ khí cùng loại để tạo vũ khí cấp cao hơn.
- Đánh boss và hoàn thành level.
- Nhận rương, thu thập trang bị vĩnh viễn.
- Phân giải trang bị không cần dùng.
- Phát triển kho trang bị và mở rộng lựa chọn chiến thuật qua các level.

Mục tiêu không chỉ là giúp người chơi vượt qua màn hiện tại, mà còn tạo động lực để họ quay lại, thu thập trang bị và thử nghiệm những cách xây dựng sức mạnh mới.

## 1.2. Bốn trụ cột gameplay

### A. Active Combat
Người chơi trực tiếp điều khiển nhân vật, di chuyển, chọn mục tiêu và phản ứng trước các đòn tấn công.

### B. In-run Build
Người chơi mua, nhặt, sắp xếp và Fuse vũ khí trong mỗi lượt chơi.

### C. Permanent Collection
Người chơi thu thập trang bị từ rương, boss và các cột mốc tiến trình.

### D. Long-term Progression
Người chơi quản lý kho, lựa chọn trang bị mang vào trận, nâng cấp hệ thống và khám phá quái vật.

---

# 2. VÒNG LẶP GAMEPLAY TỔNG THỂ

Vòng lặp chính:

HOME
→ CHUẨN BỊ TRANG BỊ
→ BẮT ĐẦU LEVEL
→ CHIẾN ĐẤU
→ MUA / NHẶT / FUSE VŨ KHÍ
→ VƯỢT WAVE VÀ ĐÁNH BOSS
→ HOÀN THÀNH LEVEL
→ NHẬN RƯƠNG
→ QUẢN LÝ TRANG BỊ
→ QUAY VỀ HOME
→ CHUẨN BỊ LEVEL TIẾP THEO

## 2.1. Vai trò của từng giai đoạn

- Home: hiển thị nhân vật, tài nguyên, tiến trình và lối vào các hệ thống.
- Chuẩn bị: chọn trang bị vĩnh viễn mang vào level.
- Chiến đấu: sử dụng kỹ năng điều khiển, tiêu diệt quái và sinh tồn.
- Xây dựng build: mua vũ khí, nhặt vũ khí, sắp xếp balo và Fuse.
- Boss: kiểm tra kỹ năng, khả năng sống sót và hiệu quả build.
- Nhận thưởng: cấp rương hoặc phần thưởng theo điều kiện hoàn thành.
- Quản lý kho: trang bị món mới, giữ bản sao hoặc phân giải món dư.
- Tiến trình dài hạn: mở rộng bộ sưu tập và chuẩn bị cho thử thách tiếp theo.

---

# 3. NGUYÊN TẮC TÁCH BIỆT TRONG TRẬN VÀ NGOÀI TRẬN

Đây là quy tắc thiết kế bắt buộc của APOCARROT.

## 3.1. Hệ thống trong trận

Bao gồm:
- Tiền kiếm được trong level.
- Vũ khí mua tại cửa hàng trong trận.
- Vũ khí nhặt được trong trận.
- Vũ khí được nâng cấp thông qua Fuse.
- Các hiệu ứng và nâng cấp chỉ tồn tại trong lượt chơi.
- Các lựa chọn build tạm thời.

Tất cả những yếu tố trên phục vụ việc vượt qua level hiện tại.

Khi lượt chơi kết thúc, tiền và build tạm thời được reset theo quy tắc của chế độ chơi.

## 3.2. Hệ thống ngoài trận

Bao gồm:
- Kho trang bị vĩnh viễn.
- Trang bị được chọn mang vào level.
- Rương nhận sau level.
- Nguyên liệu thu được từ phân giải.
- Tiến trình tài khoản.
- Hồ sơ người chơi.
- Bộ sưu tập quái vật và vũ khí.
- Các nâng cấp ngoài trận nếu được triển khai.

## 3.3. Quy tắc kinh tế

1. Tiền trong trận chỉ dùng cho cửa hàng và các lựa chọn trong lượt chơi đó.
2. Tiền trong trận không tự động chuyển thành tiền tài khoản.
3. Vũ khí Fuse trong trận không được lưu thành trang bị vĩnh viễn.
4. Trang bị nhận từ rương sau level được lưu vào kho.
5. Nguyên liệu từ phân giải thuộc hệ thống ngoài trận.
6. Nâng cấp ngoài trận không được làm mất ý nghĩa của việc mua, nhặt và Fuse vũ khí trong trận.

---

# 4. TRANG BỊ NGOÀI TRẬN

## 4.1. Mục tiêu

Trang bị ngoài trận tạo ra tiến trình lâu dài và định hướng chiến thuật cho người chơi.

Người chơi có thể lựa chọn trang bị trước khi bắt đầu level. Những lựa chọn này tạo lợi thế khởi đầu hoặc định hướng phong cách chiến đấu, nhưng không nên quyết định hoàn toàn kết quả trận đấu.

## 4.2. Trạng thái trang bị

Mỗi loại trang bị có thể có các trạng thái:

- Undiscovered: chưa được khám phá.
- Unlocked: đã mở khóa để có thể nhận hoặc sử dụng.
- Owned: đã sở hữu trong kho.
- Equipped: đang được trang bị.
- Duplicate: bản sao của trang bị đã sở hữu.

Các trạng thái này cần được định nghĩa rõ trong dữ liệu và giao diện.

## 4.3. Slot trang bị

Hệ thống slot cần được thiết kế dựa trên vai trò gameplay.

Có thể cân nhắc:
- Slot vũ khí chính.
- Slot trang bị hỗ trợ.
- Slot phụ trợ hoặc di vật.

Đây là đề xuất, chưa phải số lượng slot cuối cùng.

Không nên tạo nhiều slot chỉ để tăng độ phức tạp. Mỗi slot phải tạo ra một quyết định chiến thuật có ý nghĩa.

## 4.4. Nguyên tắc cân bằng

- Trang bị ngoài trận giúp định hướng build, không tự động thắng trận.
- Trang bị khởi đầu phải hữu ích nhưng không khiến việc nhặt và Fuse trở nên thừa thãi.
- Các trang bị khác nhau nên tạo phong cách chơi khác nhau, không chỉ tăng sát thương.
- Cần phân biệt độ hiếm với cấp nâng cấp.
- Những trang bị hiếm nên tạo lựa chọn hoặc cơ chế đặc biệt, thay vì chỉ có chỉ số lớn hơn.

---

# 5. HỆ THỐNG RƯƠNG THƯỞNG

## 5.1. Mục tiêu

Sau khi hoàn thành level, người chơi nhận một rương thưởng.

Rương là cầu nối giữa gameplay hiện tại và tiến trình dài hạn. Người chơi vượt màn để có cơ hội thu thập trang bị, mở rộng kho và chuẩn bị tốt hơn cho những thử thách tiếp theo.

## 5.2. Các loại rương đề xuất

### Rương thường
Nguồn nhận:
- Hoàn thành level thông thường.

Phần thưởng:
- Trang bị phổ thông.
- Trang bị trùng.
- Nguyên liệu cơ bản nếu hệ thống này được triển khai.

### Rương tinh anh
Nguồn nhận:
- Level khó.
- Thử thách phụ.
- Thành tích đặc biệt.

Phần thưởng:
- Tăng cơ hội nhận trang bị hiếm.
- Có thể chứa các trang bị phù hợp với giai đoạn tiến trình hiện tại.

### Rương boss
Nguồn nhận:
- Đánh bại boss.

Phần thưởng:
- Trang bị có bảng rơi riêng.
- Trang bị đặc trưng của boss.
- Nguyên liệu hiếm hoặc phần thưởng tiến trình.

Khuyến nghị: lần đầu đánh bại boss quan trọng nên bảo đảm một phần thưởng có giá trị, chẳng hạn mở khóa hoặc trao trang bị đặc trưng.

### Rương đặc biệt
Nguồn nhận:
- Thành tựu.
- Mốc tiến trình.
- Sự kiện hoặc thử thách đặc biệt.

Phần thưởng:
- Trang bị độc quyền.
- Phần thưởng bộ sưu tập.
- Vật phẩm ngoại hình nếu sau này có hệ thống cosmetic.

## 5.3. Tỷ lệ độ hiếm thử nghiệm

Các tỷ lệ dưới đây chỉ là giá trị khởi điểm để thử nghiệm. Chưa phải thông số cân bằng cuối cùng.

Giả định mỗi rương tạo ra một trang bị, sau đó chọn độ hiếm theo bảng:

| Độ hiếm | Rương thường | Rương tinh anh | Rương boss |
|---|---:|---:|---:|
| Thường | 70% | 40% | 20% |
| Hiếm | 25% | 45% | 45% |
| Sử thi | 4% | 13% | 28% |
| Huyền thoại | 1% | 2% | 7% |
| Tổng | 100% | 100% | 100% |

Các tỷ lệ cần được kiểm thử theo tốc độ người chơi nhận trang bị mới, độ khó level và thời gian cần để hoàn thành một bộ trang bị.

Nếu một rương chứa nhiều món, cần thiết kế xác suất riêng cho số lượt rơi và từng loại phần thưởng.

## 5.4. Quy tắc phần thưởng

- Lần đầu đánh bại boss quan trọng nên có phần thưởng bảo đảm.
- Rương cần có nguồn nhận rõ ràng.
- Không nên bắt người chơi dựa hoàn toàn vào may mắn để mở khóa nội dung cần thiết cho tiến trình.
- Trang bị mới phải được đánh dấu trong kho.
- Trang bị trùng không nên hoàn toàn vô dụng.
- Bảng rơi cần có thể điều chỉnh mà không phải sửa logic chiến đấu.

## 5.5. Luồng mở rương

1. Người chơi hoàn thành level.
2. Màn hình kết quả hiển thị thành tích và loại rương nhận được.
3. Người chơi mở rương.
4. Trang bị xuất hiện cùng tên, độ hiếm và thông tin chính.
5. Nếu là món mới, hệ thống đánh dấu đã khám phá và thêm vào kho.
6. Nếu là bản sao, người chơi có thể giữ hoặc phân giải sau.
7. Người chơi quay về Home hoặc tiếp tục level tiếp theo.

Đối với MVP, nên mở rương ngay sau khi hoàn thành level để tạo cảm giác phần thưởng tức thời.

---

# 6. PHÂN GIẢI TRANG BỊ DƯ THỪA

## 6.1. Mục tiêu

Phân giải, hay Salvage, cho phép chuyển trang bị không cần dùng thành nguyên liệu ngoài trận.

Hệ thống này giải quyết ba vấn đề:
- Trang bị cấp thấp có thể trở nên ít hữu ích về sau.
- Trang bị trùng có thể gây đầy kho.
- Phần thưởng không phù hợp với build hiện tại dễ tạo cảm giác vô dụng.

## 6.2. Giá trị phân giải thử nghiệm

| Độ hiếm | Nguyên liệu cơ bản đề xuất |
|---|---:|
| Thường | 1 |
| Hiếm | 3 |
| Sử thi | 8 |
| Huyền thoại | 20 |

Các giá trị này chỉ là ví dụ để bắt đầu cân bằng. Cần so sánh lượng nguyên liệu nhận được với chi phí nâng cấp trước khi chốt.

## 6.3. Quy tắc phân giải

- Người chơi có thể chọn từng món hoặc phân giải hàng loạt.
- Hiển thị lượng nguyên liệu sẽ nhận trước khi xác nhận.
- Không cho phân giải trang bị đang đeo nếu chưa tháo.
- Cho phép khóa trang bị để tránh thao tác nhầm.
- Trang bị mới hoặc độ hiếm cao cần xác nhận bổ sung.
- Trang bị mới khám phá nên được ghi nhận trong bộ sưu tập.
- Không tự động xóa một trang bị chưa sở hữu trước đó chỉ vì người chơi chọn phân giải hàng loạt.

## 6.4. Nguyên liệu dùng để làm gì?

Giai đoạn đầu:
- Dùng nguyên liệu để nâng cấp trang bị ngoài trận.
- Hoặc mở khóa một số cải tiến có giới hạn.

Giai đoạn sau:
- Dùng nguyên liệu để chế tạo một trang bị cụ thể.
- Giảm sự phụ thuộc vào may mắn từ rương.

Khuyến nghị MVP: bắt đầu với một loại nguyên liệu cơ bản. Chỉ thêm nguyên liệu chuyên biệt nếu nó tạo ra quyết định gameplay thực sự.

---

# 7. FUSE TRONG TRẬN VÀ NÂNG CẤP NGOÀI TRẬN

Hai hệ thống này phải có mục đích và vòng đời khác nhau.

## 7.1. Fuse trong trận

- Ghép vũ khí cùng loại và cùng cấp theo luật Fuse của game.
- Vũ khí sau khi ghép mạnh hơn trong lượt chơi hiện tại.
- Người chơi cần cân nhắc vị trí balo, số ô sử dụng và lợi ích của việc nâng cấp.
- Vũ khí sau Fuse không được lưu vào kho trang bị vĩnh viễn.
- Khi lượt chơi kết thúc, build Fuse được reset.

## 7.2. Nâng cấp ngoài trận

- Sử dụng trang bị sở hữu hoặc nguyên liệu ngoài trận.
- Tác động đến tiến trình lâu dài.
- Không sử dụng tiền chiến đấu.
- Có thể nâng chỉ số, mở đặc tính hoặc mở khóa biến thể nếu hệ thống sau này cần.
- Cần giới hạn mức ảnh hưởng để không làm mất giá trị của việc xây build trong trận.

| Tiêu chí | Fuse trong trận | Nâng cấp ngoài trận |
|---|---|---|
| Mục đích | Sức mạnh của lượt chơi hiện tại | Tiến trình lâu dài |
| Đầu vào | Vũ khí trong lượt chơi | Trang bị sở hữu, nguyên liệu meta |
| Thời hạn | Tạm thời | Vĩnh viễn |
| Quyết định chính | Giữ, đặt, ghép hay đổi build | Đầu tư vào trang bị nào |

---

# 8. CỬA HÀNG VÀ KINH TẾ TRONG TRẬN

Tiền chiến đấu được kiếm từ tiêu diệt quái, hoàn thành wave hoặc các sự kiện trong level.

Tiền này chỉ dùng cho lượt chơi hiện tại.

## 8.1. Mục đích cửa hàng

- Bán vũ khí để mở rộng lựa chọn chiến đấu.
- Cho người chơi cơ hội hoàn thiện build.
- Tạo quyết định giữa mua ngay và tiết kiệm.
- Bổ sung sự ngẫu nhiên có kiểm soát vào mỗi lượt chơi.

## 8.2. Nguyên tắc cân bằng

- Giá vũ khí phải phù hợp với lượng tiền có thể kiếm.
- Không nên để người chơi luôn đủ tiền mua mọi món.
- Không nên khiến người chơi liên tục thiếu tiền đến mức không thể thử build.
- Cửa hàng không nên xuất hiện quá thường xuyên, làm gián đoạn nhịp chiến đấu.
- Tiền trong trận không chuyển thành tiền tài khoản khi kết thúc level.

---

# 9. THIẾT KẾ QUÁI VẬT VÀ BOSS

## 9.1. Mỗi loại quái cần có vai trò riêng

### Quái cơ bản
- Tiếp cận và tấn công đơn giản.
- Tạo nhịp chiến đấu nền.
- Giúp người chơi làm quen với điều khiển.

### Quái nhanh
- Di chuyển nhanh, thường có lượng máu thấp.
- Buộc người chơi định vị và ưu tiên mục tiêu.

### Quái chống chịu
- Lượng máu cao, di chuyển chậm.
- Kiểm tra sát thương duy trì và khả năng xử lý đám đông.

### Quái tầm xa
- Tấn công từ xa hoặc tạo vùng nguy hiểm.
- Buộc người chơi di chuyển và lựa chọn mục tiêu.

### Quái hỗ trợ
- Cường hóa, hồi máu hoặc hỗ trợ quái khác.
- Tạo mục tiêu ưu tiên và làm thay đổi cách xử lý wave.

### Elite
- Có biến thể hành vi hoặc kỹ năng đặc biệt.
- Tạo nhịp cao trào trước boss.

### Boss
- Có dấu hiệu báo đòn rõ ràng.
- Có nhiều kiểu tấn công hoặc giai đoạn.
- Kiểm tra khả năng điều khiển, sinh tồn và hiệu quả build.
- Có thể gắn với phần thưởng trang bị đặc trưng.

## 9.2. Nguyên tắc tăng độ khó

Không chỉ tăng HP và sát thương.

Độ khó nên tăng qua:
- Hành vi quái mới.
- Đội hình quái kết hợp.
- Thời điểm và vị trí xuất hiện.
- Vùng nguy hiểm.
- Tốc độ và nhịp độ tấn công.
- Cơ chế boss.
- Yêu cầu người chơi thay đổi cách di chuyển hoặc ưu tiên mục tiêu.

---

# 10. BESTIARY: BỘ SƯU TẬP QUÁI VẬT

Bestiary lưu thông tin về những quái vật người chơi đã gặp và đánh bại.

## 10.1. Thông tin đề xuất

- Tên quái.
- Hình ảnh hoặc model.
- Mô tả ngắn.
- Số lần gặp.
- Số lần tiêu diệt.
- Kỹ năng và dấu hiệu nhận biết đòn tấn công.
- Khu vực hoặc wave xuất hiện.
- Mốc khám phá và thành tựu liên quan.

## 10.2. Mở khóa thông tin

- Gặp quái lần đầu: mở hồ sơ cơ bản.
- Đánh bại lần đầu: ghi nhận thành tựu khám phá.
- Đánh bại nhiều lần: có thể mở thêm thông tin hoặc thử thách liên quan.

Bestiary không chỉ là menu thống kê. Nó nên giúp người chơi hiểu thế giới, nhận biết hành vi quái và theo dõi thành quả.

---

# 11. HOME, PROFILE, WEAPONS VÀ BESTIARY

## 11.1. Home

Nội dung MVP:
- Nhân vật chính.
- Nút Play.
- Tài nguyên ngoài trận.
- Level hoặc khu vực hiện tại.
- Thông báo trang bị mới.
- Lối vào kho trang bị và các hệ thống chính.

Mục tiêu: giúp người chơi nhìn thấy bước tiếp theo và nhanh chóng vào trận.

## 11.2. Profile

Nội dung đề xuất:
- Level tài khoản hoặc cấp tiến trình.
- Số level đã hoàn thành.
- Level cao nhất.
- Số boss đã đánh bại.
- Số loại vũ khí đã khám phá.
- Số loại quái đã khám phá.
- Thành tựu cơ bản.

## 11.3. Weapons / Kho trang bị

Nội dung đề xuất:
- Danh sách trang bị sở hữu.
- Trang bị đang đeo.
- Chỉ số và hiệu ứng.
- Độ hiếm.
- Trạng thái khóa.
- Số lượng bản sao.
- Chức năng phân giải.
- Chức năng nâng cấp nếu đã triển khai.

## 11.4. Bestiary

Nội dung đề xuất:
- Danh sách quái đã gặp.
- Trạng thái khám phá.
- Số lần tiêu diệt.
- Mô tả hành vi.
- Thông tin khu vực xuất hiện.

## 11.5. Thứ tự ưu tiên UI

1. Home.
2. Màn hình kết quả level và mở rương.
3. Kho trang bị và loadout.
4. Phân giải.
5. Profile.
6. Bestiary.

Không nên xây UI phức tạp trước khi dữ liệu và lưu tiến trình hoạt động chính xác.

---

# 12. CẤU TRÚC LEVEL VÀ NHỊP ĐỘ

Một level có thể được tổ chức như sau:

1. Khởi đầu: giới thiệu mối đe dọa và cho người chơi ổn định build.
2. Wave tăng áp lực: kết hợp nhiều vai trò quái.
3. Intermission: cho phép mua sắm, chọn thưởng hoặc sắp xếp balo.
4. Elite hoặc sự kiện: tạo thử thách khác với wave thường.
5. Boss: kiểm tra kỹ năng và build.
6. Kết quả: thống kê, cấp rương nếu đủ điều kiện và lưu phần thưởng.

Nếu toàn bộ quái đã bị tiêu diệt, không nên bắt người chơi chờ đến hết đồng hồ. Wave tiếp theo hoặc kết quả level nên được kích hoạt khi điều kiện gameplay đã hoàn tất.

---

# 13. CÂN BẰNG GAMEPLAY

## 13.1. Chiến đấu

Theo dõi:
- Thời gian hạ quái.
- Sát thương người chơi nhận.
- Tỷ lệ chết theo wave.
- Thời gian hoàn thành level.
- Tỷ lệ người chơi sống sót đến boss.

## 13.2. Vũ khí

Theo dõi:
- Sát thương theo cấp.
- Cooldown.
- Số mục tiêu trúng đòn.
- Hiệu quả hiệu ứng đặc biệt.
- Tần suất người chơi chọn mỗi vũ khí.

Mục tiêu là tránh một vũ khí vượt trội trong mọi tình huống.

## 13.3. Fuse và balo

Theo dõi:
- Số lần Fuse mỗi level.
- Thời gian chờ nhận vũ khí.
- Số ô trống còn lại.
- Số lần người chơi không thể đặt vũ khí.
- Thời gian dành cho quản lý balo.

## 13.4. Kinh tế trong trận

Theo dõi:
- Tiền kiếm được.
- Số lần mua.
- Số món bị bỏ qua.
- Số lần người chơi không đủ tiền mua món mong muốn.

## 13.5. Rương và phân giải

Theo dõi:
- Tỷ lệ nhận trang bị mới.
- Số lượng trang bị trùng.
- Số nguyên liệu nhận được mỗi level.
- Thời gian cần để đạt một mốc nâng cấp.
- Tỷ lệ trang bị bị phân giải.

Không nên điều chỉnh HP quái, sát thương vũ khí, kinh tế và tỷ lệ rương cùng lúc trong một vòng kiểm thử. Hãy thay đổi từng nhóm để biết nguyên nhân tạo ra kết quả.

---

# 14. PHẠM VI MVP VÀ ROADMAP

## P0: Hoàn thiện nền tảng dữ liệu

- [ ] Tách Run State và Meta Progress.
- [ ] Tách tiền chiến đấu và tài nguyên tài khoản.
- [ ] Định nghĩa quy tắc reset sau level.
- [ ] Lưu kết quả thắng/thua.
- [ ] Đảm bảo phần thưởng không bị mất hoặc nhân đôi.

## P1: Hoàn thiện vòng lặp phần thưởng

- [ ] Rương sau level.
- [ ] Bảng rơi trang bị cơ bản.
- [ ] Kho trang bị.
- [ ] Trang bị và tháo trang bị.
- [ ] Phân giải trang bị dư.
- [ ] Một loại nguyên liệu cơ bản.
- [ ] Fuse trong trận hoạt động độc lập với kho vĩnh viễn.

## P2: Hoàn thiện meta-game

- [ ] Home.
- [ ] Màn hình kết quả và phần thưởng.
- [ ] Profile cơ bản.
- [ ] Bestiary cơ bản.
- [ ] Nâng cấp ngoài trận nếu nguyên liệu đã có mục đích sử dụng.
- [ ] Cập nhật giao diện khi nhận trang bị mới.

## P3: Mở rộng sau khi kiểm chứng

- [ ] Chế tạo trang bị cụ thể.
- [ ] Thành tựu và thử thách.
- [ ] Nhiều loại nguyên liệu.
- [ ] Nhiều khu vực và bảng rơi riêng.
- [ ] Rương hoặc phần thưởng sự kiện.
- [ ] Các hệ thống nâng cấp sâu hơn.

Không cần triển khai tất cả ngay. Ưu tiên là tạo ra một vòng lặp nhỏ nhưng hoàn chỉnh trước khi mở rộng.

---

# 15. MÔ HÌNH DỮ LIỆU ĐỀ XUẤT

## WeaponDefinition
Định nghĩa gốc của một loại vũ khí:
- ID.
- Tên.
- Loại.
- Footprint trong balo.
- Độ hiếm.
- Chỉ số nền.
- Hiệu ứng.
- Nguồn mở khóa.
- Icon và dữ liệu hiển thị.

## PlayerWeaponInventory
Dữ liệu sở hữu của người chơi:
- Weapon ID.
- Số lượng sở hữu.
- Cấp meta nếu có.
- Trạng thái khóa.
- Trạng thái đang trang bị.

## ChestDefinition
- ID.
- Loại rương.
- Nguồn nhận.
- Bảng độ hiếm.
- Số lượt rơi.
- Quy tắc bảo đảm lần đầu.

## LootTable
- Độ hiếm.
- Trọng số.
- Nhóm trang bị hợp lệ.
- Điều kiện khu vực hoặc boss.

## PlayerMetaProgress
- Vàng tài khoản.
- Nguyên liệu.
- Level cao nhất.
- Boss đã hạ.
- Thành tựu.
- Các mốc mở khóa.

## RunState
- Tiền trong trận.
- Vũ khí hiện có.
- Cấp Fuse.
- Wave hiện tại.
- HP hiện tại.
- Cửa hàng.
- Phần thưởng tạm thời.

## MonsterDefinition
- ID.
- Vai trò.
- HP.
- Sát thương.
- Tốc độ.
- Hành vi.
- Khu vực xuất hiện.
- Loot nếu có.

## BestiaryProgress
- ID quái.
- Số lần gặp.
- Số lần hạ.
- Trạng thái đánh bại lần đầu.

### Nguyên tắc kỹ thuật

WeaponDefinition là dữ liệu thiết kế gốc. PlayerWeaponInventory là dữ liệu sở hữu của người chơi.

Không nên lưu toàn bộ định nghĩa vũ khí vào save file của người chơi. Save chỉ nên lưu ID và trạng thái cần thiết để dễ cân bằng, cập nhật và sửa lỗi dữ liệu.

---

# 16. RỦI RO THIẾT KẾ

| Rủi ro | Tác động | Biện pháp |
|---|---|---|
| Quá nhiều hệ thống cùng lúc | Tăng thời gian phát triển và khó kiểm thử. | Triển khai theo P0, P1, P2, P3. |
| Rương tạo cảm giác vô dụng | Người chơi mất động lực. | Bảo đảm phần thưởng quan trọng và cho phép phân giải đồ trùng. |
| Trang bị ngoài trận quá mạnh | Build trong trận mất ý nghĩa. | Giới hạn sức mạnh nền và ưu tiên khác biệt lối chơi. |
| Nguyên liệu không có mục đích | Phân giải trở thành thao tác thừa. | Chỉ phát hành phân giải khi đã có cách dùng nguyên liệu. |
| Quái chỉ tăng HP | Game kéo dài nhưng không sâu hơn. | Thêm hành vi, đội hình quái và cơ chế boss. |
| UI nhiều nhưng thiếu dữ liệu | Tốn công mà không tạo giá trị. | Hoàn thiện dữ liệu và save trước khi mở rộng UI. |
| Phần thưởng quá ngẫu nhiên | Người chơi không thể dự đoán tiến trình. | Bảo đảm phần thưởng ở mốc quan trọng và kiểm soát bảng rơi. |

---

# 17. CÁC QUYẾT ĐỊNH CẦN XÁC NHẬN

Những nội dung sau vẫn là đề xuất, chưa phải quyết định cuối cùng:

1. **Rương mở ngay sau level hay lưu vào kho?**  
   Khuyến nghị MVP: mở ngay sau level.

2. **Trang bị ngoài trận có cấp nâng cấp riêng hay chỉ khác nhau về loại và độ hiếm?**  
   Khuyến nghị: giữ số tầng nâng cấp thấp ở giai đoạn đầu.

3. **Có bao nhiêu slot trang bị ngoài trận?**  
   Cần chốt dựa trên vai trò chiến đấu của từng slot.

4. **Boss có bảo đảm trang bị ở lần hạ đầu tiên không?**  
   Khuyến nghị: có với boss hoặc cột mốc quan trọng.

5. **Nguyên liệu phân giải dùng để nâng cấp hay chế tạo?**  
   Khuyến nghị: nâng cấp trước, chế tạo sau.

6. **Thua level có nhận rương không?**  
   Cần xác định rõ để tránh khai thác cơ chế farm bằng cách cố tình thua.

7. **Trang bị trùng có thể xuất hiện không?**  
   Có thể, nhưng phải có giá trị thông qua phân giải hoặc hệ thống nâng cấp sau này.

---

# 18. TIÊU CHÍ HOÀN THÀNH VÒNG LẶP ĐẦU TIÊN

- [ ] Người chơi hiểu khác biệt giữa tiền chiến đấu và tài nguyên tài khoản.
- [ ] Người chơi chọn được loadout trước khi bắt đầu level.
- [ ] Người chơi mua, nhặt, xếp và Fuse vũ khí trong trận.
- [ ] Kết thúc level không lưu nhầm build tạm thời.
- [ ] Người chơi nhận rương và thấy rõ trang bị mới hoặc bản sao.
- [ ] Người chơi có thể giữ, trang bị hoặc phân giải món đồ dư.
- [ ] Nguyên liệu nhận được có mục đích sử dụng rõ ràng.
- [ ] Boss và quái tạo thử thách bằng hành vi, không chỉ bằng chỉ số.
- [ ] Người chơi nhìn thấy tiến trình qua Home, kho trang bị hoặc hồ sơ.
- [ ] Vòng lặp tạo được lý do để người chơi tiếp tục level tiếp theo.

---

# KẾT LUẬN

Chiều sâu của APOCARROT không nên đến từ việc có thật nhiều menu hoặc loại tiền, mà từ mối liên kết giữa các quyết định.

- Trang bị ngoài trận tạo mục tiêu dài hạn.
- Rương tạo phần thưởng sau khi vượt màn.
- Phân giải biến đồ dư thành giá trị.
- Cửa hàng, balo và Fuse tạo quyết định chiến thuật trong trận.
- Quái và boss kiểm tra cách người chơi sử dụng những lựa chọn đó.
- Home, Profile, Weapons và Bestiary giúp người chơi nhìn thấy hành trình của mình.

**Ưu tiên hiện tại:** hoàn thiện một vòng lặp nhỏ nhưng trọn vẹn:

Chuẩn bị trang bị → chiến đấu và xây build → hạ boss hoặc hoàn thành level → nhận rương → quản lý trang bị → chuẩn bị level tiếp theo.

Sau khi vòng lặp này hoạt động ổn định và tạo được động lực chơi tiếp, mới mở rộng crafting, nhiệm vụ, sự kiện, nhiều khu vực và các hệ thống nâng cấp sâu hơn.
