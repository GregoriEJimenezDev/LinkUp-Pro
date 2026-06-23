using LinkUpPro.Core.Application.DTOs.Battleship;
using LinkUpPro.Core.Application.Interfaces.IServices;
using LinkUpPro.Core.Application.ViewModel.Game;
using LinkUpPro.Core.Application.ViewModel.Select;
using LinkUpPro.Core.Domain.Entities;
using LinkUpPro.Core.Domain.Enum;
using LinkUpPro.Core.Domain.Interfaces;

namespace LinkUpPro.Core.Application.Interfaces.Services
{
    public class BattleshipService(IBattleshipGameRepository gameRepository, IShipRepository shipRepository, 
        IAttackRepository attackRepository, IFriendshipRepository friendshipRepository, IUserService userService) : IBattleshipService
    {
        private readonly IBattleshipGameRepository _gameRepository = gameRepository;
        private readonly IShipRepository _shipRepository = shipRepository;
        private readonly IAttackRepository _attackRepository = attackRepository;
        private readonly IFriendshipRepository _friendshipRepository = friendshipRepository;
        private readonly IUserService _userService = userService;
        #region dictionaries
        private static readonly Dictionary<ShipType, int> ShipSizes = new()
        {
            { ShipType.size2, 2 },
            { ShipType.size3A,3 },
            { ShipType.size3B,3 },
            { ShipType.size4, 4 },
            { ShipType.size5, 5 }
        };
        #endregion

        public async Task<ServiceResult> AttackAsync(int gameId, string attackerId, int row, int col)
        {
            var game = await _gameRepository.GetWithDetailsAsync(gameId);
            if (game.Status == GameStatus.PlacingShips)
            {
                var opponent = game.FirstPlayerId == attackerId ? game.SecondPlayerId : game.FirstPlayerId;
                var opponentInfo = await _userService.GetUserBasicInfoAsync(opponent);

                return ServiceResult.Failure($"El juego todavía está en la fase de colocación de barcos. Por favor, " +
                    $"espera a que {opponentInfo.Username} termine de colocar los suyos.");
            }

            if (game.Status != GameStatus.InProgress) return ServiceResult.Failure("El juego ya ha terminado o no se encuentra en progreso.");

            if (game.CurrentTurnPlayerId != attackerId) return ServiceResult.Failure("No es tu turno.");

            var alreadyAttacked = await _attackRepository.CellAlreadyAttackedAsync(gameId, attackerId, row, col);

            if (alreadyAttacked) return ServiceResult.Failure("Ya has atacado esta casilla. Por favor, elige una diferente.");

            var opponentId = game.FirstPlayerId == attackerId ? game.SecondPlayerId : game.FirstPlayerId;

            var opponentShips = await _shipRepository.GetWithCellsByGameAndPlayerAsync(gameId, opponentId);

            Ship? hitShip = null;
            ShipCell? hitCell = null;

            foreach (var ship in opponentShips)
            {
                var cell = ship.Cells.FirstOrDefault(c => c.Row == row && c.Column == col);
                if (cell != null)
                {
                    hitShip = ship;
                    hitCell = cell;
                    break;
                }
            }
            bool isHit = hitCell != null;
            if (isHit)
            {
                hitCell!.WasAttacked = true;
                await _shipRepository.UpdateAsync(hitShip!);

                if (hitShip!.Cells.All(c => c.WasAttacked))
                {
                    hitShip.IsSunk = true;
                    await _shipRepository.UpdateAsync(hitShip);
                }
                var updatedShips = await _shipRepository.GetWithCellsByGameAndPlayerAsync(gameId, opponentId);
                if (updatedShips.All(s => s.IsSunk))
                {
                    await _attackRepository.AddAsync(new Attack
                    {
                        GameId = gameId,
                        AttackerId = attackerId,
                        Row = row,
                        Column = col,
                        IsHit = true,
                        AttackedAt = DateTime.UtcNow
                    });

                    game.Status = GameStatus.Finished;
                    game.WinnerId = attackerId;
                    game.FinishedAt = DateTime.UtcNow;
                    await _gameRepository.UpdateAsync(game);

                    return ServiceResult.Success();
                }
            }
            await _attackRepository.AddAsync(new Attack
            {
                GameId = gameId,
                AttackerId = attackerId,
                Row = row,
                Column = col,
                IsHit = isHit,
                AttackedAt = DateTime.UtcNow
            });

            game.CurrentTurnPlayerId = opponentId;
            game.LastAttackedAt = DateTime.UtcNow;
            await _gameRepository.UpdateAsync(game);

            return ServiceResult.Success();
        }
        public async Task<ServiceResult<int>> CreateGameAsync(string player1Id, string player2Id)
        {
            var hasActive = await _gameRepository.HasActiveGameWithFriendAsync(player1Id, player2Id);
            if (hasActive) return ServiceResult<int>.Failure("Ya tienes una partida activa con este amigo.");
            var game = new BattleshipGame
            {
                FirstPlayerId = player1Id,
                SecondPlayerId = player2Id,
                CurrentTurnPlayerId = player1Id,
                Status = GameStatus.PlacingShips,
                StartedAt = DateTime.UtcNow
            };
            await _gameRepository.AddAsync(game);
            return ServiceResult<int>.Success(game.Id);
        }
        public async Task<AttackBoardViewModel> GetAttackBoardAsync(int gameId, string playerId)
        {
            var game = await _gameRepository.GetWithDetailsAsync(gameId);
            if (game == null || game.Status == GameStatus.Finished) return null!;

            await CheckSingleGameTimeoutAsync(game);

            if (game.Status == GameStatus.PlacingShips) 
            {
                var placedShips = (await _shipRepository.GetByGameAndPlayerAsync(gameId, playerId)).Count();
                if (placedShips < ShipSizes.Count)
                    return null!;
            }

            var opponentId = game.FirstPlayerId == playerId ? game.SecondPlayerId : game.FirstPlayerId;
            var opponentInfo = await _userService.GetUserBasicInfoAsync(opponentId);
            var currentInfoTurn = await _userService.GetUserBasicInfoAsync(game.CurrentTurnPlayerId!);

            var myAttacks = game.Attacks
                .Where(a => a.AttackerId == playerId)
                .Select(a => new AttackDto
                {
                    Row = a.Row,
                    Column = a.Column,
                    IsHit = a.IsHit,
                    AttackerId = a.AttackerId!,
                    AttackedAt = a.AttackedAt
                }).ToList();
            return new AttackBoardViewModel
            {
                GameId = gameId,
                OpponentUsername = opponentInfo.Username,
                CurrentTurnUsername = currentInfoTurn.Username,
                MyAttacks = myAttacks,
                IsMyTurn = game.CurrentTurnPlayerId == playerId
            };
        }
        public async Task<SelectCellViewModel> GetBoardForPlacementAsync(int gameId, string playerId, string shipType)
        {
            var ship = await _shipRepository.GetWithCellsByGameAndPlayerAsync(gameId, playerId);
            var occupied = ship
                .SelectMany(s => s.Cells)
                .Select(c => (c.Row, c.Column))
                .ToList();
            var type = Enum.Parse<ShipType>(shipType);

            return new SelectCellViewModel
            {
                GameId = gameId,
                ShipType = shipType,
                ShipSize = ShipSizes[type],
                OccupiedCells = occupied
            };
        }
        public async Task<BattleshipIndexViewModel> GetIndexAsync(string userId)
        {
            await CheckTimeoutsAsync(userId);

            var active = await _gameRepository.GetActiveByUserIdAsync(userId);
            var finished = await _gameRepository.GetFinishedByUserIdAsync(userId);

            var activeDtos = new List<GameDto>();
            foreach (var g in active)
            {
                var dto = await MapGameDto(g, userId);
                activeDtos.Add(dto);
            }

            var finishedDtos = new List<GameDto>();
            foreach (var g in finished)
            {
                var dto = await MapGameDto(g, userId);
                finishedDtos.Add(dto);
            }

            return new BattleshipIndexViewModel
            {
                ActiveGames = activeDtos,
                FinishedGames = finishedDtos,
                TotalGames = finishedDtos.Count,
                WonGames = finishedDtos.Count(g => g.WinnerId == userId),
                LostGames = finishedDtos.Count(g => g.WinnerId != userId)
            };
        }
        public async Task<SelectShipViewModel> GetPendingShipsAsync(int gameId, string playerId)
        {
            var placedShipsEntities = await _shipRepository.GetWithCellsByGameAndPlayerAsync(gameId, playerId);
            var placedDtos = placedShipsEntities.Select(s => new ShipDto
            {
                Id = s.Id,
                ShipType = s.ShipType,
                Size = s.Size,
                IsSunk = s.IsSunk,
                PlayerId = s.PlayerId!,
                Cells = [.. s.Cells.Select(c => new CellDto
                {
                    Row = c.Row,
                    Column = c.Column,
                    WasAttacked = c.WasAttacked
                })]
            }).ToList();

            var placedTypes = placedDtos.Select(s => s.ShipType).ToHashSet();

            var pending = ShipSizes.Where(s => !placedTypes.Contains(s.Key))
                .Select(s => new ShipDto
                {
                    ShipType = s.Key,
                    Size = s.Value,
                    PlayerId = playerId
                }).ToList();
            return new SelectShipViewModel
            {
                GameId = gameId,
                PendingShips = pending,
                PlacedShips = placedDtos
            };
        }
        public async Task<GameResultViewModel> GetResultAsync(int gameId, string playerId)
        {
            var game = await _gameRepository.GetWithDetailsAsync(gameId);
            var opponentId = game.FirstPlayerId == playerId ? game.SecondPlayerId : game.FirstPlayerId;
            var opponentInfo = await _userService.GetUserBasicInfoAsync(opponentId);
            var myAttacks = game.Attacks
                .Where(a => a.AttackerId == playerId)
                .Select(a => new AttackDto
                {
                    Row = a.Row,
                    Column = a.Column,
                    IsHit = a.IsHit,
                    AttackerId = a.AttackerId!,
                    AttackedAt = a.AttackedAt
                }).ToList();
            var opponentAttacks = game.Attacks
               .Where(a => a.AttackerId == opponentId)
               .Select(a => new AttackDto
               {
                   Row = a.Row,
                   Column = a.Column,
                   IsHit = a.IsHit,
                   AttackerId = a.AttackerId!,
                   AttackedAt = a.AttackedAt
               }).ToList();

            var myShips = game.Ships
                .Where(s => s.PlayerId == playerId)
                .Select(s => new ShipDto
                {
                    Id = s.Id,
                    ShipType = s.ShipType,
                    Size = s.Size,
                    IsSunk = s.IsSunk,
                    PlayerId = s.PlayerId!,
                    Cells = s.Cells.Select(c => new CellDto
                    {
                        Row = c.Row,
                        Column = c.Column,
                        WasAttacked = c.WasAttacked
                    }).ToList()
                }).ToList();
            return new GameResultViewModel
            {
                GameId = gameId,
                OpponentUsername = opponentInfo.Username,
                IWon = game.WinnerId == playerId,
                StartedAt = game.StartedAt,
                FinishedAt = game.FinishedAt,
                MyAttacks = myAttacks,
                OpponentAttacks = opponentAttacks,
                MyShips = myShips
            };
        }
        public async Task<ServiceResult> PlaceShipAsync(int gameId, string playerId, string shipType, int row, int col, ShipDirection direction)
        {
            var type = Enum.Parse<ShipType>(shipType);
            var size = ShipSizes[type];
            var cells = CalculateCells(row, col, size, direction);

            if (cells.Any(c => c.Row < 0 || c.Row > 11 || c.Col < 0 || c.Col > 11))
                return ServiceResult.Failure("La ubicación del barco está fuera de los límites del tablero. " +
                "Debes cambiar o modificar la fila o la dirección seleccionada.");

            var existingShips = await _shipRepository.GetWithCellsByGameAndPlayerAsync(gameId, playerId);
            var occupied = existingShips
                .SelectMany(s => s.Cells)
                .Select(c => (c.Row, c.Column))
                .ToList();

            if (!IsValidPlacement(cells, occupied))
            {
                if (cells.Any(c => c.Row < 0 || c.Row > 11 || c.Col < 0 || c.Col > 11))
                    return ServiceResult.Failure("La ubicación del barco está fuera de los límites. " +
                        "Por favor, cambia la casilla de inicio o la dirección para que quepa dentro de la cuadrícula de 12x12.");

                return ServiceResult.Failure("El barco se superpone con un barco ya existente. " +
                    "Por favor, elige una ubicación o dirección diferente.");
            }

            var ship = new Ship
            {
                GameId = gameId,
                PlayerId = playerId,
                ShipType = type,
                Size = size,
                IsSunk = false,
                Cells = [.. cells.Select(c => new ShipCell
                {
                    Row = c.Row,
                    Column = c.Col,
                    WasAttacked = false
                })]
            };
            await _shipRepository.AddAsync(ship);

            var game = await _gameRepository.GetByIdAsync(gameId);
            var player1Done = await _shipRepository.PlayerFinishedPlacingAsync(gameId, game.FirstPlayerId);
            var player2Done = await _shipRepository.PlayerFinishedPlacingAsync(gameId, game.SecondPlayerId);

            if (player1Done && player2Done)
            {
                game.Status = GameStatus.InProgress;
                game.LastAttackedAt = DateTime.UtcNow;
                await _gameRepository.UpdateAsync(game);
            }

            return ServiceResult.Success();
        }
        public async Task<ServiceResult> SurrenderAsync(int gameId, string playerId)
        {
            var game = await _gameRepository.GetByIdAsync(gameId);
            if (game == null)
                return ServiceResult.Failure("Partida no encontrada.");
            if (game.FirstPlayerId != playerId && game.SecondPlayerId != playerId)
                return ServiceResult.Failure("No eres un jugador en esta partida.");

            game.Status = GameStatus.Finished;
            game.WinnerId = game.FirstPlayerId == playerId ? game.SecondPlayerId : game.FirstPlayerId;
            game.FinishedAt = DateTime.UtcNow;

            await _gameRepository.UpdateAsync(game);
            return ServiceResult.Success();
        }
        
        #region private helpers
        private async Task CheckTimeoutsAsync(string userId) 
        {
            var activeGames = await _gameRepository.GetActiveByUserIdAsync(userId);
            foreach (var game in activeGames.Where(g => g.Status == GameStatus.InProgress))
                await CheckSingleGameTimeoutAsync(game);
        }
        private async Task CheckSingleGameTimeoutAsync(BattleshipGame game)
        {
            if (game.Status != GameStatus.InProgress) return;
            if (!game.LastAttackedAt.HasValue) return;

            var hours = (DateTime.UtcNow - game.LastAttackedAt.Value).TotalHours;
            if (hours > 48)
            {
                game.Status = GameStatus.Finished;
                game.WinnerId = game.CurrentTurnPlayerId == game.FirstPlayerId
                    ? game.SecondPlayerId
                    : game.FirstPlayerId;
                game.FinishedAt = DateTime.UtcNow;
                await _gameRepository.UpdateAsync(game);
            }
        }
        private static List<(int Row, int Col)> CalculateCells( int startRow, int startCol, int size, ShipDirection direction)
        {
            var cells = new List<(int, int)>();
            for (int i = 0; i < size; i++)
            {
                cells.Add(direction switch
                {
                    ShipDirection.Up => (startRow - i, startCol),
                    ShipDirection.Down => (startRow + i, startCol),
                    ShipDirection.Left => (startRow, startCol - i),
                    ShipDirection.Right => (startRow, startCol + i),
                    _ => throw new ArgumentException("Dirección inválida")
                });
            }
            return cells;
        }
        private async Task<GameDto> MapGameDto(BattleshipGame game, string userId)
        {
            var opponentId = game.FirstPlayerId == userId
                ? game.SecondPlayerId
                : game.FirstPlayerId;

            var opponentInfo = await _userService.GetUserBasicInfoAsync(opponentId);
            var player1Info = await _userService.GetUserBasicInfoAsync(game.FirstPlayerId);
            var player2Info = await _userService.GetUserBasicInfoAsync(game.SecondPlayerId);

            string? winnerUsername = null;
            if (game.WinnerId != null)
            {
                var winnerInfo = await _userService.GetUserBasicInfoAsync(game.WinnerId);
                winnerUsername = game.WinnerId == userId ? "I" : winnerInfo.Username;
            }

            return new GameDto
            {
                Id = game.Id,
                Player1Id = game.FirstPlayerId,
                Player1Username = player1Info.Username,
                Player2Id = game.SecondPlayerId,
                Player2Username = player2Info.Username,
                CurrentTurnPlayerId = game.CurrentTurnPlayerId,
                Status = game.Status,
                StartedAt = game.StartedAt,
                FinishedAt = game.FinishedAt,
                WinnerId = game.WinnerId,
                WinnerUsername = winnerUsername,
                HoursElapsed = game.StartedAt != default ? (DateTime.UtcNow - game.StartedAt).TotalHours : 0
            };
        }
        private static bool IsValidPlacement(List<(int Row, int Col)> cells, List<(int Row, int Col)> occupiedCells)
        {
            if (cells.Any(c => c.Row < 0 || c.Row > 11 || c.Col < 0 || c.Col > 11))
                return false;
            if (cells.Any(c => occupiedCells.Contains(c)))
                return false;

            return true;
        }
        public Task CheckTimeoutsAsync()
        {
            return Task.CompletedTask;
        }

        #endregion
    }
}
