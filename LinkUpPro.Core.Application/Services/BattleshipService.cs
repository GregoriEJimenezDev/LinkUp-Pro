using AutoMapper;
using LinkUpPro.Core.Application.DTOs.Battleship;
using LinkUpPro.Core.Application.Interfaces.IServices;
using LinkUpPro.Core.Application.ViewModel.Game;
using LinkUpPro.Core.Application.ViewModel.Select;
using LinkUpPro.Core.Domain.DomainServices;
using LinkUpPro.Core.Domain.Entities;
using LinkUpPro.Core.Domain.Enum;
using LinkUpPro.Core.Domain.Exceptions;
using LinkUpPro.Core.Domain.Interfaces;
using LinkUpPro.Core.Application.Interfaces.Repositories;

namespace LinkUpPro.Core.Application.Services
{
    public class BattleshipService(
        IBattleshipGameRepository gameRepo,
        IShipRepository shipRepo,
        IAttackRepository attackRepo,
        IFriendshipRepository friendshipRepo,
        IUserService userService,
        IMapper mapper,
        IUnitOfWork unitOfWork,
        IShipPlacementDomainService placementService,
        IAttackDomainService attackDomainService,
        INotificationService notificationService) : IBattleshipService
    {
        private readonly IBattleshipGameRepository _gameRepo = gameRepo;
        private readonly IShipRepository _shipRepo = shipRepo;
        private readonly IAttackRepository _attackRepo = attackRepo;
        private readonly IFriendshipRepository _friendshipRepo = friendshipRepo;
        private readonly IUserService _userService = userService;
        private readonly IMapper _mapper = mapper;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IShipPlacementDomainService _placementService = placementService;
        private readonly IAttackDomainService _attackDomainService = attackDomainService;
        private readonly INotificationService _notificationService = notificationService;

        public async Task<BattleshipIndexViewModel> GetIndexAsync(string userId)
        {
            var activeGames = (await _gameRepo.GetActiveByUserIdAsync(userId)).DistinctBy(g => g.Id).ToList();
            var finishedGames = (await _gameRepo.GetFinishedByUserIdAsync(userId)).DistinctBy(g => g.Id).ToList();

            var activeDtos = await MapGamesAsync(activeGames);
            var finishedDtos = await MapGamesAsync(finishedGames);

            return new BattleshipIndexViewModel
            {
                ActiveGames = activeDtos.ToList(),
                FinishedGames = finishedDtos.ToList()
            };
        }

        public async Task<GameStatsDto> GetStatsAsync(string userId)
        {
            var allGames = await _gameRepo.GetByPlayerAsync(userId);
            var finishedGames = allGames.Where(g => g.Status == GameStatus.Finished).ToList();
            var totalFinished = finishedGames.Count;
            var won = finishedGames.Count(g => g.WinnerId == userId);

            var allAttacks = await _attackRepo.GetByAttackerAsync(userId);
            var totalAttacks = allAttacks.Count();
            var totalHits = allAttacks.Count(a => a.IsHit);

            return new GameStatsDto
            {
                TotalGames = allGames.Count(),
                WonGames = won,
                LostGames = totalFinished - won,
                WinRatio = totalFinished > 0 ? Math.Round((double)won / totalFinished * 100, 2) : 0,
                TotalAttacks = totalAttacks,
                TotalHits = totalHits,
                Accuracy = totalAttacks > 0 ? Math.Round((double)totalHits / totalAttacks * 100, 2) : 0
            };
        }

        public async Task<List<LeaderboardDto>> GetLeaderboardAsync()
        {
            var finishedGames = await _gameRepo.GetAllFinishedAsync();

            var userStats = new Dictionary<string, LeaderboardDto>();

            foreach (var game in finishedGames)
            {
                if (!userStats.ContainsKey(game.FirstPlayerId))
                {
                    var p1 = await _userService.GetUserBasicInfoAsync(game.FirstPlayerId);
                    userStats[game.FirstPlayerId] = new LeaderboardDto
                    {
                        UserId = p1?.Id ?? string.Empty,
                        Username = p1?.Username ?? string.Empty,
                        UserProfilePicture = p1?.ProfilePictureUrl,
                        TotalGames = 0,
                        WonGames = 0,
                        LostGames = 0,
                        Score = 0
                    };
                }

                if (!userStats.ContainsKey(game.SecondPlayerId))
                {
                    var p2 = await _userService.GetUserBasicInfoAsync(game.SecondPlayerId);
                    userStats[game.SecondPlayerId] = new LeaderboardDto
                    {
                        UserId = p2?.Id ?? string.Empty,
                        Username = p2?.Username ?? string.Empty,
                        UserProfilePicture = p2?.ProfilePictureUrl,
                        TotalGames = 0,
                        WonGames = 0,
                        LostGames = 0,
                        Score = 0
                    };
                }

                userStats[game.FirstPlayerId].TotalGames++;
                userStats[game.SecondPlayerId].TotalGames++;

                if (game.WinnerId == game.FirstPlayerId)
                {
                    userStats[game.FirstPlayerId].WonGames++;
                    userStats[game.FirstPlayerId].Score += 10;
                    userStats[game.SecondPlayerId].LostGames++;
                }
                else if (game.WinnerId == game.SecondPlayerId)
                {
                    userStats[game.SecondPlayerId].WonGames++;
                    userStats[game.SecondPlayerId].Score += 10;
                    userStats[game.FirstPlayerId].LostGames++;
                }
            }

            foreach (var stats in userStats.Values)
            {
                stats.WinRatio = stats.TotalGames > 0 ? Math.Round((double)stats.WonGames / stats.TotalGames * 100, 2) : 0;
            }

            return userStats.Values
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.WinRatio)
                .Take(50)
                .ToList();
        }

        public async Task<ServiceResult<int>> CreateGameAsync(string player1Id, string player2Id)
        {
            if (player1Id == player2Id)
                return ServiceResult<int>.Failure("No puedes crear un juego contigo mismo.");

            var friendship = await _friendshipRepo.GetByUsersAsync(player1Id, player2Id);
            if (friendship == null)
                return ServiceResult<int>.Failure("Debes ser amigo de este usuario para jugar.");

            var hasActive = await _gameRepo.HasActiveGameWithFriendAsync(player1Id, player2Id);
            if (hasActive)
                return ServiceResult<int>.Failure("Ya tienes un juego activo con este jugador.");

            var game = new BattleshipGame
            {
                FirstPlayerId = player1Id,
                SecondPlayerId = player2Id,
                CurrentTurnPlayerId = player1Id,
                Status = GameStatus.WaitingForOpponent,
                StartedAt = DateTime.UtcNow
            };

            await _gameRepo.AddAsync(game);
            await _unitOfWork.SaveChangesAsync();

            var inviterInfo = await _userService.GetUserBasicInfoAsync(player1Id);
            await _notificationService.CreateNotificationAsync(
                player2Id,
                "Invitación a jugar",
                $"{inviterInfo.Username} te ha invitado a una partida de Battleship.",
                "/Battleship/Index",
                NotificationType.GameInvitation
            );

            return ServiceResult<int>.Success(game.Id);
        }

        public async Task<SelectShipViewModel> GetPendingShipsAsync(int gameId, string playerId)
        {
            var game = await _gameRepo.GetWithDetailsAsync(gameId);
            ValidatePlayerInGame(game, playerId);
            var playerShips = game.Ships.Where(s => s.PlayerId == playerId).ToList();

            var allShipTypes = new[] { ShipType.size2, ShipType.size3A, ShipType.size3B, ShipType.size4, ShipType.size5 };
            var placedTypes = playerShips.Select(s => s.ShipType).ToHashSet();
            var pendingTypes = allShipTypes.Where(t => !placedTypes.Contains(t)).ToList();

            return new SelectShipViewModel
            {
                GameId = gameId,
                PendingShips = pendingTypes.Select(t => new ShipDto
                {
                    ShipType = t,
                    Size = Ship.ShipSizes[t]
                }).ToList(),
                PlacedShips = playerShips.Select(s => _mapper.Map<ShipDto>(s)).ToList()
            };
        }

        public async Task<SelectCellViewModel> GetBoardForPlacementAsync(int gameId, string playerId, string shipType)
        {
            if (!Enum.TryParse<ShipType>(shipType, out var st))
                throw new ArgumentException("Tipo de barco inválido.");

            var existingShips = await _shipRepo.GetByGameAndPlayerAsync(gameId, playerId);
            
            var game = await _gameRepo.GetWithDetailsAsync(gameId);
            ValidatePlayerInGame(game, playerId);

            var occupiedCells = existingShips.SelectMany(s => s.Cells)
                .Select(c => (c.Row, c.Column)).ToList();

            return new SelectCellViewModel
            {
                GameId = gameId,
                ShipType = shipType,
                ShipSize = Ship.ShipSizes[st],
                OccupiedCells = occupiedCells
            };
        }

        public async Task<ServiceResult> PlaceShipAsync(int gameId, string playerId, string shipType, int row, int col, ShipDirection direction)
        {
            var game = await _gameRepo.GetByIdAsync(gameId);
            if (game == null) return ServiceResult.Failure("Partida no encontrada.");
            ValidatePlayerInGame(game, playerId);

            if (!Enum.TryParse<ShipType>(shipType, out var st))
                return ServiceResult.Failure("Tipo de barco inválido.");

            if (game.Status != GameStatus.PlacingShips)
                return ServiceResult.Failure("El juego no está en fase de colocación.");

            var existingShips = await _shipRepo.GetByGameAndPlayerAsync(gameId, playerId);

            var cells = _placementService.CalculateCells(st, row, col, direction);
            if (cells == null)
                return ServiceResult.Failure("El barco queda fuera del tablero (12x12).");

            var existingShipTypes = existingShips.Select(s => s.ShipType).ToHashSet();
            if (existingShipTypes.Contains(st))
                return ServiceResult.Failure("Este tipo de barco ya ha sido colocado.");

            var occupied = existingShips.SelectMany(s => s.Cells)
                .Select(c => (c.Row, c.Column)).ToHashSet();

            if (_placementService.Overlaps(cells, occupied))
                return ServiceResult.Failure("Las celdas ya están ocupadas por otro barco.");

            var ship = new Ship
            {
                ShipType = st,
                Size = Ship.ShipSizes[st],
                GameId = gameId,
                PlayerId = playerId,
                Cells = cells.Select(c => new ShipCell { Row = c.Row, Column = c.Column }).ToList()
            };

            await _shipRepo.AddAsync(ship);

            var myCount = existingShips.Count() + 1;
            var otherPlayerId = game.FirstPlayerId == playerId ? game.SecondPlayerId : game.FirstPlayerId;
            var otherCount = await _shipRepo.CountByGameAndPlayerAsync(gameId, otherPlayerId);

            if (myCount >= 5 && otherCount >= 5)
                game.StartGame();

            await _unitOfWork.SaveChangesAsync();
            return ServiceResult.Success();
        }

        public async Task<AttackBoardViewModel> GetAttackBoardAsync(int gameId, string playerId)
        {
            var game = await _gameRepo.GetWithDetailsAsync(gameId);
            ValidatePlayerInGame(game, playerId);
            
            if (game.Status == GameStatus.InProgress && game.CheckTimeout(48))
            {
                game.Status = GameStatus.Finished;
                game.WinnerId = game.CurrentTurnPlayerId == game.FirstPlayerId ? game.SecondPlayerId : game.FirstPlayerId;
                game.FinishedAt = DateTime.UtcNow;
                await _gameRepo.UpdateAsync(game);
                await _unitOfWork.SaveChangesAsync();
            }

            var myAttacks = await _attackRepo.GetByGameAndAttackerAsync(gameId, playerId);

            var opponentId = game.FirstPlayerId == playerId ? game.SecondPlayerId : game.FirstPlayerId;
            var opponent = await _userService.GetUserBasicInfoAsync(opponentId);

            var myShips = await _shipRepo.GetWithCellsByGameAndPlayerAsync(gameId, playerId);
            var opponentAttacks = await _attackRepo.GetByGameAndAttackerAsync(gameId, opponentId);

            var opponentShips = await _shipRepo.GetWithCellsByGameAndPlayerAsync(gameId, opponentId);
            var opponentSunkenShips = opponentShips.Where(s => s.IsSunk).Select(s => _mapper.Map<ShipDto>(s)).ToList();

            return new AttackBoardViewModel
            {
                GameId = gameId,
                IsMyTurn = game.CurrentTurnPlayerId == playerId && game.Status == GameStatus.InProgress,
                IsFinished = game.Status == GameStatus.Finished || game.Status == GameStatus.Abandoned,
                OpponentUsername = opponent.Username,
                MyAttacks = myAttacks.Select(a => _mapper.Map<AttackDto>(a)).ToList(),
                MyShips = myShips.Select(s => _mapper.Map<ShipDto>(s)).ToList(),
                OpponentAttacks = opponentAttacks.Select(a => _mapper.Map<AttackDto>(a)).ToList(),
                OpponentSunkenShips = opponentSunkenShips
            };
        }

        public async Task<ServiceResult> AttackAsync(int gameId, string attackerId, int row, int col)
        {
            var game = await _gameRepo.GetWithDetailsForUpdateAsync(gameId);
            if (game == null) return ServiceResult.Failure("Partida no encontrada.");
            ValidatePlayerInGame(game, attackerId);

            if (game.Status == GameStatus.InProgress && game.CheckTimeout(48))
            {
                game.Status = GameStatus.Finished;
                game.WinnerId = game.CurrentTurnPlayerId == game.FirstPlayerId ? game.SecondPlayerId : game.FirstPlayerId;
                game.FinishedAt = DateTime.UtcNow;
                await _gameRepo.UpdateAsync(game);
                await _unitOfWork.SaveChangesAsync();
                return ServiceResult.Failure("El juego ha finalizado por inactividad del oponente.");
            }

            var turnValidation = _attackDomainService.ValidateTurn(game, attackerId);
            if (!turnValidation.Succeeded)
                return ServiceResult.Failure(turnValidation.ErrorMessage);

            var existingAttacks = game.Attacks.Where(a => a.AttackerId == attackerId);
            if (_attackDomainService.IsDuplicateAttack(existingAttacks, row, col))
                return ServiceResult.Failure("Ya atacaste esta celda.");

            var opponentId = game.FirstPlayerId == attackerId ? game.SecondPlayerId : game.FirstPlayerId;
            var opponentShips = game.Ships.Where(s => s.PlayerId == opponentId).ToList();

            var impact = _attackDomainService.EvaluateImpact(opponentShips, row, col);

            var attack = new Attack
            {
                GameId = gameId,
                AttackerId = attackerId,
                Row = row,
                Column = col,
                IsHit = impact.IsHit,
                AttackedAt = DateTime.UtcNow
            };

            game.Attacks.Add(attack);

            if (impact.IsHit && impact.HitCell != null)
                impact.HitCell.WasAttacked = true;

            if (impact.IsHit && impact.HitShip != null)
                impact.HitShip.IsSunk = _attackDomainService.CheckIfSunk(impact.HitShip);

            var isVictory = _attackDomainService.CheckVictory(opponentShips);

            if (isVictory)
            {
                game.WinnerId = attackerId;
                game.FinishedAt = DateTime.UtcNow;
                game.Status = GameStatus.Finished;
            }
            else
            {
                game.SwitchTurn();
            }

            game.LastAttackedAt = DateTime.UtcNow;

            try
            {
                await _unitOfWork.SaveChangesAsync();
            }
            catch (ConcurrencyException)
            {
                return ServiceResult.Failure("El juego fue modificado por otro jugador. Por favor intenta tu ataque de nuevo.");
            }

            return ServiceResult.Success();
        }

        public async Task<ServiceResult> SurrenderAsync(int gameId, string playerId)
        {
            var game = await _gameRepo.GetByIdAsync(gameId);
            if (game == null) return ServiceResult.Failure("Partida no encontrada.");
            ValidatePlayerInGame(game, playerId);

            if (game.Status == GameStatus.Finished)
                return ServiceResult.Failure("El juego ya ha finalizado.");

            game.SetSurrender(playerId);

            await _gameRepo.UpdateAsync(game);
            await _unitOfWork.SaveChangesAsync();

            return ServiceResult.Success();
        }

        public async Task<GameResultViewModel> GetResultAsync(int gameId, string playerId)
        {
            var game = await _gameRepo.GetWithDetailsAsync(gameId);
            ValidatePlayerInGame(game, playerId);

            var opponentId = game.FirstPlayerId == playerId ? game.SecondPlayerId : game.FirstPlayerId;
            var opponent = await _userService.GetUserBasicInfoAsync(opponentId);

            var myAttacks = game.Attacks.Where(a => a.AttackerId == playerId).ToList();
            var oppAttacks = game.Attacks.Where(a => a.AttackerId == opponentId).ToList();
            var myShips = game.Ships.Where(s => s.PlayerId == playerId).ToList();

            return new GameResultViewModel
            {
                GameId = gameId,
                OpponentUsername = opponent.Username,
                IWon = game.WinnerId == playerId,
                StartedAt = game.StartedAt,
                FinishedAt = game.FinishedAt,
                MyAttacks = myAttacks.Select(a => _mapper.Map<AttackDto>(a)).ToList(),
                OpponentAttacks = oppAttacks.Select(a => _mapper.Map<AttackDto>(a)).ToList(),
                MyShips = myShips.Select(s => _mapper.Map<ShipDto>(s)).ToList()
            };
        }

        public async Task CheckTimeoutsAsync()
        {
            var activeGames = await _gameRepo.GetAllActiveAsync();

            foreach (var game in activeGames)
            {
                if (!game.LastAttackedAt.HasValue)
                    continue;

                if ((DateTime.UtcNow - game.LastAttackedAt.Value).TotalHours >= 48)
                {
                    game.Status = GameStatus.Abandoned;
                    game.WinnerId = game.CurrentTurnPlayerId == game.FirstPlayerId
                        ? game.SecondPlayerId
                        : game.FirstPlayerId;
                    game.FinishedAt = DateTime.UtcNow;
                    await _gameRepo.UpdateAsync(game);
                }
            }

            await _unitOfWork.SaveChangesAsync();
        }

        private async Task<IEnumerable<GameDto>> MapGamesAsync(IEnumerable<BattleshipGame> games)
        {
            var dtos = new List<GameDto>();
            foreach (var game in games)
            {
                var dto = _mapper.Map<GameDto>(game);

                var p1 = await _userService.GetUserBasicInfoAsync(game.FirstPlayerId);
                var p2 = await _userService.GetUserBasicInfoAsync(game.SecondPlayerId);

                dto.Player1Username = p1.Username;
                dto.Player2Username = p2.Username;

                if (!string.IsNullOrEmpty(game.WinnerId))
                {
                    var winner = await _userService.GetUserBasicInfoAsync(game.WinnerId);
                    dto.WinnerUsername = winner.Username;
                }

                dto.HoursElapsed = Math.Round((DateTime.UtcNow - game.StartedAt).TotalHours, 1);
                dtos.Add(dto);
            }
            return dtos;
        }

        private void ValidatePlayerInGame(BattleshipGame game, string playerId)
        {
            if (game == null) throw new ArgumentNullException(nameof(game));
            if (game.FirstPlayerId != playerId && game.SecondPlayerId != playerId)
            {
                throw new UnauthorizedAccessException("No eres participante de esta partida.");
            }
        }
    }
}
