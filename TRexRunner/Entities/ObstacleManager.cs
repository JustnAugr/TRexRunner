using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TRexRunner.Entities;

public class ObstacleManager : IGameEntity
{
    private const float MIN_SPAWN_DISTANCE = 20f;
    private const int MIN_OBSTACLE_DISTANCE = 100; //in pixels
    private const int MAX_OBSTACLE_DISTANCE = 500; //in pixels

    private readonly EntityManager _entityManager;
    private readonly Trex _trex;
    private readonly ScoreBoard _scoreBoard;
    private readonly Random _random;

    private double _lastSpawnScore;
    private double _currentTargetDistance;

    public bool IsEnabled { get; set; }
    public bool CanSpawnObstacles => IsEnabled && _scoreBoard.Score >= MIN_SPAWN_DISTANCE;
    public int DrawOrder { get; set; } = 0;


    public ObstacleManager(EntityManager entityManager, Trex trex, ScoreBoard scoreBoard)
    {
        _entityManager = entityManager;
        _trex = trex;
        _scoreBoard = scoreBoard;
        _random = new Random();
    }

    public void Update(GameTime gameTime)
    {
        if (!IsEnabled)
            return;

        //if we can spawn, and if we haven't spawned, or if the distance between our last spawn and now is larger than the target dist
        //spawn another, generate a new target distance, set a new lastSpawnScore
        if (CanSpawnObstacles && ( _lastSpawnScore <= 0 || (_scoreBoard.Score - _lastSpawnScore >= _currentTargetDistance)))
        {
            //rand here gives between 0.0 -> 1.0
            //convert to a num between MIN and MAX
            _currentTargetDistance = _random.NextDouble() * (MAX_OBSTACLE_DISTANCE - MIN_OBSTACLE_DISTANCE) + MIN_OBSTACLE_DISTANCE;
            
            //TODO: Create instance of obstacle, add to entity manager

            _lastSpawnScore = _scoreBoard.Score;
        }

        foreach (var obstacle in _entityManager.GetEntitiesOfType<Obstacle>())
        {
            //-200 to factor in width of sprite
            //it needs to disappear after its RIGHT EDGE has gone off screen, not its left edge
            if (obstacle.Position.X < -200)
                _entityManager.RemoveEntity(obstacle);
        }
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
    }
}