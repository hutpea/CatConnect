using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatConnect
{
    [System.Serializable]
    public class LevelData
    {
        public string levelName;
        public List<LevelTargetJson> levelTargets;
        public int moves;
        public int boardHeight;
        public int boardWidth;
        public List<int> chooseCats;
        public string boardLayout;
    }

    [System.Serializable]
    public class LevelTargetJson{
        public string type;
        public int amount;
    }
}
