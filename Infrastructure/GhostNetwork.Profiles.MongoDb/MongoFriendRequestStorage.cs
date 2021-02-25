﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GhostNetwork.Profiles.FriendsFuntionality;
using MongoDB.Driver;

namespace GhostNetwork.Profiles.MongoDb
{
    public class MongoFriendRequestStorage : IFriendsFunctionalityStorage
    {
        private readonly MongoDbContext context;

        public MongoFriendRequestStorage(MongoDbContext context)
        {
            this.context = context;
        }

        public async Task<(IEnumerable<FriendRequest>, long)> FindManyFriends(int skip, int take, Guid userId)
        {
            var filter = Builders<FriendRequestEntity>.Filter.Eq(p => p.ToUser, userId)
                & Builders<FriendRequestEntity>.Filter.Where(x => x.Status == RequestStatus.Accepted)
                | Builders<FriendRequestEntity>.Filter.Eq(q => q.FromUser, userId)
                & Builders<FriendRequestEntity>.Filter.Where(x => x.Status == RequestStatus.Accepted);

            var totalCount = await context.FriendRequests.Find(filter).CountDocumentsAsync();

            var entitys = await context.FriendRequests
                .Find(filter)
                .Skip(skip)
                .Limit(take)
                .ToListAsync();

            return (entitys.Select(ToDomain), totalCount);
        }

        public async Task<(IEnumerable<FriendRequest>, long)> FindManyFriendRequests(int skip, int take, Guid userId)
        {
            var filter = Builders<FriendRequestEntity>.Filter.Eq(p => p.ToUser, userId)
                & Builders<FriendRequestEntity>.Filter.Where(x => x.Status == RequestStatus.Sended);

            var totalCount = await context.FriendRequests.Find(filter).CountDocumentsAsync();

            var entitys = await context.FriendRequests
                .Find(filter)
                .Skip(skip)
                .Limit(take)
                .ToListAsync();

            return (entitys.Select(ToDomain), totalCount);
        }

        public async Task<Guid> SendFriendRequest(FriendRequest friend)
        {
            var filter = Builders<FriendRequestEntity>.Filter.Eq(p => p.FromUser, friend.FromUser)
                & Builders<FriendRequestEntity>.Filter.Eq(p => p.ToUser, friend.ToUser);

            var exist = await context.FriendRequests.Find(filter).FirstOrDefaultAsync();

            if (exist != null)
            {
                return Guid.Empty;
            }

            var entity = new FriendRequestEntity
            {
                FromUser = friend.FromUser,
                ToUser = friend.ToUser,
                Status = friend.Status
            };

            await context.FriendRequests.InsertOneAsync(entity);

            return entity.Id;
        }

        public async Task DeleteFriendRequest(Guid id)
        {
            var filter = Builders<FriendRequestEntity>.Filter.Eq(p => p.Id, id);

            var friendRequest = await context.FriendRequests.Find(filter).FirstOrDefaultAsync();

            if (friendRequest == null)
            {
                return;
            }

            await context.FriendRequests.DeleteOneAsync(filter);
        }

        private static FriendRequest ToDomain(FriendRequestEntity entity)
        {
            return new FriendRequest(
                entity.Id,
                entity.FromUser,
                entity.ToUser,
                entity.Status);
        }
    }
}