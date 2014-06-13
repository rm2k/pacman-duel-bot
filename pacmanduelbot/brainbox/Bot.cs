﻿using pacmanduelbot.helpers;
using pacmanduelbot.models;
using System.Collections.Generic;
using System.Drawing;

namespace pacmanduelbot.brainbox
{
    class Bot
    {
        public Maze _maze { get; set; }
        private bool _DROP_PILL { get; set; }

        private Point _CURRENT_POSITION
        {
            get
            {
                return _maze.FindCoordinateOf(Guide._PLAYER_A);
            }
        }

        private Point _OPPONENT_POSITION
        {
            get
            {
                return _maze.FindCoordinateOf(Guide._PLAYER_B);
            }
        }

        /// <summary>
        /// Test
        /// </summary>

        public Maze _MakeMove()
        {
            if (!_CURRENT_POSITION.IsEmpty && !_OPPONENT_POSITION.IsEmpty)
            {
                var nextMove = Moves.MinMaxDecision(_maze, _CURRENT_POSITION, _OPPONENT_POSITION);
                _maze.SetSymbol(_CURRENT_POSITION.X, _CURRENT_POSITION.Y, Guide._EMPTY);
                _maze.SetSymbol(nextMove.X, nextMove.Y, Guide._PLAYER_A);
            }               
            else
            {
                var _opponent_position = new Point { X = Guide._RESPAWN_X, Y = Guide._RESPAWN_Y };
                var nextMove = Moves.MinMaxDecision(_maze, _CURRENT_POSITION, _opponent_position);
                _maze.SetSymbol(_CURRENT_POSITION.X, _CURRENT_POSITION.Y, Guide._EMPTY);
                _maze.SetSymbol(nextMove.X, nextMove.Y, Guide._PLAYER_A);
            }
            return _maze;
        }

        private int _UPPER_PILL_COUNT
        {
            get
            {
                var _pill_count = 0;
                for (var x = 0; x < Guide._TUNNEL; x++)
                {
                    for (var y = 0; y < Guide._WIDTH; y++)
                    {
                        var _symbol = _maze.GetSymbol(x, y);
                        if (_symbol.Equals(Guide._PILL))
                            _pill_count++;
                        if (_symbol.Equals(Guide._BONUS_PILL))
                            _pill_count = _pill_count + 10;
                    }
                }
                return _pill_count;
            }
        }

        private int _LOWER_PILL_COUNT
        {
            get
            {
                var _pill_count = 0;
                for (var x = Guide._TUNNEL + 1; x < Guide._HEIGHT; x++)
                {
                    for (var y = 0; y < Guide._WIDTH; y++)
                    {
                        var _symbol = _maze.GetSymbol(x, y);
                        if (_symbol.Equals(Guide._PILL))
                            _pill_count++;
                        if (_symbol.Equals(Guide._BONUS_PILL))
                            _pill_count = _pill_count + 10;
                    }
                }
                return _pill_count;
            }
        }

        public Maze MakeMove()
        {
            if (!_CURRENT_POSITION.IsEmpty)
            {
                var _next_position = NextMove();

                if (needSelfRespawn())
                    return SelfRespawn(_next_position);

                if (_DROP_PILL)
                    return MakeMoveAndDropPill(_next_position);

                _maze.SetSymbol(_CURRENT_POSITION.X,_CURRENT_POSITION.Y,Guide._EMPTY);
                _maze.SetSymbol(_next_position.X,_next_position.Y,Guide._PLAYER_A);
            }
            return _maze;
        }

        private Maze MakeMoveAndDropPill(Point _move)
        {
            _maze.SetSymbol(_CURRENT_POSITION.X,_CURRENT_POSITION.Y, Guide._POISON_PILL);
            _maze.SetSymbol(_move.X,_move.Y,Guide._PLAYER_A);
            PoisonInventory.DropPoisonPill();

            return _maze;
        }

        private Maze SelfRespawn(Point _move)
        {
            var _next = new Point();
            var list = Moves.GenerateMoves(_maze, _CURRENT_POSITION);
            foreach (var _point in list)
            {
                var _symbol = _maze.GetSymbol(_point.X,_point.Y);
                if (_symbol.Equals(Guide._POISON_PILL))
                {
                    _next = _point;
                    break;
                }
            }
            if (!_next.IsEmpty)
            {
                _maze.SetSymbol(_CURRENT_POSITION.X,_CURRENT_POSITION.Y,Guide._EMPTY);
                _maze.SetSymbol(_next.X,_next.Y,Guide._PLAYER_A);
                PoisonInventory.EmptyPoisonInventory();
                return _maze;
            }
            _maze.SetSymbol(_CURRENT_POSITION.X,_CURRENT_POSITION.Y,Guide._EMPTY);
            _maze.SetSymbol(_move.X, _move.Y, Guide._PLAYER_A);
            PoisonInventory.EmptyPoisonInventory();
            return _maze;
        }

        private bool needSelfRespawn()
        {
            return PoisonInventory.isSelfRespawn();
        }

        private Point NextMove()
        {
            var _move = new List<Point>();
            var _next_move = new Point();
            var list = new List<Point>();
            var _next = FindNearbyPill();
            var _symbol = _maze.GetSymbol(_next.X, _next.Y);
            if (_symbol.Equals(Guide._BONUS_PILL))
            {
                _move = Moves.BuildPath(_maze, _CURRENT_POSITION, _next);
                return _move[1];
            }

            //var _decide = Moves.BuildPath(_maze, _CURRENT_POSITION, _next);
            //if (_decide[0].X < 4)
              //  return Moves.MinMaxDecision(_maze, _CURRENT_POSITION, _OPPONENT_POSITION);

            var possibleMoveList = Moves.GenerateMoves(_maze, _CURRENT_POSITION);

            foreach (var _point in possibleMoveList)
            {
                _symbol = _maze.GetSymbol(_point.X,_point.Y);
                if (_symbol.Equals(Guide._PILL)
                    || _symbol.Equals(Guide._BONUS_PILL))
                    list.Add(_point);
            }

            switch (list.Count)
            {
                case 0:
                    _move = Moves.BuildPath(_maze, _CURRENT_POSITION, _next);
                    if (!PoisonInventory.arePoisonPillsExhausted()
                        && !(_CURRENT_POSITION.X == Guide._RESPAWN_X && _CURRENT_POSITION.Y == Guide._RESPAWN_Y)
                        && !(_CURRENT_POSITION.X == Guide._EXIT_UP_X && _CURRENT_POSITION.Y == Guide._EXIT_UP_Y)
                        && !(_CURRENT_POSITION.X == Guide._EXIT_DOWN_X && _CURRENT_POSITION.Y == Guide._EXIT_DOWN_Y))
                    {
                        var _gd = _move[0].X;
                        var _temp = Moves.BuildPath(_maze, new Point { X = Guide._RESPAWN_X, Y = Guide._RESPAWN_Y }, _next);
                        var _gr = _temp[0].X + 5;

                        if (_gr < _gd)
                            _DROP_PILL = true;
                     }
                    _next_move = _move[1];
                    break;
                case 1:
                    _next_move = list[0];
                    break;
                default:
                    //_next_move = Moves.MinMaxDecision(_maze, _CURRENT_POSITION, _OPPONENT_POSITION);
                    _next_move = Moves.ChoosePath(_maze, _CURRENT_POSITION, 100);
                    break;
            }
            return _next_move;
        }

        private Point FindNearbyPill()
        {
            var _next = new Point();
            var _open = new List<Point>();
            var _closed = new List<Point>();

            _open.Add(_CURRENT_POSITION);

            for (var x = 0; x < Guide._HEIGHT; x++)
            {
                for (var y = 0; y < Guide._WIDTH; y++)
                {
                    var _symbol = _maze.GetSymbol(x, y);
                    if (_symbol.Equals(Guide._BONUS_PILL))
                    {
                        _next = new Point { X = x, Y = y };
                        var _temp = Moves.BuildPath(_maze, _CURRENT_POSITION, _next);
                        var _tempg = _temp[0].X;
                        if (_tempg < 10)
                            return _next;
                    }
                }
            }

            //TODO: some intelligence
            //if
            if (_CURRENT_POSITION.X <= Guide._TUNNEL
                && _LOWER_PILL_COUNT > _UPPER_PILL_COUNT + 15)
            {
                while (_open.Count != 0)
                {
                    var _templist = Moves.GenerateMoves(_maze, _open[0]);
                    _closed.Add(_open[0]);
                    foreach (var _point in _templist)
                    {
                        var _symbol = _maze.GetSymbol(_point.X, _point.Y);
                        if (_symbol.Equals(Guide._BONUS_PILL)
                            || _symbol.Equals(Guide._PILL))
                        {
                            _next = _point;
                            if (_next.X > Guide._TUNNEL)
                                return _next;
                        }
                        if (!_closed.Contains(_point))
                            _open.Add(_point);
                    }
                    _open.Remove(_open[0]);
                }
            }

            //else
            if (_CURRENT_POSITION.X > Guide._TUNNEL
                && _UPPER_PILL_COUNT > _LOWER_PILL_COUNT + 15)
            {
                while (_open.Count != 0)
                {
                    var _templist = Moves.GenerateMoves(_maze, _open[0]);
                    _closed.Add(_open[0]);
                    foreach (var _point in _templist)
                    {
                        var _symbol = _maze.GetSymbol(_point.X, _point.Y);
                        if (_symbol.Equals(Guide._BONUS_PILL)
                            || _symbol.Equals(Guide._PILL))
                        {
                            _next = _point;
                            if (_next.X <= Guide._TUNNEL)
                                return _next;
                        }
                        if (!_closed.Contains(_point))
                            _open.Add(_point);
                    }
                    _open.Remove(_open[0]);
                }
            }

            //Otherwise
            while (_open.Count != 0)
            {
                var _templist = Moves.GenerateMoves(_maze, _open[0]);
                _closed.Add(_open[0]);
                foreach (var _point in _templist)
                {
                    var _symbol = _maze.GetSymbol(_point.X, _point.Y);
                    if (_symbol.Equals(Guide._BONUS_PILL)
                        || _symbol.Equals(Guide._PILL))
                    {
                        _next = _point;
                        return _next;
                    }
                    if (!_closed.Contains(_point))
                        _open.Add(_point);
                }
                _open.Remove(_open[0]);
            }
            return _next;
        }

        public int PillCount()
        {
            var _PILL_COUNT = 0;
            for (var x = 0; x < Guide._HEIGHT; x++)
            {
                for (var y = 0; y < Guide._WIDTH; y++)
                {
                    var _symbol = _maze.GetSymbol(x, y);
                    if (_symbol.Equals(Guide._PILL)
                        || _symbol.Equals(Guide._BONUS_PILL))
                        _PILL_COUNT++;
                }
            }
            return _PILL_COUNT;
        }
    }
}