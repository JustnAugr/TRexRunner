using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TRexRunner.Entities;

public class SkyManager : IGameEntity, IDayNightCycle
{
    private const float TRANSITION_DURATION = 2f; //seconds

    private const int CLOUD_DRAW_ORDER = -1;
    private const int STAR_DRAW_ORDER = -3;
    private const int MOON_DRAW_ORDER = -2;

    private const int CLOUD_MIN_POS_Y = 20;
    private const int CLOUD_MAX_POS_Y = 70;
    private const int CLOUD_MIN_DISTANCE = 150;
    private const int CLOUD_MAX_DISTANCE = 400;

    private const int STAR_MIN_POS_Y = 10;
    private const int STAR_MAX_POS_Y = 60;
    private const int STAR_MIN_DISTANCE = 120;
    private const int STAR_MAX_DISTANCE = 380;

    private const int MOON_POS_Y = 20;

    private const int NIGHT_TIME_SCORE = 700;
    private const int NIGHT_TIME_DURATION_SCORE = 250;

    private readonly Trex _trex;
    private readonly Texture2D _spriteSheet;
    private readonly Texture2D _invertedSpriteSheet;
    private readonly EntityManager _entityManager;
    private readonly ScoreBoard _scoreBoard;
    private Random _random;
    private Moon _moon;

    private int _targetCloudDistance;
    private int _targetStarDistance;

    private float _normalizedScreenColor = 1f; //1 for day, 0 for night
    private int _previousScore;
    private int _nightTimeStartScore;
    private bool _isTransitioningToNight;
    private bool _isTransitioningToDay;

    public int DrawOrder { get; set; } = 0;
    public int NightCount { get; private set; }

    public Color ClearColor =>
        new(_normalizedScreenColor, _normalizedScreenColor,
            _normalizedScreenColor); //1,1,1 would be rgb255,255,255 of white

    public bool IsNight => _normalizedScreenColor < 0.5f;

    public SkyManager(Trex trex, Texture2D spriteSheet, Texture2D invertedSpriteSheet, EntityManager entityManager,
        ScoreBoard scoreBoard)
    {
        _trex = trex;
        _spriteSheet = spriteSheet;
        _invertedSpriteSheet = invertedSpriteSheet;
        _entityManager = entityManager;
        _scoreBoard = scoreBoard;
        _random = new Random();
    }

    public void Update(GameTime gameTime)
    {
        if (_moon == null)
        {
            _moon = new Moon(this, _spriteSheet, _trex, new Vector2(TRexRunnerGame.WINDOW_WIDTH, MOON_POS_Y));
            _moon.DrawOrder = MOON_DRAW_ORDER;
            _entityManager.AddEntity(_moon);
        }

        HandleCloudSpawning();
        HandleStarSpawning();

        //remove skyObjects that are far gone
        foreach (var skyObject in _entityManager.GetEntitiesOfType<SkyObject>())
        {
            if (skyObject.Position.X <= -100) //aka no longer visible off the screen to the left
            {
                //moon never gets removed, just resent back to the right hand side to scroll again
                if (skyObject is Moon moon)
                    moon.Position = new Vector2(TRexRunnerGame.WINDOW_WIDTH, MOON_POS_Y);
                else
                    _entityManager.RemoveEntity(skyObject);
            }
        }

        //change to night time
        //this relies on the rounding of the division to give us different values
        //698/700 and 699/700 will be 0, but once we hit 700 - then we'll transition
        //first two conditions are for handling when we restart after a gameover
        if (_previousScore != 0 && _previousScore < _scoreBoard.DisplayScore &&
            _previousScore / NIGHT_TIME_SCORE != _scoreBoard.DisplayScore / NIGHT_TIME_SCORE)
        {
            TransitionToNightTime();
        }

        if (IsNight && _scoreBoard.DisplayScore - _nightTimeStartScore >= NIGHT_TIME_DURATION_SCORE)
        {
            TransitionToDayTime();
        }

        //if were restarting after a gameover, transition back to day on restart
        if (_scoreBoard.DisplayScore < NIGHT_TIME_SCORE && (IsNight || _isTransitioningToNight))
        {
            TransitionToDayTime();
        }

        UpdateTransition(gameTime);

        _previousScore = _scoreBoard.DisplayScore;
    }

    private void UpdateTransition(GameTime gameTime)
    {
        if (_isTransitioningToNight)
        {
            //should take 2 seconds to go from 1 to 0 and become nighttime
            _normalizedScreenColor -= (float)gameTime.ElapsedGameTime.TotalSeconds / TRANSITION_DURATION;

            if (_normalizedScreenColor < 0)
                _normalizedScreenColor = 0;

            //invert colors on certain entities
            if (_normalizedScreenColor <= 0.5f)
            {
                InvertTextures(_invertedSpriteSheet);
            }
        }
        else if (_isTransitioningToDay)
        {
            //should take 2 seconds to go from 0 to 1 and become day time
            _normalizedScreenColor += (float)gameTime.ElapsedGameTime.TotalSeconds / TRANSITION_DURATION;

            if (_normalizedScreenColor > 1)
                _normalizedScreenColor = 1;

            if (_normalizedScreenColor >= 0.5f)
            {
                InvertTextures(_spriteSheet);
            }
        }
    }

    private void InvertTextures(Texture2D spriteSheet)
    {
        foreach (var entity in _entityManager.GetEntitiesOfType<ITextureInvertible>())
        {
            entity.UpdateTexture(spriteSheet);
        }
    }

    private bool TransitionToNightTime()
    {
        if (IsNight || _isTransitioningToNight)
            return false;

        _nightTimeStartScore = _scoreBoard.DisplayScore;

        _isTransitioningToNight = true;
        _isTransitioningToDay = false;
        NightCount++;

        //make sure it starts at day
        _normalizedScreenColor = 1f;
        return true;
    }

    private bool TransitionToDayTime()
    {
        if (!IsNight || _isTransitioningToDay)
            return false;

        _isTransitioningToNight = false;
        _isTransitioningToDay = true;

        //make sure it starts at night
        _normalizedScreenColor = 0f;
        return true;
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
            var star = new Star(this, _spriteSheet, _trex, new Vector2(TRexRunnerGame.WINDOW_WIDTH, posY));
            star.DrawOrder = STAR_DRAW_ORDER;

            _entityManager.AddEntity(star);
        }
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
    }
}