using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TRexRunner.Entities;

public class ObstacleManager : IGameEntity
{
    private const float MIN_SPAWN_DISTANCE = 20f; //initial distance on game start
    private const int MIN_OBSTACLE_DISTANCE = 10; //in points, not pixels
    private const int MAX_OBSTACLE_DISTANCE = 30; //in points, not pixels

    //we're going to scale this value by the current speed to make sure we have a better min dist between obstacles
    private const int OBSTACLE_DISTANCE_SPEED_TOLERANCE = 5;
    private const int LARGE_CACTUS_POS_Y = 80;
    private const int SMALL_CACTUS_POS_Y = 94;

    private const int OBSTACLE_DRAW_ORDER = 12;
    private const int OBSTACLE_DESPAWN_POS_X = -200;

    private readonly EntityManager _entityManager;
    private readonly Trex _trex;
    private readonly ScoreBoard _scoreBoard;
    private readonly Random _random;

    private double _lastSpawnScore = -1.0;
    private double _currentTargetDistance;
    private Texture2D _spriteSheet;

    public bool IsEnabled { get; set; }
    public bool CanSpawnObstacles => IsEnabled && _scoreBoard.Score >= MIN_SPAWN_DISTANCE;
    public int DrawOrder { get; set; } = 0;

    public ObstacleManager(EntityManager entityManager, Trex trex, ScoreBoard scoreBoard, Texture2D spriteSheet)
    {
        _entityManager = entityManager;
        _trex = trex;
        _scoreBoard = scoreBoard;
        _random = new Random();
        _spriteSheet = spriteSheet;
    }

    public void Update(GameTime gameTime)
    {
        if (!IsEnabled)
            return;

        //if we can spawn, and if we haven't spawned, or if the distance between our last spawn and now is larger than the target dist
        //spawn another, generate a new target distance, set a new lastSpawnScore
        if (CanSpawnObstacles &&
            (_lastSpawnScore <= 0 || (_scoreBoard.Score - _lastSpawnScore >= _currentTargetDistance)))
        {
            //rand here gives between 0.0 -> 1.0
            //convert to a num between MIN and MAX
            _currentTargetDistance = _random.NextDouble() * (MAX_OBSTACLE_DISTANCE - MIN_OBSTACLE_DISTANCE) +
                                     MIN_OBSTACLE_DISTANCE;
            //add a bit more to the distance to factor in for how fast we're going
            //essentially our speed as pct of the MAX_SPEED, times the tolerance
            _currentTargetDistance += (_trex.Speed - Trex.START_SPEED) / (Trex.MAX_SPEED - Trex.START_SPEED) *
                                      OBSTACLE_DISTANCE_SPEED_TOLERANCE;

            SpawnRandomObstacle();

            _lastSpawnScore = _scoreBoard.Score;
        }

        foreach (var obstacle in _entityManager.GetEntitiesOfType<Obstacle>())
        {
            //-200 to factor in width of sprite
            //it needs to disappear after its RIGHT EDGE has gone off screen, not its left edge
            if (obstacle.Position.X < OBSTACLE_DESPAWN_POS_X)
                _entityManager.RemoveEntity(obstacle);
        }
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
    }

    private void SpawnRandomObstacle()
    {
        //todo: create instance of obstacle and add to entity manager

        Obstacle obstacle;

        //generate cactusgroup
        var randomGroupSize =
            (CactusGroup.GroupSize)_random.Next((int)CactusGroup.GroupSize.Small, (int)CactusGroup.GroupSize.Large + 1);
        var isLarge = _random.NextDouble() > 0.5;
        obstacle = new CactusGroup(_spriteSheet, isLarge, randomGroupSize, _trex,
            new Vector2(TRexRunnerGame.WINDOW_WIDTH, isLarge ? LARGE_CACTUS_POS_Y : SMALL_CACTUS_POS_Y));

        obstacle.DrawOrder = OBSTACLE_DRAW_ORDER;
        _entityManager.AddEntity(obstacle);
    }

    public void Reset()
    {
        foreach (var obstacle in _entityManager.GetEntitiesOfType<Obstacle>())
        {
            _entityManager.RemoveEntity(obstacle);
        }

        //to make sure on replay we don't rack up free points by having no obstacles for ages
        _currentTargetDistance = 0;
        _lastSpawnScore = -1;
    }
}