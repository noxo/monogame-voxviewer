using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using Windows.Storage;
using Windows.Storage.Streams;

using MonoGame;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace XnaVoxViewer
{
    class VoxViewer : Microsoft.Xna.Framework.Game
    {

        GraphicsDeviceManager graphics;
        RasterizerState WIREFRAME_RASTERIZER_STATE = new RasterizerState() { CullMode = CullMode.None, FillMode = FillMode.WireFrame };

        Matrix viewMatrix;
        Matrix projectionMatrix;
        Matrix cameraRotationMatrix;

        float cameraAngle = 0;

        VertexPositionColorTexture[] cubeVertexData;
        int[] cubeIndexData;

        SpriteBatch spriteBatch;
        Texture2D background;

        List<Tuple<Vector3, Color>> blocks = new List<Tuple<Vector3, Color>>();

        public VoxViewer()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.IsFullScreen = true;
            graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
            graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
            graphics.ApplyChanges();
            CreateCube(Color.Pink, out cubeVertexData, out cubeIndexData);
            Content.RootDirectory = "Content";
        }

 

        protected async override void LoadContent()
        {
            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
            background = Content.Load<Texture2D>("smooth");
 
            await BuildModel("chr_knight.vox");
            await Task.Delay(3000);
            await BuildModel("chr_fox.vox");
            await Task.Delay(3000);
            await BuildModel("chr_gumi.vox");
            await Task.Delay(3000);
            await BuildModel("chr_jp.vox");
            await Task.Delay(3000);
            await BuildModel("chr_old.vox");
            await Task.Delay(3000);
            await BuildModel("chr_sword.vox");

        }

        public void CreateCube(Color color, out VertexPositionColorTexture[] vertexData, out int[] indexData)
        {

            vertexData = new VertexPositionColorTexture[]
            {

                // front
                new VertexPositionColorTexture(new Vector3(-0.5f, -0.5f, -0.5f), color, new Vector2(0, 0)),
                new VertexPositionColorTexture(new Vector3(0.5f, -0.5f, -0.5f), color, new Vector2(1, 0)),
                new VertexPositionColorTexture(new Vector3(-0.5f, 0.5f, -0.5f), color, new Vector2(0, 1)),
                new VertexPositionColorTexture(new Vector3(0.5f, 0.5f, -0.5f), color, new Vector2(1, 1)),

                // top
                new VertexPositionColorTexture(new Vector3(-0.5f, 0.5f, -0.5f), color, new Vector2(0, 0)),
                new VertexPositionColorTexture(new Vector3(0.5f, 0.5f, -0.5f), color, new Vector2(1, 0)),
                new VertexPositionColorTexture(new Vector3(-0.5f, 0.5f, 0.5f), color, new Vector2(0, 1)),
                new VertexPositionColorTexture(new Vector3(0.5f, 0.5f, 0.5f), color, new Vector2(1, 1)),

                // back
                new VertexPositionColorTexture(new Vector3(0.5f, -0.5f, 0.5f), color, new Vector2(1, 1)),
                new VertexPositionColorTexture(new Vector3(-0.5f, -0.5f, 0.5f), color, new Vector2(0, 1)),
                new VertexPositionColorTexture(new Vector3(0.5f, 0.5f, 0.5f), color, new Vector2(1, 0)),
                new VertexPositionColorTexture(new Vector3(-0.5f, 0.5f, 0.5f), color, new Vector2(0, 0)),

                // bottom
                new VertexPositionColorTexture(new Vector3(0.5f, -0.5f, -0.5f), color, new Vector2(1, 1)),
                new VertexPositionColorTexture(new Vector3(-0.5f, -0.5f, -0.5f), color, new Vector2(0, 1)),
                new VertexPositionColorTexture(new Vector3(0.5f, -0.5f, 0.5f), color, new Vector2(1, 0)),
                new VertexPositionColorTexture(new Vector3(-0.5f, -0.5f, 0.5f), color, new Vector2(0, 0)),

                // left
                new VertexPositionColorTexture(new Vector3(-0.5f, -0.5f, 0.5f), color, new Vector2(1, 0)),
                new VertexPositionColorTexture(new Vector3(-0.5f, -0.5f, -0.5f), color, new Vector2(0, 0)),
                new VertexPositionColorTexture(new Vector3(-0.5f, 0.5f, 0.5f), color, new Vector2(1, 1)),
                new VertexPositionColorTexture(new Vector3(-0.5f, 0.5f, -0.5f), color, new Vector2(0, 1)),

                // right
                new VertexPositionColorTexture(new Vector3(0.5f, -0.5f, -0.5f), color, new Vector2(0, 0)),
                new VertexPositionColorTexture(new Vector3(0.5f, -0.5f, 0.5f), color, new Vector2(1, 0)),
                new VertexPositionColorTexture(new Vector3(0.5f, 0.5f, -0.5f), color, new Vector2(0, 1)),
                new VertexPositionColorTexture(new Vector3(0.5f, 0.5f, 0.5f), color, new Vector2(1, 1)),

            };

            indexData = new int[] { 
                0, 1, 2, 3, 2, 1,
                4, 5, 6, 7, 6, 5,
                8, 9, 10, 11, 10, 9,
                12, 13, 14, 15, 14, 13,
                16, 17, 18, 19, 18, 17,
                20, 21, 22, 23, 22, 21
             };

        }

        protected override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {

            GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.CornflowerBlue);

            spriteBatch.Begin();
            spriteBatch.Draw(background, new Rectangle(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height), Color.White);
            spriteBatch.End();

            cameraAngle += 2.5f;
            cameraRotationMatrix =  Matrix.CreateTranslation(new Vector3(0, 0, 40)) * Matrix.CreateRotationY(MathHelper.ToRadians(cameraAngle));
  
            viewMatrix = Matrix.CreateLookAt(cameraRotationMatrix.Translation, new Vector3(-10, 5, 10), Vector3.Up);
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, graphics.GraphicsDevice.Viewport.AspectRatio, 1, 100);

            GraphicsDevice.DepthStencilState = DepthStencilState.Default; 

            int blockCount = blocks.Count;

            BasicEffect effect = new BasicEffect(graphics.GraphicsDevice);
            effect.View = viewMatrix;
            effect.Projection = projectionMatrix;

            lock (blocks)
            {
                for (int i = 0; i < blockCount; i++)
                {
                    Tuple<Vector3, Color> block = blocks[i];
                    effect.DiffuseColor = block.Item2.ToVector3();
                    effect.World = Matrix.CreateTranslation(block.Item1.X, block.Item1.Y, block.Item1.Z);
                    effect.World = effect.World * Matrix.CreateRotationX(MathHelper.ToRadians(90)) * Matrix.CreateRotationZ(MathHelper.ToRadians(180));

                    foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColorTexture>(PrimitiveType.TriangleList, cubeVertexData, 0, (4 * 6), cubeIndexData, 0, cubeIndexData.Length / 3);
                    }

                }
            }

            base.Draw(gameTime); 
           
        }

        private async Task BuildModel(String modelName)
        {
            lock (blocks)
            {
                blocks.Clear();
            }

            VoxImporter modelReader = new VoxImporter(true);
            modelReader.BlockConstructed += modelReader_BlockConstructed;
            
            var file = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(modelName);
            Stream stream = await file.OpenStreamForReadAsync();

            await Task.Run(async () =>
            {
                await modelReader.ReadMagica(stream);
            });
        }

        void modelReader_BlockConstructed(Tuple<Vector3, int> blockData)
        {
            
            int color = blockData.Item2;
            int a = (color >> 24) & 0xff;
		    int r = (color >> 16) & 0xff;
			int g = (color >> 8) & 0xff;
			int b = color & 0xff;

            Color xnaColor = new Color(r,g,b);
            blocks.Add(new Tuple<Vector3, Color>(blockData.Item1, xnaColor));
        }
    }
}
