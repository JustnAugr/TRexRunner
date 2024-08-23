using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TRexRunner.Graphics;

namespace TRexRunner.Entities;

public class GroundManager : IGameEntity
{
    private const float GROUND_TILE_POS_Y = 119;

    private const int SPRITE_WIDTH = 600;
    private const int SPRITE_HEIGHT = 14;

    private const int SPRITE_POS_X = 2;
    private const int SPRITE_POS_Y = 54;

    private Texture2D _spriteSheet;
    private readonly List<GroundTile> _groundTiles = new();

    private EntityManager _entityManager;

    private Sprite _regularSprite;
    private Sprite _bumpySprite;

    private Trex _trex;

    private Random _random;

    public int DrawOrder { get; set; }

    public GroundManager(Texture2D spriteSheet, EntityManager entityManager, Trex trex)
    {
        _spriteSheet = spriteSheet;
        _entityManager = entityManager;
        _trex = trex;

        _regularSprite = new Sprite(spriteSheet, SPRITE_POS_X, SPRITE_POS_Y, SPRITE_WIDTH, SPRITE_HEIGHT);
        _bumpySprite = new Sprite(spriteSheet, SPRITE_POS_X + SPRITE_WIDTH, SPRITE_POS_Y, SPRITE_WIDTH, SPRITE_HEIGHT);

        _random = new Random();
    }

    public void Update(GameTime gameTime)
    {
        //being less than 0 means the ground tile has started to move off screen, so we need another after it
        //to prevent empty spaces to the right
        float maxPosX = _groundTiles.Max(g => g.PositionX);
        if (_groundTiles.Count > 0 && maxPosX < 0)
        {
            SpawnTile(maxPosX);
        }

        List<GroundTile> tilesToRemove = new();
        foreach (var groundTile in _groundTiles)
        {
            //the ground should 'scroll' by the timeAdjusted speed of the trex, to give the impression we're running forward
            //at some pixels per second
            groundTile.PositionX -= _trex.Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

            //completely off screen, we can remove the entity
            if (groundTile.PositionX < -SPRITE_WIDTH)
            {
                _entityManager.RemoveEntity(groundTile);
                tilesToRemove.Add(groundTile);
            }
        }

        //to avoid concurrent modification, we use a separate 'toRemove' list
        foreach (var tile in tilesToRemove)
        {
            _groundTiles.Remove(tile);
        }
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
    }

    public void Initialize()
    {
        _groundTiles.Clear();
        var groundTile = CreateRegularTile(0);
        _groundTiles.Add(groundTile);

        _entityManager.AddEntity(groundTile);
    }

    private GroundTile CreateRegularTile(float positionX)
    {
        var groundTile = new GroundTile(positionX, GROUND_TILE_POS_Y, _regularSprite);
        return groundTile;
    }

    private GroundTile CreateBumpyTile(float positionX)
    {
        var groundTile = new GroundTile(positionX, GROUND_TILE_POS_Y, _bumpySprite);
        return groundTile;
    }

    private void SpawnTile(float maxPosX)
    {
        double randomNumber = _random.NextDouble();

        //we want to spawn right after the biggest sprite currently in the list
        float posX = maxPosX + SPRITE_WIDTH;

        GroundTile groundTile;

        if (randomNumber > 0.5)
            groundTile = CreateBumpyTile(posX);
        else
            groundTile = CreateRegularTile(posX);

        _entityManager.AddEntity(groundTile);
        _groundTiles.Add(groundTile);
    }
}