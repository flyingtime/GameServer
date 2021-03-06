﻿using InibinSharp;
using LeagueSandbox.GameServer.Core.Logic.RAF;
using LeagueSandbox.GameServer.Logic.Enet;
using System;
using System.Linq;
using System.Numerics;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class Monster : Unit
    {
        private RAFManager _rafManager = Program.ResolveDependency<RAFManager>();

        public Vector2 Facing { get; private set; }
        public string Name { get; private set; }
        public string SpawnAnimation { get; private set; }
        public byte CampId { get; private set; }
        public byte CampUnk { get; private set; }
        public float SpawnAnimationTime { get; private set; }

        public Monster(
            float x,
            float y,
            float facingX,
            float facingY,
            string model,
            string name,
            string spawnAnimation = "",
            byte campId = 0x01,
            byte campUnk = 0x2A,
            float spawnAnimationTime = 0.0f,
            uint netId = 0
        ) : base(model, new Stats(), 40, x, y, 0, netId)
        {
            SetTeam(TeamId.TEAM_NEUTRAL);

            var teams = Enum.GetValues(typeof(TeamId)).Cast<TeamId>();
            foreach (var team in teams)
                SetVisibleByTeam(team, true);

            MoveOrder = MoveOrder.MOVE_ORDER_MOVE;
            this.Facing = new Vector2(facingX, facingY);
            this.Name = name;
            this.SpawnAnimation = spawnAnimation;
            this.CampId = campId;
            this.CampUnk = campUnk;
            this.SpawnAnimationTime = spawnAnimationTime;

            Inibin inibin;
            if (!_rafManager.readInibin("DATA/Characters/" + model + "/" + model + ".inibin", out inibin))
            {
                _logger.LogCoreError("couldn't find monster stats for " + model);
                return;
            }

            stats.HealthPoints.BaseValue = inibin.getFloatValue("Data", "BaseHP");
            stats.CurrentHealth = stats.HealthPoints.Total;
            stats.ManaPoints.BaseValue = inibin.getFloatValue("Data", "BaseMP");
            stats.CurrentMana = stats.ManaPoints.Total;
            stats.AttackDamage.BaseValue = inibin.getFloatValue("DATA", "BaseDamage");
            stats.Range.BaseValue = inibin.getFloatValue("DATA", "AttackRange");
            stats.MoveSpeed.BaseValue = inibin.getFloatValue("DATA", "MoveSpeed");
            stats.Armor.BaseValue = inibin.getFloatValue("DATA", "Armor");
            stats.MagicResist.BaseValue = inibin.getFloatValue("DATA", "SpellBlock");
            stats.HealthRegeneration.BaseValue = inibin.getFloatValue("DATA", "BaseStaticHPRegen");
            stats.ManaRegeneration.BaseValue = inibin.getFloatValue("DATA", "BaseStaticMPRegen");
            stats.AttackSpeedFlat = 0.625f / (1 + inibin.getFloatValue("DATA", "AttackDelayOffsetPercent"));

            stats.HealthPerLevel = inibin.getFloatValue("DATA", "HPPerLevel");
            stats.ManaPerLevel = inibin.getFloatValue("DATA", "MPPerLevel");
            stats.AdPerLevel = inibin.getFloatValue("DATA", "DamagePerLevel");
            stats.ArmorPerLevel = inibin.getFloatValue("DATA", "ArmorPerLevel");
            stats.MagicResistPerLevel = inibin.getFloatValue("DATA", "SpellBlockPerLevel");
            stats.HealthRegenerationPerLevel = inibin.getFloatValue("DATA", "HPRegenPerLevel");
            stats.ManaRegenerationPerLevel = inibin.getFloatValue("DATA", "MPRegenPerLevel");
            stats.GrowthAttackSpeed = inibin.getFloatValue("DATA", "AttackSpeedPerLevel");

            IsMelee = inibin.getBoolValue("DATA", "IsMelee");
            CollisionRadius = inibin.getIntValue("DATA", "PathfindingCollisionRadius");

            var autoAttack = _rafManager.GetAutoAttackData(model);
            if (autoAttack == null)
            {
                _logger.LogCoreError("Couldn't find monster auto-attack data for {0}", model);
                return;
            }

            AutoAttackDelay = autoAttack.getFloatValue("SpellData", "castFrame") / 30.0f;
            AutoAttackProjectileSpeed = autoAttack.getFloatValue("SpellData", "MissileSpeed");
        }

        public override void update(long diff)
        {
            base.update(diff);
        }

        public override bool isInDistress()
        {
            return DistressCause != null;
        }
    }
}
