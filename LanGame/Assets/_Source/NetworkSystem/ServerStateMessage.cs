using System;
using System.Collections.Generic;

namespace NetworkSystem
{
    [Serializable]
    public class ServerStateMessage
    {
        public string type;
        public Dictionary<string, PlayerState> players;
        public Dictionary<string, BulletState> bullets;
    }

    [Serializable]
    public class PlayerState
    {
        public string id;
        public float x;
        public float y;
        public int hp;
        public bool isDead;
    }

    [Serializable]
    public class BulletState
    {
        public string id;
        public string ownerId;
        public float x;
        public float y;
    }
}