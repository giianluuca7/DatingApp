using API.Entities;

namespace api.Entities
{
    public class UserLike
    {
        public AppUser SourceUser { get; set; }
        
        public int SoucerUserId { get; set; }
        
        public AppUser TargetUser { get; set; }
        
        public int TargetUserId { get; set; }
    }
}