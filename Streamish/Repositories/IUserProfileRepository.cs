using Streamish.Models;
using System.Collections.Generic;

namespace Streamish.Repositories
{
    public interface IUserProfileRepository
    {
        void Add(UserProfile userprofile);
        void Delete(int id);
        List<UserProfile> GetAll();
        UserProfile GetById(int id);
        void Update(UserProfile userprofile);
        public KeyValuePair<UserProfile, List<Video>> GetUserVideos(int userProfileId);
        public UserProfile GetByIdWithVideos(int id);
    }
}