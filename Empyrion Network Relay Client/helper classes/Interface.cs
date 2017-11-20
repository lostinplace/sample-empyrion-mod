using System.Collections.Generic;

namespace ENRC.data
{
    public class PVector3 : ObservableClass
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }

        public PVector3()
        {
        }

        public PVector3(float nX, float nY, float nZ)
        {
            x = nX; y = nY; z = nZ;
        }

        public Eleon.Modding.PVector3 ToPVector3()
        {
            return new Eleon.Modding.PVector3(x, y, z);
        }
    }

    public class ItemStack : ObservableClass
    {
        public int ammo { get; set; }
        public int count { get; set; }
        public int decay { get; set; }
        public int id { get; set; }
        public byte slotIdx { get; set; }

        public void FromItemStack(Eleon.Modding.ItemStack itemStack)
        {
            ammo = itemStack.ammo;
            count = itemStack.count;
            decay = itemStack.decay;
            id = itemStack.id;
            slotIdx = itemStack.slotIdx;
        }
    }

    public class PlayerInfo : ObservableClass
    {
        public int clientId { get; set; }
        public int entityId { get; set; }
        public string steamId { get; set; }
        public string steamOwnerId { get; set; }
        public string playerName { get; set; }
        public string playfield { get; set; }
        public string startPlayfield { get; set; }
        public PVector3 pos { get; set; }
        public PVector3 rot { get; set; }
        public byte factionGroup { get; set; }
        public int factionId { get; set; }
        public byte factionRole { get; set; }
        public int origin { get; set; }
        public float health { get; set; }
        public float healthMax { get; set; }
        public float oxygen { get; set; }
        public float oxygenMax { get; set; }
        public float stamina { get; set; }
        public float staminaMax { get; set; }
        public float kills { get; set; }
        public float died { get; set; }
        public double credits { get; set; }
        public List<ItemStack> toolbar { get; set; }
        public List<ItemStack> bag { get; set; }
        public int exp { get; set; }
        public int upgrade { get; set; }
        public float bpRemainingTime { get; set; }
        public Dictionary<int, float> bpResourcesInFactory { get; set; }
        public string bpInFactory { get; set; }
        public List<string> producedPrefabs { get; set; }
        public int ping { get; set; }

        public void FromPlayerInfo(Eleon.Modding.PlayerInfo playerinfo)
        {
            clientId = playerinfo.clientId;
            entityId = playerinfo.entityId;
            steamId = playerinfo.steamId;
            steamOwnerId = playerinfo.steamOwnerId;
            playerName = playerinfo.playerName;
            playfield = playerinfo.playfield;
            startPlayfield = playerinfo.startPlayfield;
            pos = new PVector3();
            pos.x = playerinfo.pos.x;
            pos.y = playerinfo.pos.y;
            pos.z = playerinfo.pos.z;
            OnPropertyChanged("pos");
            rot = new PVector3();
            rot.x = playerinfo.rot.x;
            rot.y = playerinfo.rot.y;
            rot.z = playerinfo.rot.z;
            OnPropertyChanged("rot");
            factionGroup = playerinfo.factionGroup;
            factionId = playerinfo.factionId;
            factionRole = playerinfo.factionRole;
            origin = playerinfo.origin;
            health = playerinfo.health;
            healthMax = playerinfo.healthMax;
            oxygen = playerinfo.oxygen;
            oxygenMax = playerinfo.oxygenMax;
            stamina = playerinfo.stamina;
            staminaMax = playerinfo.staminaMax;
            kills = playerinfo.kills;
            died = playerinfo.died;
            credits = playerinfo.credits;

            if (playerinfo.toolbar != null)
            {
                toolbar = new List<ItemStack>();
                ItemStack iSt;
                foreach (Eleon.Modding.ItemStack itemStack in playerinfo.toolbar)
                {
                    iSt = new ItemStack();
                    iSt.FromItemStack(itemStack);
                    toolbar.Add(iSt);
                }
            }

            if (playerinfo.bag != null)
            {
                bag = new List<ItemStack>();
                ItemStack iSt;
                foreach (Eleon.Modding.ItemStack itemStack in playerinfo.bag)
                {
                    iSt = new ItemStack();
                    iSt.FromItemStack(itemStack);
                    bag.Add(iSt);
                }
            }

            exp = playerinfo.exp;
            upgrade = playerinfo.upgrade;
            bpRemainingTime = playerinfo.bpRemainingTime;
            bpResourcesInFactory = playerinfo.bpResourcesInFactory;
            bpInFactory = playerinfo.bpInFactory;
            producedPrefabs = playerinfo.producedPrefabs;
            ping = playerinfo.ping;
        }
    }

    public class StructureInfo : ObservableClass
    {
        public int id { get; set; }
        public string playfield { get; set; }
        public byte type { get; set; }
        public byte factionGroup { get; set; }
        public int factionId { get; set; }
        public string name { get; set; }
        public long lastVisited { get; set; }
        public PVector3 pos { get; set; }
        public PVector3 rot { get; set; }
        public int cntDevices { get; set; }
        public int cntBlocks { get; set; }
        public int cntLights { get; set; }
        public int cntTriangles { get; set; }
        public int classNr { get; set; }
        public int fuel { get; set; }
        public bool powered { get; set; }
        public List<int> dockedShips;
        public int coreType { get; set; } // 0 = no, 1 = the core of the player, 2 = admin core, 3 = alien core, 4 = admin alien core
        public int pilotId { get; set; }

        public string dockedships_Str
        {
            get
            {
                if (dockedShips == null) { return ""; }
                string tmp = "";
                foreach (int ship in dockedShips)
                {
                    tmp = tmp  + ship + "; ";
                }
                return tmp;
            }
        }

        public void FromStructureInfo(Eleon.Modding.GlobalStructureInfo structureInfo, string _playfield)
        {
            id = structureInfo.id;
            type = structureInfo.type;
            factionGroup = structureInfo.factionGroup;
            factionId = structureInfo.factionId;
            playfield = _playfield;
            name = structureInfo.name;
            lastVisited = structureInfo.lastVisitedUTC;
            pos = new PVector3();
            pos.x = structureInfo.pos.x;
            pos.y = structureInfo.pos.y;
            pos.z = structureInfo.pos.z;
            OnPropertyChanged("pos");
            rot = new PVector3();
            rot.x = structureInfo.rot.x;
            rot.y = structureInfo.rot.y;
            rot.z = structureInfo.rot.z;
            OnPropertyChanged("rot");
            cntDevices = structureInfo.cntDevices;
            cntBlocks = structureInfo.cntBlocks;
           cntLights = structureInfo.cntLights;
            cntTriangles = structureInfo.cntTriangles;
            classNr = structureInfo.classNr;
           fuel= structureInfo.fuel;
            powered =structureInfo.powered;
            dockedShips = structureInfo.dockedShips;
            coreType = structureInfo.coreType;
            pilotId = structureInfo.pilotId;
        }
    }

    public class EntityInfo : ObservableClass
    {
        public int id { get; set; }
        public string playfield { get; set; }
        public int type { get; set; }
        public PVector3 pos { get; set; }

        public void FromEntityInfo(Eleon.Modding.EntityInfo entityInfo, string _playfield)
        {
            id = entityInfo.id;
            type = entityInfo.type;
            playfield = _playfield;
            pos = new PVector3();
            pos.x = entityInfo.pos.x;
            pos.y = entityInfo.pos.y;
            pos.z = entityInfo.pos.z;
            OnPropertyChanged("pos");        
        }
    }
}
