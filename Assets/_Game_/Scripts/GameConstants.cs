// GameConstants.cs

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameConstants
{
    public enum BotMoveType
    {
        Infantry,
        Tank,
        Thuyen_Higgins_SPXe,
        Thuyen_Cano,
        Xe,
        Helicopter,
        Thuyen_Higgins_SPTank,
        Infantry_On_Vehicle,
        Bot_Somali04,
        Aircraft_Y8_01,
        Aircraft_Swordfish,
        J15_JetFighter,
        Tank_M1A2,
        Aircraft_A10Warthog,
        Infantry_Nhaydu,
        Thuyen_PR1124,
        Robot_Mech_one_hand,
        Robot_mech_ronin,
        heli_24hind_mi28_tutorial,
    }

    public enum PlayMode
    {
        Default,
        BaseRaidS
    }

    public enum Gift
    {
        GiftFirerate,
        GiftElectricrate,
        GiftWeapon81,
        GiftWeapon123,
        GiftWeapon114,
    }

    public enum Weapon
    {
        weapon_26,
        weapon_81,
        weapon_123,
        weapon_114,
        weapon_50,
    }

    public enum MissileControl
    {
        None = 0,
        Weapon38 = 1,
    }

    public enum Missile_Player
    {
        Missile,
    }

    public enum Missile_Enemy
    {
    }

    public enum EffectType
    {
        None = 0,
        VFX_DirtImpact,
        VFX_MetalAirImpact,
        VFX_BloodEffect_Optimize,
        vfx_ConcreteImpact,
    }

    public enum ProjecctileZombie
    {
        None
    }

    public enum ProjecttilePlayer
    {
        None,
        Projectile_Bullet_Norman,
        Projectile_Bullet_BBQ,
        bullet_Rocket,
        projectile_Bullet_Electric,

    }

    public enum ProjectileEnemy
    {
        None,
        Projectile_Bullet_Rocket,
        Projectile_Bullet_BBQ,
        bullet_Rocket,
        projectile_Bullet_Electric,
        rocket_Panzerwerfer,
        Explsion,
        RocketTankWanze,
        BombDropWarthog,
        RocketTungWarthog2,
        RocketSupersoldat,
        ParachuteHeli111,
        BulletSourceForRonin,
        MiniRocket_RoninPhase3_Fake,
        MiniRocket_RoninPhase3_Real,
    }
    public enum Other
    {
    }

    public enum GameState
    {
        None,
        Loading,
        InGame,
        GameOver,
        GameWin
    }

    public enum AchievementAnimationParameter
    {
        None = 0,
        Killmark_center_1 = 1,
        Killmark_center_2 = 2,
        Killmark_center_3 = 3,
        Killmark_center_4 = 4,
        Killmark_center_5 = 4,
        Move_icon1 = 5,
        Move_icon2 = 6,
        Move_icon3 = 7,
    }

    public enum TypeStartAnim
    {
        None = 0,
        TrenThuyen = 1,
        TrenTrucThang = 2,
        TrucThangThaDay = 3,
    }

    public enum EnemyState
    {
        Start,
        Idle,
        Move,
        Attack,
        Reload,
        Dead,
        DeadExplosion,
        DeadExplosionHelicopter, // Trực thăng nổ tung để người bay ra ngoài
        DropTroops, // Trực thăng thả quân, thuyền thả quân thả vehicle
        Falling, // Trực thăng quay mòng mòng rơi xuống đất
        Retreat, // Aircraft retreat phase
        LoopBack, // Aircraft loopback phase với 720° roll
        Shield, // lá chắn năng lượng
        Stun, //choáng
        Special, // trạng thái đặc biệt, ví dụ mech ronin bay lên trời, hoặc supersoldat biến hình
    }

    public enum AudioType
    {
        None = 0,
        GameLooping = 1,
        GameWin = 2,
        GameOver = 3,
        CallTeamMove = 4,
        CallTeamAttack = 5,
        Suicide = 6,
        BotDeath = 7,
        BotAttack = 8,
        GetHit
    }

    public enum AchievementType
    {
        Killmark1,
        Killmark2,
        Killmark3
    }


    #region Hash cho animator

    public static readonly string HashStart = "Start";
    public static readonly string HashEndStart = "EndStart";
    public static readonly string HashIdle = "Idle";
    public static readonly string HashMove = "Move";
    public static readonly string HashAttack = "Attack";
    public static readonly string HashReload = "Reload";
    public static readonly string HashDead = "Dead";
    public static readonly string HashDeadExplosion = "DeadExplosion";
    public static readonly string HashAnimType = "AnimType";
    public static readonly string HashOpenDoor = "OpenDoor";
    public static readonly string HashCloseDoor = "CloseDoor";
    public static readonly string HashStun = "Stun";
    public static readonly string HashShield = "Shield";

    #endregion
}