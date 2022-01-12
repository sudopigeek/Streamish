﻿using Streamish.Models;
using System.Collections.Generic;

namespace Streamish.Repositories
{
    public interface IVideoRepository
    {
        void Add(Video video);
        void Delete(int id);
        List<Video> GetAll();
        Video GetById(int id);
        public Video GetVideoByIdWithComments(int id);
        void Update(Video video);
        public List<Video> GetAllWithComments();
        public List<Video> Search(string criterion, bool sortDescending);
    }
}