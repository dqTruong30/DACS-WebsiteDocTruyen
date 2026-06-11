using HutechNovel.Models;

namespace HutechNovel.Services;

public interface INhiemVuService
{
    Task EnsureDefaultNhiemVuAsync();
    Task<NhiemVuProgressResult> CapNhatTienDoAsync(string userId, string loaiDieuKien, int soLuong = 1);
}

public sealed class NhiemVuProgressResult
{
    public bool DaCapNhat { get; set; }
    public bool VuaHoanThanh { get; set; }
    public NhiemVu? NhiemVu { get; set; }
}
