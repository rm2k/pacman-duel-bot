﻿using pacmanduelbot.helpers;
using pacmanduelbot.models;
using pacmanduelbot.shared;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace pacmanduelbot.brainbox
{
    class Bot
    {
        public Maze _maze { get; set; }
        private bool _DROP_PILL { get; set; }
        
        public Maze MakeMove()
        {
            if (!_CURRENT_POSITION.IsEmpty)
            {
                var _next_position = BotMove();

                if (SelfRespawnNeeded())
                    return SelfRespawn(_next_position);

                if (_DROP_PILL)
                    return MakeMoveAndDropPill(_next_position);

                _maze.SetSymbol(_CURRENT_POSITION.X, _CURRENT_POSITION.Y, Symbols._EMPTY);
                _maze.SetSymbol(_next_position.X, _next_position.Y, Symbols._PLAYER_A);
            }
            return _maze;
        }

        private Maze MakeMoveAndDropPill(Point _move)
        {
            _maze.SetSymbol(_CURRENT_POSITION.X,_CURRENT_POSITION.Y, Symbols._POISON_PILL);
            _maze.SetSymbol(_move.X,_move.Y,Symbols._PLAYER_A);
            PoisonInventory.DropPoisonPill();

            return _maze;
        }

        private Maze SelfRespawn(Point _move)
        {
            var _next = new Point();
            var list = Moves.GenerateMoves(_maze, _CURRENT_POSITION);
            foreach (var _point in list)
            {
                if (_maze.GetSymbol(_point).Equals(Symbols._POISON_PILL))
                {
                    _next = _point;
                    break;
                }
            }
            if (!_next.IsEmpty)
            {
                _maze.SetSymbol(_CURRENT_POSITION.X,_CURRENT_POSITION.Y,Symbols._EMPTY);
                _maze.SetSymbol(_next.X,_next.Y,Symbols._PLAYER_A);
                PoisonInventory.EmptyPoisonInventory();
                return _maze;
            }
            _maze.SetSymbol(_CURRENT_POSITION.X,_CURRENT_POSITION.Y,Symbols._EMPTY);
            _maze.SetSymbol(_move.X, _move.Y, Symbols._PLAYER_A);
            PoisonInventory.EmptyPoisonInventory();
            return _maze;
        }

 

        private Point BotMove()
        {
            var _move = new List<Point>();            
            var _next = FindNearbyPill();
            
            if (_maze.GetSymbol(_next).Equals(Symbols._BONUS_PILL))
            {
                _move = Moves.FindPathToPill(_maze, _CURRENT_POSITION, _next);
                return _move[1];
            }

            //var _decide = Moves.BuildPath(_maze, _CURRENT_POSITION, _next);
            //if (_decide[0].X < 4)
              //  return Moves.MinMaxDecision(_maze, _CURRENT_POSITION, _OPPONENT_POSITION);

            var possibleMoveList = Moves.GenerateMoves(_maze, _CURRENT_POSITION);
            var list = new List<Point>();
            foreach (var _point in possibleMoveList)
            {
                if (_maze.GetSymbol(_point).Equals(Symbols._PILL))
                {
                    list.Add(_point);
                    if (list.Count > 1) break;
                }
            }

            switch (list.Count)
            {
                case 0:
                    _move = Moves.FindPathToPill(_maze, _CURRENT_POSITION, _next);
                    if (!PoisonInventory.ArePoisonPillsExhausted()
                        && !(_CURRENT_POSITION.X == Properties.Settings.Default._MazeRespawnX && _CURRENT_POSITION.Y == Properties.Settings.Default._MazeRespawnY)
                        && !(_CURRENT_POSITION.X == Properties.Settings.Default._MazeRespawnExitUpX && _CURRENT_POSITION.Y == Properties.Settings.Default._MazeRespawnExitUpY)
                        && !(_CURRENT_POSITION.X == Properties.Settings.Default._MazeRespawnExitDownX && _CURRENT_POSITION.Y == Properties.Settings.Default._MazeRespawnExitDownY))
                    {
                        var _gd = _move[0].X;
                        var _temp = Moves.FindPathToPill(_maze, new Point { X = Properties.Settings.Default._MazeRespawnX, Y = Properties.Settings.Default._MazeRespawnY }, _next);
                        var _gr = _temp[0].X + 5;

                        if (_gr < _gd)
                            _DROP_PILL = true;
                     }
                    return _move[1];
                case 1:
                    return list[0];
                default:
                    return Moves.PathSelect(_maze, _CURRENT_POSITION,1000);
            }
        }

        private Point FindNearbyPill()
        {
            var _next = new Point();
            var _open = new List<Point>
            {
                _CURRENT_POSITION
            };
            var _closed = new List<Point>();
            var _explored = new List<Point>();

            for (var x = 0; x < Properties.Settings.Default._MazeHeight; x++)
            {
                for (var y = 0; y < Properties.Settings.Default._MazeWidth; y++)
                {
                    if (_maze.GetSymbol(x, y).Equals(Symbols._BONUS_PILL))
                    {
                        _next = new Point { X = x, Y = y };
                        var _temp = Moves.FindPathToPill(_maze, _CURRENT_POSITION, _next);
                        var _tempg = _temp[0].X;
                        if (_tempg < 10)
                            return _next;
                    }
                }
            }

            //TODO: some intelligence
            //if
            if (_CURRENT_POSITION.X <= Properties.Settings.Default._MazeTunnel
                && _LOWER_PILL_COUNT > _UPPER_PILL_COUNT + 15)
            {
                while (_open.Count != 0)
                {
                    var _templist = Moves.GenerateMoves(_maze, _open[0]);
                    _closed.Add(_open[0]);
                    foreach (var _point in _templist)
                    {
                        var _symbol = _maze.GetSymbol(_point);
                        if (_symbol.Equals(Symbols._BONUS_PILL)
                            || _symbol.Equals(Symbols._PILL))
                        {
                            if (_point.X > Properties.Settings.Default._MazeTunnel)
                                return _point;
                        }
                        if (!_closed.Contains(_point))
                            _open.Add(_point);
                    }
                    _open.Remove(_open[0]);
                }
            }

            //else
            if (_CURRENT_POSITION.X > Properties.Settings.Default._MazeTunnel
                && _UPPER_PILL_COUNT > _LOWER_PILL_COUNT + 15)
            {
                while (_open.Count != 0)
                {
                    var _templist = Moves.GenerateMoves(_maze, _open[0]);
                    _closed.Add(_open[0]);
                    foreach (var _point in _templist)
                    {
                        var _symbol = _maze.GetSymbol(_point);
                        if (_symbol.Equals(Symbols._BONUS_PILL)
                            || _symbol.Equals(Symbols._PILL))
                        {
                            if (_point.X <= Properties.Settings.Default._MazeTunnel)
                                return _point;
                        }
                        if (!_closed.Contains(_point))
                            _open.Add(_point);
                    }
                    _open.Remove(_open[0]);
                }
            }

            //Otherwise
            var _opp_target = new Point();
            var _opp_cost = 0;
            while (_open.Count != 0)
            {
                _closed.Add(_open[0]);
                var _templist = Moves.GenerateMoves(_maze, _open[0]);                
                foreach (var _point in _templist)
                {
                    if(!_explored.Contains(_point))
                    {
                        if (_maze.GetSymbol(_point).Equals(Symbols._PILL))
                        {
                            //TODO:                        
                            if (!_OPPONENT_POSITION.IsEmpty)
                            {
                                var _my_cost = Moves.FindPathToPill(_maze, _CURRENT_POSITION, _point)[0].X;
                                if (_opp_target.IsEmpty)
                                {
                                    _opp_target = _point;
                                    _opp_cost = Moves.FindPathToPill(_maze, _OPPONENT_POSITION, _point)[0].X;
                                }
                                else
                                {
                                    _opp_cost = Moves.FindPathToPill(_maze, _OPPONENT_POSITION, _opp_target)[0].X
                                    + Moves.FindPathToPill(_maze, _opp_target, _point)[0].X;
                                }
                                if (_my_cost <= _opp_cost)
                                    return _point;
                                _opp_target = _point;
                            }
                            else
                            {
                                return _point;
                            }
                        }
                        _explored.Add(_point);
                    }
                    if (!_closed.Contains(_point))
                        _open.Add(_point);
                }
                _open.Remove(_open[0]);
            }

            //Give up
            _open.Clear(); _open.Add(_CURRENT_POSITION);
            _closed.Clear(); _explored.Clear();
            while (_open.Count != 0)
            {
                _closed.Add(_open[0]);
                var _templist = Moves.GenerateMoves(_maze, _open[0]);
                foreach (var _point in _templist)
                {
                    if (!_explored.Contains(_point))
                    {
                        if (_maze.GetSymbol(_point).Equals(Symbols._PILL))
                            return _point;
                        _explored.Add(_point);
                    }
                    if (!_closed.Contains(_point))
                        _open.Add(_point);
                }
                _open.Remove(_open[0]);
            }            
            return _next;
        }

        private bool SelfRespawnNeeded()
        {
            return PoisonInventory.IsSelfRespawn();
        }

        private Point _CURRENT_POSITION
        {
            get
            {
                return _maze.FindCoordinateOf(Symbols._PLAYER_A);
            }
        }

        private Point _OPPONENT_POSITION
        {
            get
            {
                return _maze.FindCoordinateOf(Symbols._PLAYER_B);
            }
        }

        private int _UPPER_PILL_COUNT
        {
            get
            {
                var _pill_count = 0;
                for (var x = 0; x < Properties.Settings.Default._MazeTunnel; x++)
                {
                    for (var y = 0; y < Properties.Settings.Default._MazeWidth; y++)
                    {
                        var _symbol = _maze.GetSymbol(x, y);
                        if (_symbol.Equals(Symbols._PILL))
                            _pill_count++;
                        if (_symbol.Equals(Symbols._BONUS_PILL))
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
                for (var x = Properties.Settings.Default._MazeTunnel + 1; x < Properties.Settings.Default._MazeHeight; x++)
                {
                    for (var y = 0; y < Properties.Settings.Default._MazeWidth; y++)
                    {
                        var _symbol = _maze.GetSymbol(x, y);
                        if (_symbol.Equals(Symbols._PILL))
                            _pill_count++;
                        if (_symbol.Equals(Symbols._BONUS_PILL))
                            _pill_count = _pill_count + 10;
                    }
                }
                return _pill_count;
            }
        }

        public int PillCount()
        {
            var _PILL_COUNT = 0;
            for (var x = 0; x < Properties.Settings.Default._MazeHeight; x++)
            {
                for (var y = 0; y < Properties.Settings.Default._MazeWidth; y++)
                {
                    var _symbol = _maze.GetSymbol(x, y);
                    if (_symbol.Equals(Symbols._PILL)
                        || _symbol.Equals(Symbols._BONUS_PILL))
                        _PILL_COUNT++;
                }
            }
            return _PILL_COUNT;
        }
    }
}