using System;

namespace Discord_Mobile.Models
{
    class UsersTyping
    {
        public string Username { set; get; }
        public DateTime TimeChecked { set; get; }

        public bool ToBeDeleted()
        {
            if (DateTime.Now - TimeChecked > TimeSpan.FromSeconds(5))
                return true;
            else
                return false;
        }

        public void UpdateTime()
        {
            TimeChecked = DateTime.Now;
        }
    }
}
