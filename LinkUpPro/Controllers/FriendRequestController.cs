using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LinkUpPro.Core.Application.Interfaces.IServices;

namespace LinkUpPro.Controllers
{
    [Authorize]
    public class FriendRequestController(
        IFriendRequestService friendRequestService,
        IUserService userService) : BaseController
    {
        private readonly IFriendRequestService _friendRequestService = friendRequestService;
        private readonly IUserService _userService = userService;

        public async Task<IActionResult> Index()
        {
            var userId = UserId;
            var vm = await _friendRequestService.GetRequestsAsync(userId);
            return View(vm);
        }

        public async Task<IActionResult> Add(string? search)
        {
            var userId = UserId;
            var vm = await _friendRequestService.GetAvailableUsersAsync(userId, search);
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Send(string receiverId)
        {
            var senderId = UserId;
            var result = await _friendRequestService.SendAsync(senderId, receiverId);

            if (!result.Succeeded)
            {
                TempData["Error"] = result.ErrorMessage;
            }
            else
            {
                TempData["Success"] = "Solicitud enviada correctamente.";
            }

            return RedirectToAction(nameof(Add));
        }

        [HttpGet]
        public IActionResult ConfirmAccept(int id) => View(id);

        [HttpPost]
        public async Task<IActionResult> Accept(int id)
        {
            var userId = UserId;
            var result = await _friendRequestService.AcceptAsync(id, userId);

            if (!result.Succeeded)
            {
                TempData["Error"] = result.ErrorMessage;
            }
            else
            {
                TempData["Success"] = "Usuario agregado como amigo correctamente.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult ConfirmReject(int id) => View(id);

        [HttpPost]
        public async Task<IActionResult> Reject(int id)
        {
            var userId = UserId;
            var result = await _friendRequestService.RejectAsync(id, userId);

            if (!result.Succeeded)
            {
                TempData["Error"] = result.ErrorMessage;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult ConfirmCancel(int id) => View(id);

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = UserId;
            var result = await _friendRequestService.DeleteAsync(id, userId);

            if (!result.Succeeded)
            {
                TempData["Error"] = result.ErrorMessage;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult ConfirmHide(int id) => View(id);

        [HttpPost]
        public async Task<IActionResult> RemoveFromHistory(int id)
        {
            var userId = UserId;
            var result = await _friendRequestService.RemoveFromHistoryAsync(id, userId);

            if (!result.Succeeded)
            {
                TempData["Error"] = result.ErrorMessage;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

