// Areas/Admin/Controllers/BaseAdminController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HutechNovel.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public abstract class BaseAdminController : Controller
    {
        protected void SetSuccessMessage(string message) => TempData["SuccessMessage"] = message;
        protected void SetErrorMessage(string message) => TempData["ErrorMessage"] = message;
        protected void SetWarningMessage(string message) => TempData["WarningMessage"] = message;
    }

    /// <summary>
    /// Base dành cho các controller cho phép cả Uploader và Admin.
    /// </summary>
    [Area("Admin")]
    [Authorize(Roles = "Uploader,Admin")]
    public abstract class BaseUploaderController : Controller
    {
        protected void SetSuccessMessage(string message) => TempData["SuccessMessage"] = message;
        protected void SetErrorMessage(string message) => TempData["ErrorMessage"] = message;
        protected void SetWarningMessage(string message) => TempData["WarningMessage"] = message;
    }
}