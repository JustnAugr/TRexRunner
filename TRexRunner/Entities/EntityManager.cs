﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TRexRunner.Entities;

public class EntityManager
{
    private readonly List<IGameEntity> _entities = new();
    private readonly List<IGameEntity> _entitiesToAdd = new();
    private readonly List<IGameEntity> _entitiesToRemove = new();

    public IEnumerable<IGameEntity> Entities => new ReadOnlyCollection<IGameEntity>(_entities);

    public void Update(GameTime gameTime)
    {
        foreach (var entity in _entities)
        {
            //if we're about to remove this entity in this tick, don't process updates for it
            //this helps prevents issues with the death event where we die, but the obstacles we are about to remove still get Updated()
            //right before being deleted, double playing the death sound
            if (_entitiesToRemove.Contains(entity))
                continue;

            entity.Update(gameTime);
        }

        //ingest our new entities, clear out toAdd
        foreach (var entity in _entitiesToAdd)
        {
            _entities.Add(entity);
        }

        _entitiesToAdd.Clear();

        foreach (var entity in _entitiesToRemove)
        {
            _entities.Remove(entity);
        }

        _entitiesToRemove.Clear();
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        //if the draw order is wrong, we'd end up drawing the background in front of the foreground etc
        foreach (var entity in _entities.OrderBy(e => e.DrawOrder))
        {
            entity.Draw(spriteBatch, gameTime);
        }
    }

    public void AddEntity(IGameEntity entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity), "Can't add null entity to _entities list");

        //we can't just do _entities.Add(entity)
        //what if we're in Update() of this class, calling Update() on each entity in the list,
        //and one of these entity's Update() methods invokes AddEntity() to create a new entity here in the manager?
        //we'd run into concurrent modification issues
        //so let's store our new entities, and then at the end of an Update() manager call, take in our new entities
        _entitiesToAdd.Add(entity);
    }

    public void RemoveEntity(IGameEntity entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity), "Can't remove null entity to _entities list");

        _entitiesToRemove.Add(entity);
    }

    public void Clear()
    {
        _entitiesToRemove.AddRange(_entities);
    }

    //generic method that returns all entities of specified type
    //"where T : IGameEntity" serves to restrict type passed to method
    public IEnumerable<T> GetEntitiesOfType<T>() where T : IGameEntity
    {
        return _entities.OfType<T>(); //extension method from LINQ that filters to matching types
    }
}