using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace HutechNovel.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
            // Khởi tạo các Collection trong Constructor
            DanhDaus = new HashSet<DanhDau>();
            YeuThichs = new HashSet<YeuThich>();
            LichSuDocs = new HashSet<LichSuDoc>();
            DanhGias = new HashSet<DanhGia>();
            LuotXems = new HashSet<LuotXem>();
            DayTruyens = new HashSet<DayTruyen>();
            TheoDoiTruyens = new HashSet<TheoDoiTruyen>();
            BinhLuans = new HashSet<BinhLuan>();
            TruyenDaDangs = new HashSet<Truyen>();
        }

        public DateTime KhaiSinh { get; set; } = DateTime.Now;
        public int SoChuongDaDoc { get; set; } = 0;
        public int SoBinhLuan { get; set; } = 0;
        public int SoPhutDaDoc { get; set; } = 0;
        public int VeDaySach { get; set; } = 0;
        public int HutechXu { get; set; } = 0;
        public int DiemKinhNghiem { get; set; } = 0;
        public DateTime? NgayDiemDanhCuoi { get; set; }
        public string CaiDatMauNen { get; set; } = "Light";
        public string CaiDatFontChu { get; set; } = "Arial";
        public int CaiDatCoChu { get; set; } = 18;
        public string? HoTen { get; set; }
        public string? Avatar { get; set; }

        public virtual ICollection<DanhDau> DanhDaus { get; set; }
        public virtual ICollection<YeuThich> YeuThichs { get; set; }
        public virtual ICollection<LichSuDoc> LichSuDocs { get; set; }
        public virtual ICollection<DanhGia> DanhGias { get; set; }
        public virtual ICollection<LuotXem> LuotXems { get; set; }
        public virtual ICollection<DayTruyen> DayTruyens { get; set; }
        public virtual ICollection<TheoDoiTruyen> TheoDoiTruyens { get; set; }
        public virtual ICollection<BinhLuan> BinhLuans { get; set; }
        public virtual ICollection<Truyen> TruyenDaDangs { get; set; }
    }
}
