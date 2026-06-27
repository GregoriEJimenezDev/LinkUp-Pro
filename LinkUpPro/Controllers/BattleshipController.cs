using LinkUpPro.Core.Application.Interfaces.IServices;
using LinkUpPro.Core.Domain.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkUpPro.Controllers
{
    [Authorize]
    public class BattleshipController : Controller
    {
        private readonly IBattleshipService _battleshipService;
        private readonly IUserService _userService;
        private readonly IFriendshipService _friendshipService;

        public BattleshipController(
            IBattleshipService battleshipService,
            IUserService userService,
            IFriendshipService friendshipService)
        {
            _battleshipService = battleshipService;
            _userService = userService;
            _friendshipService = friendshipService;
        }

        private string CurrentUserId =>
            _userService.GetUserIdByUsernameAsync(User.Identity!.Name!).GetAwaiter().GetResult();

        public async Task<IActionResult> Index()
        {
            var userId = await _userService.GetUserIdByUsernameAsync(User.Identity!.Name!);
            var vm = await _battleshipService.GetIndexAsync(userId);
            ViewBag.Stats = await _battleshipService.GetStatsAsync(userId);
            return View(vm);
        }

        public async Task<IActionResult> Create()
        {
            var userId = await _userService.GetUserIdByUsernameAsync(User.Identity!.Name!);
            var friends = await _friendshipService.GetFriendsAsync(userId);
            return View(friends);
        }

        [HttpPost]
        public async Task<IActionResult> Create(string friendId)
        {
            var userId = await _userService.GetUserIdByUsernameAsync(User.Identity!.Name!);
            var result = await _battleshipService.CreateGameAsync(userId, friendId);

            if (!result.Succeeded)
            {
                TempData["Error"] = result.ErrorMessage;
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(SelectShip), new { gameId = result.Data });
        }

        public async Task<IActionResult> SelectShip(int gameId)
        {
            var userId = await _userService.GetUserIdByUsernameAsync(User.Identity!.Name!);
            var vm = await _battleshipService.GetPendingShipsAsync(gameId, userId);
            return View(vm);
        }

        public async Task<IActionResult> PlaceShip(int gameId, string shipType)
        {
            var userId = await _userService.GetUserIdByUsernameAsync(User.Identity!.Name!);
            var vm = await _battleshipService.GetBoardForPlacementAsync(gameId, userId, shipType);
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> PlaceShip(int gameId, string shipType, int row, int col, ShipDirection direction)
        {
            var userId = await _userService.GetUserIdByUsernameAsync(User.Identity!.Name!);
            var result = await _battleshipService.PlaceShipAsync(gameId, userId, shipType, row, col, direction);

            if (!result.Succeeded)
            {
                TempData["Error"] = result.ErrorMessage;
                return RedirectToAction(nameof(PlaceShip), new { gameId, shipType });
            }

            return RedirectToAction(nameof(SelectShip), new { gameId });
        }

        public async Task<IActionResult> AttackBoard(int gameId)
        {
            var userId = await _userService.GetUserIdByUsernameAsync(User.Identity!.Name!);
            var vm = await _battleshipService.GetAttackBoardAsync(gameId, userId);
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Attack(int gameId, int row, int col)
        {
            var userId = await _userService.GetUserIdByUsernameAsync(User.Identity!.Name!);
            var result = await _battleshipService.AttackAsync(gameId, userId, row, col);

            if (!result.Succeeded)
            {
                TempData["Error"] = result.ErrorMessage;
                return RedirectToAction(nameof(AttackBoard), new { gameId });
            }

            var game = await _battleshipService.GetResultAsync(gameId, userId);

            if (game.IWon || game.FinishedAt.HasValue)
                return RedirectToAction(nameof(Result), new { gameId });

            return RedirectToAction(nameof(AttackBoard), new { gameId });
        }

        [HttpPost]
        public async Task<IActionResult> Surrender(int gameId)
        {
            var userId = await _userService.GetUserIdByUsernameAsync(User.Identity!.Name!);
            var result = await _battleshipService.SurrenderAsync(gameId, userId);

            if (!result.Succeeded)
                TempData["Error"] = result.ErrorMessage;

            return RedirectToAction(nameof(Result), new { gameId });
        }

        public async Task<IActionResult> Result(int gameId)
        {
            var userId = await _userService.GetUserIdByUsernameAsync(User.Identity!.Name!);
            var vm = await _battleshipService.GetResultAsync(gameId, userId);
            return View(vm);
        }

        public IActionResult Leaderboard()
        {
            return View();
        }
    }
}
