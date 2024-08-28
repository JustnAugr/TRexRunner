using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TRexRunner.Entities;

public class SkyManager : IGameEntity
{
    private const int CLOUD_DRAW_ORDER = -1;
    private const int STAR_DRAW_ORDER = -2;

    private const int CLOUD_MIN_POS_Y = 20;
    private const int CLOUD_MAX_POS_Y = 70;
    private const int CLOUD_MIN_DISTANCE = 150;
    private const int CLOUD_MAX_DISTANCE = 400;

    private const int STAR_MIN_POS_Y = 10;
    private const int STAR_MAX_POS_Y = 60;
    private const int STAR_MIN_DISTANCE = 120;
    private const int STAR_MAX_DISTANCE = 380;

    private readonly Trex _trex;
    private readonly Texture2D _spriteSheet;
    private readonly EntityManager _entityManager;
    private readonly ScoreBoard _scoreBoard;
    private Random _random;

    private int _targetCloudDistance;
    private int _targetStarDistance;

    public int DrawOrder { get; set; } = 0;

    public SkyManager(Trex trex, Texture2D spriteSheet, EntityManager entityManager, ScoreBoard scoreBoard)
    {
        _trex = trex;
        _spriteSheet = spriteSheet;
        _entityManager = entityManager;
        _scoreBoard = scoreBoard;
        _random = new Random();
    }

    public void Update(GameTime gameTime)
    {
        HandleCloudSpawning();
        HandleStarSpawning();

        //remove skyObjects that are far gone
        foreach (var skyObject in _entityManager.GetEntitiesOfType<SkyObject>())
        {
            if (skyObject.Position.X <= -200) //aka no longer visible off the screen to the left
                _entityManager.RemoveEntity(skyObject);
        }
    }

    private void HandleCloudSpawning()
    {
        var clouds = _entityManager.GetEntitiesOfType<Cloud>();
        //if we don't have clouds currently, or if we've moved our target distance
        if (!clouds.Any() || (TRexRunnerGame.WINDOW_WIDTH - clouds.Max(c => c.Position.X) >= _targetCloudDistance))
        {
            _targetCloudDistance = _random.Next(CLOUD_MIN_DISTANCE, CLOUD_MAX_DISTANCE + 1);

            int posY = _random.Next(CLOUD_MIN_POS_Y, CLOUD_MAX_POS_Y + 1);
            var cloud = new Cloud(_spriteSheet, _trex, new Vector2(TRexRunnerGame.WINDOW_WIDTH, posY));
            cloud.DrawOrder = CLOUD_DRAW_ORDER;

            _entityManager.AddEntity(cloud);
        }
    }

    private void HandleStarSpawning()
    {
        var stars = _entityManager.GetEntitiesOfType<Star>();
        if (!stars.Any() || (TRexRunnerGame.WINDOW_WIDTH - stars.Max(s => s.Position.X) >= _targetStarDistance))
        {
            _targetStarDistance = _random.Next(STAR_MIN_DISTANCE, STAR_MAX_DISTANCE + 1);

            int posY = _random.Next(STAR_MIN_POS_Y, STAR_MAX_POS_Y + 1);
            var star = new Star(_spriteSheet, _trex, new Vector2(TRexRunnerGame.WINDOW_WIDTH, posY));
            star.DrawOrder = STAR_DRAW_ORDER;

            _entityManager.AddEntity(star);
        }
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
    }
}