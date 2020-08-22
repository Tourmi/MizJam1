﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MizJam1.Inputs;
using MizJam1.Levels;
using MizJam1.Rendering;
using MizJam1.UIComponents;
using MizJam1.UIComponents.Commands;
using MizJam1.Units;
using MizJam1.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace MizJam1
{
    public class MizJam1Game : Game
    {
        public enum GameStates
        {
            MainMenu,
            Playing,
            Paused,
            PrefightPhase,
            FightPhase,
            OpenDialog,
            SelectAttack,
            DefensePhase
        }

        public enum Actions
        {
            Wait,
            Move,
            Defend,
            Attack,
            Reroll,
            Heal,
            EndTurn,
            Cancel
        }

        private GraphicsDeviceManager graphics;
        private SpriteBatch mapSpriteBatch;
        private SpriteBatch screenSpriteBatch;
        private Camera camera;

        private int levelIndex = 0;

        private SpriteFont mizjamBigFont;
        private SpriteFont mizjamSmallFont;
        private Texture2D whitePixel;
        private Texture2D[] textures;
        private Texture2D windowBorder;
        public Texture2D TransparentTileSelect { get; set; }
        public Texture2D SelectedUnitBorder { get; set; }
        public Texture2D statSlider { get; set; }
        public Texture2D statSliderPin { get; set; }
        public Texture2D dice { get; set; }
        public Dictionary<Actions, Texture2D> Dialogs { get; set; }
        public Dictionary<Actions, Texture2D> SelectedDialogs { get; set; }
        private Level[] levels;
        private Level currentLevel;
        private UIContainer mainMenu;

        public MizJam1Game()
        {
            graphics = new GraphicsDeviceManager(this);
            textures = new Texture2D[4];
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
            GameState = GameStates.MainMenu;
            Dialogs = new Dictionary<Actions, Texture2D>();
            SelectedDialogs = new Dictionary<Actions, Texture2D>();

        }

        public GameStates GameState { get; set; }

        protected override void Initialize()
        {
            GameState = GameStates.MainMenu;

            Window.AllowUserResizing = true;
            Window.Title = "MizJam1 Game";

            graphics.PreferredBackBufferWidth = 1920;
            graphics.PreferredBackBufferHeight = 1080;
            if (GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width == 1920 && GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height == 1080)
            {
                Window.IsBorderless = true;
                Window.Position = Point.Zero;
            }

            graphics.ApplyChanges();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            mapSpriteBatch = new SpriteBatch(GraphicsDevice);
            screenSpriteBatch = new SpriteBatch(GraphicsDevice);

            mizjamBigFont = Content.Load<SpriteFont>("Fonts/mizjam36");
            mizjamSmallFont = Content.Load<SpriteFont>("Fonts/mizjam24");
            mizjamSmallFont.LineSpacing = mizjamSmallFont.LineSpacing + 5;
            whitePixel = Content.Load<Texture2D>("whitePixel");
            textures[0] = Content.Load<Texture2D>("colored_packed");
            textures[1] = Content.Load<Texture2D>("colored_transparent_packed");
            textures[2] = Content.Load<Texture2D>("monochrome_packed");
            textures[3] = Content.Load<Texture2D>("monochrome_transparent_packed");
            windowBorder = Content.Load<Texture2D>("Textures/WindowBorder");
            Dialogs[Actions.Move] = Content.Load<Texture2D>("Textures/Dialogs/MoveDialog");
            Dialogs[Actions.Attack] = Content.Load<Texture2D>("Textures/Dialogs/AttackDialog");
            Dialogs[Actions.Defend] = Content.Load<Texture2D>("Textures/Dialogs/DefendDialog");
            Dialogs[Actions.Heal] = Content.Load<Texture2D>("Textures/Dialogs/HealDialog");
            Dialogs[Actions.Reroll] = Content.Load<Texture2D>("Textures/Dialogs/RerollDialog");
            Dialogs[Actions.Wait] = Content.Load<Texture2D>("Textures/Dialogs/WaitDialog");
            Dialogs[Actions.Cancel] = Content.Load<Texture2D>("Textures/Dialogs/CancelDialog");
            SelectedDialogs[Actions.Move] = Content.Load<Texture2D>("Textures/Dialogs/MoveSelectedDialog");
            SelectedDialogs[Actions.Attack] = Content.Load<Texture2D>("Textures/Dialogs/AttackSelectedDialog");
            SelectedDialogs[Actions.Defend] = Content.Load<Texture2D>("Textures/Dialogs/DefendSelectedDialog");
            SelectedDialogs[Actions.Heal] = Content.Load<Texture2D>("Textures/Dialogs/HealSelectedDialog");
            SelectedDialogs[Actions.Reroll] = Content.Load<Texture2D>("Textures/Dialogs/RerollSelectedDialog");
            SelectedDialogs[Actions.Wait] = Content.Load<Texture2D>("Textures/Dialogs/WaitSelectedDialog");
            SelectedDialogs[Actions.Cancel] = Content.Load<Texture2D>("Textures/Dialogs/CancelSelectedDialog");
            TransparentTileSelect = Content.Load<Texture2D>("Textures/TransparentTileSelect");
            SelectedUnitBorder = Content.Load<Texture2D>("Textures/SelectedUnitBorder");
            statSlider = Content.Load<Texture2D>("Textures/Slider");
            statSliderPin = Content.Load<Texture2D>("Textures/SliderPin");
            dice = Content.Load<Texture2D>("Textures/Dice");

            string[] levelFiles = Directory.GetFiles("Content/Levels");
            levels = new Level[levelFiles.Length];
            for (int i = 0; i < levelFiles.Length; i++)
            {
                XDocument levelDoc = XDocument.Parse(File.ReadAllText(levelFiles[i]));
                levels[i] = new Level(levelDoc, this);
            }
            currentLevel = levels[0];
            camera = new Camera(1080, 1080, currentLevel.Width, currentLevel.Height);

            mainMenu = new UIContainer(Point.Zero, new Point(1920, 1080), true);
            UIMenu menu = new UIMenu(Point.Zero, new Point(1000, 600), true)
            {
                Vertical = true,
                SpaceBetweenChildren = 50
            };
            UILabel startGame = new UILabel("START GAME", mizjamBigFont, Global.Colors.Main1) { SelectedTextColor = Global.Colors.Accent1 };
            startGame.AddCommand(new StartGameCommand(this));
            UILabel options = new UILabel("OPTIONS", mizjamBigFont, Global.Colors.Main1) { SelectedTextColor = Global.Colors.Accent1 };
            options.AddCommand(new OpenOptionsCommand(this));
            UILabel exitGame = new UILabel("EXIT GAME", mizjamBigFont, Global.Colors.Main1) { SelectedTextColor = Global.Colors.Accent1 };
            exitGame.AddCommand(new ExitGameCommand(this));

            menu.AddChild(startGame);
            menu.AddChild(options);
            menu.AddChild(exitGame);

            mainMenu.AddChild(menu);
        }

        protected override void Update(GameTime gameTime)
        {
            MouseAdapter.Update(gameTime);

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                GameState = GameStates.MainMenu;

            if (GameState == GameStates.MainMenu)
            {
                foreach (var child in ((UIMenu)mainMenu.Child).Children)
                {
                    if (child.Contains(MouseAdapter.Position))
                    {
                        child.Select();
                    }
                    else
                    {
                        child.Deselect();
                    }
                }

                if (MouseAdapter.ConsumeLeftClick)
                {
                    mainMenu.Execute();
                }
            }
            else if (GameState == GameStates.Playing)
            {
                currentLevel.Start();
            }
            else
            {
                if (MouseAdapter.ScrollUp)
                    camera.Zoom += 0.5f;
                if (MouseAdapter.ScrollDown)
                    camera.Zoom -= 0.5f;

                if (Keyboard.GetState().IsKeyDown(Keys.Left))
                    camera.MoveCamera(new Vector2(-3, 0), true);
                if (Keyboard.GetState().IsKeyDown(Keys.Right))
                    camera.MoveCamera(new Vector2(3, 0), true);
                if (Keyboard.GetState().IsKeyDown(Keys.Up))
                    camera.MoveCamera(new Vector2(0, -3), true);
                if (Keyboard.GetState().IsKeyDown(Keys.Down))
                    camera.MoveCamera(new Vector2(0, 3), true);
                if (MouseAdapter.Position.X > 420 && MouseAdapter.Position.X < 1500) //Mouse inside of game window
                {
                    currentLevel.MouseOver(camera.ScreenToWorld(MouseAdapter.Position.ToVector2()).ToPoint(), MouseAdapter.Position);
                    if (MouseAdapter.ConsumeLeftClick)
                    {
                        currentLevel.LeftClick(camera.ScreenToWorld(MouseAdapter.Position.ToVector2()).ToPoint(), MouseAdapter.Position);
                    }
                }
                else
                {
                    currentLevel.MouseOver(new Point(-10000, -10000), MouseAdapter.Position);
                }
            }

            currentLevel.Update(gameTime);
            if (currentLevel.LevelFinished)
            {
                levelIndex++;
                currentLevel = levels[levelIndex];
                currentLevel.Start();
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Global.Colors.Background2);
            screenSpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            mapSpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: camera.TransformationMatrix);

            if (GameState == GameStates.MainMenu)
            {
                mainMenu.Draw(screenSpriteBatch);
            }
            else
            {
                drawLevel();
            }

            //Dice
            //mapSpriteBatch.Draw(textures[0], new Rectangle() { X = 16, Y = 16, Height = 16, Width = 16 }, new Rectangle() { X = 672, Y = 224, Height = 16, Width = 16 }, Color.White);

            //Draw debug world cursor position
            //screenSpriteBatch.DrawString(mainFont, camera.ScreenToWorld(MouseAdapter.Position.ToVector2()).ToString(), Vector2.Zero, Color.White);
            //Draw cursor
            screenSpriteBatch.Draw(textures[1], new Rectangle(MouseAdapter.Position, new Point(32, 32)), new Rectangle(560, 160, 16, 16), Color.White);
            base.Draw(gameTime);
            mapSpriteBatch.End();
            screenSpriteBatch.End();
        }

        private void drawLevel()
        {
            currentLevel.Draw(mapSpriteBatch, screenSpriteBatch, textures);
            
            drawLeftWindow();
            drawRightWindow();
        }

        private void drawLeftWindow()
        {
            screenSpriteBatch.Draw(whitePixel, new Rectangle(0, 0, 420, 1080), Global.Colors.Background2);
            screenSpriteBatch.Draw(windowBorder, new Rectangle(0, 0, 420, 1080), Global.Colors.Main1);

            Cell cell;
            if ((cell = currentLevel.MouseOverCell).ID != 0)
            {
                string infoTitles = "SOLID: \nDIFF: ";
                string information = "\n{0}\n{1}";

                CellProperties props = cell.Properties;

                information = string.Format(information,
                    props.IsSolid ? "YES" : "NO",
                    props.Difficulty);
                int width = (int)Math.Ceiling(mizjamSmallFont.MeasureString(infoTitles).X);
                screenSpriteBatch.DrawString(mizjamSmallFont, "CURRENT TILE\n" + infoTitles, new Vector2(48), Global.Colors.Main1);
                screenSpriteBatch.DrawString(mizjamSmallFont, information, new Vector2(48 + width, 48), Global.Colors.Main1);
            }
        }

        private void drawRightWindow()
        {
            screenSpriteBatch.Draw(whitePixel, new Rectangle(1500, 0, 420, 1080), Global.Colors.Background2);
            screenSpriteBatch.Draw(windowBorder, new Rectangle(1500, 0, 420, 1080), Global.Colors.Main1);

            Unit unit;
            if ((unit = currentLevel.SelectedUnit ?? currentLevel.MouseOverUnit) != null)
            {
                string infoTitles = "UNIT\nNAME:  \nTYPE:  \nALLY:  \nHP:  \nATT:  \nDEF:  \nMAG:  \nMDEF:  \nRNG:  \nSPD:  \n";
                string information = "\n{0}\n{1}\n{2}\n{3}/{4}\n{5}\n{6}\n{7}\n{8}\n{9}\n{10}\n";

                information = string.Format(information,
                    unit.Name.ToUpper(),
                    unit.UnitClass.Name.ToUpper(),
                    unit.Enemy ? "NO" : "YES",
                    unit.Health,
                    unit.Stats.ContainsKey(Stats.MaxHealth) ? unit.Stats[Stats.MaxHealth].ToString() : "",
                    unit.Stats.ContainsKey(Stats.Attack) ? unit.Stats[Stats.Attack].ToString() : "",
                    unit.Stats.ContainsKey(Stats.Defense) ? unit.Stats[Stats.Defense].ToString() : "",
                    unit.Stats.ContainsKey(Stats.Magic) ? unit.Stats[Stats.Magic].ToString() : "",
                    unit.Stats.ContainsKey(Stats.MagicDefense) ? unit.Stats[Stats.MagicDefense].ToString() : "",
                    unit.Stats.ContainsKey(Stats.Range) ? unit.Stats[Stats.Range].ToString() : "",
                    unit.Stats.ContainsKey(Stats.Speed) ? unit.Stats[Stats.Speed].ToString() : "");
                Point size = mizjamSmallFont.MeasureString(infoTitles).ToPoint();
                screenSpriteBatch.DrawString(mizjamSmallFont, infoTitles, new Vector2(1500 + 48, 48), Global.Colors.Main1);
                screenSpriteBatch.DrawString(mizjamSmallFont, information, new Vector2(1500 + 48 + size.X, 48), Global.Colors.Main1);
                int sliderHeight = size.Y + mizjamSmallFont.LineSpacing * 2 + 48;
                string leftStat = StatsUtils.GetName(unit.UnitClass.OppositeStats.Item1);
                string rightStat = StatsUtils.GetName(unit.UnitClass.OppositeStats.Item2);
                screenSpriteBatch.DrawString(mizjamSmallFont, leftStat, new Point(1500 + 48 + 18, sliderHeight - mizjamSmallFont.LineSpacing).ToVector2(), Global.Colors.Main1);
                screenSpriteBatch.DrawString(mizjamSmallFont, rightStat, new Point(1920 - 48 - 18 - mizjamSmallFont.MeasureString(rightStat).ToPoint().X, sliderHeight - mizjamSmallFont.LineSpacing).ToVector2(), Global.Colors.Main1);
                screenSpriteBatch.Draw(statSlider, new Rectangle(new Point(1500 + 48 + 18, sliderHeight), new Point(6 * Global.SpriteWidth * 3, Global.SpriteHeight * 3)), Color.White);
                if (unit.Stats.ContainsKey(unit.UnitClass.OppositeStats.Item1))
                {
                    int sliderPinOffset = 48;
                    int statValue = unit.Stats[unit.UnitClass.OppositeStats.Item1];
                    if (statValue > 1)
                    {
                        sliderPinOffset += 48;
                    }
                    if (statValue > 3)
                    {
                        sliderPinOffset += 48;
                    }
                    if (statValue > 5)
                    {
                        sliderPinOffset += 48;
                    }

                    screenSpriteBatch.Draw(
                        statSliderPin,
                        new Rectangle(new Point(1500 + 48 + 18 + sliderPinOffset, sliderHeight),
                        Global.SpriteSize.Multiply(3)),
                        Color.White);
                    screenSpriteBatch.Draw(
                        dice,
                        new Rectangle(new Point(1500 + 48 + 18, sliderHeight),
                        Global.SpriteSize.Multiply(3)), new Rectangle(new Point((statValue - 1) * 16, 16),
                        Global.SpriteSize),
                        Global.Colors.Main1);
                    screenSpriteBatch.Draw(
                        dice,
                        new Rectangle(new Point(1500 + 48 + 18 + 5 * 48, sliderHeight),
                        Global.SpriteSize.Multiply(3)),
                        new Rectangle(new Point((6 - (statValue)) * 16, 0),
                        Global.SpriteSize),
                        Global.Colors.Main1);
                }
            }
        }

        public void Reset()
        {
            GameState = GameStates.MainMenu;
        }
    }
}
