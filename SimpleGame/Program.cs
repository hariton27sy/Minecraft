﻿using System;
using OpenTK;
using SimpleGame.GameCore;
using SimpleGame.GameCore.Persons;
using SimpleGame.GameCore.Worlds;
using SimpleGame.Graphic;
using SimpleGame.Graphic.Shaders;

namespace SimpleGame
{
    class Program
    {
        static void Main(string[] args)
        {
            // var renderer = new Renderer(new StaticShader());

            var player = new Player(new Vector3(0, 100, 0));
            var world = new OverWorld(player, new Random().Next(Int32.MaxValue));
            var window = new Game(world, player);
            window.Run();
        }
    }
}
