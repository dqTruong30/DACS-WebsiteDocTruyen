using System;
using HutechNovel.Models;

namespace HutechNovel.Helpers
{
    public class CultivationInfo
    {
        public string TenCanhGioi { get; set; } = string.Empty;
        public string TieuCanhGioi { get; set; } = string.Empty;
        public int CapBac { get; set; } // 1 -> 45
        public int ExpHienTai { get; set; }
        public int ExpYeuCauTieuCanhGioi { get; set; } // EXP required to pass current minor realm
        public int ExpTieuCanhGioiTruoc { get; set; } // EXP required to reach current minor realm
        public double PhanTramTieuCanhGioi { get; set; } // Progress % in current minor realm
        public string MauSac { get; set; } = "#ffffff";
    }

    public static class CultivationHelper
    {
        // Danh sách 15 Đại cảnh giới
        private static readonly string[] MajorRealms = new[]
        {
            "Luyện Khí", "Trúc Cơ", "Kết Đan", "Nguyên Anh", "Hóa Thần",
            "Luyện Hư", "Hợp Thể", "Đại Thừa", "Độ Kiếp", "Địa Tiên",
            "Nhân Tiên", "Thiên Tiên", "Kim Tiên", "Đại La Kim Tiên", "Hỗn Nguyên Đại La Kim Tiên"
        };

        private static readonly string[] Colors = new[]
        {
            "#a8a8a8", "#68b04d", "#3b82f6", "#8b5cf6", "#ec4899",
            "#f59e0b", "#10b981", "#ef4444", "#8b0000", "#eab308",
            "#06b6d4", "#6366f1", "#d946ef", "#f43f5e", "#fbbf24"
        };

        // Danh sách 3 Tiểu cảnh giới
        private static readonly string[] MinorRealms = new[] { "Tiền Kỳ", "Trung Kỳ", "Hậu Kỳ" };

        public static CultivationInfo CalculateCultivation(ApplicationUser user)
        {
            if (user == null) return new CultivationInfo { TenCanhGioi = "Phàm Nhân", MauSac = "#777777" };

            // Công thức EXP: Phút = 10 EXP, Chương = 50 EXP, BL = 100 EXP
            int totalExp = (user.SoPhutDaDoc * 10) + (user.SoChuongDaDoc * 50) + (user.SoBinhLuan * 100);

            // Base EXP để đạt Luyện Khí Tầng 1 là 0.
            // Công thức EXP cần cho cấp thứ N: BaseExp * (Multiplier ^ N)
            // Ta dùng công thức EXP đơn giản: Level * 1000 * (1.2 ^ Level)
            
            int currentLevel = 0;
            int maxLevel = MajorRealms.Length * MinorRealms.Length; // 15 * 3 = 45

            int expForCurrentLevel = 0;
            int expForNextLevel = 1000;

            for (int i = 1; i <= maxLevel; i++)
            {
                int requiredExp = (int)(1000 * i * Math.Pow(1.3, i));
                
                if (totalExp < requiredExp || i == maxLevel)
                {
                    currentLevel = i;
                    expForNextLevel = requiredExp;
                    break;
                }
                expForCurrentLevel = requiredExp;
            }

            int majorIndex = (currentLevel - 1) / 3;
            int minorIndex = (currentLevel - 1) % 3;

            if (majorIndex >= MajorRealms.Length)
            {
                majorIndex = MajorRealms.Length - 1;
                minorIndex = MinorRealms.Length - 1;
                expForNextLevel = totalExp; // Đạt đỉnh
            }

            string tenCanhGioi = MajorRealms[majorIndex];
            string tieuCanhGioi = MinorRealms[minorIndex];
            
            // Nếu là Hỗn Nguyên Đại La thì đổi màu vàng chóe
            string mauSac = Colors[majorIndex];

            double progress = 100.0;
            if (expForNextLevel > expForCurrentLevel)
            {
                progress = (double)(totalExp - expForCurrentLevel) / (expForNextLevel - expForCurrentLevel) * 100;
            }

            return new CultivationInfo
            {
                TenCanhGioi = tenCanhGioi,
                TieuCanhGioi = tieuCanhGioi,
                CapBac = currentLevel,
                ExpHienTai = totalExp,
                ExpTieuCanhGioiTruoc = expForCurrentLevel,
                ExpYeuCauTieuCanhGioi = expForNextLevel,
                PhanTramTieuCanhGioi = Math.Round(Math.Min(progress, 100.0), 2),
                MauSac = mauSac
            };
        }
    }
}
