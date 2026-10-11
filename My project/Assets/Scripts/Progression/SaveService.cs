using System;
using System.IO;
using UnityEngine;

namespace Apocarrot.Progression
{
    /// <summary>
    /// Dịch vụ lưu trữ dữ liệu an toàn cục bộ (Gate 7).
    /// Hỗ trợ nạp, lưu, xử lý lỗi corrupt file và khôi phục mặc định.
    /// </summary>
    public static class SaveService
    {
        private const string SAVE_FILE_NAME = "apocarrot_account_save.json";

        private static string SaveFilePath => Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);

        public static bool HasSaveFile()
        {
            try
            {
                return File.Exists(SaveFilePath);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SaveService] Lỗi kiểm tra file save: {ex.Message}");
                return false;
            }
        }

        public static bool Save(AccountData data)
        {
            if (data == null)
            {
                Debug.LogError("[SaveService] Dữ liệu AccountData null, hủy lưu.");
                return false;
            }

            try
            {
                string json = JsonUtility.ToJson(data, true);
                string tempPath = SaveFilePath + ".tmp";

                // Ghi vào file tạm trước để tránh bị hỏng file nếu crash giữa chừng
                File.WriteAllText(tempPath, json);

                if (File.Exists(SaveFilePath))
                {
                    File.Delete(SaveFilePath);
                }

                File.Move(tempPath, SaveFilePath);
                Debug.Log($"[SaveService] Đã lưu tiến trình tài khoản thành công! (Level: {data.RabbitLevel}, XP: {data.CurrentXP})");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveService] Không thể lưu file: {ex.Message}");
                return false;
            }
        }

        public static AccountData Load()
        {
            if (!HasSaveFile())
            {
                Debug.Log("[SaveService] Chưa có file save, tạo tài khoản mới mặc định.");
                var fresh = AccountData.CreateDefault();
                Save(fresh);
                return fresh;
            }

            try
            {
                string json = File.ReadAllText(SaveFilePath);
                if (string.IsNullOrEmpty(json))
                {
                    Debug.LogWarning("[SaveService] File save trống, tạo lại mặc định.");
                    var fallback = AccountData.CreateDefault();
                    Save(fallback);
                    return fallback;
                }

                AccountData data = JsonUtility.FromJson<AccountData>(json);
                if (data == null)
                {
                    Debug.LogWarning("[SaveService] Giải mã JSON thất bại, khôi phục mặc định.");
                    var fallback = AccountData.CreateDefault();
                    Save(fallback);
                    return fallback;
                }

                // Đảm bảo danh sách thành tựu không bị rỗng sau khi nâng cấp version
                if (data.Achievements == null || data.Achievements.Count == 0)
                {
                    data.InitializeDefaultAchievements();
                }

                Debug.Log($"[SaveService] Tải thành công tài khoản: {data.RabbitName} (Cấp {data.RabbitLevel}, Vàng TK: {data.PermanentGold})");
                return data;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveService] Lỗi đọc file save ({ex.Message}), khôi phục an toàn.");
                var fallback = AccountData.CreateDefault();
                Save(fallback);
                return fallback;
            }
        }

        public static void ResetSave()
        {
            try
            {
                if (File.Exists(SaveFilePath))
                {
                    File.Delete(SaveFilePath);
                }
                Debug.Log("[SaveService] Đã xóa dữ liệu save cục bộ thành công.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveService] Lỗi xóa file save: {ex.Message}");
            }
        }
    }
}
