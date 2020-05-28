﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using SimpleGame.Graphic.Models;
using SimpleGame.Graphic.Shaders;

namespace SimpleGame.Graphic
{
    public class Renderer
    {
        private Matrix4 projectionMatrix = Matrix4.Identity;

        public Matrix4 ProjectionMatrix
        {
            get => projectionMatrix;
            set
            {
                projectionMatrix = value;
                shader.ProjectionMatrix = value;
            }
        }

        public float Aspect
        {
            set =>
                ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90), 
                    value, 0.01f, 1000);
        }

        private readonly StaticShader shader;

        public Renderer(StaticShader shader)
        {
            this.shader = shader;
        }

        public void Start()
        {
            shader.Initialize();
            using (shader.Start())
            {
                shader.ProjectionMatrix = ProjectionMatrix;
            }
            GL.Enable(EnableCap.DepthTest);
        }

        public void Clear(Color color=default)
        {
            GL.ClearColor(color);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        public void Render(ICamera camera, ITextureStorage storage, IEnumerable<IEntity> entities)
        {
            using (shader.Start())
            {
                foreach (var entity in entities)
                {
                    var model = entity.GetModel(storage, camera);
                    if (model == null) 
                        continue;
                    using (model.Start())
                    {
                        shader.IsTextured = model.IsTextured;
                        shader.TransformationMatrix = entity.TransformMatrix;
                        shader.ViewMatrix = camera.ViewMatrix;
                        GL.DrawElements(model.DrawingMode, model.VerticesCount, 
                            DrawElementsType.UnsignedInt, 0);
                    }
                }
            }
        }
    }
}