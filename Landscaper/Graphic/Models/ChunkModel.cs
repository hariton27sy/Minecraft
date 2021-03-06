﻿using System;
using System.Collections.Generic;
using System.Threading;
using NLog.LayoutRenderers;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using SimpleGame.GameCore.Worlds;
using SimpleGame.Graphic.Models.Templates;


namespace SimpleGame.Graphic.Models
{
    public class ChunkModel : IModel
    {
        private readonly List<float> vertices = new List<float>();
        private readonly List<float> textureCoords = new List<float>();
        private readonly List<int> indices = new List<int>();
        private int lastIndex;
        
        private readonly BaseChunk chunk;
        private readonly ITextureStorage storage;
        private bool shouldLoadToGl;

        private bool isDisposed;
        private int verticesVbo;
        private int textureVbo;
        private int indicesVbo;
        private int vao = -1;

        public ChunkModel(BaseChunk chunk, ITextureStorage storage)
        {
            this.chunk = chunk;
            this.storage = storage;
            
            UpdateModel();
        }

        public void UpdateModel()
        {
            for (var y = 0; y < BaseChunk.Height; y++)
            for (var x = 0; x < BaseChunk.Width; x++)
            for (var z = 0; z < BaseChunk.Length; z++)
            {
                if (chunk.Map[x, y, z] != 0)
                    AddBlock(x, y, z);
            }

            shouldLoadToGl = true;
        }

        private void LoadToVideoCard()
        {
            if (vao == -1)
                vao = GlHelper.VaoCreator();
            
            GlHelper.VaoBinder(vao);
            GlHelper.DeleteVbos(verticesVbo, indicesVbo, textureVbo);
            indicesVbo = GlHelper.LoadIndices(indices.ToArray());
            verticesVbo = GlHelper.LoadVbo(0, 3, vertices.ToArray());
            textureVbo = GlHelper.LoadVbo(2, 2, textureCoords.ToArray());
        }
        
        private bool HasNeighbourOn(int x, int y, int z, BlockEdge edge)
        {
            var dx = 0;
            var dy = 0;
            var dz = 0;
            
            switch (edge)
            {
                case BlockEdge.Right:
                    dx = 1;
                    break;
                case BlockEdge.Left:
                    dx = -1;
                    break;
                case BlockEdge.Front:
                    dz = 1;
                    break;
                case BlockEdge.Back:
                    dz = -1;
                    break;
                case BlockEdge.Top:
                    dy = 1;
                    break;
                case BlockEdge.Bottom:
                    dy = -1;
                    break;
            }

            var neighbour = 0;
            try
            {
                neighbour = chunk.Map[x + dx, y + dy, z + dz];
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }

            return neighbour != 0;
        }

        private void AddBlock(int x, int y, int z)
        {
            const int air = 0;
            if (chunk.Map[x, y, z] == air)
                return;
            var blockTexture = storage[chunk.Map[x, y, z]];
            var offset = new Vector3(x + 0.5f, y + 0.5f, z + 0.5f);
            var toSee = new HashSet<BlockEdge>();
            
            if (!HasNeighbourOn(x, y, z, BlockEdge.Right))
                toSee.Add(BlockEdge.Right);
            if (!HasNeighbourOn(x, y, z, BlockEdge.Left))
                toSee.Add(BlockEdge.Left);
            if (!HasNeighbourOn(x, y, z, BlockEdge.Top))
                toSee.Add(BlockEdge.Top);
            if (!HasNeighbourOn(x, y, z, BlockEdge.Bottom))
                toSee.Add(BlockEdge.Bottom);
            if (!HasNeighbourOn(x, y, z, BlockEdge.Front)) 
                toSee.Add(BlockEdge.Front);
            if (!HasNeighbourOn(x, y, z, BlockEdge.Back))
                toSee.Add(BlockEdge.Back);

            foreach (var edge in toSee)
            {
                AddFace(BlockModel.GetEdge(edge), offset);
                textureCoords.AddRange(blockTexture.GetEdge(edge));
                AddIndices();
            }
        }

        private void AddIndices()
        {
            indices.AddRange(new []{lastIndex, lastIndex + 1, lastIndex + 2, 
                lastIndex + 2, lastIndex + 3, lastIndex});

            lastIndex += 4;
        }

        private void AddFace(Vector3[] faceVertices, Vector3 offset)
        {
            foreach (var vertex in faceVertices)
            {
                var position = vertex + offset;
                vertices.Add(position.X);
                vertices.Add(position.Y);
                vertices.Add(position.Z);
            }
        }

        public void Dispose()
        {
            if (isDisposed)
                return;
            
            GL.DisableVertexAttribArray(0);
            GL.DisableVertexAttribArray(2);
            
            // GlHelper.VaoBinder(0);
            isDisposed = true;
        }

        public BeginMode DrawingMode => BeginMode.Triangles;
        public int VerticesCount => indices.Count;
        public bool IsTextured => true;
        public IModel Start()
        {
            if (shouldLoadToGl)
            {
                LoadToVideoCard();
                shouldLoadToGl = false;
            }

            GlHelper.VaoBinder(vao);
            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(2);
            GL.BindTexture(TextureTarget.Texture2D, storage[1].AtlasGlId);
            return this;
        }

        ~ChunkModel()
        {
            Dispose();
            GlHelper.VaoRemover(new []{vao});
            GlHelper.DeleteVbos(verticesVbo, indicesVbo, textureVbo);
        }
    }
}