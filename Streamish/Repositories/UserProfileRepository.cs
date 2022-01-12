using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Streamish.Models;
using Streamish.Utils;

namespace Streamish.Repositories
{
    public class UserProfileRepository : BaseRepository, IUserProfileRepository
    {
        public UserProfileRepository(IConfiguration configuration) : base(configuration) { }
        public List<UserProfile> GetAll()
        {
            using (var conn = Connection)
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT * FROM UserProfile";
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        var userprofiles = new List<UserProfile>();
                        while (reader.Read())
                        {
                            UserProfile p = new UserProfile()
                            {
                                Id = DbUtils.GetInt(reader, "Id"),
                                Name = DbUtils.GetString(reader, "Name"),
                                Email = DbUtils.GetString(reader, "Email"),
                                DateCreated = DbUtils.GetDateTime(reader, "DateCreated")
                            };
                            if (DbUtils.IsDbNull(reader, "ImageUrl"))
                            {
                                p.ImageUrl = null;
                            }
                            else
                            {
                                p.ImageUrl = DbUtils.GetString(reader, "ImageUrl");
                            }
                            userprofiles.Add(p);
                        }
                        return userprofiles;
                    }
                }
            }
        }
        public UserProfile GetById(int id)
        {
            using (var conn = Connection)
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT * FROM UserProfile
                           WHERE v.Id = @Id";
                    DbUtils.AddParameter(cmd, "@Id", id);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        UserProfile userprofile = null;
                        if (reader.Read())
                        {
                            userprofile = new UserProfile()
                            {
                                Id = DbUtils.GetInt(reader, "Id"),
                                Name = DbUtils.GetString(reader, "Name"),
                                Email = DbUtils.GetString(reader, "Email"),
                                DateCreated = DbUtils.GetDateTime(reader, "DateCreated")
                            };
                            if (DbUtils.IsDbNull(reader, "ImageUrl"))
                            {
                                userprofile.ImageUrl = null;
                            }
                            else
                            {
                                userprofile.ImageUrl = DbUtils.GetString(reader, "ImageUrl");
                            }
                        }
                        return userprofile;
                    }
                }
            }
        }     
        public void Add(UserProfile userprofile)
        {
            using (var conn = Connection)
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        INSERT INTO UserProfile (Name, Email, ImageUrl, DateCreated)
                        OUTPUT INSERTED.ID
                        VALUES (@name, @email, @imageUrl, @dateCreated)";

                    DbUtils.AddParameter(cmd, "@name", userprofile.Name);
                    DbUtils.AddParameter(cmd, "@email", userprofile.Email);
                    DbUtils.AddParameter(cmd, "@imageUrl", userprofile.ImageUrl);
                    DbUtils.AddParameter(cmd, "@dateCreated", userprofile.DateCreated);
                    userprofile.Id = (int)cmd.ExecuteScalar();
                }
            }
        }
        public void Update(UserProfile userprofile)
        {
            using (var conn = Connection)
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        UPDATE UserProfile
                           SET Name = @name,
                               Email = @email,
                               ImageUrl = @imageUrl,
                               DateCreated = @dateCreated
                         WHERE Id = @id";
                    DbUtils.AddParameter(cmd, "@name", userprofile.Name);
                    DbUtils.AddParameter(cmd, "@email", userprofile.Email);
                    DbUtils.AddParameter(cmd, "@imageUrl", userprofile.ImageUrl);
                    DbUtils.AddParameter(cmd, "@dateCreated", userprofile.DateCreated);
                    DbUtils.AddParameter(cmd, "@id", userprofile.Id);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        public void Delete(int id)
        {
            using (var conn = Connection)
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM UserProfile WHERE Id = @Id";
                    DbUtils.AddParameter(cmd, "@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        } 
        public KeyValuePair<UserProfile, List<Video>> GetUserVideos(int userProfileId)
        {    
            using (var conn = Connection)
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                SELECT v.Id AS VideoId, v.Title, v.Description, v.Url, 
                       v.DateCreated AS VideoDateCreated, v.UserProfileId As VideoUserProfileId,

                       up.Name, up.Email, up.DateCreated AS UserProfileDateCreated,
                       up.ImageUrl AS UserProfileImageUrl,
                        
                       c.Id AS CommentId, c.Message, c.UserProfileId AS CommentUserProfileId
                  FROM Video v 
                       JOIN UserProfile up ON v.UserProfileId = up.Id
                       LEFT JOIN Comment c on c.VideoId = v.id
             WHERE v.Id = @id
             ORDER BY  v.DateCreated
            ";
                    DbUtils.AddParameter(cmd, "@id", userProfileId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        List<Video> videos = new List<Video>();
                        UserProfile userProfile = null;
                        while (reader.Read())
                        {
                            var videoId = DbUtils.GetInt(reader, "VideoId");

                            var existingVideo = videos.FirstOrDefault(p => p.Id == videoId);
                            
                            if (userProfile == null)
                            {
                                userProfile = new UserProfile()
                                {
                                    Id = DbUtils.GetInt(reader, "VideoUserProfileId"),
                                    Name = DbUtils.GetString(reader, "Name"),
                                    Email = DbUtils.GetString(reader, "Email"),
                                    DateCreated = DbUtils.GetDateTime(reader, "UserProfileDateCreated"),
                                    ImageUrl = DbUtils.GetString(reader, "UserProfileImageUrl"),
                                };
                            }

                            if (existingVideo == null)
                            {
                                existingVideo = new Video()
                                {
                                    Id = videoId,
                                    Title = DbUtils.GetString(reader, "Title"),
                                    Description = DbUtils.GetString(reader, "Description"),
                                    DateCreated = DbUtils.GetDateTime(reader, "VideoDateCreated"),
                                    Url = DbUtils.GetString(reader, "Url"),
                                    UserProfileId = DbUtils.GetInt(reader, "VideoUserProfileId"),
                                    UserProfile = new UserProfile()
                                    {
                                        Id = DbUtils.GetInt(reader, "VideoUserProfileId"),
                                        Name = DbUtils.GetString(reader, "Name"),
                                        Email = DbUtils.GetString(reader, "Email"),
                                        DateCreated = DbUtils.GetDateTime(reader, "UserProfileDateCreated"),
                                        ImageUrl = DbUtils.GetString(reader, "UserProfileImageUrl"),
                                    },
                                    Comments = new List<Comment>()
                                };

                                videos.Add(existingVideo);
                            }

                            if (DbUtils.IsNotDbNull(reader, "CommentId"))
                            {
                                existingVideo.Comments.Add(new Comment()
                                {
                                    Id = DbUtils.GetInt(reader, "CommentId"),
                                    Message = DbUtils.GetString(reader, "Message"),
                                    VideoId = videoId,
                                    UserProfileId = DbUtils.GetInt(reader, "CommentUserProfileId")
                                });
                            }
                        }
                        return KeyValuePair.Create(userProfile, videos);
                    }
                }
            }
        }
        public UserProfile GetByIdWithVideos(int id)
        {
            UserProfile profile = null;
            using (var conn = Connection)
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                    select up.*,
                    v.Id VideoId,
                    v.Title, 
                    v.Description, 
                    v.Url, 
                    v.DateCreated VideoDateCreated,
                    c.Id CommentId,
                    c.Message, 
                    c.UserProfileId CommentUserProfileId
                    from UserProfile up
                    LEFT JOIN Video v ON v.UserProfileId = up.Id
                    LEFT JOIN Comment c ON c.VideoId = v.Id
                    WHERE up.Id = @id;
                    ";
                    DbUtils.AddParameter(cmd, "@id", id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (profile == null)
                            {
                                profile = new UserProfile
                                {
                                    Id = DbUtils.GetInt(reader, "Id"),
                                    Name = DbUtils.GetString(reader, "Name"),
                                    Email = DbUtils.GetString(reader, "Email"),
                                    DateCreated = DbUtils.GetDateTime(reader, "DateCreated"),
                                    ImageUrl = DbUtils.GetString(reader, "ImageUrl"),
                                    Videos = new List<Video>()
                                };
                            }

                            if (DbUtils.IsNotDbNull(reader, "VideoId"))
                            {
                                var existingVideo = profile.Videos.FirstOrDefault(v => v.Id == DbUtils.GetInt(reader, "VideoId"));
                                if (existingVideo == null)
                                {
                                    existingVideo = new Video
                                    {
                                        Id = DbUtils.GetInt(reader, "VideoId"),
                                        Title = DbUtils.GetString(reader, "Title"),
                                        Description = DbUtils.GetString(reader, "Description"),
                                        DateCreated = DbUtils.GetDateTime(reader, "VideoDateCreated"),
                                        Url = DbUtils.GetString(reader, "Url"),
                                        Comments = new List<Comment>()
                                    };

                                    profile.Videos.Add(existingVideo);
                                }

                                if (DbUtils.IsNotDbNull(reader, "CommentId"))
                                {
                                    existingVideo.Comments.Add(new Comment
                                    {
                                        Id = DbUtils.GetInt(reader, "CommentId"),
                                        Message = DbUtils.GetString(reader, "Message"),
                                        UserProfileId = DbUtils.GetInt(reader, "CommentUserProfileId")
                                    });
                                }
                            }
                        }
                    }
                }
            }

            return profile;
        }
    }
}
